using Microsoft.EnterpriseManagement.Security;
using System;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMUserRoleProfile")]
    public class GetSCUserRoleProfileCommand : SMCmdletBase
    {
        private string _name = null;
        private Regex r;

        [Parameter(ValueFromPipeline = false)]
        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        protected override void BeginProcessing()
        {
            //This will set the _mg which is the EnterpriseManagementGroup object for the connection to the server
            base.BeginProcessing();
            if (Name != null)
            {
                r = new Regex(Name, RegexOptions.IgnoreCase);
            }
        }

        protected override void EndProcessing()
        {
            foreach (Profile profile in _mg.Security.GetProfiles())
            {
                if (_name == null)
                {
                    PSObject o = new PSObject(profile);
                    WriteObject(o);
                }
                else
                {
                    if (r.Match(profile.Name).Success || r.Match(profile.DisplayName).Success)
                    {
                        PSObject o = new PSObject(profile);
                        WriteObject(o);
                    }
                }
            }

        }

    }

}