using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMAnnouncement", SupportsShouldProcess = true)]
    public class GetAnnouncement : SMCmdletBase
    {

        protected override void ProcessRecord()
        {
            try
            {
                ManagementPackClass clsAnnouncement = SMHelpers.GetManagementPackClass(ClassTypes.System_Announcement_Item, SMHelpers.GetManagementPack(ManagementPacks.System_AdminItem_Library, _mg), _mg);
                foreach(EnterpriseManagementObject emo in _mg.EntityObjects.GetObjectReader<EnterpriseManagementObject>(clsAnnouncement,ObjectQueryOptions.Default))
                {
                    WriteObject(ServiceManagerObjectHelper.AdaptManagementObject(this, emo));
                }
            }
            catch (Exception)
            {
            }
        }
    }
}