using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMRelationshipClass")]
    public class GetSMRelationshipClassCommand : EntityTypeHelper
    {
        private Guid _id = Guid.Empty;
        [Parameter]
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }
        protected override void ProcessRecord()
        {
            if (Id != Guid.Empty)
            {
                try { WriteObject(_mg.EntityTypes.GetRelationshipClass(Id)); }
                catch (ObjectNotFoundException e) { WriteError(new ErrorRecord(e, "Relationship not found", ErrorCategory.ObjectNotFound, Id)); }
                catch (Exception e) { WriteError(new ErrorRecord(e, "Unknown error", ErrorCategory.NotSpecified, Id)); }
            }
            else
            {
                Regex r = new Regex(Name, RegexOptions.IgnoreCase);
                foreach (ManagementPackRelationship o in _mg.EntityTypes.GetRelationshipClasses())
                {
                    if (r.Match(o.Name).Success)
                    {
                        WriteObject(o);
                    }
                }
            }
        }
    }
}