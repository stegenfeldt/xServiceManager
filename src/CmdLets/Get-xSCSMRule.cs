using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{

    [Cmdlet(VerbsCommon.Get, "xSCSMRule", DefaultParameterSetName = "name")]
    public class GetSMRuleCommand : ObjectCmdletHelper
    {
        private string _name = ".*";
        [Parameter(Position = 0, ParameterSetName = "name")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private Guid _id = Guid.Empty;
        [Parameter(Position = 0, ParameterSetName = "id")]
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private Regex r = null;
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (Id == Guid.Empty)
            {
                r = new Regex(Name, RegexOptions.IgnoreCase);
            }
        }
        protected override void ProcessRecord()
        {
            if (Id != Guid.Empty)
            {
                ManagementPackRule rule = _mg.Monitoring.GetRule(Id);
                PSObject o = new PSObject(rule);
                o.Members.Add(new PSNoteProperty("ManagementPack", rule.GetManagementPack()));
                WriteObject(o);
            }
            else
            {
                foreach (ManagementPackRule rule in _mg.Monitoring.GetRules())
                {
                    if (r.Match(rule.Name).Success)
                    {
                        PSObject o = new PSObject(rule);
                        o.Members.Add(new PSNoteProperty("ManagementPack", rule.GetManagementPack()));
                        WriteObject(o);
                    }
                }
            }
        }

    }

}
