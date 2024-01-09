using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.ConnectorFramework;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Remove, "xSCSMRelationshipObject", SupportsShouldProcess = true)]
    public class RemoveSMRelationshipObjectCommand : ObjectCmdletHelper
    {
        // The adapted EMO
        private PSObject _smobject;
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public PSObject SMObject
        {
            set { _smobject = value; }
            get { return _smobject; }
        }

        // Remove is done via IncrementalDiscoveryData
        private IncrementalDiscoveryData idd;
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            // pendingDelete = _mg.ManagementPacks.GetManagementPack(SystemManagementPack.System).GetEnumeration("System.ConfigItem.ObjectStatusEnum.PendingDelete");
            idd = new IncrementalDiscoveryData();
        }
        protected override void ProcessRecord()
        {
            EnterpriseManagementRelationshipObject<EnterpriseManagementObject> orig = (EnterpriseManagementRelationshipObject<EnterpriseManagementObject>)SMObject.BaseObject;
            if (ShouldProcess(orig.Id.ToString()))
            {
                try
                {
                    idd.Remove(orig);
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, "Object", ErrorCategory.NotSpecified, orig.Id));
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