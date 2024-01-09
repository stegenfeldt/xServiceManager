using System;
using System.Management.Automation;
using System.Text.RegularExpressions;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMManagementPack", DefaultParameterSetName = "Name")]
    public class GetManagementPackCommand : SMCmdletBase
    {
        // Parameters

        private string _name = ".*";
        [Parameter(Position = 0, ValueFromPipeline = true, ParameterSetName = "Name")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private Guid[] _id;
        [Parameter(Position = 0, ValueFromPipeline = true, ParameterSetName = "ID")]
        public Guid[] Id
        {
            get { return _id; }
            set { _id = value; }
        }

        protected override void ProcessRecord()
        {
            if (ParameterSetName == "Name")
            {
                Regex r = new Regex(Name, RegexOptions.IgnoreCase);
                foreach (ManagementPack mp in _mg.ManagementPacks.GetManagementPacks())
                {
                    if (r.Match(mp.Name).Success)
                    {
                        WriteObject(mp);
                    }
                }
            }
            if (ParameterSetName == "ID")
            {
                foreach (Guid g in Id)
                {
                    WriteVerbose("Looking for id: " + g.ToString());
                    try
                    {
                        WriteObject(_mg.ManagementPacks.GetManagementPack(g));
                    }
                    catch (ObjectNotFoundException e)
                    {
                        WriteError(new ErrorRecord(e, "ManagementPack id '" + g + "' does not exist", ErrorCategory.ObjectNotFound, g.ToString()));
                    }
                    catch (Exception e)
                    {
                        WriteError(new ErrorRecord(e, "ManagementPack id '" + g + "' does not exist", ErrorCategory.ObjectNotFound, g.ToString()));
                    }
                }
            }
        }
    }
}
