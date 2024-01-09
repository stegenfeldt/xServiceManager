using System.Management.Automation;

namespace xServiceManager.Module
{

    [Cmdlet(VerbsCommon.Get, "xSCSMWhoAmI")]
    public class GetSCSMWhoAmICommand : ObjectCmdletHelper
    {
        private SwitchParameter _raw;
        [Parameter]
        public SwitchParameter Raw
        {
            get { return _raw; }
            set { _raw = value; }
        }
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            string userName = _mg.GetUserName();
            if (Raw)
            {
                WriteObject(userName);
            }
            else
            {
                WriteObject(UserHelper.GetUserObjectFromString(_mg, userName, this));
            }
        }
    }
}
