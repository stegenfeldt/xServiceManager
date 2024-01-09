using System;
using System.Management.Automation;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Remove, "xSCSMQueue", SupportsShouldProcess = true)]
    public class RemoveSCQueueCommand : SMCmdletBase
    {
        private EnterpriseManagementGroupObject[] _queue;
        [Parameter(Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public EnterpriseManagementGroupObject[] Queue
        {
            get { return _queue; }
            set { _queue = value; }
        }
        protected override void ProcessRecord()
        {
            foreach (EnterpriseManagementGroupObject q in Queue)
            {
                if (q.ManagementPack.Sealed)
                {
                    WriteError(new ErrorRecord(new InvalidOperationException("Can't remove from sealed management pack"), "SealedMP", ErrorCategory.InvalidOperation, q));
                }
                else
                {
                    if (ShouldProcess(q.DisplayName))
                    {
                        q.ManagementPack.DeleteEnterpriseManagementObjectGroup(q.__EnterpriseManagementObject);
                    }
                }
            }
        }
    }
}