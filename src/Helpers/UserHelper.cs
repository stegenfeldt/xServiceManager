using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    public class UserHelper
    {
        public static object GetUserObjectFromString(EnterpriseManagementGroup EMG, string userName, Cmdlet currentCmdlet)
        {
            try
            {
                ManagementPackClass userClass = EMG.EntityTypes.GetClass("System.Domain.User", EMG.ManagementPacks.GetManagementPack(SystemManagementPack.System));
                string name = userName.Split('\\')[1];
                string domain = userName.Split('\\')[0];
                EnterpriseManagementObjectCriteria c = new EnterpriseManagementObjectCriteria(String.Format("UserName = '{0}' and Domain = '{1}'", name, domain), userClass);
                IObjectReader<EnterpriseManagementObject> reader = EMG.EntityObjects.GetObjectReader<EnterpriseManagementObject>(c, ObjectQueryOptions.Default);
                if (reader.Count == 1)
                {
                    return ServiceManagerObjectHelper.AdaptManagementObject(reader.GetData(0));
                }
                else
                {
                    return userName;
                }
            }
            catch (Exception e)
            {
                currentCmdlet.WriteError(new ErrorRecord(e, "GetUserObjectFromString", ErrorCategory.NotSpecified, userName));
                return userName;
            }
        }
    }
}
