using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMPageSet", DefaultParameterSetName = "Name")]
    public class GetSCSMPageSetCommand : PresentationCmdletBase
    {
        private IList<ManagementPackUIPageSet> list;
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            list = _mg.Presentation.GetPageSets();
        }
        protected override void ProcessRecord()
        {
            foreach (string p in Name)
            {
                WildcardPattern pattern = new WildcardPattern(p, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
                foreach (ManagementPackUIPageSet v in list)
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
