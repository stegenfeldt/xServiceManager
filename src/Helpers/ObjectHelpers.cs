using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{

    public class ObjectCmdletHelper : SMCmdletBase
    {
        // For the Object cmdlets, they all pretty much can use ObjectQueryOptions, so
        // we'll just make it part of the base class
        private ObjectQueryOptions _queryOption = ObjectQueryOptions.Default;
        [Parameter]
        public ObjectQueryOptions QueryOption
        {
            get { return _queryOption; }
            set { _queryOption = value; }
        }

        public void AssignNewValues(EnterpriseManagementObject o, Hashtable ht)
        {
            foreach (string s in ht.Keys)
            {
                bool found = false;
                WriteDebug("Attempting to find property " + s);
                foreach (ManagementPackProperty p in o.GetProperties())
                {
                    int CompareResult = String.Compare(p.Name, s, ic);
                    WriteDebug("PROPERTY " + p.Name + " == " + s + " Result: " + CompareResult);
                    if (CompareResult == 0)
                    {
                        found = true;
                        WriteDebug("Assigning " + ht[s] + " to " + p.Name);
                        try
                        {
                            AssignNewValue(p, o[p], ht[s]);
                        }
                        catch (Exception e)
                        {
                            string errmsg = "Assigning " + ht[s] + " to " + p.Name;
                            WriteError(new ErrorRecord(e, errmsg, ErrorCategory.InvalidOperation, ht[s]));
                        }
                        break;
                    }
                }
                if (!found)
                {
                    WriteDebug("Could not find property " + s + " on object");
                }
            }
        }

        // in PowerShell, string comparisons are done with case ignored
        public StringComparison ic = StringComparison.OrdinalIgnoreCase;
        // Assign a value to a managementpack property
        // This code is clever enough to handle nearly all of the types that 
        // a management pack property can be
        // if you attempt to assign something which is an enum type
        // we'll go looking for the appropriate enumeration and use that
        // TODO: handle the binary property type (it should take a stream)
        public void AssignNewValue(ManagementPackProperty p, EnterpriseManagementSimpleObject so, object newValue)
        {
            string PropertyType = p.Type.ToString();
            WriteVerbose("Want to set " + p.Name + "(" + PropertyType + ") to " + newValue);
            // so, if new value is null, set and return immediately
            if (newValue == null)
            {
                try
                {
                    so.Value = null;
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, "Could not assign " + p.Name + " to null", ErrorCategory.InvalidOperation, newValue));
                }
                return;
            }
            switch (PropertyType)
            {
                case "richtext":
                case "string":
                    try
                    {
                        so.Value = newValue;
                    }
                    catch (InvalidSimpleObjectValueException)
                    {
                        WriteWarning("Converting new value ('" + newValue.GetType().ToString() + ":" + newValue.ToString() + "') to string");
                        so.Value = newValue.ToString();
                    }
                    break;
                case "double":
                case "int":
                case "decimal":
                case "bool":
                    so.Value = newValue;
                    break;
                case "guid":
                    // This should be done in a try/catch
                    // otherwise it will fail poorly
                    try
                    {
                        so.Value = new Guid(newValue.ToString());
                    }
                    catch (Exception e)
                    {
                        WriteError(new ErrorRecord(e, "Could not assign guid", ErrorCategory.InvalidOperation, newValue));
                    }
                    break;
                case "enum":
                    WriteDebug("Looking for Enumeration: " + newValue);
                    ManagementPackEnumeration mpe;
                    // We might have gotten an enum, try the assignment
                    if (newValue is ManagementPackEnumeration)
                    {
                        WriteVerbose("Actually an enumeration");
                        try
                        {
                            mpe = (ManagementPackEnumeration)newValue;
                            so.Value = mpe;
                        }
                        catch (Exception e)
                        {
                            WriteError(new ErrorRecord(e, "Could not assign enum", ErrorCategory.InvalidOperation, newValue));
                        }
                    }
                    else
                    {
                        WriteVerbose("Looking Enum via string " + newValue.ToString() + " in " + p.EnumType.GetElement());
                        try
                        {
                            mpe = SMHelpers.GetEnum(newValue.ToString(), p.EnumType.GetElement());
                            WriteVerbose("found enum: " + mpe.ToString());
                            so.Value = mpe;
                            WriteVerbose("set the value");
                        }
                        catch (Exception e)
                        {
                            WriteError(new ErrorRecord(e, "Could not assign enum ", ErrorCategory.ObjectNotFound, newValue));
                        }
                    }
                    break;
                case "datetime":
                    // TODO: handle failure gracefully
                    try
                    {
                        //AG: we no need to parse string if newValue is already DateTime
                        if (newValue is DateTime)
                        {
                            so.Value = newValue;
                        }
                        else
                        {
                            // AG: what reason to convert DateTime to string?
                            //so.Value = DateTime.Parse(newValue.ToString(), CultureInfo.CurrentCulture).ToString();
                            so.Value = DateTime.Parse(newValue.ToString(), CultureInfo.CurrentCulture);
                        }
                    }
                    catch (Exception e)
                    {
                        WriteError(new ErrorRecord(e, "Could not assign date ", ErrorCategory.ObjectNotFound, newValue));
                    }
                    break;
                case "binary":
                    // TODO: HANDLE filename
                    FileStream myStream = null;
                    // Deal with the case that we got a PSObject
                    if (newValue is PSObject)
                    {
                        myStream = ((PSObject)newValue).BaseObject as FileStream;
                    }
                    else
                    {
                        // see if we get lucky
                        myStream = newValue as FileStream;
                    }
                    if (myStream != null)
                    {
                        so.Value = myStream;
                    }
                    else
                    {
                        WriteError(new ErrorRecord(new ArgumentException(newValue.ToString()),
                           String.Format("Property must be a file stream, received a {0}", newValue.GetType().FullName), ErrorCategory.InvalidArgument, newValue));
                    }
                    break;
                default:
                    WriteVerbose("Could not find type setter for " + PropertyType);
                    WriteError(new ErrorRecord(new ItemNotFoundException("PropertySetterNotFound"), "No such property setter", ErrorCategory.ObjectNotFound, PropertyType));
                    break;
            }

        }

        // All this does is convert the PowerShell filter language to the criteria syntax
        public string ConvertFilterToGenericCriteria(string filter)
        {
            Dictionary<string, string> OpToOp = new Dictionary<string, string>();
            Regex re;
            // "-gt","-ge","-lt","-le","-eq","-ne","-like","-notlike","-match","-notmatch"
            // Add -isnull and -isnotnull, even though they aren't PowerShell operators
            OpToOp.Add("-and", "and");
            OpToOp.Add("-or", "or");
            OpToOp.Add("-eq", "=");
            OpToOp.Add("-ne", "!=");
            OpToOp.Add("-lt", "<");
            OpToOp.Add("-gt", ">");
            OpToOp.Add("-le", "<=");
            OpToOp.Add("-ge", ">=");
            OpToOp.Add("-like", "like");
            OpToOp.Add("-notlike", "! like");
            OpToOp.Add("-isnull", "is null");
            OpToOp.Add("-isnotnull", "is not null");
            re = new Regex("\\*");
            filter = re.Replace(filter, "%");
            re = new Regex("\\?");
            filter = re.Replace(filter, "_");
            re = new Regex("\"");
            filter = re.Replace(filter, "'");
            foreach (string k in OpToOp.Keys)
            {
                re = new Regex(k, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                filter = re.Replace(filter, OpToOp[k]);
            }
            return filter;
        }
        public string FixUpPropertyNames(string filter, ManagementPackClass mpClass)
        {
            Dictionary<string, string> propertyFixes = new Dictionary<string, string>();
            Regex re;
            foreach (ManagementPackProperty p in mpClass.GetProperties(BaseClassTraversalDepth.Recursive))
            {
                re = new Regex(p.Name, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                filter = re.Replace(filter, p.Name);
            }
            WriteDebug("returning: " + filter);
            return filter;
        }

        public string ConvertFilterToCriteriaString(ManagementPackClass mpClass, string filter, bool isProjection)
        {
            WriteVerbose("ConvertFilterToCriteriaString: " + filter);
            StringBuilder sb = new StringBuilder();
            List<PropertyOperatorValue> POVs = new List<PropertyOperatorValue>();
            foreach (string subFilter in Regex.Split(filter, "-or", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
            {
                WriteVerbose("SubFilter => " + subFilter);
                try
                {
                    POVs.Add(new PropertyOperatorValue(subFilter));
                }
                catch
                {
                    WriteError(new ErrorRecord(new ObjectNotFoundException("criteria"), "Bad Filter", ErrorCategory.NotSpecified, filter));
                    return null;
                }
            }

            // Construct the Criteria XML
            // This should really be done with an XML DOM methods
            sb.Append("<Criteria xmlns='http://Microsoft.EnterpriseManagement.Core.Criteria/'>");
            sb.Append("<Reference Id='");
            sb.Append(mpClass.GetManagementPack().Name);
            sb.Append("' Version='");
            sb.Append(mpClass.GetManagementPack().Version.ToString());
            sb.Append("'");
            if (mpClass.GetManagementPack().KeyToken != null)
            {
                sb.Append(" PublicKeyToken='");
                sb.Append(mpClass.GetManagementPack().KeyToken);
                sb.Append("'");
            }
            sb.Append(" Alias='myMP' />");
            // JWT START OF EXPRESSION
            // CHECK FOR AND/OR HERE
            if (POVs.Count > 1)
            {
                sb.Append("<Expression>");
                sb.Append("<Or>");
            }
            foreach (PropertyOperatorValue POV in POVs)
            {

                sb.Append("<Expression>");
                sb.Append("<SimpleExpression>");
                // check to be sure the property exists on the class
                // do this with a creatable EMO as *all* the properties are presented
                // If the class is abstract, you can't create it. This means you have 
                // to use the properties on the class as you get it.
                List<ManagementPackProperty> proplist = new List<ManagementPackProperty>();
                if (mpClass.Abstract)
                {
                    foreach (ManagementPackProperty p in mpClass.PropertyCollection)
                    {
                        proplist.Add(p);
                    }
                }
                else
                {
                    // The proper way to get at the properties of a class
                    foreach (ManagementPackProperty p in mpClass.GetProperties(BaseClassTraversalDepth.Recursive, PropertyExtensionMode.All))
                    {
                        proplist.Add(p);
                    }
                }

                bool foundproperty = false;
                foreach (ManagementPackProperty p in proplist)
                {
                    if (String.Compare(POV.Property, p.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        sb.Append("<ValueExpressionLeft>");
                        if (isProjection)
                        {
                            // projection not quite supported
                            sb.Append("<Property>$Target/Property[Type='myMP!" + mpClass.Name + "']/" + p.Name + "$</Property>");
                        }
                        else
                        {
                            sb.Append("<Property>$Target/Property[Type='myMP!" + mpClass.Name + "']/" + p.Name + "$</Property>");
                        }

                        sb.Append("</ValueExpressionLeft>");
                        foundproperty = true;
                        break;
                    }
                }
                // perhaps the provided property is a generic property
                if (!foundproperty)
                {
                    foreach (GenericProperty p in GenericProperty.GetGenericProperties())
                    {
                        if (String.Compare(POV.Property, p.PropertyName, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            sb.Append("<ValueExpressionLeft>");
                            // sb.Append("<GenericProperty>$Target/Property[Type='myMP!" + mpClass.Name + "']/" + p.PropertyName + "$</GenericProperty>");
                            sb.Append("<GenericProperty>" + p.PropertyName + "</GenericProperty>");
                            sb.Append("</ValueExpressionLeft>");
                            foundproperty = true;
                        }
                    }
                }
                if (!foundproperty)
                {
                    WriteError(new ErrorRecord(new ObjectNotFoundException("property"), "Property Not Found", ErrorCategory.NotSpecified, filter));
                    return null;
                }

                // Now add the operator
                sb.Append("<Operator>" + POV.Operator + "</Operator>");
                // Finally, the value - no checking here, just add it
                sb.Append("<ValueExpressionRight><Value>");
                // TODO: HANDLE ENUMS
                sb.Append(POV.Value);
                sb.Append("</Value></ValueExpressionRight>");
                sb.Append("</SimpleExpression>");
                sb.Append("</Expression>");
            }
            if (POVs.Count > 1) { sb.Append("</Or>"); sb.Append("</Expression>"); }
            // JWT END OF EXPRESSION
            sb.Append("</Criteria>");
            WriteVerbose(sb.ToString());
            return sb.ToString();
        }

        // This doesn't completely work yet
        // TODO: handle generic property queries
        // I think it will look like:
        // PROPERTY OPERATOR VALUE
        // so, you could do:
        // DISPLAYNAME -EQ 'Boo ya!'
        // and you'll get the filter back
        // Also need to support Enumeration values, so user can provide a string rather than a guid for an enum
        // value
        public EnterpriseManagementObjectCriteria ConvertFilterToObjectCriteria(ManagementPackClass mpClass, string filter)
        {
            EnterpriseManagementObjectCriteria myCriteria = null;
            string filterString = null;
            // First attempt to use the simple constructor for the ObjectCriteria, just the filter and the class
            // First replace all the PowerShell operators
            // this will return a criteria if we have success
            try
            {
                WriteVerbose("Original Filter: " + filter);
                filterString = ConvertFilterToGenericCriteria(filter);
                filterString = FixUpPropertyNames(filterString, mpClass);
                WriteVerbose("Fixed Filter: " + filterString);
                myCriteria = new EnterpriseManagementObjectCriteria(filterString, mpClass);
                WriteVerbose("Using " + filterString + " as criteria");
                return myCriteria;
            }
            catch // This is non-catastrophic - it's our first attempt
            {
                WriteDebug("failed: " + filter);
            }

            try
            {
                filterString = ConvertFilterToCriteriaString(mpClass, filter, false);
                myCriteria = new EnterpriseManagementObjectCriteria(filterString, mpClass, _mg);
            }
            catch (InvalidCriteriaException e)
            {
                ThrowTerminatingError(new ErrorRecord(e, "Bad Filter", ErrorCategory.InvalidOperation, filterString));
            }
            catch (Exception e)
            {
                ThrowTerminatingError(new ErrorRecord(e, "Bad Filter", ErrorCategory.InvalidOperation, filter));
            }
            return myCriteria;
        }
        public ObjectProjectionCriteria ConvertFilterToProjectionCriteria(ManagementPackTypeProjection projection, string filter)
        {
            XmlDocument x = new XmlDocument();
            try
            {
                // if you can create an ObjectProjectionCriteria because you got some XML, groovy
                x.LoadXml(filter);
                // if we get to here, then we know we at least have some well formed XML
                // now try to create an ObjectProjectionCriteria
                WriteVerbose(filter);
                ObjectProjectionCriteria opc = new ObjectProjectionCriteria(filter, projection, _mg);
                return opc;
            }
            // don't do anything here, just keep going as an XML exception
            // means that we probably got a real filter rather than some XML Criteria
            catch (XmlException) {; }
            // OK, we got valid XML but a bad criteria, notify and bail
            catch (InvalidCriteriaException e)
            {
                ThrowTerminatingError(new ErrorRecord(e, "InvalidCriteria", ErrorCategory.InvalidData, filter));
            }

            string filterString = ConvertFilterToCriteriaString(projection.TargetType, filter, true);
            ObjectProjectionCriteria myCriteria = new ObjectProjectionCriteria(filterString, projection, _mg);
            return myCriteria;
        }

    }

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

    public sealed class ServiceManagerObjectHelper
    {
        // a helper class  to read a stream and adapt an EMO

        // We handle the retrival of binary types
        // if it's a binary property type, we'll return an array of
        // bytes
        private static Byte[] GetBytes(Stream s)
        {
            // Why SM binary fields don't have a valid length property is a mystery
            if (s == null) { return null; }
            byte[] buffer = new byte[4096];
            Collection<Byte[]> balist = new Collection<Byte[]>();
            int count;
            int totalLength = 0;
            while ((count = s.Read(buffer, 0, 4096)) != 0)
            {
                totalLength += count;
                byte[] tbyte = new byte[count];
                Array.Copy(buffer, tbyte, count);
                balist.Add(tbyte);
            }
            byte[] ReturnBytes = new byte[totalLength];
            int offset = 0;
            foreach (byte[] b in balist)
            {
                b.CopyTo(ReturnBytes, offset);
                offset += b.Length;
            }
            return ReturnBytes;
        }

        public static PSObject AdaptProjection(Cmdlet myCmdlet, EnterpriseManagementObjectProjection p, string projectionName)
        {
            myCmdlet.WriteVerbose("Adapting " + p);
            /*
             * We can't just wrap a type projection because it is Enumerable. This means that we would only see the
             * components of the projection in the output so we have to construct this artificial wrapper. It would be easier if
             * projections weren't Enumerable, which means that PowerShell wouldn't treat a projection as a collection, or if 
             * PowerShell understood that certain collections shouldn't be unspooled, but that's not how PowerShell works.
             * Neither of those two options are available, so we adapt the object and present a PSObject with all the component
             * parts.
             */
            PSObject o = new PSObject();
            o.Members.Add(new PSNoteProperty("__base", p));
            o.Members.Add(new PSScriptMethod("GetAsXml", ScriptBlock.Create("[xml]($this.__base.CreateNavigator().OuterXml)")));
            o.Members.Add(new PSNoteProperty("Object", AdaptManagementObject(myCmdlet, p.Object)));
            // Now promote all the properties on Object
            foreach (EnterpriseManagementSimpleObject so in p.Object.Values)
            {
                try
                {
                    o.Members.Add(new PSNoteProperty(so.Type.Name, so.Value));
                }
                catch
                {
                    myCmdlet.WriteWarning("could not promote: " + so.Type.Name);
                }
            }

            o.TypeNames[0] = String.Format(CultureInfo.CurrentCulture, "EnterpriseManagementObjectProjection#{0}", projectionName);
            o.TypeNames.Insert(1, "EnterpriseManagementObjectProjection");
            o.Members.Add(new PSNoteProperty("__ProjectionType", projectionName));

            foreach (KeyValuePair<ManagementPackRelationshipEndpoint, IComposableProjection> helper in p)
            {
                // EnterpriseManagementObject myEMO = (EnterpriseManagementObject)helper.Value.Object;
                myCmdlet.WriteVerbose("Adapting related objects: " + helper.Key.Name);
                String myName = helper.Key.Name;
                PSObject adaptedEMO = AdaptManagementObject(myCmdlet, helper.Value.Object);
                // If the MaxCardinality is greater than one, it's definitely a collection
                // so start out that way
                if (helper.Key.MaxCardinality > 1)
                {
                    // OK, this is a collection, so add the critter
                    // This is so much easier in PowerShell
                    if (o.Properties[myName] == null)
                    {
                        o.Members.Add(new PSNoteProperty(myName, new ArrayList()));
                    }
                    ((ArrayList)o.Properties[myName].Value).Add(adaptedEMO);
                }
                else
                {
                    try
                    {
                        o.Members.Add(new PSNoteProperty(helper.Key.Name, adaptedEMO));
                    }
                    catch (ExtendedTypeSystemException e)
                    {
                        myCmdlet.WriteVerbose("Readapting relationship object -> collection :" + e.Message);
                        // We should really only get this exception if we
                        // try to add a create a new property which already exists
                        Object currentPropertyValue = o.Properties[myName].Value;
                        ArrayList newValue = new ArrayList();
                        newValue.Add(currentPropertyValue);
                        newValue.Add(adaptedEMO);
                        o.Properties[myName].Value = newValue;
                        // TODO
                        // If this already exists, it should be converted to a collection
                    }
                }
            }
            return o;
        }

        // Adapt the EnterpriseManagementObject
        // We need to do this because the interest bits of the EMO (from our perspective)
        // are in the values collection, we promote them to NoteProperties so displaying 
        // the contents work better. This should really be done with a Type adapter, but
        // this is ok.
        public static PSObject AdaptManagementObject(Cmdlet myCmdlet, EnterpriseManagementObject managementObject)
        {
            PSObject PromotedObject = new PSObject(managementObject);
            PromotedObject.TypeNames.Insert(1, managementObject.GetType().FullName);
            PromotedObject.TypeNames[0] = String.Format(CultureInfo.CurrentCulture, "EnterpriseManagementObject#{0}", managementObject.GetLeastDerivedNonAbstractClass().Name);
            // loop through the properties and promote them into the PSObject we're going to return
            foreach (ManagementPackProperty p in managementObject.GetProperties())
            {
                try
                {
                    if (p.SystemType.ToString() == "System.IO.Stream")
                    {
                        if (managementObject[p].Value != null)
                        {
                            PSObject StreamObject = new PSObject(managementObject[p].Value);
                            Byte[] Data = GetBytes(managementObject[p].Value as Stream);
                            StreamObject.Members.Add(new PSNoteProperty("Data", Data));
                            PromotedObject.Members.Add(new PSNoteProperty(p.Name, StreamObject));
                        }
                        else
                        {
                            PromotedObject.Members.Add(new PSNoteProperty(p.Name, new Byte[0]));
                        }
                    }
                    else
                    {
                        PromotedObject.Members.Add(new PSNoteProperty(p.Name, managementObject[p].Value));
                    }
                }
                catch (ExtendedTypeSystemException ets)
                {
                    myCmdlet.WriteWarning(String.Format("The property '{0}' already exists, skipping.\nException: {1}", p.Name, ets.Message));
                }
                catch (Exception e)
                {
                    myCmdlet.WriteError(new ErrorRecord(e, "Property", ErrorCategory.NotSpecified, p.Name));
                }
            }

            PromotedObject.Members.Add(new PSNoteProperty("__InternalId", managementObject.Id));
            return PromotedObject;

        }

        // This overload is so we can call the adapter from the script layer 
        // where we may not have a cmdlet reference. In the case of errors, we'll 
        // just let it get thrown
        public static PSObject AdaptManagementObject(EnterpriseManagementObject managementObject)
        {
            PSObject PromotedObject = new PSObject(managementObject);
            PromotedObject.TypeNames.Insert(1, managementObject.GetType().FullName);
            PromotedObject.TypeNames[0] = String.Format(CultureInfo.CurrentCulture, "EnterpriseManagementObject#{0}", managementObject.GetLeastDerivedNonAbstractClass().Name);
            // loop through the properties and promote them into the PSObject we're going to return
            foreach (ManagementPackProperty p in managementObject.GetProperties())
            {
                try
                {
                    if (p.SystemType.ToString() == "System.IO.Stream")
                    {
                        if (managementObject[p].Value != null)
                        {
                            PSObject StreamObject = new PSObject(managementObject[p].Value);
                            Byte[] Data = GetBytes(managementObject[p].Value as Stream);
                            StreamObject.Members.Add(new PSNoteProperty("Data", Data));
                            PromotedObject.Members.Add(new PSNoteProperty(p.Name, StreamObject));
                        }
                        else
                        {
                            PromotedObject.Members.Add(new PSNoteProperty(p.Name, new Byte[0]));
                        }
                    }
                    else
                    {
                        PromotedObject.Members.Add(new PSNoteProperty(p.Name, managementObject[p].Value));
                    }
                }
                catch (ExtendedTypeSystemException ets)
                {
                    throw (new InvalidOperationException(String.Format("The property '{0}' already exists, skipping.\nException: {1}", p.Name, ets.Message)));
                }
                catch (Exception e)
                {
                    throw (e);
                }
            }
            PromotedObject.Members.Add(new PSNoteProperty("__InternalId", managementObject.Id));
            return PromotedObject;

        }
        private ServiceManagerObjectHelper() {; }
    }

    public enum Change { Property, Relationship, Instance };
    public enum ChangeType { Modify, Insert, Delete };
    public class PropertyChange
    {
        public Change WhatChanged;
        public ChangeType TypeOfChange;
        public string Name;
        public object OldValue;
        public object NewValue;
        public PropertyChange() {; }
        public PropertyChange(Change type, ChangeType operation, string name, object oldval, object newval)
        {
            WhatChanged = type;
            TypeOfChange = operation;
            Name = name;
            OldValue = oldval;
            NewValue = newval;
        }
    }
    public class ObjectChange
    {
        public List<PropertyChange> Changes;
        public DateTime LastModified;
        public string UserName;
        public string Connector;
        public ObjectChange()
        {
            Changes = new List<PropertyChange>();
        }
    }
    public class SCSMHistory
    {
        public EnterpriseManagementObject Instance;
        public List<ObjectChange> History;
        private List<EnterpriseManagementObjectHistoryTransaction> __HistoryData;
        public List<EnterpriseManagementObjectHistoryTransaction> get_RawHistoryData()
        {
            return this.__HistoryData;
        }
        private SCSMHistory()
        {
            History = new List<ObjectChange>();
            __HistoryData = new List<EnterpriseManagementObjectHistoryTransaction>();
        }
        public SCSMHistory(EnterpriseManagementObject emo)
        {
            History = new List<ObjectChange>();
            __HistoryData = new List<EnterpriseManagementObjectHistoryTransaction>();
            Instance = emo;
            foreach (EnterpriseManagementObjectHistoryTransaction ht in emo.ManagementGroup.EntityObjects.GetObjectHistoryTransactions(emo))
            {
                __HistoryData.Add(ht);
                ObjectChange pc = new ObjectChange();
                pc.LastModified = ht.DateOccurred.ToLocalTime();
                pc.UserName = ht.UserName;
                pc.Connector = ht.ConnectorDisplayName;
                bool addToHistory = false;
                foreach (KeyValuePair<Guid, EnterpriseManagementObjectHistory> h in ht.ObjectHistory)
                {
                    foreach (EnterpriseManagementObjectClassHistory ch in h.Value.ClassHistory)
                    {
                        foreach (KeyValuePair<ManagementPackProperty, Pair<EnterpriseManagementSimpleObject, EnterpriseManagementSimpleObject>> hpc in ch.PropertyChanges)
                        {
                            addToHistory = true;
                            pc.Changes.Add(new PropertyChange(Change.Property, ChangeType.Modify, hpc.Key.DisplayName, hpc.Value.First, hpc.Value.Second));
                        }
                    }
                    foreach (EnterpriseManagementObjectRelationshipHistory rh in h.Value.RelationshipHistory)
                    {
                        addToHistory = true;
                        ManagementPackRelationship mpr = emo.ManagementGroup.EntityTypes.GetRelationshipClass(rh.ManagementPackRelationshipTypeId);
                        pc.Changes.Add(new PropertyChange(Change.Relationship, ChangeType.Modify, mpr.DisplayName, rh.SourceObjectId, rh.TargetObjectId));
                    }

                }
                if (addToHistory) { History.Add(pc); }
            }
        }

    }
}
