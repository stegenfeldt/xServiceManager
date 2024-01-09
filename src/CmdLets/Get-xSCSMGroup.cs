using System.Management.Automation;

namespace xServiceManager.Module
{
    /// <summary>
    /// Represents a cmdlet to retrieve a Service Manager group.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "xSCSMGroup", DefaultParameterSetName = "DISPLAYNAME")]
    public class GetSCSMGroupCommand : GetGroupQueueCommand
    {
        /// <summary>
        /// Gets the name of the class needed for retrieving the group.
        /// </summary>
        public override string neededClassName
        {
            get { return "Microsoft.SystemCenter.ConfigItemGroup"; }
        }
    }
}