using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Subscriptions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Remove,"xSCSMSubscription", SupportsShouldProcess=true)]
    public class RemoveSMSubscriptionCommand : SMCmdletBase
    {
        private WorkflowSubscription _subscription;
        [Parameter(Position=0,Mandatory=true,ValueFromPipeline=true)]
        public WorkflowSubscription Subscription
        {
            get { return _subscription; }
            set { _subscription = value; }
        }
        protected override void ProcessRecord()
        {
            try
            {
                if ( ShouldProcess(_subscription.Name))
                {
                    _subscription.ManagementGroup.Subscription.DeleteSubscription(_subscription);
                }
            }
            catch ( Exception e )
            {
                WriteError(new ErrorRecord(e, "Unknown error", ErrorCategory.NotSpecified, _subscription));
            }

        }
    }
}
