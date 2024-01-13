using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
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
}
