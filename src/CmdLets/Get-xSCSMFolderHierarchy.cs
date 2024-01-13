using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMFolderHierarchy",DefaultParameterSetName="HIERARCHY")]
    public class GetSCSMFolderHierarchyCommand : ObjectCmdletHelper
    {
        private ManagementPackFolder[] _folder;
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ParameterSetName="HIERARCHY")]
        public ManagementPackFolder[] Folder
        {
            get { return _folder; }
            set { _folder = value; }
        }
        private SwitchParameter _root;
        [Parameter(Mandatory = true, ParameterSetName = "ROOT")]
        public SwitchParameter Root
        {
            get { return _root; }
            set { _root = value; }
        }
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            if (ParameterSetName == "ROOT")
            {
                foreach (ManagementPackFolder f in _mg.Presentation.GetFolders())
                {
                    if (f.ParentFolder == null)
                    {
                        WriteObject(_mg.Presentation.GetFolderHierarchy(f.Id));
                    }
                }
            }
            else
            {
                foreach (ManagementPackFolder f in Folder)
                {
                    WriteObject(_mg.Presentation.GetFolderHierarchy(f.Id));
                }
            }
        }
    }
}
