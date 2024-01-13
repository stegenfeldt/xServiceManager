using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    #region SCSMFolder cmdlets

    [Cmdlet(VerbsCommon.Get, "xSCSMFolder", DefaultParameterSetName = "Name")]
    public class GetSCSMFolderCommand : PresentationCmdletBase
    {
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }
        protected override void ProcessRecord()
        {
            if (this.Id != Guid.Empty)
            {
                WriteObject(_mg.Presentation.GetFolder(this.Id));
            }
            else if (!string.IsNullOrEmpty(this.DisplayName))
            {
                var cr = new ManagementPackFolderCriteria(string.Format("DisplayName Like '%{0}%'", this.DisplayName));
                var list = _mg.Presentation.GetFolders(cr);
                foreach (ManagementPackFolder v in list)
                {
                    WriteObject(v);
                }
            }
            else
            {
                var list = _mg.Presentation.GetFolders();
                foreach (string p in Name)
                {
                    //WildcardPattern pattern = new WildcardPattern(p, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
                    Regex r = new Regex(p, RegexOptions.IgnoreCase);
                    foreach (ManagementPackFolder v in list)
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

#endregion
}
