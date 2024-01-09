using System;
using System.Management.Automation;

namespace xServiceManager.Module
{
    public class PresentationCmdletBase : SMCmdletBase
    {
        private string[] _name = { "*" };
        [Parameter(Position = 0, ParameterSetName = "Name")]
        public string[] Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private Guid _id;
        [Parameter(Position = 0, ParameterSetName = "Id")]
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _dislayName;
        [Parameter(Position = 0, ParameterSetName = "DisplayName")]
        public string DisplayName
        {
            get { return _dislayName; }
            set { _dislayName = value; }
        }

    }

}
