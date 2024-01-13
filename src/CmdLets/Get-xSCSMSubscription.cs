using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Subscriptions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get,"xSCSMSubscription")]
    public class GetSMSubscriptionCommand : SMCmdletBase
    {
        private string _filter = "Name LIKE '%'";
        [Parameter(ValueFromPipeline=true)]
        public string Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }
        protected override void ProcessRecord()
        {
            ManagementPackRuleCriteria criteria = new ManagementPackRuleCriteria(Filter);
            foreach (WorkflowSubscriptionBase ws in _mg.Subscription.GetSubscriptionsByCriteria(criteria))
            {
                WriteObject(ws);
            }

        }
    }
}
