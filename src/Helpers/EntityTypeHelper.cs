using System.Management.Automation;

namespace xServiceManager.Module
{
    /// <summary>
    /// Helper class for managing entity types.
    /// </summary>
    public class EntityTypeHelper : SMCmdletBase
    {
        // Leaving Name unset (to "") returns nothing, making "return all" opt-in, as requested by customers.
        // This is different from SMLets in that you must set -Name "*" to get all the objects.
        private string _name = "";
        [Parameter(Position = 0, ValueFromPipeline = true)]
        public string Name
        {
            get {
                // return a proper regex 
                return "^" + _name.Replace("?",".").Replace("*",".*") + "$";
            }
            set { _name = value; }
        }
    }
}
