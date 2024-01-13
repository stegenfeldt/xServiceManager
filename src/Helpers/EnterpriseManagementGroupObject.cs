using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    public class EnterpriseManagementGroupObject
    {
        public EnterpriseManagementObject __EnterpriseManagementObject;
        public ManagementPackClass __Class;
        public String Description;
        public String DisplayName;
        public Guid Id;
        public String Name;
        public ManagementPack ManagementPack;
        public List<XmlNode> MembershipRules;
        public List<EnterpriseManagementObject> IncludeList;
        public List<EnterpriseManagementObject> ExcludeList;
        public List<EnterpriseManagementObject> Members;
        public string Configuration;
        public EnterpriseManagementGroupObject(EnterpriseManagementObject emo)
        {
            __EnterpriseManagementObject = emo;
            __Class = emo.GetLeastDerivedNonAbstractClass();
            Id = emo.Id;
            Description = emo.GetLeastDerivedNonAbstractClass().Description;
            DisplayName = emo.GetLeastDerivedNonAbstractClass().DisplayName;
            Name = __Class.Name;
            ManagementPack = __Class.GetManagementPack();
            ManagementPackDiscovery d = ManagementPack.GetDiscovery(Name + ".Discovery");
            Configuration = d.DataSource.Configuration;
            XmlDocument xmld = new XmlDocument();
            xmld.LoadXml(d.CreateNavigator().OuterXml);
            MembershipRules = new List<XmlNode>();
            Hashtable includeHT = new Hashtable();
            Hashtable excludeHT = new Hashtable();
            XmlNodeList l;
            foreach (XmlNode node in xmld.SelectNodes("Discovery/DataSource/MembershipRules/MembershipRule"))
            {
                MembershipRules.Add(node);
                l = node.SelectNodes("IncludeList/MonitoringObjectId");
                if (l.Count > 0)
                {
                    foreach (XmlNode MO in l)
                    {
                        string value = MO.FirstChild.Value;
                        if (value != string.Empty && !includeHT.ContainsKey(value)) { includeHT.Add(value, 1); }
                    }
                }
                l = node.SelectNodes("ExcludeList/MonitoringObjectId");
                if (l.Count > 0)
                {
                    foreach (XmlNode MO in l)
                    {
                        string value = MO.FirstChild.Value;
                        if (value != string.Empty && !excludeHT.ContainsKey(value)) { excludeHT.Add(value, 1); }
                    }
                }

            }
            IncludeList = new List<EnterpriseManagementObject>();

            foreach (string s in includeHT.Keys)
            {
                IncludeList.Add(emo.ManagementGroup.EntityObjects.GetObject<EnterpriseManagementObject>(new Guid(s), ObjectQueryOptions.Default));
            }
            ExcludeList = new List<EnterpriseManagementObject>();
            foreach (string s in excludeHT.Keys)
            {
                ExcludeList.Add(emo.ManagementGroup.EntityObjects.GetObject<EnterpriseManagementObject>(new Guid(s), ObjectQueryOptions.Default));
            }

            Members = new List<EnterpriseManagementObject>();
            foreach (EnterpriseManagementObject remo in emo.ManagementGroup.EntityObjects.GetRelatedObjects<EnterpriseManagementObject>(emo.Id, TraversalDepth.OneLevel, ObjectQueryOptions.Default))
            {
                Members.Add(remo);
            }
        }
    }
}
