using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMTopLevelEnumeration")]
    public class GetSMTopLevelEnumerationCommand : EntityTypeHelper
    {
        protected override void ProcessRecord()
        {
            Regex r = new Regex(Name, RegexOptions.IgnoreCase);
            foreach (ManagementPackEnumeration o in _mg.EntityTypes.GetTopLevelEnumerations())
            {
                if (r.Match(o.Name).Success)
                {
                    WriteObject(o);
                }
            }
        }
    }

}