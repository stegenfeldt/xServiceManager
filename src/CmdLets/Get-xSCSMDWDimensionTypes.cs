using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get,"xSCSMDWDimensionTypes")]
    public class GetSCDWDimensionTypesCommand : DWHelper
    {
        protected override void ProcessRecord()
        {
            if ( Id != Guid.Empty )
            {
                try { WriteObject(_mg.DataWarehouse.GetDimensionType(Id)); }
                catch ( ObjectNotFoundException e ) { WriteError(new ErrorRecord(e, "DimensionType not found", ErrorCategory.ObjectNotFound, Id)); }
                catch ( Exception e ) { WriteError(new ErrorRecord(e, "Unknown error", ErrorCategory.NotSpecified, Id)); }
            }
            else
            {
                Regex r = new Regex(Name, RegexOptions.IgnoreCase);
                foreach(ManagementPackDimensionType o in _mg.DataWarehouse.GetDimensionTypes())
                {
                    if ( r.Match(o.Name).Success )
                    {
                        WriteObject(o);
                    }
                }
            }
        }
    }
}
