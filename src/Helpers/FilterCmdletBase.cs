using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    public class FilterCmdletBase : ObjectCmdletHelper
    {
        protected string _filter = null;
        [Parameter]
        public string Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }
        private Int32 _maxCount = Int32.MaxValue;
        [Parameter]
        public Int32 MaxCount
        {
            get { return _maxCount; }
            set { _maxCount = value; }
        }

        // By default, we'll collect the projections base on 
        // when they were created
        private string _sortBy = "TimeAdded";
        [Parameter]
        public string SortBy
        {
            get { return _sortBy; }
            set { _sortBy = value; }
        }
        internal const string xmlns = "xmlns=\"http://Microsoft.EnterpriseManagement.Core.Sorting\"";
        internal const string ascending = "Ascending";
        internal const string descending = "Descending";
        internal string Order = ascending;

        internal void addSortProperty(ObjectQueryOptions options, string sortProperty, ManagementPackClass c)
        {
            SortingOrder sOrder = SortingOrder.Ascending;
            if (sortProperty == null || c == null || options == null) { return; }
            if (sortProperty[0] == '-')
            {
                sOrder = SortingOrder.Descending;
                sortProperty = SortBy.Substring(1);
            }
            WriteVerbose("look for instance properties first");

            foreach (ManagementPackProperty mpp in c.GetProperties(BaseClassTraversalDepth.Recursive))
            {
                if (string.Compare(mpp.Name, sortProperty, true) == 0)
                {
                    options.AddSortProperty(mpp, sOrder);
                    return;
                }
            }

            WriteVerbose("now look for generic properties");

            foreach (GenericProperty gp in GenericProperty.GetGenericProperties())
            {
                if (string.Compare(gp.PropertyName, sortProperty, true) == 0)
                {
                    Type t = typeof(EnterpriseManagementObjectGenericPropertyName);
                    EnterpriseManagementObjectGenericPropertyName pn = (EnterpriseManagementObjectGenericPropertyName)Enum.Parse(t, gp.PropertyName);
                    EnterpriseManagementObjectGenericProperty gProperty = new EnterpriseManagementObjectGenericProperty(pn);
                    options.AddSortProperty(gProperty, sOrder);
                    return;
                }
            }

        }

        internal string makeSortCriteriaString(string sortProperty, ManagementPackClass c)
        {
            WriteDebug("makeSortCriteriaString with MP Class");
            Order = ascending;
            // Now that we have a projection, create the sort order
            if (sortProperty[0] == '-')
            {
                Order = descending;
                sortProperty = SortBy.Substring(1);
            }

            WriteVerbose("checking for targettype property");
            foreach (ManagementPackProperty mpp in c.GetProperties(BaseClassTraversalDepth.Recursive))
            {
                WriteVerbose("Checking TargetType properties: " + mpp.Name + " - " + sortProperty);
                if (string.Compare(mpp.Name, sortProperty, true) == 0)
                {
                    WriteVerbose("Sort Property: " + mpp.Name);
                    return String.Format(
                        "<Sorting {0}><SortProperty SortOrder=\"{1}\">$Target/Property[Type='{2}']/{3}$</SortProperty></Sorting>",
                        xmlns, Order, c.Id, mpp.Name
                        );
                }
            }

            // OK, we'll check targettype properties first
            foreach (GenericProperty gp in GenericProperty.GetGenericProperties())
            {
                if (string.Compare(gp.PropertyName, sortProperty, true) == 0)
                {
                    WriteVerbose("Sort property: " + gp.PropertyName);
                    return String.Format(
                        "<Sorting {0}><GenericSortProperty SortOrder=\"{1}\">{2}</GenericSortProperty></Sorting>",
                        xmlns, Order, gp.PropertyName
                        );
                }
            }

            return null;
        }

        internal string makeSortCriteriaString(string sortProperty, ManagementPackTypeProjection p)
        {
            WriteDebug("start makeSortCriteriaString");
            Order = ascending;
            // Now that we have a projection, create the sort order
            if (SortBy[0] == '-')
            {
                Order = descending;
                sortProperty = SortBy.Substring(1);
            }
            // OK, we'll check generic properties first
            WriteVerbose("makeSortCriteriaString");
            foreach (GenericProperty gp in GenericProperty.GetGenericProperties())
            {
                if (string.Compare(gp.PropertyName, sortProperty, true) == 0)
                {
                    WriteVerbose("Sort property: " + gp.PropertyName);
                    return String.Format(
                        "<Sorting {0}><GenericSortProperty SortOrder=\"{1}\">{2}</GenericSortProperty></Sorting>",
                        xmlns, Order, gp.PropertyName
                        );
                }
            }
            WriteVerbose("checking further");
            foreach (ManagementPackProperty mpp in p.TargetType.GetProperties(BaseClassTraversalDepth.Recursive))
            {
                WriteVerbose("Checking TargetType properties: " + mpp.Name + " - " + sortProperty);
                if (string.Compare(mpp.Name, sortProperty, true) == 0)
                {
                    WriteVerbose("Sort Property: " + mpp.Name);
                    return String.Format(
                        "<Sorting {0}><SortProperty SortOrder=\"{1}\">$Target/Property[Type='{2}']/{3}$</SortProperty></Sorting>",
                        xmlns, Order, p.TargetType.Id, mpp.Name
                        );
                }
            }
            return null;
        }

    }
}
