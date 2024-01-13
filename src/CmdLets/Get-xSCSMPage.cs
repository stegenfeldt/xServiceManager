using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMPage", DefaultParameterSetName = "Name")]
    public class GetSCSMPageCommand : PresentationCmdletBase
    {
        private IList<ManagementPackUIPage> list;
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            list = _mg.Presentation.GetPages();
        }
        protected override void ProcessRecord()
        {
            foreach (string p in Name)
            {
                WildcardPattern pattern = new WildcardPattern(p, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
                foreach (ManagementPackUIPage v in list)
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
