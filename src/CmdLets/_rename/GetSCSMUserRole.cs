using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Security;
using System;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMUserRole", DefaultParameterSetName = "name")]
    public class GetSCSMUserRole : SMCmdletBase
    {


        private string[] _name;
        [Parameter(Position = 0, ParameterSetName = "name")]
        public string[] Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private Guid[] _id;
        [Parameter(Position = 0, ParameterSetName = "id")]
        public Guid[] Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string[] _displayName;
        [Parameter(Position = 0, ParameterSetName = "displayName")]
        public string[] DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        private SwitchParameter _noAdapt;
        [Parameter]
        public SwitchParameter NoAdapt
        {
            get { return _noAdapt; }
            set { _noAdapt = value; }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }
        protected override void ProcessRecord()
        {
            if (this.Id != null && this.Id.Length > 0)
            {
                foreach (var id in this.Id)
                {
                    var role = _mg.Security.GetUserRole(id);
                    WriteRole(role);
                }
            }
            else
            {
                var list = _mg.Security.GetUserRoles();
                if (Name != null && Name.Length > 0)
                {
                    foreach (UserRole role in list)
                    {
                        foreach (String n in Name)
                        {
                            Regex r = new Regex(n, RegexOptions.IgnoreCase);
                            if (r.Match(role.Name).Success)
                            {
                                WriteRole(role);
                            }
                        }
                    }
                }
                else if (DisplayName != null && DisplayName.Length > 0)
                {
                    foreach (UserRole role in list)
                    {
                        foreach (String n in DisplayName)
                        {
                            Regex r = new Regex(n, RegexOptions.IgnoreCase);
                            if (r.Match(role.DisplayName).Success)
                            {
                                WriteRole(role);
                            }
                        }
                    }
                }
                else
                {
                    foreach (UserRole role in list)
                    {
                        WriteRole(role);
                    }
                }
            }
        }

        private void WriteRole(UserRole role)
        {
            if (this.NoAdapt)
                WriteObject(role);
            else
                WriteObject(AdaptUserRole(role));
        }

        public Role AdaptUserRole(UserRole role)
        {
            return new Role(role);
        }

        private EnterpriseManagementObject[] GetScopeItems(Guid type, UserRoleScope scope)
        {
            ManagementPackClass managementPackClass = _mg.EntityTypes.GetClass(type);
            return (from q in _mg.EntityObjects.GetObjectReader<EnterpriseManagementObject>(managementPackClass, ObjectQueryOptions.Default)
                    where scope.Objects.Contains(q.Id)
                    select q).ToArray<EnterpriseManagementObject>();
        }

    }

}