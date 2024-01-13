using System;
using System.Collections.ObjectModel;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using System.Reflection;

namespace xServiceManager.Module
{

    public static class CriteriaHelper<T>
    {
        //
        // Usage pattern:
        // ManagementPackClassCriteria mpcc = CriteriaHelper<ManagementPackClassCriteria>.CreateGenericCriteria("Name -like '*Entity'");
        // EnterpriseManagementRelationshipObjectGenericCriteria foo = CriteriaHelper<EnterpriseManagementRelationshipObjectGenericCriteria>.CreateCriteria("TargetObjectDisplayName -like 'Custom%'");
        // 

        // If the Criteria schema changes this will break
        public const string CriteriaFormatString = "<Criteria xmlns='http://Microsoft.EnterpriseManagement.Core.Criteria/'><Expression><SimpleExpression><ValueExpressionLeft><GenericProperty>{0}</GenericProperty></ValueExpressionLeft><Operator>{1}</Operator><ValueExpressionRight><Value>{2}</Value></ValueExpressionRight></SimpleExpression></Expression></Criteria>";
        public static string critXml;

        public static string getGenericXml(Type t, string filter)
        {
            String message = "unknown error";
            try
            {
                PropertyOperatorValue POV = new PropertyOperatorValue(filter);

                BindingFlags flags = BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static;
                string method = "GetValidPropertyNames";
                if (string.Compare(t.Name, "EnterpriseManagementObjectCriteria", true) == 0) { method = "GetSpecialPropertyNames"; }
                ReadOnlyCollection<string> propertyNames = (ReadOnlyCollection<String>)t.InvokeMember(method, flags, null, t, null);
                string[] names = new string[propertyNames.Count];
                propertyNames.CopyTo(names, 0);
                message = String.Format("Property '{0}' not found, allowed values: {1}", POV.Property, String.Join(", ", names));
                foreach (string pn in propertyNames)
                {
                    if (String.Compare(pn, POV.Property, true) == 0)
                    {
                        return String.Format(CriteriaFormatString, pn, POV.Operator, POV.Value);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            throw new ObjectNotFoundException(message);
        }
        // Usefull for Generic Criteria and ManagementPack
        public static T CreateGenericCriteria(string filter)
        {
            critXml = getGenericXml(typeof(T), filter);
            if (critXml == null) { return default(T); }
            return (T)Activator.CreateInstance(typeof(T), critXml);
        }
        public static T CreateGenericCriteria(string filter, ManagementPackTypeProjection p, EnterpriseManagementGroup emg)
        {
            critXml = getGenericXml(typeof(T), filter);
            if (critXml == null) { return default(T); }
            return (T)Activator.CreateInstance(typeof(T), critXml, p, emg);
        }
        public static T CreateGenericCriteria(string filter, ManagementPackClass c, EnterpriseManagementGroup emg)
        {
            critXml = getGenericXml(typeof(T), filter);
            if (critXml == null) { return default(T); }
            return (T)Activator.CreateInstance(typeof(T), critXml, c, emg);
        }
    }
}
