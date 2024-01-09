using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMConsoleTask", DefaultParameterSetName = "Name")]
    public class GetSCSMConsoleTaskCommand : PresentationCmdletBase
    {
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            if (this.Id != Guid.Empty)
            {
                WriteObject(_mg.TaskConfiguration.GetConsoleTask(this.Id));
            }
            else if (!string.IsNullOrEmpty(this.DisplayName))
            {
                var cr = new ManagementPackConsoleTaskCriteria(string.Format("DisplayName Like '%{0}%'", this.DisplayName));
                var list = _mg.TaskConfiguration.GetConsoleTasks(cr);
                foreach (ManagementPackConsoleTask v in list)
                {
                    WriteObject(v);
                }
            }
            else
            {
                var list = _mg.TaskConfiguration.GetConsoleTasks();
                foreach (string p in Name)
                {
                    //WildcardPattern pattern = new WildcardPattern(p, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
                    Regex r = new Regex(p, RegexOptions.IgnoreCase);
                    foreach (ManagementPackConsoleTask v in list)
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
