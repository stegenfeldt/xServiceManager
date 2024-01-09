using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMTask", DefaultParameterSetName = "Guid")]
    public class GetSMTaskCommand : ObjectCmdletHelper
    {
        private Guid _id = Guid.Empty;
        [Parameter(ParameterSetName = "Guid", Position = 0)]
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private ManagementPackTaskCriteria _criteria = null;
        [Parameter(ParameterSetName = "Criteria", Position = 0)]
        public ManagementPackTaskCriteria Criteria
        {
            get { return _criteria; }
            set { _criteria = value; }
        }
        protected override void ProcessRecord()
        {
            if (Id == Guid.Empty && Criteria == null)
            {
                foreach (ManagementPackTask t in _mg.TaskConfiguration.GetTasks())
                {
                    WriteObject(t);
                }
            }
            else if (Id != Guid.Empty)
            {
                WriteObject(_mg.TaskConfiguration.GetTask(Id));
            }
            // If someone provides us a filter, we'll use that instead of a criteria
            else if (Criteria != null)
            {
                foreach (ManagementPackTask t in _mg.TaskConfiguration.GetTasks(Criteria))
                {
                    WriteObject(t);
                }
            }
        }
    }

}
