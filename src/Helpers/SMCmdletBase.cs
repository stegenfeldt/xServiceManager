using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement;
using System.Globalization;

namespace xServiceManager.Module
{
    public class SMCmdletBase : PSCmdlet
    {
        // This contains the ComputerName parameter and the 
        // setup for the ManagementGroup where needed

        // data
        internal EnterpriseManagementGroup _mg;
        // Parameters
        private string _computerName = "localhost";
        [Parameter(Mandatory = false, HelpMessage = "The computer to use for the connection to the Service Manager Data Access Service")]
        [ValidateNotNullOrEmpty]
        public string ComputerName
        {
            get { return _computerName; }
            set { _computerName = value; }
        }
        private PSCredential _credential = null;
        [Parameter]
        public PSCredential Credential
        {
            get { return _credential; }
            set { _credential = value; }
        }
        private EnterpriseManagementGroup _scsmSession = null;
        [Parameter(HelpMessage = "A connection to a Service Manager Data Access Service")]
        public EnterpriseManagementGroup SCSMSession
        {
            get { return _scsmSession; }
            set { _scsmSession = value; }
        }

        private string _threeLetterWindowsLanguageName = CultureInfo.CurrentUICulture.ThreeLetterWindowsLanguageName;
        [Parameter(HelpMessage = "Language code for connection. The default is current UI Culture", Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public string ThreeLetterWindowsLanguageName
        {
            get { return _threeLetterWindowsLanguageName; }
            set { _threeLetterWindowsLanguageName = value; }
        }

        protected override void BeginProcessing()
        {
            try
            {
                // A provided session always wins
                if (SCSMSession != null)
                {
                    WriteVerbose("SCSMSession provided, use it");
                    // Make sure that we have this session in our hash table
                    ConnectionHelper.SetMG(SCSMSession);
                    _mg = SCSMSession;
                }
                else // No session, go hunting
                {
                    WriteVerbose("Checking SMDefaultSession...");
                    PSVariable DefaultSession = SessionState.PSVariable.Get("SMDefaultSession");
                    if (DefaultSession != null && (DefaultSession.Value is EnterpriseManagementGroup || (DefaultSession.Value is PSObject && (DefaultSession.Value as PSObject).BaseObject is EnterpriseManagementGroup)))
                    {
                        WriteVerbose("Default SCSMSession found");
                        _mg = DefaultSession.Value is EnterpriseManagementGroup ?
                            (EnterpriseManagementGroup)DefaultSession.Value :
                             (EnterpriseManagementGroup)(DefaultSession.Value as PSObject).BaseObject;
                        ConnectionHelper.SetMG(_mg);
                    }
                    else
                    {
                        WriteVerbose("Checking SMDefaultComputer...");
                        PSVariable DefaultComputer = SessionState.PSVariable.Get("SMDefaultComputer");
                        if (DefaultComputer != null)
                        {
                            WriteVerbose($"Connect using SMDefaultComputer '{DefaultComputer.Value}'");
                            _mg = ConnectionHelper.GetMG(DefaultComputer.Value.ToString(), _credential, this._threeLetterWindowsLanguageName);
                        }
                        else
                        {
                            WriteVerbose($"Connect using ComputerName '{ComputerName}'");
                            _mg = ConnectionHelper.GetMG(ComputerName, _credential, this._threeLetterWindowsLanguageName);
                        }
                    }
                }
            }
            catch (Exception e) // If we had a problem, the connection is bad, so we have to stop
            {
                ThrowTerminatingError(
                        new ErrorRecord(e, "GenericMessage",
                            ErrorCategory.InvalidOperation, ComputerName)
                        );
            }
        }
    }
}
