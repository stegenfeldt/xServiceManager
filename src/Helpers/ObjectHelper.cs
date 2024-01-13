using System;
using System.Xml.XPath;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    public sealed class ObjectHelper
    {
        public static PSObject PromoteProjectionProperties(EnterpriseManagementObjectProjection projection, string node, string typeName)
        {
            PSObject PromotedObject = new PSObject();
            PromotedObject.TypeNames[0] = typeName;
            try
            {
                Collection<PSObject> objectList = new Collection<PSObject>();
                XPathNavigator Navigator = projection.CreateNavigator();
                if (Navigator.Select(node).Count > 1)
                {
                    foreach (XPathNavigator xnav in Navigator.Select(node))
                    {
                        PSObject listmember = new PSObject();
                        IComposableProjection composedProjection = (IComposableProjection)xnav.UnderlyingObject;
                        listmember.Members.Add(new PSNoteProperty("__base", composedProjection.Object));
                        foreach (ManagementPackProperty p in composedProjection.Object.GetProperties())
                        {
                            listmember.Members.Add(new PSNoteProperty(p.Name, composedProjection.Object[p].Value));
                        }
                        objectList.Add(listmember);
                    }
                    PromotedObject.Members.Add(new PSNoteProperty(typeName, objectList));

                }
                else
                {
                    XPathNavigator singleNodeNavigator = Navigator.SelectSingleNode(node);
                    IComposableProjection composedProjection = (IComposableProjection)singleNodeNavigator.UnderlyingObject;
                    PromotedObject.Members.Add(new PSNoteProperty("__base", composedProjection.Object));
                    foreach (ManagementPackProperty p in composedProjection.Object.GetProperties())
                    {
                        PromotedObject.Members.Add(new PSNoteProperty(p.Name, composedProjection.Object[p].Value));
                    }
                }
            }
            catch (Exception e)
            {
                PromotedObject.Members.Add(new PSNoteProperty("PromotionFailure", e.Message));
            }
            return PromotedObject;
        }
        // This looks through *all* the enumerations in the system
        // This could be done more efficiently, but at least does't
        // require a round trip to the server since these are cached
        // the client side.
        public static ManagementPackEnumeration GetEnumerationFromName(EnterpriseManagementGroup emg, string name)
        {
            foreach (ManagementPackEnumeration e in emg.EntityTypes.GetEnumerations())
            {
                int CompareResult = String.Compare(e.Name, name, StringComparison.OrdinalIgnoreCase);
                if (CompareResult == 0)
                {
                    return e;
                }
            }
            return null;
        }

        private ObjectHelper() {; }
    }
}
