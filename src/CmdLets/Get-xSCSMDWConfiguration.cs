using System.Management.Automation;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMDWConfiguration")]
    public class GetDataWarehouseConfigurationCommand : SMCmdletBase
    {
        protected override void EndProcessing()
        {
            WriteObject(_mg.DataWarehouse.GetDataWarehouseConfiguration());
        }
    }
}
