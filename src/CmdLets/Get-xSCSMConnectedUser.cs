using System.Management.Automation;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMConnectedUser")]
    public class GetSCSMConnectedUserCommand : ObjectCmdletHelper
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
            foreach (string s in _mg.GetConnectedUserNames())
            {
                if (Raw)
                {
                    WriteObject(s);
                }
                else
                {
                    WriteObject(UserHelper.GetUserObjectFromString(_mg, s, this));
                }
            }
        }
    }

#region SCSMSession cmdlets
    
    #endregion
}
