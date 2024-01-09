using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.ConnectorFramework;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.New, "xSCSMObjectProjection", SupportsShouldProcess = true)]
    public class NewSCSMObjectProjectionCommand : ObjectCmdletHelper
    {
        private string _type = null;
        [Parameter(Mandatory = true, Position = 0)]
        public String Type
        {
            get { return _type; }
            set { _type = value; }
        }

        /*
    ** A projection is represented as a complicated hashtable which has the following layout
    ** @{
    **      __CLASS = <classname of seed object>
    **      __OBJECT = @{
    **          <property value pairs of seed object>
    **          }
    **      # ALIASNAME is the RELATIONSHIP alias as known by the projection
    **      ALIASNAME = @{ 
    **          __CLASS = <classname of the object in the relationship>
    **          # this could be an array, but unlike the case of the seed object,
    **          # this object may already exist, so we have to support a combination
    **          # of EMOs or HashTables (for example, in an incident we may want to associate
    **          # a number of comment logs with the incident, but there's no reason that we shouldn't
    **          # create the comments in real time
    **          __OBJECT = EMO1,EMO2,@{
    **              __CLASS = <classname of target>
    **              __OBJECT = @{
    **                  <property value pairs of target object
    **                  }
    **              },EMO3,etc
    **          }
    **  }
    */
        private Hashtable _projection = null;
        [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true)]
        public Hashtable Projection
        {
            get { return _projection; }
            set { _projection = value; }
        }

        private ManagementPackObjectTemplate _template;
        [Parameter]
        public ManagementPackObjectTemplate Template
        {
            get { return _template; }
            set { _template = value; }
        }
        private SwitchParameter _passThru;
        [Parameter]
        public SwitchParameter PassThru
        {
            get { return _passThru; }
            set { _passThru = value; }
        }

        private SwitchParameter _bulk;
        [Parameter]
        public SwitchParameter Bulk
        {
            get { return _bulk; }
            set { _bulk = value; }
        }

        private SwitchParameter _noCommit;
        [Parameter]
        public SwitchParameter NoCommit
        {
            get { return _noCommit; }
            set { _noCommit = value; }
        }

        private ManagementPackTypeProjection emop = null;
        private List<string> aliasCollection = null;
        private Hashtable aliasHT;

        private int count = 0;
        private int batchSize = 200;
        private IncrementalDiscoveryData pendingChanges;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            if (Bulk && !NoCommit)
            {
                pendingChanges = new IncrementalDiscoveryData();
            }
            if (NoCommit) { Bulk = false; }
            // Find the projection we need
            Regex r = new Regex(Type, RegexOptions.IgnoreCase);
            foreach (ManagementPackTypeProjection p in _mg.EntityTypes.GetTypeProjections())
            {
                if (r.Match(p.Name).Success)
                {
                    emop = p;
                }
            }
            if (emop == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ItemNotFoundException("Projection"), "No such projection", ErrorCategory.ObjectNotFound, Type));
            }
            aliasHT = new Hashtable();
            aliasCollection = new List<string>();
            foreach (ManagementPackTypeProjectionComponent pc in emop.ComponentCollection)
            {
                aliasCollection.Add(pc.Alias);
                // Is there a way to handle SeedRole='Target' here?
                aliasHT.Add(pc.Alias, pc.TargetEndpoint);
                // WriteVerbose("Adding Alias: " + pc.Alias);
            }
            // WriteObject(aliasHT);
        }

        // emop guaranteed to be a valid projection type
        private EnterpriseManagementObjectProjection p = null;
        protected override void ProcessRecord()
        {
            if (Projection.ContainsKey("__SEED"))
            {
                // WriteVerbose("SEED");
                if (Projection["__SEED"] is PSObject)
                {
                    WriteVerbose("Seed is PSObject");
                    PSObject o = (PSObject)Projection["__SEED"];
                    WriteVerbose("Type of seed: " + o.GetType());
                    if (o.ImmediateBaseObject is EnterpriseManagementObject)
                    {
                        WriteVerbose("Attempting to cast");
                        EnterpriseManagementObject seed = (EnterpriseManagementObject)o.ImmediateBaseObject;
                        WriteVerbose("Attempting to create projection");
                        WriteVerbose("Seed is a " + seed.GetType());
                        p = new EnterpriseManagementObjectProjection(seed);
                        WriteVerbose("Created Projection");
                    }
                    else
                    {
                        ThrowTerminatingError(new ErrorRecord(new NullReferenceException("Projection"), "Bad Projection", ErrorCategory.InvalidArgument, o));
                    }
                }
                else if (Projection["__SEED"] is EnterpriseManagementObject)
                {
                    WriteVerbose("Seed is EMO");
                    EnterpriseManagementObject seed = (EnterpriseManagementObject)Projection["__SEED"];
                    p = new EnterpriseManagementObjectProjection(seed);
                }
                else
                {
                    ThrowTerminatingError(new ErrorRecord(new NullReferenceException("Projection"), "Bad Projection", ErrorCategory.InvalidArgument, Projection["__SEED"]));
                }
            }
            else
            {
                // Construct the projection seed
                if (!Projection.ContainsKey("__CLASS"))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException("__CLASS"), "Hashtable Failure", ErrorCategory.InvalidArgument, Projection));
                }
                // WriteVerbose("Seed Class is " + (string)Projection["__CLASS"]);
                ManagementPackClass c = getClassFromName((string)Projection["__CLASS"]);
                if (c == null)
                {
                    ThrowTerminatingError(new ErrorRecord(new ItemNotFoundException("CLASS"), "No such class", ErrorCategory.ObjectNotFound, Projection["__CLASS"]));
                }
                if (!Projection.ContainsKey("__OBJECT"))
                {
                    // WriteObject(Projection);
                    // ThrowTerminatingError( new ErrorRecord(new ArgumentException("__OBJECT"), "Hashtable Failure", ErrorCategory.InvalidArgument, Projection));
                    WriteError(new ErrorRecord(new ArgumentException("__OBJECT"), "Hashtable Failure", ErrorCategory.InvalidArgument, Projection));
                }
                // CreatableEnterpriseManagementObject cemo = new CreatableEnterpriseManagementObject(c.ManagementGroup,c);
                Hashtable seedHash = (Hashtable)Projection["__OBJECT"];
                // WriteVerbose("__OBJECT is " + seedHash);
                p = new EnterpriseManagementObjectProjection(_mg, c);
                AssignNewValues(p.Object, seedHash);
            }
            WriteVerbose("Null Projection? " + (p == null).ToString());
            // OK - the seed is now complete - so work on the rest of the projection. 
            // go through the projection again - since hash tables are
            // TODO:: HANDLE CASE INSENSITIVE COMPARISON
            foreach (string k in Projection.Keys)
            {
                // skip the __CLASS (we did it above)
                if (
                    String.Compare(k, "__CLASS", true) == 0 ||
                    String.Compare(k, "__OBJECT", true) == 0 ||
                    String.Compare(k, "__SEED", true) == 0)
                {
                    continue;
                }
                WriteVerbose("Hunting for key: " + k);
                if (aliasCollection.Contains(k))
                {
                    // WriteVerbose(">>>> setting up alias " + k);
                    // TODO: Check the endpoint take the source or target endpoint where needed.
                    ManagementPackRelationshipEndpoint endpoint = (ManagementPackRelationshipEndpoint)aliasHT[k];
                    // ok - we've got something that is pointed to by an alias
                    if (Projection[k] is Array)
                    {
                        // WriteVerbose("**** inspecting array ****");
                        foreach (Object o in (Array)Projection[k])
                        {
                            if (o is PSObject)
                            {
                                PSObject hashValue = (PSObject)o;
                                // WriteVerbose("  PSObject is of type: " + ((PSObject)o).ImmediateBaseObject.GetType());
                                if (hashValue.ImmediateBaseObject is EnterpriseManagementObject)
                                {
                                    try
                                    {
                                        // WriteVerbose("   Adding EMO to projection");
                                        EnterpriseManagementObject target = (EnterpriseManagementObject)hashValue.ImmediateBaseObject;
                                        p.Add(target, endpoint);
                                    }
                                    catch (Exception e)
                                    {
                                        WriteError(new ErrorRecord(e, "Could not add EMO to projection", ErrorCategory.InvalidOperation, Projection[k]));
                                    }
                                }
                                else if (hashValue.ImmediateBaseObject is EnterpriseManagementObjectProjection)
                                {
                                    try
                                    {
                                        // WriteVerbose("   Adding EMOP to projection");
                                        EnterpriseManagementObjectProjection target = (EnterpriseManagementObjectProjection)hashValue.ImmediateBaseObject;
                                        p.Add(target, endpoint);
                                    }
                                    catch (Exception e)
                                    {
                                        WriteError(new ErrorRecord(e, "Could not add EMOP to projection", ErrorCategory.InvalidOperation, Projection[k]));
                                    }
                                }
                            }
                            else if (o is Hashtable)
                            {
                                // WriteVerbose("  Subelement is hash, creating CEMO");
                                Hashtable v = (Hashtable)o;
                                // WriteObject(v);
                                CreatableEnterpriseManagementObject cEMO = MakecEMOFromHash(v);
                                // WriteVerbose("    Adding cEMO to projection for " + k);
                                p.Add(cEMO, endpoint);
                            }
                            else
                            {
                                // WriteVerbose("   Object is of type: " + o.GetType() + " IGNORING THIS OBJECT");
                            }
                        }
                        // WriteVerbose("**** Done inspecting array ****");
                    }
                    else if (Projection[k] is EnterpriseManagementObject)
                    {
                        // WriteVerbose("Object is an EMO: " + Projection[k]);

                        try
                        {
                            // WriteVerbose("Adding to projection");
                            p.Add((EnterpriseManagementObject)Projection[k], (ManagementPackRelationshipEndpoint)aliasHT[k]);
                        }
                        catch (Exception e)
                        {
                            WriteError(new ErrorRecord(e, "foo", ErrorCategory.InvalidOperation, Projection[k]));
                        }

                    }
                    else if (Projection[k] is PSObject)
                    {
                        PSObject pso = (PSObject)Projection[k];
                        // WriteVerbose("PSObject ImmediateBase: " + pso.ImmediateBaseObject.GetType());
                        if (pso.ImmediateBaseObject is EnterpriseManagementObject)
                        {
                            EnterpriseManagementObject target = (EnterpriseManagementObject)pso.ImmediateBaseObject;
                            try
                            {
                                // WriteVerbose("Adding " + target + " to projection as " + target);
                                p.Add(target, endpoint);
                            }
                            catch (Exception e)
                            {
                                WriteError(new ErrorRecord(e, "foo", ErrorCategory.InvalidOperation, Projection[k]));
                            }
                        }
                    }
                    else if (Projection[k] is EnterpriseManagementObjectProjection)
                    {
                        // WriteVerbose("Object is an EMOP: " + Projection[k]);
                        try
                        {
                            EnterpriseManagementObjectProjection target = (EnterpriseManagementObjectProjection)Projection[k];
                            p.Add(target, endpoint);
                        }
                        catch (Exception e)
                        {
                            WriteError(new ErrorRecord(e, "foo", ErrorCategory.InvalidOperation, Projection[k]));
                        }
                    }
                    else if (Projection[k] is Hashtable)
                    {
                        // WriteVerbose("got a hashtable - will need to build EMO");
                        try
                        {
                            Hashtable relHash = (Hashtable)Projection[k];
                            CreatableEnterpriseManagementObject cEMO = MakecEMOFromHash(relHash);
                            p.Add(cEMO, endpoint);
                        }
                        catch (Exception e)
                        {
                            WriteError(new ErrorRecord(e, "foo", ErrorCategory.InvalidOperation, Projection[k]));
                        }
                        // new CreatableEnterpriseManagementObject(_mg,mpc);
                    }
                    else if (Projection[k] == null)
                    {
                        WriteWarning("!!!! " + k + " value is null, ignoring");
                    }
                    else
                    {
                        WriteVerbose("Got something strange : " + Projection[k].GetType());
                    }
                    // WriteObject(emop[k]);
                    // WriteObject(cEMO);
                    // emop[k];
                    // now attach the object to the projection
                    // p.Add(cEMO, k);
                    // foreach(ManagementPackRelationship o in _mg.EntityTypes.GetRelationshipClasses())
                    // {
                    // ok, we've got a match, so let's add the build and add the object
                    // if ( String.Compare(o.Alias, k, true)) { p.Add(cEMO, o.Target); }
                    // }
                }
                else
                {
                    WriteError(new ErrorRecord(new ObjectNotFoundException(k), "Alias not found on projection", ErrorCategory.NotSpecified, k));
                }
            }
            if (Template != null) { p.ApplyTemplate(Template); }
            // WriteObject(p);
            // WriteObject(emop);
            // WriteVerbose("So far so good!");
            // WriteObject(p);
            // NoCommit is used in those cases where you need a projection
            // as an element of another projection
            if (ShouldProcess("projection batch"))
            {
                if (NoCommit)
                {
                    WriteObject(p);
                }
                else if (Bulk)
                {
                    WriteVerbose("!!!! Adding projection to IDD");
                    pendingChanges.Add(p);
                    count++;
                    if (count >= batchSize)
                    {
                        WriteVerbose("!!!! committing " + count + " projections");
                        pendingChanges.Commit(_mg);
                        pendingChanges = new IncrementalDiscoveryData();
                        count = 0;
                    }
                }
                else
                {
                    try
                    {
                        p.Commit();
                    }
                    catch (Exception e)
                    {
                        WriteError(new ErrorRecord(e, "projection commit failure", ErrorCategory.InvalidOperation, p));

                    }
                }
            }
            if (PassThru) { WriteObject(p); }
        }
        protected override void EndProcessing()
        {
            base.EndProcessing();
            if (ShouldProcess("!!!! Commit last batch of " + count + " projections"))
            {
                if (Bulk && count > 0)
                {
                    WriteVerbose("!!!! Committing last batch of " + count + " incidents");
                    pendingChanges.Commit(_mg);
                }
            }
        }
        private CreatableEnterpriseManagementObject MakecEMOFromHash(Hashtable ht)
        {
            ManagementPackClass mpc = getClassFromName((string)ht["__CLASS"]);
            WriteDebug("type of __OBJECT: " + ht["__OBJECT"].GetType().FullName);
            Hashtable relValues = (Hashtable)ht["__OBJECT"];
            CreatableEnterpriseManagementObject cEMO = new CreatableEnterpriseManagementObject(_mg, mpc);
            WriteDebug("Created new cEMO based on " + mpc.Name);
            AssignNewValues(cEMO, relValues);
            return cEMO;
        }
        private ManagementPackClass getClassFromName(string name)
        {
            foreach (ManagementPackClass c in _mg.EntityTypes.GetClasses())
            {
                if (String.Compare(name, c.Name, true) == 0) { return c; }
            }
            return null;
        }
    }
}