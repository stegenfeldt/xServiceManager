using System.Management.Automation;
using Microsoft.EnterpriseManagement;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Remove,"xSCSMSession")]
    public class RemoveSCSMSession : PSCmdlet
    {
        private EnterpriseManagementGroup _emg;
        [Parameter(Mandatory=true,ValueFromPipeline=true,Position=0)]
        public EnterpriseManagementGroup EMG
        {
            get { return _emg; }
            set { _emg = value; }
        }
        protected override void ProcessRecord()
        {
            ConnectionHelper.RemoveMG(EMG.ConnectionSettings);
        }
    }
    
}
