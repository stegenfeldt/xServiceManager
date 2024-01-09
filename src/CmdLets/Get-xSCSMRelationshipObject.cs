using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMRelationshipObject", DefaultParameterSetName = "ID")]
    public class GetSCSMRelationshipObjectCommand : ObjectCmdletHelper
    {
        private Guid _id = Guid.Empty;
        [Parameter(Position = 0, ParameterSetName = "ID", Mandatory = true)]
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private ManagementPackRelationship[] _relationship = null;
        [Parameter(ParameterSetName = "RELATIONSHIP", Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public ManagementPackRelationship[] Relationship
        {
            get { return _relationship; }
            set { _relationship = value; }
        }
        private ManagementPackRelationship _trelationship;
        [Parameter(ParameterSetName = "TARGETANDRELATIONSHIP", Mandatory = true)]
        public ManagementPackRelationship TargetRelationship
        {
            get { return _trelationship; }
            set { _trelationship = value; }
        }

        private ManagementPackClass _target = null;
        [Parameter(ParameterSetName = "TARGET", Mandatory = true)]
        public ManagementPackClass Target
        {
            get { return _target; }
            set { _target = value; }
        }

        private ManagementPackClass _source = null;
        [Parameter(ParameterSetName = "SOURCE", Mandatory = true)]
        public ManagementPackClass Source
        {
            get { return _source; }
            set { _source = value; }
        }

        private EnterpriseManagementObject _byTarget;
        [Parameter(ParameterSetName = "TARGETOBJECT", Mandatory = true)]
        [Parameter(ParameterSetName = "RELATIONSHIP", Mandatory = false)]
        public EnterpriseManagementObject ByTarget
        {
            get { return _byTarget; }
            set { _byTarget = value; }
        }

        [Parameter(ParameterSetName = "TARGETANDRELATIONSHIP", Mandatory = true)]
        public EnterpriseManagementObject TargetObject
        {
            get { return _byTarget; }
            set { _byTarget = value; }
        }

        private EnterpriseManagementObject _bySource;
        [Parameter(ParameterSetName = "SOURCEOBJECT", Mandatory = true)]
        [Parameter(ParameterSetName = "RELATIONSHIP", Mandatory = false)]
        public EnterpriseManagementObject BySource
        {
            get { return _bySource; }
            set { _bySource = value; }
        }

        private string _filter = null;
        [Parameter(ParameterSetName = "SOURCEOBJECT")]
        [Parameter(ParameterSetName = "TARGET")]
        [Parameter(ParameterSetName = "SOURCE")]
        [Parameter(ParameterSetName = "RELATIONSHIP")]
        [Parameter(ParameterSetName = "FILTER", Mandatory = true)]
        public string Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }


        private bool _recursive = true;
        [Parameter(ParameterSetName = "SOURCEOBJECT")]
        [Parameter(ParameterSetName = "TARGET")]
        [Parameter(ParameterSetName = "SOURCE")]
        [Parameter(ParameterSetName = "RELATIONSHIP")]
        [Parameter(ParameterSetName = "FILTER")]
        public bool Recursive
        {
            get { return _recursive; }
            set { _recursive = value; }
        }
        private enum QueryBy { TargetClass, SourceClass, Target, Source, Relationship, TargetAndRelationship };

        #region GetRelationshipsHelper
        private IList<EnterpriseManagementRelationshipObject<EnterpriseManagementObject>> GetRelationshipObjects(ManagementPackRelationship r) { return GetRelationshipObjects(null, null, r, QueryBy.Relationship, null); }
        private IList<EnterpriseManagementRelationshipObject<EnterpriseManagementObject>> GetRelationshipObjects(ManagementPackRelationship r, string filter) { return GetRelationshipObjects(null, null, r, QueryBy.Relationship, filter); }
        private IList<EnterpriseManagementRelationshipObject<EnterpriseManagementObject>> GetRelationshipObjects(string filter) { return GetRelationshipObjects(null, null, null, QueryBy.Relationship, filter); }
        private IList<EnterpriseManagementRelationshipObject<EnterpriseManagementObject>> GetRelationshipObjects(ManagementPackClass c, QueryBy q) { return GetRelationshipObjects(c, null, null, q, null); }
        private IList<EnterpriseManagementRelationshipObject<EnterpriseManagementObject>> GetRelationshipObjects(ManagementPackClass c, QueryBy q, string filter) { return GetRelationshipObjects(c, null, null, q, filter); }
        private IList<EnterpriseManagementRelationshipObject<EnterpriseManagementObject>> GetRelationshipObjects(EnterpriseManagementObject emo, QueryBy q) { return GetRelationshipObjects(null, emo, null, q, null); }
        private IList<EnterpriseManagementRelationshipObject<EnterpriseManagementObject>> GetRelationshipObjects(EnterpriseManagementObject emo, QueryBy q, string filter) { return GetRelationshipObjects(null, emo, null, q, filter); }
        private IList<EnterpriseManagementRelationshipObject<EnterpriseManagementObject>> GetRelationshipObjects(EnterpriseManagementObject emo, ManagementPackRelationship r) { return GetRelationshipObjects(null, emo, r, QueryBy.TargetAndRelationship, null); }

        private IList<EnterpriseManagementRelationshipObject<EnterpriseManagementObject>> GetRelationshipObjects(ManagementPackClass classType, EnterpriseManagementObject emo, ManagementPackRelationship r, QueryBy q, string filter)
        {
            EnterpriseManagementRelationshipObjectGenericCriteria criteria = null;
            IList<EnterpriseManagementRelationshipObject<EnterpriseManagementObject>> Results = null;
            WriteVerbose("Retrieving Relationship Objects. QueryBy is:" + q.ToString());
            WriteVerbose("Recursive:" + this.Recursive);
            if (filter != null)
            {
                WriteVerbose(" Using Filter: " + filter);
                Regex re;
                foreach (string s in EnterpriseManagementRelationshipObjectGenericCriteria.GetValidPropertyNames())
                {
                    re = new Regex(s, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    filter = re.Replace(filter, s);
                }
                WriteVerbose(" After property name substitution: " + filter);
                try
                {
                    string convertedFilter = ConvertFilterToGenericCriteria(filter);
                    WriteVerbose(" Converted filter is: " + convertedFilter);
                    criteria = new EnterpriseManagementRelationshipObjectGenericCriteria(convertedFilter);
                }
                catch (Exception e)
                {
                    ThrowTerminatingError(new ErrorRecord(e, "CreateRelationshipCriteria", ErrorCategory.InvalidOperation, filter));
                }
            }
            try
            {
                switch (q)
                {
                    case QueryBy.TargetClass:
                        {
                            if (criteria != null)
                            {
                                Results = _mg.EntityObjects.GetRelationshipObjectsByTargetClass<EnterpriseManagementObject>(criteria, classType, ObjectQueryOptions.Default);
                            }
                            else
                            {
                                Results = _mg.EntityObjects.GetRelationshipObjectsByTargetClass<EnterpriseManagementObject>(classType, ObjectQueryOptions.Default);
                            }
                            break;
                        }
                    case QueryBy.SourceClass:
                        {
                            if (criteria != null)
                            {
                                Results = _mg.EntityObjects.GetRelationshipObjectsBySourceClass<EnterpriseManagementObject>(criteria, classType, ObjectQueryOptions.Default);
                            }
                            else
                            {
                                Results = _mg.EntityObjects.GetRelationshipObjectsBySourceClass<EnterpriseManagementObject>(classType, ObjectQueryOptions.Default);
                            }
                            break;
                        }
                    case QueryBy.Target:
                        {
                            Results = _mg.EntityObjects.GetRelationshipObjectsWhereTarget<EnterpriseManagementObject>(emo.Id, ObjectQueryOptions.Default);
                            break;
                        }
                    case QueryBy.TargetAndRelationship:
                        {
                            Results = _mg.EntityObjects.GetRelationshipObjectsWhereTarget<EnterpriseManagementObject>(emo.Id, r, DerivedClassTraversalDepth.Recursive, this.Recursive ? TraversalDepth.Recursive : TraversalDepth.OneLevel, ObjectQueryOptions.Default);
                            break;
                        }
                    case QueryBy.Source:
                        {
                            if (criteria != null)
                            {
                                Results = _mg.EntityObjects.GetRelationshipObjectsWhereSource<EnterpriseManagementObject>(emo.Id, criteria, this.Recursive ? TraversalDepth.Recursive : TraversalDepth.OneLevel, ObjectQueryOptions.Default);
                            }
                            else
                            {
                                Results = _mg.EntityObjects.GetRelationshipObjectsWhereSource<EnterpriseManagementObject>(emo.Id, this.Recursive ? TraversalDepth.Recursive : TraversalDepth.OneLevel, ObjectQueryOptions.Default);
                            }
                            break;
                        }
                    case QueryBy.Relationship:
                        {
                            if (criteria != null)
                            {
                                WriteVerbose("Relationship with criteria");
                                Results = _mg.EntityObjects.GetRelationshipObjects<EnterpriseManagementObject>(criteria, ObjectQueryOptions.Default);
                            }
                            else
                            {
                                Results = _mg.EntityObjects.GetRelationshipObjects<EnterpriseManagementObject>(r, DerivedClassTraversalDepth.Recursive, ObjectQueryOptions.Default);
                                WriteVerbose("Relationship via r: " + r.Id.ToString() + ". Count = " + Results.Count.ToString());
                            }
                            break;
                        }
                    default:
                        {
                            ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("No relationship query type specified"), "BadRelationshipQueryRequest", ErrorCategory.InvalidOperation, this));
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                ThrowTerminatingError(new ErrorRecord(e, "RelationshipQuery", ErrorCategory.InvalidOperation, this));
            }
            foreach (EnterpriseManagementRelationshipObject<EnterpriseManagementObject> o in Results) { WriteVerbose("ID: " + o.Id.ToString()); }
            return Results;
        }
        #endregion GetRelationshipHelper

        protected override void ProcessRecord()
        {
            if (this.ParameterSetName == "TARGET")
            {
                WriteObject(GetRelationshipObjects(Target, QueryBy.Target, Filter), true);
            }
            else if (this.ParameterSetName == "RELATIONSHIP")
            {
                foreach (ManagementPackRelationship r in Relationship)
                {
                    if (this.BySource == null && this.ByTarget == null)
                        WriteObject(GetRelationshipObjects(r, Filter), true);
                    else if (this.BySource != null)
                        WriteObject(GetRelationshipObjects(BySource, r), true);
                    else if (this.ByTarget != null)
                        WriteObject(GetRelationshipObjects(ByTarget, r), true);
                }
            }
            else if (this.ParameterSetName == "SOURCE")
            {
                WriteObject(GetRelationshipObjects(Source, QueryBy.Source, Filter), true);
            }
            else if (this.ParameterSetName == "TARGETOBJECT")
            {
                WriteObject(GetRelationshipObjects(ByTarget, QueryBy.Target), true);
            }
            else if (this.ParameterSetName == "TARGETANDRELATIONSHIP")
            {
                WriteObject(GetRelationshipObjects(TargetObject, TargetRelationship));
            }
            else if (this.ParameterSetName == "SOURCEOBJECT")
            {
                WriteObject(GetRelationshipObjects(BySource, QueryBy.Source, Filter), true);
            }
            else if (this.ParameterSetName == "FILTER")
            {
                WriteObject(GetRelationshipObjects(Filter), true);
            }
            else
            {
                WriteObject(_mg.EntityObjects.GetRelationshipObject<EnterpriseManagementObject>(Id, ObjectQueryOptions.Default));
                WriteVerbose("By Id: " + Id.ToString());
            }
        }
    }
}