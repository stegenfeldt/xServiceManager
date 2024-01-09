using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMObjectHistory")]
    public class GetSCSMObjectHistoryCommand : ObjectCmdletHelper
    {
        private EnterpriseManagementObject[] _object;
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public EnterpriseManagementObject[] Object
        {
            get { return _object; }
            set { _object = value; }
        }
        protected override void ProcessRecord()
        {
            foreach (EnterpriseManagementObject emo in Object)
            {
                WriteObject(new SCSMHistory(emo));
            }
        }
    }

}