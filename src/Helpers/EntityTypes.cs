using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    public class EntityTypeHelper : SMCmdletBase
    {
        // Parameters
        private string _name = ".*";
        [Parameter(Position=0,ValueFromPipeline=true)]
        public string Name
        {
            get {return _name; }
            set { _name = value; }
        }
    }

    public abstract class GetGroupQueueCommand : ObjectCmdletHelper
    {
        // This needs to be overridden in the Get-Group/Queue cmdlet to
        // get the appropriate objects
        public abstract string neededClassName
        {
            get;
        }

        private string[] _displayname = { "*" };
        [Parameter(Position = 0, ParameterSetName = "DISPLAYNAME")]
        public string[] DisplayName
        {
            get { return _displayname; }
            set { _displayname = value; }
        }
        private string[] _name;
        [Parameter(ParameterSetName = "NAME", Position = 0)]
        public string[] Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private Guid[] _id;
        [Parameter(Position = 0, ParameterSetName = "ID")]
        public Guid[] Id
        {
            get { return _id; }
            set { _id = value; }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }
        protected override void EndProcessing()
        {
            // Short circuit the entire process if you got a collection of IDs
            if (ParameterSetName == "ID")
            {
                foreach (Guid g in Id)
                {
                    WriteObject(new EnterpriseManagementGroupObject(_mg.EntityObjects.GetObject<EnterpriseManagementObject>(g, ObjectQueryOptions.Default)));
                }
                return;
            }
            // OK - we're in the Name/DisplayName parametersets, so figure out what base class to get
            ManagementPackClass cig = null;
            foreach (ManagementPackClass c in _mg.EntityTypes.GetClasses())
            {
                // Microsoft.SystemCenter.ConfigItemGroup or System.WorkItemGroup
                if (String.Compare(c.Name, neededClassName, true) == 0)
                {
                    cig = c;
                }
            }
            foreach (EnterpriseManagementObject emo in _mg.EntityObjects.GetObjectReader<EnterpriseManagementObject>(cig, ObjectQueryOptions.Default))
            {
                switch (ParameterSetName)
                {
                    case "DISPLAYNAME":
                        foreach (string s in DisplayName)
                        {
                            WildcardPattern wc = new WildcardPattern(s, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
                            if (wc.IsMatch(emo.DisplayName))
                            {
                                WriteObject(new EnterpriseManagementGroupObject(emo));
                            }
                        }
                        break;
                    case "NAME":
                        foreach (string s in Name)
                        {
                            WildcardPattern wc = new WildcardPattern(s, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
                            // We need to match against the ClassName
                            if (wc.IsMatch(emo.GetLeastDerivedNonAbstractClass().Name))
                            {
                                WriteObject(new EnterpriseManagementGroupObject(emo));
                            }
                        }
                        break;
                    default:
                        ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("Bad switch"), "GroupOutput", ErrorCategory.InvalidOperation, this));
                        break;
                }
            }
        }
    }

}
