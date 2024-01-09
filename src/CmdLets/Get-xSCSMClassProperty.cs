using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMClassProperty")]
    public class GetSMClassPropertyCommand : SMCmdletBase
    {
        // Parameters
        private ManagementPackClass _class = null;
        [Parameter(
            ParameterSetName = "Class", 
            Position = 0, 
            Mandatory = true, 
            ValueFromPipeline = true
            )]
        public ManagementPackClass Class
        {
            get { return _class; }
            set { _class = value; }
        }


        private SwitchParameter _recursive;
        [Parameter]
        public SwitchParameter Recursive
        {
            get { return _recursive; }
            set { _recursive = value; }
        }

        private SwitchParameter _includeExtensions;
        [Parameter]
        public SwitchParameter IncludeExtensions
        {
            get { return _includeExtensions; }
            set { _includeExtensions = value; }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            WriteVerbose("Process class " + this.Class.Name);
            var recursion = this.Recursive.ToBool() ? BaseClassTraversalDepth.Recursive : BaseClassTraversalDepth.None;
            var extensionMode = this.IncludeExtensions.ToBool() ? PropertyExtensionMode.All : PropertyExtensionMode.None;
            this.WriteVerbose($"Recursion: {recursion}. extensionMode: {extensionMode}");
            var retCollection = this.Class.GetProperties(recursion, extensionMode);
            this.WriteObject(retCollection, true);
        }

    }
}