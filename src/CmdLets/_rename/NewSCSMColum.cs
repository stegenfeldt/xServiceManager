using System;
using System.Management.Automation;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.New, "xSCSMColumn")]
    public class NewSCSMColum : SMCmdletBase
    {
        private string _name;
        private string _displayname;
        private string _width = "100";
        private string _bindingpath;
        private string _datatype = "s:String";
        private SwitchParameter _PassThru = false;

        [Parameter(Mandatory = true)]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [Parameter(Mandatory = true)]
        public string BindingPath
        {
            get { return _bindingpath; }
            set { _bindingpath = value; }
        }

        [Parameter(Mandatory = false)]
        public string DisplayName
        {
            get { return _displayname; }
            set { _displayname = value; }
        }
        
        [Parameter]
        public string Width
        {
            get { return _width; }
            set { _width = value; }
        }

        [Parameter]
        public string DataType
        {
            get { return _datatype; }
            set { _datatype = value; }
        }

        [Parameter]
        public SwitchParameter PassThru
        {
            get { return _PassThru; }
            set { _PassThru = value; }
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
        }

        protected override void  EndProcessing()
        {
 	        base.EndProcessing();

            Column columnOutput = new Column();
            columnOutput.DataType = _datatype;
            columnOutput.DisplayMemberBinding = String.Format("{{Binding Path={0}}}", _bindingpath);
            columnOutput.DisplayNameId = SMHelpers.MakeMPElementSafeUniqueIdentifier(_displayname);
            columnOutput.DisplayNameString = _displayname;
            if (_name != null)
                columnOutput.Name = _name;
            else
                columnOutput.Name = SMHelpers.MakeMPElementSafeUniqueIdentifier("Column");
            columnOutput.Property = _bindingpath;
            columnOutput.Width = _width;

            if (PassThru)
            {
                WriteObject(columnOutput);
            }
        }
    }
}
