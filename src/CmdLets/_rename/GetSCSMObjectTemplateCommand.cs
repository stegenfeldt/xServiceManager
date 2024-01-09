using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMObjectTemplate", DefaultParameterSetName = "Name")]
    public class GetSCSMObjectTemplateCommand : EntityTypeHelper
    {
        private Guid[] _id = null;
        [Parameter(ParameterSetName = "Id")]
        public Guid[] Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private string[] _displayName = null;
        [Parameter(ParameterSetName = "DisplayName")]
        public string[] DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        protected override void ProcessRecord()
        {
            if (ParameterSetName == "Id")
            {
                foreach (Guid i in Id)
                {
                    try { WriteObject(_mg.Templates.GetObjectTemplate(i)); }
                    catch (ObjectNotFoundException e) { WriteError(new ErrorRecord(e, "ObjectTemplate not found", ErrorCategory.ObjectNotFound, Id)); }
                    catch (Exception e) { WriteError(new ErrorRecord(e, "Unknown error", ErrorCategory.NotSpecified, i)); }
                }
            }

            else if (ParameterSetName == "DisplayName")
            {
                foreach (string n in DisplayName)
                {
                    Regex r = new Regex(n, RegexOptions.IgnoreCase);
                    foreach (ManagementPackObjectTemplate o in _mg.Templates.GetObjectTemplates())
                    {
                        if (r.Match(o.DisplayName).Success)
                        {
                            WriteObject(o);
                        }
                    }
                }
            }
            else
            {
                if (Name == null)
                {
                    foreach (ManagementPackObjectTemplate ot in _mg.Templates.GetObjectTemplates())
                    {
                        WriteObject(ot);
                    }
                }
                else
                {
                    Regex r = new Regex(Name, RegexOptions.IgnoreCase);
                    foreach (ManagementPackObjectTemplate ot in _mg.Templates.GetObjectTemplates())
                    {
                        if (r.Match(ot.Name).Success) { WriteObject(ot); }
                    }
                }
            }

        }
    }
}
