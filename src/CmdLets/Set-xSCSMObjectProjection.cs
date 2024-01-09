using System;
using System.Collections;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Set, "xSCSMObjectProjection", SupportsShouldProcess = true)]
    public class SetSCSMObjectProjectionCommand : ObjectCmdletHelper
    {
        private PSObject _projection = null;
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public PSObject Projection
        {
            get { return _projection; }
            set { _projection = value; }
        }

        private Hashtable _propertyValues = null;
        [Parameter(Mandatory = true, Position = 1)]
        [Alias("ph")]
        public Hashtable PropertyValues
        {
            get { return _propertyValues; }
            set { _propertyValues = value; }
        }

        private SwitchParameter _passThru;
        [Parameter]
        public SwitchParameter PassThru
        {
            get { return _passThru; }
            set { _passThru = value; }
        }

        protected override void ProcessRecord()
        {
            EnterpriseManagementObjectProjection p = Projection.Members["__base"].Value as EnterpriseManagementObjectProjection;
            // EnterpriseManagementObject o = (EnterpriseManagementObject)SMObject.Members["__base"].Value;
            if (p != null)
            {
                EnterpriseManagementObject o = p.Object;
                // create a hashtable of management pack properties
                Hashtable ht = new Hashtable(StringComparer.OrdinalIgnoreCase);
                Hashtable valuesToUse = new Hashtable(StringComparer.OrdinalIgnoreCase);
                foreach (ManagementPackProperty prop in o.GetProperties())
                {
                    ht.Add(prop.Name, prop);
                }
                // TODO: Add support for relationships
                foreach (string s in PropertyValues.Keys)
                {
                    if (!ht.ContainsKey(s))
                    {
                        WriteError(new ErrorRecord(new ObjectNotFoundException(s), "property not found on object", ErrorCategory.NotSpecified, o));
                    }
                    else
                    {
                        valuesToUse.Add(s, PropertyValues[s]);
                    }
                }
                AssignNewValues(o, valuesToUse);
                if (ShouldProcess("Save changes to projection"))
                {
                    p.Commit();
                }
                if (PassThru) { WriteObject(p); }
            }
            else
            {
                WriteError(new ErrorRecord(new ArgumentException("SetProjection"), "object was not a projection", ErrorCategory.InvalidOperation, Projection));
            }
        }
    }
}