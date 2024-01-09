using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMViewType", DefaultParameterSetName = "Name")]
    public class GetSCSMViewTypeCommand : PresentationCmdletBase
    {
        private IList<ManagementPackViewType> list;
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            list = _mg.Presentation.GetViewTypes();
        }
        protected override void ProcessRecord()
        {
            foreach (string p in Name)
            {
                WildcardPattern pattern = new WildcardPattern(p, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
                foreach (ManagementPackViewType v in list)
                {
                    if (pattern.IsMatch(v.Name))
                    {
                        WriteObject(v);
                    }
                }
            }
        }
    }
}
