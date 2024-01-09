using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.ConnectorFramework;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Remove, "xSCSMObject", SupportsShouldProcess = true)]
    public class RemoveSMObjectCommand : ObjectCmdletHelper
    {
        // The adapted EMO
        private PSObject _smobject;
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public PSObject SMObject
        {
            set { _smobject = value; }
            get { return _smobject; }
        }

        private SwitchParameter _force;
        [Parameter]
        public SwitchParameter Force
        {
            set { _force = value; }
            get { return _force; }
        }

        private SwitchParameter _progress;
        [Parameter]
        public SwitchParameter Progress
        {
            get { return _progress; }
            set { _progress = value; }
        }

        private ManagementPackEnumeration pendingDelete = null;

        // Remove is done via IncrementalDiscoveryData
        private IncrementalDiscoveryData idd;
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            pendingDelete = _mg.ManagementPacks.GetManagementPack(SystemManagementPack.System).GetEnumeration("System.ConfigItem.ObjectStatusEnum.PendingDelete");
            idd = new IncrementalDiscoveryData();
        }
        protected override void ProcessRecord()
        {
            EnterpriseManagementObject orig = (EnterpriseManagementObject)SMObject.BaseObject;
            if (ShouldProcess(orig.Name))
            {
                try
                {
                    if (Progress) { WriteProgress(new ProgressRecord(0, "Remove Instance", orig.Name)); }
                    if (Force)
                    {
                        idd.Remove(orig);
                    }
                    else
                    {
                        try
                        {
                            orig[null, "ObjectStatus"].Value = pendingDelete;
                            idd.Add(orig);
                        }
                        catch (NullReferenceException e)
                        {
                            ErrorRecord er = new ErrorRecord(e, "ObjectStatus Property", ErrorCategory.ObjectNotFound, orig);
                            ErrorDetails ed = new ErrorDetails("This object cannot be marked for deletion because it is not derived from System.ConfigItem");
                            ed.RecommendedAction = "Use the -Force parameter to remove this object";
                            er.ErrorDetails = ed;
                            WriteError(er);
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, "Object", ErrorCategory.NotSpecified, orig.Name));
                }
            }
        }

        // after you've added all the instances to the incrementaldiscoverydata
        // commit the changes
        protected override void EndProcessing()
        {
            if (ShouldProcess("Commit"))
            {
                try
                {
                    if (Progress) { WriteProgress(new ProgressRecord(0, "Remove Instance", "Committing Removal")); }
                    idd.Commit(_mg);
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, "Object", ErrorCategory.NotSpecified, "Commit"));
                }
            }
        }
    }
}