using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{

    // Implementation of Remove-ManagementPack
    // This removes a managementpack from the system
    [Cmdlet(VerbsCommon.Remove, "xSCSMManagementPack", SupportsShouldProcess = true)]
    public class RemoveManagementPackCommand : SMCmdletBase
    {
        // Private data
        private ManagementPack _mp;
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public ManagementPack ManagementPack
        {
            get { return _mp; }
            set { _mp = value; }
        }

        protected override void ProcessRecord()
        {
            string mpInfo = _mp.Name;
            if (_mp.DisplayName != null)
            {
                mpInfo = _mp.DisplayName;
            }
            if (ShouldProcess(mpInfo))
            {
                _mg.ManagementPacks.UninstallManagementPack(ManagementPack);
            }

        }
    }
}
