using Microsoft.EnterpriseManagement.Security;
using System.Management.Automation;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Remove, "xSCSMUserRole", SupportsShouldProcess = true)]
    public class RemoveSCSMUserRole : SMCmdletBase
    {
        private UserRole[] _userroles;
        [Parameter(Mandatory = true,
            ValueFromPipeline = true)]
        public UserRole[] UserRole
        {
            get { return _userroles; }
            set { _userroles = value; }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (_userroles != null)
            {
                foreach (UserRole userrole in _userroles)
                {
                    if (!userrole.IsSystem)
                    {
                        string userInfo = userrole.Name;
                        if (userrole.DisplayName != null)
                        {
                            userInfo = userrole.DisplayName;
                        }
                        if (ShouldProcess(userInfo))
                        {
                            _mg.Security.DeleteUserRole(userrole);
                        }
                    }
                }
            }

        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }

    }

}