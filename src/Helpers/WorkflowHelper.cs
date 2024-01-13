using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Subscriptions;

namespace xServiceManager.Module
{
    public class WorkflowHelper
    {
        public static PSObject[] GetJobStatus(PSObject instance)
        {
            ManagementPackRule rule = (ManagementPackRule)instance.BaseObject;
            List<PSObject> statuslist = new List<PSObject>();
            foreach (SubscriptionJobStatus s in rule.ManagementGroup.Subscription.GetSubscriptionStatusById(rule.Id))
            {
                PSObject o = new PSObject(s);
                // sometimes the object id is empty, handle that gracefully
                try
                {
                    if (s.ObjectId == Guid.Empty)
                    {
                        o.Members.Add(new PSNoteProperty("Object", null));
                    }
                    else
                    {
                        o.Members.Add(new PSNoteProperty("Object", rule.ManagementGroup.EntityObjects.GetObject<EnterpriseManagementObject>(s.ObjectId, ObjectQueryOptions.Default)));
                    }
                }
                catch
                {
                    o.Members.Add(new PSNoteProperty("Object", null));
                }

                o.Members.Add(new PSNoteProperty("Rule", rule.ManagementGroup.Monitoring.GetRule(s.RuleId)));
                statuslist.Add(o);
            }
            PSObject[] jobstatus = new PSObject[statuslist.Count];
            statuslist.CopyTo(jobstatus);
            return jobstatus;
        }
    }
}
