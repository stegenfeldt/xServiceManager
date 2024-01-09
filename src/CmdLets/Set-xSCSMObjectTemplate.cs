using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Set, "xSCSMObjectTemplate", SupportsShouldProcess = true)]
    public class SetSCSMObjectTemplateCommand : SMCmdletBase
    {
        private EnterpriseManagementObjectProjection[] _projection = null;
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "P", ValueFromPipeline = true)]
        public EnterpriseManagementObjectProjection[] Projection
        {
            get { return _projection; }
            set { _projection = value; }
        }

        private EnterpriseManagementObject[] _object = null;
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "O", ValueFromPipeline = true)]
        public EnterpriseManagementObject[] Object
        {
            get { return _object; }
            set { _object = value; }
        }

        private string _name = null;
        // [Parameter(Position=1,Mandatory=true)]
        // [Parameter(ParameterSetName="Projection")]
        // [Parameter(ParameterSetName="Object")]
        [Parameter]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private ManagementPackObjectTemplate _template = null;
        // [Parameter(Position=1,Mandatory=true)]
        // [Parameter(ParameterSetName="Projection")]
        // [Parameter(ParameterSetName="Object")]
        [Parameter]
        public ManagementPackObjectTemplate Template
        {
            get { return _template; }
            set { _template = value; }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (Template == null && Name != null)
            {
                Regex r = new Regex(Name, RegexOptions.IgnoreCase);
                foreach (ManagementPackObjectTemplate ot in _mg.Templates.GetObjectTemplates())
                {
                    if (r.Match(ot.Name).Success)
                    {
                        Template = ot;
                        return;
                    }
                }
            }
        }

        protected override void ProcessRecord()
        {
            try
            {
                if (Object != null)
                {
                    foreach (EnterpriseManagementObject o in Object)
                    {
                        if (ShouldProcess(o[null, "Id"].Value.ToString()))
                        {
                            o.ApplyTemplate(Template);
                            o.Overwrite();
                        }
                    }
                }
                else if (Projection != null)
                {
                    foreach (EnterpriseManagementObjectProjection p in Projection)
                    {
                        if (ShouldProcess(p.Object[null, "Id"].Value.ToString()))
                        {
                            p.ApplyTemplate(Template);
                            p.Overwrite();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "ApplyTemplate", ErrorCategory.InvalidOperation, Template));

            }
        }
    }
}
