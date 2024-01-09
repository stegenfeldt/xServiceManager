using System;
using System.Text;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Configuration.IO;

namespace xServiceManager.Module
{
    /// <summary>
    /// This cmdlet creates a new group
    /// TODO: Support dynamic members
    /// </summary>
    [Cmdlet(VerbsCommon.New, "xSCSMGroup", SupportsShouldProcess = true)]
    public class NewSCSMGroupCommand : ObjectCmdletHelper
    {
        #region parameters
        private string _managementPackName = null;
        [Parameter(Position = 1, ParameterSetName = "MPName")]
        public string ManagementPackName
        {
            get { return _managementPackName; }
            set { _managementPackName = value; }
        }
        private string _managementPackFriendlyName = null;
        [Parameter(ParameterSetName = "MPName")]
        public string ManagementPackFriendlyName
        {
            get { return _managementPackFriendlyName; }
            set { _managementPackFriendlyName = value; }
        }

        private EnterpriseManagementObject[] _include = null;
        [Parameter(Position = 2, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public EnterpriseManagementObject[] Include
        {
            get { return _include; }
            set { _include = value; }
        }
        private EnterpriseManagementObject[] _exclude = null;
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public EnterpriseManagementObject[] Exclude
        {
            get { return _exclude; }
            set { _exclude = value; }
        }
        private EnterpriseManagementGroupObject[] _subGroup = null;
        [Parameter]
        public EnterpriseManagementGroupObject[] SubGroup
        {
            get { return _subGroup; }
            set { _subGroup = value; }
        }
        private string _name = null;
        [Parameter(Position = 0)]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _description;
        [Parameter(ParameterSetName = "MPName")]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private ManagementPack _managementPack;
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "FromMP")]
        public ManagementPack ManagementPack
        {
            get { return _managementPack; }
            set { _managementPack = value; }
        }

        private SwitchParameter _passThru;
        [Parameter]
        public SwitchParameter PassThru
        {
            get { return _passThru; }
            set { _passThru = value; }
        }

        private SwitchParameter _import;
        [Parameter]
        public SwitchParameter Import
        {
            get { return _import; }
            set { _import = value; }
        }
        public enum __GroupType { InstanceGroup, WorkItemGroup };
        private __GroupType _groupType = __GroupType.InstanceGroup;
        [Parameter]
        public __GroupType GroupType
        {
            get { return _groupType; }
            set { _groupType = value; }
        }
        private SwitchParameter _force;
        [Parameter]
        public SwitchParameter Force
        {
            get { return _force; }
            set { _force = value; }
        }
        #endregion
        // private String myGroupType;
        // private Guid InstanceGroupRelationship;
        private ManagementPackModuleType GroupPopulatorModuleType;

        private Dictionary<ManagementPackClass, List<Guid>> excludedList;
        private Dictionary<ManagementPackClass, List<Guid>> includedList;
        private List<EnterpriseManagementGroupObject> subGroupCollection;

        private string myGroupName;
        private string myDiscoveryName;

        private string myRelationshipName;
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (ParameterSetName == "MPName")
            {
                if (ManagementPackName == null)
                {
                    ManagementPackName = "MP_" + Guid.NewGuid().ToString().Replace("-", "");
                }
                if (ManagementPackFriendlyName == null)
                {
                    ManagementPackFriendlyName = ManagementPackName;
                }
            }
            if (Name == null)
            {
                Name = "Group_" + Guid.NewGuid().ToString().Replace("-", "");
            }
            includedList = new Dictionary<ManagementPackClass, List<Guid>>();
            excludedList = new Dictionary<ManagementPackClass, List<Guid>>();
            // Handle the case where you get something on the command line
            if (Exclude != null)
            {
                foreach (EnterpriseManagementObject emo in Exclude)
                {
                    ManagementPackClass c = emo.GetLeastDerivedNonAbstractClass();
                    if (!excludedList.ContainsKey(c))
                    {
                        List<Guid> l = new List<Guid>();
                        excludedList.Add(c, l);
                    }
                    excludedList[c].Add(emo.Id);
                }
            }
            if (Include != null)
            {
                foreach (EnterpriseManagementObject emo in Include)
                {
                    ManagementPackClass c = emo.GetLeastDerivedNonAbstractClass();
                    if (!includedList.ContainsKey(c))
                    {
                        List<Guid> l = new List<Guid>();
                        includedList.Add(c, l);
                    }
                    WriteVerbose("In BeginProcessing, adding " + emo.Id.ToString() + " to include list");
                    includedList[c].Add(emo.Id);
                }
            }
            if (SubGroup != null)
            {
                foreach (EnterpriseManagementGroupObject go in SubGroup)
                {
                    if (!go.ManagementPack.Sealed)
                    {
                        WriteError(new ErrorRecord(new InvalidOperationException(go.ManagementPack.Name + " is not sealed"), "Error adding SubGroup", ErrorCategory.InvalidOperation, go));
                    }
                    else
                    {
                        if (subGroupCollection == null)
                        {
                            subGroupCollection = new List<EnterpriseManagementGroupObject>();
                        }
                        subGroupCollection.Add(go);
                    }
                }
            }
        }
        protected override void ProcessRecord()
        {
            myGroupName = "Group_" + Guid.NewGuid().ToString().Replace("-", "");
            myDiscoveryName = myGroupName + ".Discovery";
            myRelationshipName = myGroupName + "_Contains_WorkItem";
            if (Include != null)
            {
                foreach (EnterpriseManagementObject o in Include)
                {
                    ManagementPackClass c = o.GetLeastDerivedNonAbstractClass();
                    if (!includedList.ContainsKey(c))
                    {
                        includedList.Add(c, new List<Guid>());
                    }
                    // This test is needed because we may have added to the include list in
                    // BeginProcessing
                    if (!includedList[c].Contains(o.Id))
                    {
                        WriteVerbose("ProcessRecord, adding " + o.Id.ToString() + " to include list");
                        includedList[c].Add(o.Id);
                    }
                }
            }
        }
        protected override void EndProcessing()
        {
            // We know that we need these at least.
            // for now, we'll just build a new MP and ignore the one we're handed
            ManagementPack mp;
            if (ParameterSetName == "FromMP")
            {
                mp = ManagementPack;
            }
            else
            {
                mp = new ManagementPack(ManagementPackName, ManagementPackFriendlyName, new Version(1, 0, 0, 0), _mg);
            }
            // Now collect MPs which we'll use in our references section
            #region References
            ManagementPack SysMP = _mg.ManagementPacks.GetManagementPack(SystemManagementPack.System);
            ManagementPack scl = _mg.ManagementPacks.GetManagementPack(SystemManagementPack.SystemCenter);
#if ( _SERVICEMANAGER_R2_ ) // R2 allows for version to be null
            ManagementPack IGL = _mg.ManagementPacks.GetManagementPack("Microsoft.SystemCenter.InstanceGroup.Library", SysMP.KeyToken, null);
            ManagementPack WI = _mg.ManagementPacks.GetManagementPack("System.WorkItem.Library", SysMP.KeyToken, null);
            ManagementPack SWAL = _mg.ManagementPacks.GetManagementPack("System.WorkItem.Activity.Library", SysMP.KeyToken, null);
            ManagementPack SNL = _mg.ManagementPacks.GetManagementPack("System.Notifications.Library", SysMP.KeyToken, null);
#else 
            ManagementPack IGL = _mg.ManagementPacks.GetManagementPack("Microsoft.SystemCenter.InstanceGroup.Library", SysMP.KeyToken, SysMP.Version);
            ManagementPack WI = _mg.ManagementPacks.GetManagementPack("System.WorkItem.Library", SysMP.KeyToken, SysMP.Version);
            ManagementPack SWAL = _mg.ManagementPacks.GetManagementPack("System.WorkItem.Activity.Library", SysMP.KeyToken, SysMP.Version);
            ManagementPack SNL = _mg.ManagementPacks.GetManagementPack("System.Notifications.Library", SysMP.KeyToken, SysMP.Version);
#endif
            ManagementPack Windows = _mg.ManagementPacks.GetManagementPack(SystemManagementPack.Windows);
            List<ManagementPack> mplist = new List<ManagementPack>();
            mplist.Add(SysMP);
            mplist.Add(scl);
            mplist.Add(IGL);
            mplist.Add(WI);
            mplist.Add(Windows);
            mplist.Add(SWAL);
            mplist.Add(SNL);
            foreach (ManagementPack m in mplist)
            {
                if (!mp.References.ContainsValue(m))
                {
                    try { mp.References.Add(m.Name.Replace('.', '_'), m); } catch { ; }
                }
            }

            GroupPopulatorModuleType = _mg.Monitoring.GetModuleType("Microsoft.SystemCenter.GroupPopulator", scl);
            #endregion
            ManagementPackClass baseClass = _mg.EntityTypes.GetClass("Microsoft.SystemCenter.ConfigItemGroup", IGL);
            if (GroupType == __GroupType.WorkItemGroup)
            {
                baseClass = _mg.EntityTypes.GetClass("System.WorkItemGroup", WI);
            }

            c = GetNewClass(mp, myGroupName, baseClass);

            ManagementPackDiscovery d = GetNewDiscovery(mp, myDiscoveryName, c);
            ManagementPackRelationship relationshipForWorkItemGroup = null;
            if (GroupType == __GroupType.WorkItemGroup)
            {
                // We need to create a new relationship type based on the object that we got.
                relationshipForWorkItemGroup = AddRelationship(mp, baseClass);
            }

            AddDiscoveryRelationship(d, GroupType, relationshipForWorkItemGroup, _mg);

            AddLanguagePack(mp, c, d, Name, Description);

            #region Configuration
            string alias = String.Empty;
            StringBuilder sb = new StringBuilder();
            sb.Append("<RuleId>$MPElement$</RuleId>");
            sb.Append(String.Format("<GroupInstanceId>$MPElement[Name=\"{0}\"]$</GroupInstanceId>", myGroupName));
            sb.Append("<MembershipRules>");
            List<ManagementPackClass> IncludedAndExcluded = new List<ManagementPackClass>();
            foreach (ManagementPackClass key in includedList.Keys) { if (!IncludedAndExcluded.Contains(key)) { WriteVerbose("Adding key for " + key.Name); IncludedAndExcluded.Add(key); } }
            foreach (ManagementPackClass key in excludedList.Keys) { if (!IncludedAndExcluded.Contains(key)) { WriteVerbose("Adding key for " + key.Name); IncludedAndExcluded.Add(key); } }
            foreach (ManagementPackClass lc in IncludedAndExcluded)
            {
                bool found = false;
                sb.Append("<MembershipRule>");
                foreach (KeyValuePair<string, ManagementPackReference> kv in mp.References)
                {
                    if (lc.GetManagementPack() == kv.Value.GetManagementPack())
                    {
                        WriteVerbose("found a reference for " + lc.Name + " in " + lc.GetManagementPack().Name);
                        alias = kv.Key;
                        found = true;
                    }
                }
                if (!found)
                {
                    WriteVerbose("Adding reference for " + lc.GetManagementPack().Name);
                    // Add a reference!
                    alias = lc.GetManagementPack().Name.Replace('.', '_');
                    try { mp.References.Add(alias, lc.GetManagementPack()); } catch { ; }
                }
                if (alias == String.Empty) { throw new InvalidOperationException("could not find alias"); }
                string monitorString = String.Format("<MonitoringClass>$MPElement[Name=\"{0}!{1}\"]$</MonitoringClass>", alias, lc.Name);
                sb.Append(monitorString);
                // JWT
                sb.Append(String.Format("<RelationshipClass>$MPElement[Name=\"{0}!Microsoft.SystemCenter.InstanceGroupContainsEntities\"]$</RelationshipClass>", IGL.Name.Replace('.', '_')));
                if (includedList.ContainsKey(lc))
                {
                    sb.Append("<IncludeList>");
                    foreach (Guid g in includedList[lc])
                    {
                        sb.Append(String.Format("<MonitoringObjectId>{0}</MonitoringObjectId>", g));
                    }
                    sb.Append("</IncludeList>");
                }
                if (excludedList.ContainsKey(lc))
                {
                    sb.Append("<ExcludeList>");
                    foreach (Guid g in excludedList[lc])
                    {
                        sb.Append(String.Format("<MonitoringObjectId>{0}</MonitoringObjectId>", g));
                    }
                    sb.Append("</ExcludeList>");
                }
                sb.Append("</MembershipRule>");
            }
            sb.Append("</MembershipRules>");
            WriteVerbose(sb.ToString());
            d.DataSource.Configuration = sb.ToString();
            #endregion
            // Verification errors are fatal
            try
            {
                mp.AcceptChanges(ManagementPackVerificationTypes.XSDVerification);
                mp.Verify();
            }
            catch (Exception e)
            {
                ThrowTerminatingError(new ErrorRecord(e, "MPVerify", ErrorCategory.InvalidResult, mp));
            }
            // Import errors are fatal
            if (Import)
            {
                try
                {
                    if (ShouldProcess("Import " + myGroupName))
                    {
                        _mg.ManagementPacks.ImportManagementPack(mp);
                    }
                }
                catch (Exception e)
                {
                    if (Force)
                    {
                        WriteObject(mp);
                    }
                    ThrowTerminatingError(new ErrorRecord(e, "ImportGroupMP", ErrorCategory.InvalidResult, mp));
                }
            }
            if (PassThru)
            {
                if (ParameterSetName == "FromMP")
                {
                    WriteObject(c);
                }
                else
                {
                    WriteObject(mp);
                }
            }
        }

        private ManagementPackClass GetNewClass(ManagementPack m, string name, ManagementPackClass baseClass)
        {
            ManagementPackClass c = new ManagementPackClass(m, name, ManagementPackAccessibility.Public);
            c.Base = baseClass;
            c.Abstract = false;
            c.Hosted = false;
            c.Singleton = true;
            c.Extension = false;
            return c;
        }
        private ManagementPackDiscovery GetNewDiscovery(ManagementPack m, string name, ManagementPackClass target)
        {
            ManagementPackDiscovery d = new ManagementPackDiscovery(m, myDiscoveryName);
            d.Category = ManagementPackCategoryType.Discovery;
            d.Enabled = ManagementPackMonitoringLevel.@true;
            d.Target = target;
            d.ConfirmDelivery = false;
            d.Remotable = true;
            d.Priority = ManagementPackWorkflowPriority.Normal;
            return d;
        }

        private void AddDiscoveryRelationship(ManagementPackDiscovery d, __GroupType groupType, ManagementPackRelationship r, EnterpriseManagementGroup emg)
        {
            ManagementPackDiscoveryRelationship dr = new ManagementPackDiscoveryRelationship();
            if (groupType == __GroupType.InstanceGroup)
            {
                ManagementPack mpIGL = SMHelpers.GetManagementPack(ManagementPacks.Microsoft_SystemCenter_InstanceGroup_Library, emg);
                dr.TypeID = _mg.EntityTypes.GetRelationshipClass("Microsoft.SystemCenter.InstanceGroupContainsEntities", mpIGL);
            }
            else
            {
                dr.TypeID = r;
            }
            d.DiscoveryRelationshipCollection.Add(dr);
            ManagementPackDataSourceModule dsm = new ManagementPackDataSourceModule(d, "GroupPopulationDataSource");
            dsm.TypeID = (ManagementPackDataSourceModuleType)GroupPopulatorModuleType;
            d.DataSource = dsm;
            return;
        }
        private void AddLanguagePack(ManagementPack m, ManagementPackClass c, ManagementPackDiscovery d, string name, string classDescription)
        {
            ManagementPackLanguagePack lp = new ManagementPackLanguagePack(m, "ENU");
            lp.IsDefault = true;
            ManagementPackDisplayString ds1 = new ManagementPackDisplayString(c, "ENU");
            ds1.Name = name;
            ds1.Description = classDescription;
            ManagementPackDisplayString ds2 = new ManagementPackDisplayString(d, "ENU");
            ds2.Name = name + "_Discovery";
            ds2.Description = "Discovery for Group " + name;
        }
        private ManagementPackRelationship AddRelationship(ManagementPack m, ManagementPackClass targetClass)
        {
            ManagementPackRelationship r = new ManagementPackRelationship(m, myRelationshipName, ManagementPackAccessibility.Public);
            r.Accessibility = ManagementPackAccessibility.Public;
            r.Abstract = false;
            r.Base = SMHelpers.GetManagementPackRelationship(RelationshipTypes.Microsoft_SystemCenter_InstanceGroupContainsEntities, SMHelpers.GetManagementPack(ManagementPacks.Microsoft_SystemCenter_InstanceGroup_Library, _mg), _mg);
            ManagementPackRelationshipEndpoint source = new ManagementPackRelationshipEndpoint(r, "ContainedByGroup");
            source.MinCardinality = 0;
            source.MaxCardinality = Int32.MaxValue;
            source.Type = c;
            r.Source = source;
            ManagementPackRelationshipEndpoint target = new ManagementPackRelationshipEndpoint(r, "GroupContains");
            target.MinCardinality = 0;
            target.MaxCardinality = Int32.MaxValue;
            target.Type = targetClass;
            r.Target = target;
            return r;
        }

        ManagementPackClass c;
    }

}