using System;
using System.Management.Automation;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Remove, "xSCSMGroup", SupportsShouldProcess = true)]
    public class RemoveSCGroupCommand : SMCmdletBase
    {
        private EnterpriseManagementGroupObject[] _group;
        [Parameter(Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public EnterpriseManagementGroupObject[] Group
        {
            get { return _group; }
            set { _group = value; }
        }
        protected override void ProcessRecord()
        {
            foreach (EnterpriseManagementGroupObject g in Group)
            {
                if (g.ManagementPack.Sealed)
                {
                    WriteError(new ErrorRecord(new InvalidOperationException("Can't remove from sealed management pack"), "SealedMP", ErrorCategory.InvalidOperation, g));
                }
                else
                {
                    if (ShouldProcess(g.DisplayName))
                    {
                        g.ManagementPack.DeleteEnterpriseManagementObjectGroup(g.__EnterpriseManagementObject);
                    }
                }
            }
        }
    }
}