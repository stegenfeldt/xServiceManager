using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager
{
    [Cmdlet(VerbsCommon.Get, "xSCSMObject", DefaultParameterSetName = "Class")]
    public class GetSMObjectCommand : FilterCmdletBase
    {
        // Note: Four parameter sets so you can retrieve by class, guid or criteria

        private ManagementPackClass _class = null;
        [Parameter(ParameterSetName = "Class", Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [Parameter(ParameterSetName = "Statistic", Mandatory = true, ValueFromPipeline = true)]
        public ManagementPackClass Class
        {
            get { return _class; }
            set { _class = value; }
        }
        private Guid _id = Guid.Empty;
        [Parameter(ParameterSetName = "Guid", Position = 0, Mandatory = true)]
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private EnterpriseManagementObjectCriteria _criteria;
        [Parameter(ParameterSetName = "Criteria", Mandatory = true)]
        public EnterpriseManagementObjectCriteria Criteria
        {
            get { return _criteria; }
            set { _criteria = value; }
        }

        // If set, don't wrap the EMO
        private SwitchParameter _noAdapt;
        [Parameter]
        public SwitchParameter NoAdapt
        {
            get { return _noAdapt; }
            set { _noAdapt = value; }
        }

        // Only retrieve statistics
        private SwitchParameter _statistic;
        [Parameter(ParameterSetName = "Statistic")]
        public SwitchParameter Statistic
        {
            get { return _statistic; }
            set { _statistic = value; }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            if (Statistic)
            {
                QueryOption = new ObjectQueryOptions();
                QueryOption.DefaultPropertyRetrievalBehavior = ObjectPropertyRetrievalBehavior.None;
                QueryOption.ObjectRetrievalMode = ObjectRetrievalOptions.NonBuffered;
                return;
            }
            else
            {
                QueryOption = new ObjectQueryOptions();
                QueryOption.DefaultPropertyRetrievalBehavior = ObjectPropertyRetrievalBehavior.All;
                if (MaxCount != Int32.MaxValue)
                {
                    QueryOption.MaxResultCount = MaxCount;
                    QueryOption.ObjectRetrievalMode = ObjectRetrievalOptions.NonBuffered;
                }
            }
            string sortProperty = SortBy;
        }

        protected override void ProcessRecord()
        {
            if (Id != Guid.Empty)
            {
                WriteObject(ServiceManagerObjectHelper.AdaptManagementObject(this, _mg.EntityObjects.GetObject<EnterpriseManagementObject>(Id, ObjectQueryOptions.Default)));
                return;
            }
            // If someone provides us a filter, we'll use that instead of a criteria
            if (Filter != null)
            {
                Criteria = ConvertFilterToObjectCriteria(Class, Filter);
            }
            if (Class == null && Criteria != null)
            {
                Class = Criteria.ManagementPackClass;
            }
            try
            {
                addSortProperty(QueryOption, SortBy, Class);
            }
            catch
            {
                ;
            }
            int count = 0;
            if (Criteria == null)  // no criteria and no filter, get all the instances of the class
            {
                // If getting statistics, don't do anything
                if (Statistic)
                {
                    WriteObject(new ItemStatistics(Class, Class.Name, _mg.EntityObjects.GetObjectReader<EnterpriseManagementObject>(Class, QueryOption).Count));
                    return;
                }
                else
                {
                    foreach (EnterpriseManagementObject o in _mg.EntityObjects.GetObjectReader<EnterpriseManagementObject>(Class, QueryOption))
                    {
                        count++;
                        if (NoAdapt)
                        {
                            WriteObject(o);
                        }
                        else
                        {
                            WriteObject(ServiceManagerObjectHelper.AdaptManagementObject(this, o));
                        }
                        if (count >= MaxCount) { break; }
                    }
                }
            }
            else // OK, we got a criteria - we'll use that
            {
                if (Statistic)
                {
                    WriteObject(new ItemStatistics(Class, Class.Name, _mg.EntityObjects.GetObjectReader<EnterpriseManagementObject>(Criteria, QueryOption).Count));
                    return;
                }
                foreach (EnterpriseManagementObject o in _mg.EntityObjects.GetObjectReader<EnterpriseManagementObject>(Criteria, QueryOption))
                {
                    count++;
                    if (NoAdapt)
                    {
                        WriteObject(o);
                    }
                    else
                    {
                        WriteObject(ServiceManagerObjectHelper.AdaptManagementObject(this, o));
                    }
                    if (count >= MaxCount) { break; }
                }
            }
        }

    }
}