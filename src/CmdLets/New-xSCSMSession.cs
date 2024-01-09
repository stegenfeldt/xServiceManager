using System.Management.Automation;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.New,"xSCSMSession")]
    public class NewSCSMSession : SMCmdletBase
    {
        private SwitchParameter _passthru;
        [Parameter]
        public SwitchParameter PassThru
        {
            get { return _passthru; }
            set { _passthru = value; }
        }

        protected override void BeginProcessing()
        {
            // A provided session always wins
            if (SCSMSession != null)
            {
                // Make sure that we have this session in our hash table
                ConnectionHelper.SetMG(SCSMSession);
                _mg = SCSMSession;
            }
            else // No session, go hunting
            {
                PSVariable DefaultComputer = SessionState.PSVariable.Get("SMDefaultComputer");
                if (DefaultComputer != null)
                {
                    _mg = ConnectionHelper.GetMG(DefaultComputer.Value.ToString(), this.Credential, this.ThreeLetterWindowsLanguageName);
                }
                else
                {
                    _mg = ConnectionHelper.GetMG(ComputerName,this.Credential, this.ThreeLetterWindowsLanguageName);
                }
            }
        }

        protected override void ProcessRecord()
        {
            if ( PassThru ) { WriteObject(_mg); }
        }
    }
}
