using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Presentation;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMViewSetting", DefaultParameterSetName = "Name")]
    public class GetSCSMViewSettingCommand : PresentationCmdletBase
    {
        private IList<ViewSetting> list;
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            list = _mg.Presentation.GetViewSettings();
        }
        protected override void ProcessRecord()
        {
            foreach (string p in Name)
            {
                WildcardPattern pattern = new WildcardPattern(p, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
                foreach (ViewSetting v in list)
                {
                    if (pattern.IsMatch(v.ViewId.ToString()))
                    {
                        WriteObject(v);
                    }
                }
            }
        }
    }
}
