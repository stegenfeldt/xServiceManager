using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMObjectProjection", DefaultParameterSetName = "Wrapped")]
    public class GetSMObjectProjectionCommand : FilterCmdletBase
    {

        private PSObject _projectionObject;
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Wrapped")]
        [Parameter(ParameterSetName = "Statistics", ValueFromPipeline = true, Mandatory = true)]
        public PSObject ProjectionObject
        {
            get { return _projectionObject; }
            set { _projectionObject = value; }
        }

        private ManagementPackTypeProjection _projection;
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Raw", ValueFromPipeline = true)]
        public ManagementPackTypeProjection Projection
        {
            get { return _projection; }
            set { _projection = value; }
        }

        private string _projectionName = null;
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Name")]
        public string ProjectionName
        {
            get { return _projectionName; }
            set { _projectionName = value; }
        }

        private ObjectProjectionCriteria _criteria = null;
        [Parameter(ParameterSetName = "Criteria", Mandatory = true)]
        public ObjectProjectionCriteria Criteria
        {
            get { return _criteria; }
            set { _criteria = value; }
        }

        private SwitchParameter _noSort;
        [Parameter]
        public SwitchParameter NoSort
        {
            get { return _noSort; }
            set { _noSort = value; }
        }

        private SwitchParameter _statistic;
        [Parameter(ParameterSetName = "Statistics", Mandatory = true)]
        public SwitchParameter Statistic
        {
            get { return _statistic; }
            set { _statistic = value; }
        }


        private SwitchParameter _adoptWithTargetEndpoint;
        [Parameter(ParameterSetName = "Raw", Mandatory = false)]
        [Parameter(ParameterSetName = "Name", Mandatory = false)]
        [Parameter(ParameterSetName = "Criteria", Mandatory = false)]
        public SwitchParameter AdoptWithTargetEndpoint
        {
            get { return _adoptWithTargetEndpoint; }
            set { _adoptWithTargetEndpoint = value; }
        }
        // This is used for instream projection
        // creation. needed for those projections whose
        // alias targets are themselves a projection
        // (such as ResolutionAndBillableLog 
        // alias BillableLogs while requires a billabletime 
        // and workeduponbyuser
        private SwitchParameter _noCommit;
        [Parameter]
        public SwitchParameter NoCommit
        {
            get { return _noCommit; }
            set { _noCommit = value; }
        }

        protected override void BeginProcessing()
        {

            base.BeginProcessing();

            if (Statistic)
            {
                WriteDebug("Getting Statistics");
                QueryOption = new ObjectQueryOptions();
                QueryOption.DefaultPropertyRetrievalBehavior = ObjectPropertyRetrievalBehavior.None;
                QueryOption.ObjectRetrievalMode = ObjectRetrievalOptions.Buffered;
                return;
            }

            string sortProperty = SortBy;
            QueryOption = new ObjectQueryOptions();
            QueryOption.DefaultPropertyRetrievalBehavior = ObjectPropertyRetrievalBehavior.All;
            QueryOption.ObjectRetrievalMode = ObjectRetrievalOptions.NonBuffered;
            if (MaxCount != Int32.MaxValue)
            {
                QueryOption.MaxResultCount = MaxCount;
                QueryOption.ObjectRetrievalMode = ObjectRetrievalOptions.NonBuffered;
            }

            if (ProjectionName != null)
            {
                foreach (ManagementPackTypeProjection p in _mg.EntityTypes.GetTypeProjections())
                {
                    if (String.Compare(p.Name, ProjectionName, StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        Projection = p;
                        break;
                    }
                }
                if (Projection == null)
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentNullException("No Projection found"), "Need Projection", ErrorCategory.InvalidOperation, "projection"));
                }
            }

            // Only build the sortCriteria if the Projection is not null
            // Current architecture means that we can't sort if we pipe a TypeProjection
            // TODO: AddSortProperty to QueryOptions for each projection seen on the pipeline
            if (Projection != null && !NoSort)
            {
                WriteVerbose("Sort property is: " + sortProperty);
                // sort the results
                // string sortCriteria = String.Format("<Sorting {0}><GenericSortProperty SortOrder=\"{1}\">{2}</GenericSortProperty></Sorting>", xmlns, Order, pName);
                string sortCriteria = null;
                try
                {
                    sortCriteria = makeSortCriteriaString(sortProperty, Projection.TargetType);
                    WriteDebug("sorting criteria : " + sortCriteria);
                    QueryOption.AddSortProperty(sortCriteria, Projection, _mg);
                }
                catch (Exception e) // It's not a failure
                {
                    WriteError(new ErrorRecord(e, "Sort Failure", ErrorCategory.InvalidArgument, sortCriteria));
                }
            }
            else
            {
                WriteDebug("Not Sorting");
            }
        }

        private int count = 0;
        protected override void ProcessRecord()
        {

            ObjectProjectionCriteria myCriteria = null;
            // If we got a wrapped object, unwrap it
            if (ProjectionObject != null)
            {
                WriteDebug("unwrapping PSObject to get projection");
                Projection = (ManagementPackTypeProjection)ProjectionObject.Properties["__base"].Value;
            }
            if (Statistic)
            {
                // Should this just be a call to get the seed?
                WriteVerbose("Getting Statistics");
                WriteDebug("Before Criteria: " + DateTime.Now.ToString());
                ObjectProjectionCriteria StatisticCriteria = new ObjectProjectionCriteria(Projection);
                WriteDebug("Before Reader: " + DateTime.Now.ToString());
                IObjectProjectionReader<EnterpriseManagementObject> reader = _mg.EntityObjects.GetObjectProjectionReader<EnterpriseManagementObject>(StatisticCriteria, QueryOption);
                WriteDebug("After Reader: " + DateTime.Now.ToString());
                WriteObject(new ItemStatistics(Projection, Projection.Name, reader.Count));
                WriteDebug("After Reader.Count: " + DateTime.Now.ToString());
                return;
            }

            // Create the criteria
            // first, by checking whether there's a filter (and no criteria)
            // This has to be created for each object in the pipeline because we may have gotten a 
            // heterogenous collection of projections
            if (Criteria != null)
            {
                myCriteria = Criteria;
            }
            if (Filter != null && Criteria == null)
            {
                WriteDebug("converting filter to criteria");
                myCriteria = ConvertFilterToProjectionCriteria(Projection, Filter);
            }
            // ok - neither criteria, nor filter was provided, build a criteria from the projection
            if (myCriteria == null)
            {
                WriteDebug("null criteria");
                myCriteria = new ObjectProjectionCriteria(Projection);
            }
            QueryOption.ObjectRetrievalMode = ObjectRetrievalOptions.Buffered;
            // QueryOption.DefaultPropertyRetrievalBehavior = ObjectPropertyRetrievalBehavior.None;
            WriteDebug("Retrieval Mode: " + QueryOption.ObjectRetrievalMode.ToString());
            WriteDebug("Before projectionReader: " + DateTime.Now.ToString());

            IObjectProjectionReader<EnterpriseManagementObject> projectionReader =
                _mg.EntityObjects.GetObjectProjectionReader<EnterpriseManagementObject>(myCriteria, QueryOption);
            WriteDebug("After projectionReader: " + DateTime.Now.ToString() + " Count is :" + projectionReader.Count);
            // Set the page size to a small number to decrease initial time to results
            projectionReader.PageSize = 1;
            WriteDebug("MaxCount = " + projectionReader.MaxCount);
            WriteDebug("Enter foreach: " + DateTime.Now.ToString());
            // while(projectionReader.
            // EnterpriseManagementObjectProjection p = projectionReader.First<EnterpriseManagementObjectProjection>();
            // for(int i=0;i < 1; i++) 
            foreach (EnterpriseManagementObjectProjection p in projectionReader)
            {
                count++;
                WriteDebug("Current count: " + count + " at " + DateTime.Now.ToString());
                if (count > MaxCount) { break; }
                WriteVerbose("Adapting " + p);
                /*
                 * We can't just wrap a type projection because it is Enumerable. This means that we would only see the
                 * components of the projection in the output so we have to construct this artificial wrapper. It would be easier if
                 * projections weren't Enumerable, which means that PowerShell wouldn't treat a projection as a collection, or if 
                 * PowerShell understood that certain collections shouldn't be unspooled, but that's not how PowerShell works.
                 * Neither of those two options are available, so we adapt the object and present a PSObject with all the component
                 * parts.
                 */
                PSObject o = new PSObject();
                o.Members.Add(new PSNoteProperty("__base", p));
                o.Members.Add(new PSScriptMethod("GetAsXml", ScriptBlock.Create("[xml]($this.__base.CreateNavigator().OuterXml)")));
                o.Members.Add(new PSNoteProperty("Object", ServiceManagerObjectHelper.AdaptManagementObject(this, p.Object)));
                // Now promote all the properties on Object
                foreach (EnterpriseManagementSimpleObject so in p.Object.Values)
                {
                    try
                    {
                        o.Members.Add(new PSNoteProperty(so.Type.Name, so.Value));
                    }
                    catch
                    {
                        WriteWarning("could not promote: " + so.Type.Name);
                    }
                }

                o.TypeNames[0] = String.Format(CultureInfo.CurrentCulture, "EnterpriseManagementObjectProjection#{0}", myCriteria.Projection.Name);
                o.TypeNames.Insert(1, "EnterpriseManagementObjectProjection");
                o.Members.Add(new PSNoteProperty("__ProjectionType", myCriteria.Projection.Name));

                if (this.AdoptWithTargetEndpoint)
                    AdoptProjectionComponent(o, p);
                else
                    AdoptProjectionComponent(o, p, myCriteria.Projection);

                WriteObject(o);
            }
        }
        private void AdoptProjectionComponent(PSObject parentObject, IComposableProjection projComp, ITypeProjectionComponent tpComp)
        {
            foreach (var tpSubComp in tpComp)
            {
                WriteVerbose("Adapting related objects by alias: " + tpSubComp.Value.Alias);
                foreach (var subObjProj in projComp[tpSubComp.Key])
                {
                    string myName = tpSubComp.Value.Alias;
                    PSObject adaptedEMO = ServiceManagerObjectHelper.AdaptManagementObject(this, subObjProj.Object);
                    // If the MaxCardinality is greater than one, it's definitely a collection
                    // so start out that way
                    if (tpSubComp.Key.MaxCardinality > 1)
                    {
                        // OK, this is a collection, so add the critter
                        // This is so much easier in PowerShell
                        if (parentObject.Properties[myName] == null)
                        {
                            parentObject.Members.Add(new PSNoteProperty(myName, new ArrayList()));
                        }
                        ((ArrayList)parentObject.Properties[myName].Value).Add(adaptedEMO);
                    }
                    else
                    {
                        try
                        {
                            parentObject.Members.Add(new PSNoteProperty(myName, adaptedEMO));
                        }
                        catch (ExtendedTypeSystemException e)
                        {
                            WriteVerbose("Readapting relationship object -> collection :" + e.Message);
                            // We should really only get this exception if we
                            // try to add a create a new property which already exists
                            Object currentPropertyValue = parentObject.Properties[myName].Value;
                            ArrayList newValue = new ArrayList();
                            newValue.Add(currentPropertyValue);
                            newValue.Add(adaptedEMO);
                            parentObject.Properties[myName].Value = newValue;
                            // TODO
                            // If this already exists, it should be converted to a collection
                        }
                    }

                    AdoptProjectionComponent(adaptedEMO, subObjProj, tpSubComp.Value);
                }

            }
        }

        private void AdoptProjectionComponent(PSObject parentObject, IComposableProjection projComp)
        {
            foreach (KeyValuePair<ManagementPackRelationshipEndpoint, IComposableProjection> helper in projComp)
            {
                // EnterpriseManagementObject myEMO = (EnterpriseManagementObject)helper.Value.Object;
                WriteVerbose("Adapting related objects: " + helper.Key.Name);
                String myName = helper.Key.Name;
                PSObject adaptedEMO = ServiceManagerObjectHelper.AdaptManagementObject(this, helper.Value.Object);
                // If the MaxCardinality is greater than one, it's definitely a collection
                // so start out that way
                if (helper.Key.MaxCardinality > 1)
                {
                    // OK, this is a collection, so add the critter
                    // This is so much easier in PowerShell
                    if (parentObject.Properties[myName] == null)
                    {
                        parentObject.Members.Add(new PSNoteProperty(myName, new ArrayList()));
                    }
                    ((ArrayList)parentObject.Properties[myName].Value).Add(adaptedEMO);
                }
                else
                {
                    try
                    {
                        parentObject.Members.Add(new PSNoteProperty(helper.Key.Name, adaptedEMO));
                    }
                    catch (ExtendedTypeSystemException e)
                    {
                        WriteVerbose("Readapting relationship object -> collection :" + e.Message);
                        // We should really only get this exception if we
                        // try to add a create a new property which already exists
                        Object currentPropertyValue = parentObject.Properties[myName].Value;
                        ArrayList newValue = new ArrayList();
                        newValue.Add(currentPropertyValue);
                        newValue.Add(adaptedEMO);
                        parentObject.Properties[myName].Value = newValue;
                        // TODO: If this already exists, it should be converted to a collection
                    }


                }

                AdoptProjectionComponent(adaptedEMO, helper.Value);

            }
        }
    }
}