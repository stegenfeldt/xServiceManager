using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMManagementPackReference", DefaultParameterSetName="NAME")]
    public class GetSCSMManagementPackReference : SMCmdletBase
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
      
        private ManagementPack _managementpack;

        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public ManagementPack ManagementPack
        {
            get { return _managementpack; }
            set { _managementpack = value; }
        }
        private string [] _alias = { "*" };
        [Parameter(ParameterSetName="ALIAS")]
        [ValidateNotNullOrEmpty]
        public string [] Alias
        {
            get { return _alias; }
            set { _alias = value; }
        }
        private string [] _name = { "*" };
        [Parameter(ParameterSetName="NAME")]
        public string [] Name
        {
            get { return _name; }
            set { _name = value; }
        }

        protected override void ProcessRecord()
        {
            try
            {
                foreach (KeyValuePair<string,ManagementPackReference> mpref in _managementpack.References)
                {
                    if (ParameterSetName == "NAME")
                    {
                        foreach(string s in Name)
                        {
                            WildcardPattern wp = new WildcardPattern(s, WildcardOptions.CultureInvariant|WildcardOptions.IgnoreCase);
                            if ( wp.IsMatch(mpref.Value.Name) )
                            {
                                PSObject o = new PSObject(mpref.Value);
                                o.Members.Add(new PSNoteProperty("Alias", mpref.Key));
                                WriteObject(o);
                            }
                        }
                    }
                    else
                    {
                        foreach(string s in Alias)
                        {
                            WildcardPattern wp = new WildcardPattern(s, WildcardOptions.CultureInvariant|WildcardOptions.IgnoreCase);
                            if ( wp.IsMatch(mpref.Key) )
                            {
                                PSObject o = new PSObject(mpref.Value);
                                o.Members.Add(new PSNoteProperty("Alias", mpref.Key));
                                WriteObject(o);
                            }
                        }
                    }
                    // WriteObject(mpref);
                }
            }
            catch (Exception e)
            {
                ThrowTerminatingError(new ErrorRecord(e, "Error", ErrorCategory.InvalidOperation, _managementpack.Name));
            }
        }
    }
}
