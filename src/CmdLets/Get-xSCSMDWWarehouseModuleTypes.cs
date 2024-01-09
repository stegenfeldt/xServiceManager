using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get,"xSCSMDWWarehouseModuleTypes")]
    public class GetSCDWWareHoueModuleTypesCommand : DWHelper
    {
        protected override void ProcessRecord()
        {
            if ( Id != Guid.Empty )
            {
                try { WriteObject(_mg.DataWarehouse.GetWarehouseModuleType(Id)); }
                catch ( ObjectNotFoundException e ) { WriteError(new ErrorRecord(e, "WarehouseModuleType not found", ErrorCategory.ObjectNotFound, Id)); }
                catch ( Exception e ) { WriteError(new ErrorRecord(e, "Unknown error", ErrorCategory.NotSpecified, Id)); }
            }
            else
            {
                Regex r = new Regex(Name, RegexOptions.IgnoreCase);
                foreach(ManagementPackWarehouseModuleType o in _mg.DataWarehouse.GetWarehouseModuleTypes())
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
