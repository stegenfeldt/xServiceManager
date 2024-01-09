using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMView", DefaultParameterSetName = "Name")]
    public class GetSCSMViewCommand : PresentationCmdletBase
    {
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            
        }
        protected override void ProcessRecord()
        {
            if (this.Id != Guid.Empty)
            {
                WriteObject(_mg.Presentation.GetView(this.Id));
            }
            else if (!string.IsNullOrEmpty(this.DisplayName))
            {
                var cr = new ManagementPackViewCriteria(string.Format("DisplayName Like '%{0}%'", this.DisplayName));
                var list = _mg.Presentation.GetViews(cr);
                foreach (ManagementPackView v in list)
                {
                    WriteObject(v);
                }
            }
            else
            {
                var list = _mg.Presentation.GetViews();
                foreach (string p in Name)
                {
                    //WildcardPattern pattern = new WildcardPattern(p, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
                    Regex r = new Regex(p, RegexOptions.IgnoreCase);
                    foreach (ManagementPackView v in list)
                    {
                        if (r.Match(v.Name).Success)
                        {
                            WriteObject(v);
                        }
                    }
                }
            }
        }
    }

}
