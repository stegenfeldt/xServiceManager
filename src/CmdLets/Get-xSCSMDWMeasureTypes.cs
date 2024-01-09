using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get,"xSCSMDWMeasureTypes")]
    public class GetSCDWMeasureTypesCommand : DWHelper
    {
        protected override void ProcessRecord()
        {
            if ( Id != Guid.Empty )
            {
                try { WriteObject(_mg.DataWarehouse.GetMeasureType(Id)); }
                catch ( ObjectNotFoundException e ) { WriteError(new ErrorRecord(e, "MeasureType not found", ErrorCategory.ObjectNotFound, Id)); }
                catch ( Exception e ) { WriteError(new ErrorRecord(e, "Unknown error", ErrorCategory.NotSpecified, Id)); }
            }
            else
            {
                Regex r = new Regex(Name, RegexOptions.IgnoreCase);
                foreach(ManagementPackMeasureType o in _mg.DataWarehouse.GetMeasureTypes())
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
