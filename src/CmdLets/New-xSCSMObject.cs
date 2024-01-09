using System;
using System.Collections;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.ConnectorFramework;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.New, "xSCSMObject", SupportsShouldProcess = true, DefaultParameterSetName = "name")]
    public class NewSMObjectCommand : ObjectCmdletHelper
    {
        private ManagementPackClass _class = null;
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "class")]
        public ManagementPackClass Class
        {
            get { return _class; }
            set { _class = value; }
        }

        private Hashtable _property;
        [Parameter(Position = 1, Mandatory = true, ValueFromPipeline = true)]
        public Hashtable PropertyHashtable
        {
            get { return _property; }
            set { _property = value; }
        }

        private ManagementPackObjectTemplate _template = null;
        [Parameter]
        public ManagementPackObjectTemplate Template
        {
            get { return _template; }
            set { _template = value; }
        }

        private SwitchParameter _passthru;
        [Parameter]
        public SwitchParameter PassThru
        {
            get { return _passthru; }
            set { _passthru = value; }
        }

        // On NoCommit, don't commit, just return
        // the created object. This is needed for those
        // operations that require that an instance not
        // already exist
        private SwitchParameter _noCommit;
        [Parameter]
        public SwitchParameter NoCommit
        {
            get { return _noCommit; }
            set { _noCommit = value; }
        }

        private IncrementalDiscoveryData pendingChanges;
        private int batchSize = 200;
        private int toCommit = 0;
        private SwitchParameter _bulk;
        [Parameter]
        public SwitchParameter Bulk
        {
            get { return _bulk; }
            set { _bulk = value; }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (Bulk) { pendingChanges = new IncrementalDiscoveryData(); }
            if (Class == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentNullException("Class"), "Class Not Found", ErrorCategory.ObjectNotFound, "Class"));
            }
        }
        // We're going to call commit for each object passed to us
        // TODO: Create an array of objects and commit them in
        // one operation
        protected override void ProcessRecord()
        {
            // Create an object
            CreatableEnterpriseManagementObject o = new CreatableEnterpriseManagementObject(_mg, Class);
            // Apply the template if needed
            if (Template != null) { o.ApplyTemplate(Template); }
            // Just to make things easier to deal with, we'll create a hash table of 
            // the properties in the object.
            //
            // TODO: ADD GENERIC PROPERTIES
            // Create a hashtable of the properties, ignore case so if we find one
            // we can grab the property and assign the new value!
            Hashtable ht = new Hashtable(StringComparer.OrdinalIgnoreCase);
            foreach (ManagementPackProperty prop in o.GetProperties())
            {
                try
                {
                    ht.Add(prop.Name, prop);
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, "property '" + prop.Name + "' has already been added to collection", ErrorCategory.InvalidOperation, prop));
                }
            }

            // now go through the hashtable that has the values we we want to use and
            // assigned them into the new values
            foreach (string s in PropertyHashtable.Keys)
            {
                if (!ht.ContainsKey(s))
                {
                    WriteError(new ErrorRecord(new ObjectNotFoundException(s), "property not found on object", ErrorCategory.NotSpecified, o));
                }
                else
                {
                    ManagementPackProperty p = ht[s] as ManagementPackProperty;
                    AssignNewValue(p, o[p], PropertyHashtable[s]);
                }
            }

            // Now that we're done, we can commit it
            // TODO: if we get an exception indicating we're disconnected
            // Reconnect and try again.
            if (ShouldProcess(Class.Name))
            {
                try
                {
                    if (Bulk)
                    {
                        toCommit++;
                        pendingChanges.Add(o);
                        if (toCommit >= batchSize)
                        {
                            toCommit = 0;
                            pendingChanges.Commit(_mg);
                            pendingChanges = new IncrementalDiscoveryData();
                        }
                    }
                    else
                    {
                        if (NoCommit)
                        {
                            WriteObject(o);
                        }
                        else
                        {
                            o.Commit();
                            // on PassThru get the ID and call GetObject
                            // we don't want to hand back the CreatableEnterpriseObject, but rather the thing
                            // that was saved.
                            if (_passthru)
                            {
                                WriteObject(
                                    ServiceManagerObjectHelper.AdaptManagementObject(
                                     this, _mg.EntityObjects.GetObject<EnterpriseManagementObject>(o.Id, ObjectQueryOptions.Default)
                                     )
                                 );
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, "Could not save new object", ErrorCategory.InvalidOperation, o));
                }
            }
        }
        protected override void EndProcessing()
        {
            base.EndProcessing();
            if (Bulk)
            {
                try
                {
                    pendingChanges.Commit(_mg);
                }
                catch (Exception e)
                {
                    WriteWarning("Commit failed, trying Overwrite: " + e.Message);
                    try
                    {
                        pendingChanges.Overwrite(_mg);
                    }
                    catch (Exception x)
                    {
                        WriteError(new ErrorRecord(x, "Could not save new object with commit or overwrite", ErrorCategory.InvalidOperation, pendingChanges));
                    }
                }
            }
        }
    }

}