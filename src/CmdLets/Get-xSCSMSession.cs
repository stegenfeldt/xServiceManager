using System.Collections.Generic;
using System.Management.Automation;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get,"xSCSMSession")]
    public class GetSCSMSession : PSCmdlet
    {
        private string _computerName = ".*";
        [Parameter(Position=0,ValueFromPipeline=true)]
        public string ComputerName
        {
            get { return _computerName; }
            set { _computerName = value; }
        }
        private List<string> l = null;
        protected override void ProcessRecord()
        {
            l = ConnectionHelper.GetMGList(ComputerName);
            foreach(string n in l) 
            {
                WriteObject(ConnectionHelper.GetMG(n));
            }
        }
    }
}
