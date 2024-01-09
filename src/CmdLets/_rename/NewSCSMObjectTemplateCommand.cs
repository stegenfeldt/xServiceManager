using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.New, "xSCSMObjectTemplate", SupportsShouldProcess = true,DefaultParameterSetName="Projection")]
    public class NewSCSMObjectTemplateCommand : SMCmdletBase
    {
        private string _displayName;
        [Parameter(Position=0, Mandatory=true, ValueFromPipeline = true)]
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        private ManagementPack _managementPack;
        [Parameter(Position = 1, Mandatory = true)]
        public ManagementPack ManagementPack
        {
            get { return _managementPack; }
            set
            {
                if (value.Sealed)
                {
                    throw (new ArgumentException("ManagementPack must not be sealed"));
                }
                else
                {
                    _managementPack = value;
                }
            }
        }
        private ManagementPackClass _class;
        [Parameter(Position = 2, Mandatory = true, ParameterSetName = "Class")]
        public ManagementPackClass Class
        {
            get { return _class; }
            set { _class = value; }
        }
        private ManagementPackTypeProjection _projection;
        [Parameter(Position = 2, Mandatory = true, ParameterSetName = "Projection")]
        public ManagementPackTypeProjection Projection
        {
            get { return _projection; }
            set { _projection = value; }
        }
        /*
         * Commented out for now, just create a blank template which can then be modifed at a later date via the
         * UI or cmdlet
         * private Hashtable _templateData;
         * [Parameter(Position = 3)]
         * public Hashtable TemplateData
         * {
         *   get { return _templateData; }
         *   set { _templateData = value; }
         * }
         */
        private string _name;
        [Parameter]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private string _description;
        [Parameter]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        private SwitchParameter _passThru;
        [Parameter]
        public SwitchParameter PassThru
        {
            get { return _passThru; }
            set { _passThru = value; }
        }
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }
        protected override void ProcessRecord()
        {
            if (Name == null)
            {
                Name = String.Format("Template.{0:N}", Guid.NewGuid());
            }
            ManagementPackObjectTemplate template = new ManagementPackObjectTemplate(this.ManagementPack, Name);
            template.DisplayName = DisplayName;
            if (Description != null) { template.Description = Description; }
            if (Class != null)
            {
                template.TypeID = Class;
            }
            else
            {
                template.TypeID = Projection;
            }
            if (ShouldProcess(DisplayName))
            {
                this.ManagementPack.AcceptChanges();
            }
            WriteObject(template);
        }
    }
}
