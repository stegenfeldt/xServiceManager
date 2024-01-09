using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.ConnectorFramework;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Set, "xSCSMObject", SupportsShouldProcess = true)]
    public class SetSMObjectCommand : ObjectCmdletHelper
    {
        // Set properties on an EMO
        // this takes a hashtable where the 
        // KEY => PropertyName
        // VALUE => new value for the property

        // The adapted EMO
        private PSObject _smobject;
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public PSObject SMObject
        {
            set { _smobject = value; }
            get { return _smobject; }
        }
        // the property/value pairs
        private Hashtable _propertyValueHashTable;
        [Parameter(ParameterSetName = "hashtable", Position = 1, Mandatory = true)]
        [Alias("PH")]
        public Hashtable PropertyHashtable
        {
            get { return _propertyValueHashTable; }
            set { _propertyValueHashTable = value; }
        }
        // The following two parameters are a short-cut, 
        // if you only want to set a single property
        // You don't need the hashtable
        private string _property;
        [Parameter(ParameterSetName = "pair", Position = 1, Mandatory = true)]
        public string Property
        {
            set { _property = value; }
            get { return _property; }
        }

        private List<Guid> objectList;
        private SwitchParameter _passThru;
        [Parameter]
        public SwitchParameter PassThru
        {
            get { return _passThru; }
            set { _passThru = value; }
        }
        private string _value;
        [Parameter(ParameterSetName = "pair", Position = 2, Mandatory = true)]
        [AllowEmptyString]
        [AllowNull]
        public string Value
        {
            set { _value = value; }
            get { return _value; }
        }
        // By default, we use IncrementalDiscoveryData to reduce the round
        // trips to the CMDB
        private SwitchParameter _noBulkOperation;
        [Parameter]
        public SwitchParameter NoBulkOperation
        {
            get { return _noBulkOperation; }
            set { _noBulkOperation = value; }
        }
        private IncrementalDiscoveryData pendingChanges;

        private Hashtable ht;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            objectList = new List<Guid>();
            if (ParameterSetName == "pair")
            {
                ht = new Hashtable();
                ht.Add(Property, Value);
            }
            else
            {
                ht = new Hashtable(PropertyHashtable);
            }
            // if we're doing bulk operations, we'll need this
            if (!NoBulkOperation)
            {
                pendingChanges = new IncrementalDiscoveryData();
            }
        }

        protected override void ProcessRecord()
        {
            // EnterpriseManagementObject o = (EnterpriseManagementObject)SMObject.Members["__base"].Value;
            // Coerce the 
            EnterpriseManagementObject o = (EnterpriseManagementObject)SMObject.BaseObject;
            AssignNewValues(o, ht);

            // If we're not doing bulk operations, we'll need to call this for each object
            if (NoBulkOperation)
            {
                if (ShouldProcess("Commit Change for " + o.Id))
                {
                    o.Overwrite();
                    if (PassThru)
                    {
                        WriteObject(ServiceManagerObjectHelper.AdaptManagementObject(this, _mg.EntityObjects.GetObject<EnterpriseManagementObject>(o.Id, ObjectQueryOptions.Default)));
                    }

                }
            }
            else
            {
                // One could argue that ShouldProcess is called here,
                // but to reduce verbosity, I do it below
                WriteVerbose("Adding " + o.Id + " to change list");
                objectList.Add(o.Id);
                pendingChanges.Add(o);
            }
        }
        protected override void EndProcessing()
        {
            // If we're doing bulk operations
            if (!NoBulkOperation)
            {
                if (ShouldProcess("SMObjects"))
                {
                    pendingChanges.Overwrite(_mg);
                    if (PassThru)
                    {
                        foreach (EnterpriseManagementObject emo in _mg.EntityObjects.GetObjectReader<EnterpriseManagementObject>(objectList, ObjectQueryOptions.Default))
                        {
                            WriteObject(ServiceManagerObjectHelper.AdaptManagementObject(this, emo));
                        }
                    }
                }
            }
        }
    }
}