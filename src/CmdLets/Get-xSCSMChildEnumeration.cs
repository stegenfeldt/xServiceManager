using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMChildEnumeration")]
    public class GetSMChildEnumerationCommand : EntityTypeHelper
    {
        private TraversalDepth _depth = TraversalDepth.Recursive;
        [Parameter]
        public TraversalDepth Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }

        private ManagementPackEnumeration _enumeration;
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public ManagementPackEnumeration Enumeration
        {
            get { return _enumeration; }
            set { _enumeration = value; }
        }

        protected override void ProcessRecord()
        {
            foreach (ManagementPackEnumeration o in _mg.EntityTypes.GetChildEnumerations(Enumeration.Id, Depth))
            {
                WriteObject(o);
            }
        }
    }

}