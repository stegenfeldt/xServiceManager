using System;
using System.Management.Automation;

namespace xServiceManager.Module
{
    public class DWHelper : SMCmdletBase
    {
        // Parameters
        private string _name = ".*";
        [Parameter(Position=0,ValueFromPipeline=true)]
        public string Name
        {
            get {return _name; }
            set { _name = value; }
        }
        private Guid _id = Guid.Empty;
        [Parameter]
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }
    }
}
