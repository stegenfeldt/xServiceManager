using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMForm", DefaultParameterSetName = "Name")]
    public class GetSCSMFormCommand : PresentationCmdletBase
    {
        ManagementPackElement _target;
        [Parameter(Position = 1)]
        public ManagementPackElement Target
        {
            get { return _target; }
            set { _target = value; }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }
        protected override void ProcessRecord()
        {
            string targetCriteria = this.BuildTargetCriteria();
            if (this.Id != Guid.Empty)
            {
                WriteObject(_mg.Presentation.GetForm(this.Id));
            }
            else if (!string.IsNullOrEmpty(this.DisplayName))
            {
                string criteria = string.Format("DisplayName Like '%{0}%'", this.DisplayName);
                if (!string.IsNullOrEmpty(targetCriteria))
                {
                    criteria += string.Format(" AND ({0})", targetCriteria);
                }
                var cr = new ManagementPackFormCriteria(criteria);
                var list = _mg.Presentation.GetForms(cr);
                foreach (ManagementPackForm v in list)
                {
                    WriteObject(v);
                }
            }
            else if (this.Name.Length > 0)
            {
                IList<ManagementPackForm> list = null;
                if (!string.IsNullOrEmpty(targetCriteria))
                    list = _mg.Presentation.GetForms(new ManagementPackFormCriteria(targetCriteria));
                else
                    list = _mg.Presentation.GetForms();

                foreach (string p in Name)
                {
                    //WildcardPattern pattern = new WildcardPattern(p, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
                    Regex r = new Regex(p, RegexOptions.IgnoreCase);
                    foreach (ManagementPackForm v in list)
                    {
                        if (r.Match(v.Name).Success)
                        {
                            WriteObject(v);
                        }
                    }
                }
            }
            else if (this.Target != null)
            {
                var cr = new ManagementPackFormCriteria(this.BuildTargetCriteria());
                var list = _mg.Presentation.GetForms(cr);
                foreach (ManagementPackForm v in list)
                {
                    WriteObject(v);
                }
            }
        }

        private string BuildTargetCriteria()
        {
            if (this.Target != null)
            {
                WriteVerbose(string.Format("Build criteria based on {0}", this.Target.Name));
                List<string> targets = new List<string>();
                targets.Add(string.Format("Target = '{0}'", this.Target.Id));
                if (this.Target is ManagementPackClass)
                {
                    // get all type projections for this class
                    var crTP = new ManagementPackTypeProjectionCriteria(string.Format("Type = '{0}'", this.Target.Id));
                    var allTPs = _mg.EntityTypes.GetTypeProjections(crTP);
                    foreach (var tp in allTPs)
                    {
                        targets.Add(string.Format("Target = '{0}'", tp.Id));
                    }
                }
                var criteria = string.Join(" OR ", targets.ToArray());
                WriteVerbose(string.Format("Target criteria: {0}", criteria));
                return criteria;
            }

            return "";
        }
    }
}
