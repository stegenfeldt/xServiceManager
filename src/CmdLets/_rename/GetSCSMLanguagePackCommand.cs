using System;
using System.Globalization;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMLanguagePackCulture")]
    public class GetSCSMLanguagePackCommand : SMCmdletBase
    {
        List<string> cultures;
        CultureInfo[] systemCultures;
        IList<ManagementPackLanguagePack> lpList;
        protected override void BeginProcessing()
        {
            WriteVerbose("BeginProcessing Begin");
            base.BeginProcessing();
            cultures = new List<string>();
            systemCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            lpList = _mg.LanguagePacks.GetLanguagePacks();
            WriteVerbose("BeginProcessing End");
        }
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            WriteVerbose("ProcessRecord");
            foreach (ManagementPackLanguagePack lp in lpList)
            {
                foreach (CultureInfo ci in systemCultures)
                {
                    if (String.Compare(ci.ThreeLetterWindowsLanguageName, lp.Name, true) == 0 && !cultures.Contains(lp.Name))
                    {
                        WriteObject(ci);
                        cultures.Add(lp.Name);
                        break;
                    }
                }
            }
        }
    }
}
