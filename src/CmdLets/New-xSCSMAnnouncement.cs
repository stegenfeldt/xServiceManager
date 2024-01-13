using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    //TODO: Separate into files
    [Cmdlet(VerbsCommon.New, "xSCSMAnnouncement", SupportsShouldProcess = true)]
    public class AddAnnouncement : SMCmdletBase
    {
       
        private String _DisplayName = null;
        private String _Body = null;
        private String _Priority = null;
        private DateTime _ExpirationDate;
        
        [Parameter(Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The title of the annoucncement.")]
        [ValidateNotNullOrEmpty]
        public string DisplayName
        {
            get{return _DisplayName;}
            set{_DisplayName = value;}
        }
        
        [Parameter(Position = 1,
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The body of the announcement")]
        [ValidateNotNullOrEmpty]
        public string Body
        {
            get{return _Body;}
            set{_Body = value;}
        }

        [Parameter(Position = 2,
        Mandatory = false,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The priority of the announcement.  Must be exactly 'Low', 'Medium', or 'Critical'.")]
        [ValidateNotNullOrEmpty]
        [ValidateSet("Low","Medium","Critical")]
        public string Priority
        {
            get { return _Priority; }
            set { _Priority = value; }
        }

        [Parameter(Position = 3,
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The expiration date of the announcement.  Pass a datetime object.  Convert to UTC time first.  Required.")]
        [ValidateNotNullOrEmpty]
        public DateTime ExpirationDate
        {
            get { return _ExpirationDate; }
            set { _ExpirationDate = value; }
        }

        private SwitchParameter _passThru;
        [Parameter]
        public SwitchParameter PassThru
        {
            get { return _passThru; }
            set { _passThru = value; }
        }



        protected override void ProcessRecord()
        {
            try
            {
                ManagementPackClass clsAnnouncement = SMHelpers.GetManagementPackClass(ClassTypes.System_Announcement_Item, SMHelpers.GetManagementPack(ManagementPacks.System_AdminItem_Library, _mg), _mg);
                ManagementPackEnumeration enumPriority = null;
                switch (_Priority)
                {
                    case "Low":
                        enumPriority = SMHelpers.GetEnum(Enumerations.System_Announcement_PriorityEnum_Low, _mg);
                        break;
                    case "Critical":
                        enumPriority = SMHelpers.GetEnum(Enumerations.System_Announcement_PriorityEnum_Critical, _mg);
                        break;
                    case "Medium":
                        enumPriority = SMHelpers.GetEnum(Enumerations.System_Announcement_PriorityEnum_Medium, _mg);
                        break;
                    default:
                        enumPriority = SMHelpers.GetEnum(Enumerations.System_Announcement_PriorityEnum_Medium, _mg);
                        break;
                }

                CreatableEnterpriseManagementObject emo = new CreatableEnterpriseManagementObject(_mg, clsAnnouncement);

                emo[clsAnnouncement, "Id"].Value = System.Guid.NewGuid().ToString();
                if(_DisplayName != null)
                    emo[clsAnnouncement, "DisplayName"].Value = _DisplayName;
                    emo[clsAnnouncement, "Title"].Value = _DisplayName;
                
                if(_Body != null)
                    emo[clsAnnouncement, "Body"].Value = _Body;
                emo[clsAnnouncement, "Priority"].Value = enumPriority.Id;
                emo[clsAnnouncement, "ExpirationDate"].Value = _ExpirationDate;

                emo.Commit();
                if ( _passThru )
                {
                    WriteObject(ServiceManagerObjectHelper.AdaptManagementObject(this, _mg.EntityObjects.GetObject<EnterpriseManagementObject>(emo.Id, ObjectQueryOptions.Default)));
                }

            }
            catch (Exception)
            {
            }
        }

    }
}