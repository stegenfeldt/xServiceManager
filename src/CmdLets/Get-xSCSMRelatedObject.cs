using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMRelatedObject", DefaultParameterSetName = "Wrapped")]
    public class GetSMRelatedObjectCommand : ObjectCmdletHelper
    {
        private EnterpriseManagementObject _smobject;
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Wrapped")]
        public EnterpriseManagementObject SMObject
        {
            get { return _smobject; }
            set { _smobject = value; }
        }

        private ManagementPackRelationship _relationship;
        [Parameter]
        public ManagementPackRelationship Relationship
        {
            get { return _relationship; }
            set { _relationship = value; }
        }

        private TraversalDepth _depth = TraversalDepth.OneLevel;
        [Parameter]
        public TraversalDepth Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }

        protected override void ProcessRecord()
        {
            if (Relationship != null)
            {
                foreach (EnterpriseManagementObject o in
                _mg.EntityObjects.GetRelatedObjects<EnterpriseManagementObject>(SMObject.Id, Relationship, Depth, QueryOption))
                {
                    WriteObject(ServiceManagerObjectHelper.AdaptManagementObject(this, o));
                }
            }
            else
            {
                foreach (EnterpriseManagementObject o in
                    _mg.EntityObjects.GetRelatedObjects<EnterpriseManagementObject>(SMObject.Id, Depth, QueryOption))
                {
                    WriteObject(ServiceManagerObjectHelper.AdaptManagementObject(this, o));
                }
            }
        }

    }
}