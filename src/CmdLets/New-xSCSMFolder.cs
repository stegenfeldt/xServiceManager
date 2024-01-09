using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.New, "xSCSMFolder")]
    public class NewSMFolderCommand : ObjectCmdletHelper
    {
        private string _displayname;
        ManagementPackFolder _parentfolder;
        ManagementPack _managementpack;
        //TODO: Add support for this someday
        //ManagementPackImage _image;

        [Parameter(ValueFromPipeline = false, Mandatory = true)]
        public string DisplayName
        {
            get { return _displayname; }
            set { _displayname = value; }
        }

        [Parameter(ValueFromPipeline = false, Mandatory = true)]
        public ManagementPackFolder ParentFolder
        {
            get { return _parentfolder; }
            set { _parentfolder = value; }
        }

        [Parameter(ValueFromPipeline = false, Mandatory = true)]
        public ManagementPack ManagementPack
        {
            get { return _managementpack; }
            set { _managementpack = value; }
        }

        //TODO: Add support for this at some point
        /*
        [Parameter(ValueFromPipeline = false, Mandatory = false)]
        public ManagementPackImage Image
        {
            get { return _image; }
            set { _image = value; }
        }
        */

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            //Create a new folder and set it's parent folder and display name
            ManagementPackFolder folder = new ManagementPackFolder(_managementpack, SMHelpers.MakeMPElementSafeUniqueIdentifier("Folder"), ManagementPackAccessibility.Public);
            folder.DisplayName = _displayname;
            folder.ParentFolder = _parentfolder;

            //TODO: Parameterize this someday
            //Set the systemfolder icon to be the icon that is used
            ManagementPackElementReference<ManagementPackImage> foldericonreference = (ManagementPackElementReference<ManagementPackImage>)_mg.Resources.GetResource<ManagementPackImage>(Images.Microsoft_EnterpriseManagement_ServiceManager_UI_Console_Image_Folder, SMHelpers.GetManagementPack(ManagementPacks.Microsoft_EnterpriseManagement_ServiceManager_UI_Console, _mg));
            ManagementPackImageReference image = new ManagementPackImageReference(folder, foldericonreference, _managementpack);

            //Submit changes
            _managementpack.AcceptChanges();
        }
    }
}
