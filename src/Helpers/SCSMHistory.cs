using System;
using System.Collections.Generic;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
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
