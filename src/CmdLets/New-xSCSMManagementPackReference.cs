using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.New, "xSCSMManagementPackReference")]
    public class NewSCSMManagementPackReference : SMCmdletBase
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private string _alias;
        private ManagementPack _managementpack;

        [Parameter(Mandatory = true, ValueFromPipeline = false)]
        public string Alias
        {
            get { return _alias; }
            set { _alias = value; }
        }
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public ManagementPack ManagementPack
        {
            get { return _managementpack; }
            set { _managementpack = value; }
        }
        
        protected override void ProcessRecord()
        {
            try
            {
                ManagementPackReference mpref = new ManagementPackReference(_managementpack);
                KeyValuePair<string, ManagementPackReference> kvp = new KeyValuePair<string, ManagementPackReference>(_alias, mpref);
                WriteObject(kvp);
            }
            catch (Exception e)
            {
                ThrowTerminatingError(new ErrorRecord(e, "Error", ErrorCategory.InvalidOperation, Alias));
            }
        }
    }
}
