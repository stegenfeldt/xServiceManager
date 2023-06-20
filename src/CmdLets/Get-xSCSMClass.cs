using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    /// <summary>
    /// Represents a cmdlet for retrieving a Service Manager class.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "xSCSMClass")]
    public class GetSMClassCommand : EntityTypeHelper
    {
        private Guid _id = Guid.Empty;

        /// <summary>
        /// Gets or sets the ID of the class to retrieve.
        /// </summary>
        [Parameter]
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// Processes the record based on the provided ID or class name.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Id != Guid.Empty)
            {
                try
                {
                    WriteObject(_mg.EntityTypes.GetClass(Id));
                }
                catch (ObjectNotFoundException e)
                {
                    WriteError(new ErrorRecord(e, "Class not found", ErrorCategory.ObjectNotFound, Id));
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, "Unknown error", ErrorCategory.NotSpecified, Id));
                }
            }
            else
            {
                Regex r = new Regex(Name, RegexOptions.IgnoreCase);
                foreach (ManagementPackClass o in _mg.EntityTypes.GetClasses())
                {
                    //FIXME: There must be a better way to do this than returning *everything* and looping through them.
                    if (r.IsMatch(o.Name))
                    {
                        WriteObject(o);
                    }
                }
            }
        }
    }
}