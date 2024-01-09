using System.Management.Automation;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMQueue", DefaultParameterSetName = "DISPLAYNAME")]
    public class GetSCQueueCommand : GetGroupQueueCommand
    {
        public override string neededClassName
        {
            get { return "System.WorkItemGroup"; }
        }
    }
}