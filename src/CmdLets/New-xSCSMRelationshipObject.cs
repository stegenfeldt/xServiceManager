using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.ConnectorFramework;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.New, "xSCSMRelationshipObject", SupportsShouldProcess = true)]
    public class NewSCSMRelationshipObject : ObjectCmdletHelper
    {
        private ManagementPackRelationship _relationship;
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public ManagementPackRelationship Relationship
        {
            get { return _relationship; }
            set { _relationship = value; }
        }
        private Hashtable _properties;
        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true)]
        public Hashtable Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }

        private SwitchParameter _passThru;
        [Parameter]
        public SwitchParameter PassThru
        {
            get { return _passThru; }
            set { _passThru = value; }
        }

        private EnterpriseManagementObject _source;
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public EnterpriseManagementObject Source
        {
            get { return _source; }
            set { _source = value; }
        }

        private EnterpriseManagementObject _target;
        [Parameter(Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public EnterpriseManagementObject Target
        {
            get { return _target; }
            set { _target = value; }
        }

        private SwitchParameter _bulk;
        [Parameter(ParameterSetName = "bulk")]
        public SwitchParameter Bulk
        {
            get { return _bulk; }
            set { _bulk = value; }
        }
        private IncrementalDiscoveryData idd = null;

        private SwitchParameter _noCommit;
        [Parameter(ParameterSetName = "NoCommit")]
        public SwitchParameter NoCommit
        {
            get { return _noCommit; }
            set { _noCommit = value; }
        }

        private SwitchParameter _progress;
        [Parameter(ParameterSetName = "bulk")]
        public SwitchParameter Progress
        {
            get { return _progress; }
            set { _progress = value; }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (Bulk)
            {
                idd = new IncrementalDiscoveryData();
            }
        }

        private int count = 0;
        protected override void ProcessRecord()
        {
            CreatableEnterpriseManagementRelationshipObject ro = new Microsoft.EnterpriseManagement.Common.CreatableEnterpriseManagementRelationshipObject(_mg, Relationship);
            try
            {
                ro.SetSource(Source);
            }
            catch (Exception e)
            {
                ThrowTerminatingError(new ErrorRecord(e, "SourceError", ErrorCategory.InvalidOperation, ro));
            }
            try
            {
                ro.SetTarget(Target);
            }
            catch (Exception e)
            {
                ThrowTerminatingError(new ErrorRecord(e, "TargetError", ErrorCategory.InvalidOperation, ro));
            }
            IList<ManagementPackProperty> props = ro.GetProperties();
            if (Properties != null)
            {
                foreach (string s in Properties.Keys)
                {
                    try
                    {
                        WriteVerbose("looking for property " + s);
                        foreach (ManagementPackProperty p in props)
                        {
                            if (String.Compare(p.Name, s, true) == 0)
                            {
                                WriteVerbose("Setting " + s + " to " + Properties[s]);
                                AssignNewValue(p, ro[p], Properties[s]);
                                // ro[p].Value = Properties[s];
                                break;
                            }
                        }
                    }
                    catch
                    {
                        WriteError(new ErrorRecord(new ItemNotFoundException(s), "Value " + s + " is null", ErrorCategory.ObjectNotFound, Properties));
                    }
                }
            }
            if (ShouldProcess("Commit changes"))
            {
                try
                {
                    if (Bulk)
                    {
                        WriteVerbose("Adding " + ro.RelationshipId.ToString() + " to IDD");
                        if (Progress)
                        {
                            count++;
                            WriteProgress(new ProgressRecord(1, "Adding to incremental discovery data", ro.TargetObject.DisplayName));
                        }
                        idd.Add(ro);
                    }
                    else if (NoCommit)
                    {
                        WriteObject(ro);
                    }
                    else
                    {
                        ro.Commit();
                    }
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, "Relationshipship Error", ErrorCategory.InvalidOperation, ro));
                }
            }
            if (PassThru && !NoCommit) { WriteObject(ro); }
            // WriteObject(ro);
        }
        protected override void EndProcessing()
        {
            base.EndProcessing();
            if (Bulk)
            {
                if (ShouldProcess("Commit Relationship Object"))
                {
                    if (Progress)
                    {
                        WriteProgress(new ProgressRecord(1, "Committing Relationships", count + " instances"));
                    }
                    idd.Commit(_mg);
                }
            }
        }
    }
}