using System.Management.Automation;

namespace xServiceManager.Module
{
    /// <summary>
    /// Helper class for managing entity types.
    /// </summary>
    public class EntityTypeHelper : SMCmdletBase
    {
        // Parameters
        private string _name = ".*";
        [Parameter(Position = 0, ValueFromPipeline = true)]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}
