using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMStringResource", DefaultParameterSetName = "Name")]
    public class GetSCSMStringResourceCommand : PresentationCmdletBase
    {
        private List<ManagementPackStringResource> l;
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            l = new List<ManagementPackStringResource>();
            foreach (ManagementPack mp in _mg.ManagementPacks.GetManagementPacks())
            {
                foreach (ManagementPackStringResource r in mp.GetStringResources())
                {
                    l.Add(r);
                }
            }
        }
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            foreach (string n in Name)
            {
                //WildcardPattern wp = new WildcardPattern(n, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
                Regex r = new Regex(n, RegexOptions.IgnoreCase);
                foreach (ManagementPackStringResource v in l)
                {
                    if (r.Match(v.Name).Success)
                    {
                        WriteObject(r);
                    }
                }
            }
        }
    }
}
