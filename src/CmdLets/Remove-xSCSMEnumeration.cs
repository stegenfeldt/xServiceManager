using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Remove, "xSCSMEnumeration", SupportsShouldProcess = true)]
    public class RemoveSMEnumerationCommand : EntityTypeHelper
    {
        private ManagementPackEnumeration _enumeration;

        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public ManagementPackEnumeration Enumeration
        {
            get { return _enumeration; }
            set { _enumeration = value; }

        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            ManagementPackEnumeration enumeration = _mg.EntityTypes.GetEnumeration(_enumeration.Id);
            ManagementPack mp = enumeration.GetManagementPack();
            enumeration.Status = ManagementPackElementStatus.PendingDelete;
            string enumInfo = _enumeration.Name;
            if (_enumeration.DisplayName != null)
            {
                enumInfo = _enumeration.DisplayName;
            }
            if (ShouldProcess(enumInfo))
            {
                mp.AcceptChanges();
            }
        }
    }

}