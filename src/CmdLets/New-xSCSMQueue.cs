using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Configuration.IO;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    /// <summary>
    /// Create a new queue
    /// Usage Pattern is:
    /// "Status -eq 'Active'" | new-scqueue -class (get-scsmclass workitem.incident$) -mp (get-scsmmanagementpack default)
    /// "DisplayName -eq 'foo'","DisplayName -eq 'bar'" | new-scqueue -class (get-scsmclass workitem.incident$) -mp (get-scsmmanagementpack default)
    /// etc
    /// which will create
    /// </summary>
    [Cmdlet(VerbsCommon.New, "xSCSMQueue", SupportsShouldProcess = true)]
    public class NewSCQueueCommand : ObjectCmdletHelper
    {
        private string _name;
        [Parameter(Position = 0)]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private string _description;
        [Parameter]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        private ManagementPack _managementPack;
        [Parameter(Position = 1, ParameterSetName = "MP")]
        [Alias("MP")]
        public ManagementPack ManagementPack
        {
            get { return _managementPack; }
            set { _managementPack = value; }
        }
        private string _managementPackName;
        [Parameter(Position = 1, ParameterSetName = "MPName", Mandatory = true)]
        public string ManagementPackName
        {
            get { return _managementPackName; }
            set { _managementPackName = value; }
        }
        private string _managementPackFriendlyName;
        [Parameter(ParameterSetName = "MPName")]
        public string ManagementPackFriendlyName
        {
            get { return _managementPackFriendlyName; }
            set { _managementPackFriendlyName = value; }
        }
        private ManagementPackClass _class;
        [Parameter(Position = 2, Mandatory = true)]
        public ManagementPackClass Class
        {
            get { return _class; }
            set { _class = value; }
        }

        private string[] _filter;
        [Parameter(Position = 3, Mandatory = true, ValueFromPipeline = true)]
        public string[] Filter
        {
            get { return _filter; }
            set { _filter = value; }
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
        // if force, then output the MP even if not verified
        private SwitchParameter _force;
        [Parameter]
        public SwitchParameter Force
        {
            get { return _force; }
            set { _force = value; }
        }

        private string FilterToDiscoveryCriteria(string filter, ManagementPackClass c)
        {
            Regex r = new Regex(" or | -or ", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (r.Match(filter).Success)
            {
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("OR is not allowed, only '-AND'"), "Bad Filter", ErrorCategory.InvalidOperation, filter));
            }
            ReadOnlyCollection<string> GenericProperties = EnterpriseManagementObjectCriteria.GetSpecialPropertyNames();
            CreatableEnterpriseManagementObject cemo = new CreatableEnterpriseManagementObject(c.ManagementGroup, c);
            ReadOnlyCollection<ManagementPackProperty> propertyList = (ReadOnlyCollection<ManagementPackProperty>)cemo.GetProperties();
            List<string> pNamelist = new List<string>();


            r = new Regex(" AND | -AND ", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            string[] subfilters = r.Split(filter);
            StringBuilder sb = new StringBuilder();
            sb.Append("<Expression>\r\n");
            bool multipleExpressions = false;
            if (subfilters.Length > 1)
            {
                multipleExpressions = true;
                sb.Append(" <And>\r\n");
            }
            foreach (string subfilter in subfilters)
            {
                bool found = false;
                ManagementPackProperty currentProperty = null;
                if (multipleExpressions)
                {
                    sb.Append("  <Expression>\r\n");
                }
                string sub = subfilter.Trim();
                PropertyOperatorValue POV = new PropertyOperatorValue(sub);
                sb.Append("   <SimpleExpression>\r\n");
                sb.Append("    <ValueExpression>\r\n");
                foreach (string s in GenericProperties)
                {
                    if (string.Compare(s, POV.Property, true) == 0)
                    {
                        found = true;
                        sb.Append("     <GenericProperty>" + s + "</GenericProperty>\r\n");
                    }
                }
                foreach (ManagementPackProperty p in propertyList)
                {
                    if (!found && string.Compare(p.Name, POV.Property, true) == 0)
                    {
                        // sb.Append("     <Property>" + p.Id + "</Property>\r\n");
                        sb.Append(String.Format("     <Property>$Context/Property[Type='{0}!{1}']/{2}$</Property>\r\n", c.GetManagementPack().Name.Replace('.', '_'), c.Name, p.Name));
                        currentProperty = p;
                    }
                }
                sb.Append("    </ValueExpression>\r\n");
                sb.Append("    <Operator>" + POV.Operator + "</Operator>\r\n");
                sb.Append("    <ValueExpression>\r\n");
                if (currentProperty != null && currentProperty.SystemType == typeof(Enum))
                {
                    ManagementPackElementReference<ManagementPackEnumeration> mpe = currentProperty.EnumType;
                    ManagementPackEnumeration e = SMHelpers.GetEnum(POV.Value, mpe.GetElement());
                    sb.Append("     <Value>{" + e.Id.ToString() + "}</Value>\r\n");
                }
                else
                {
                    sb.Append("     <Value>" + POV.Value + "</Value>\r\n");
                }
                sb.Append("    </ValueExpression>\r\n");
                sb.Append("   </SimpleExpression>\r\n");
                if (multipleExpressions)
                {
                    sb.Append("  </Expression>\r\n");
                }
            }
            if (multipleExpressions)
            {
                sb.Append(" </And>\r\n");
            }
            sb.Append("</Expression>\r\n");

            return sb.ToString();
        }
        private string myQueueName;
        private string myRelationshipName;
        private ManagementPackModuleType GroupPopulatorModuleType;
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (Name == null)
            {
                Name = "WorkItemGroup." + Guid.NewGuid().ToString().Replace("-", "");
            }
            if (ParameterSetName == "MPName" && ManagementPackFriendlyName == null)
            {
                ManagementPackFriendlyName = ManagementPackName;
            }
            myQueueName = "WorkItemGroup." + Guid.NewGuid().ToString().Replace("-", "");
            myDiscoveryName = myQueueName + ".Discovery";
            myRelationshipName = myQueueName + "_Contains_" + Class.Name;
        }
        private string convertedFilter;
        protected override void ProcessRecord()
        {
            foreach (string f in Filter)
            {
                convertedFilter = FilterToDiscoveryCriteria(f, Class);
                WriteVerbose(convertedFilter);
            }
        }
        private string myDiscoveryName;
        protected override void EndProcessing()
        {
            ManagementPack mp;

            if (ParameterSetName == "MP")
            {
                mp = ManagementPack;
            }
            else
            {
                mp = new ManagementPack(ManagementPackName, ManagementPackFriendlyName, new Version(1, 0, 0, 0), _mg);
            }

            #region References

            // Now collect MPs which we'll use in our references section
            ManagementPack SysMP = _mg.ManagementPacks.GetManagementPack(SystemManagementPack.System);
            ManagementPack scl = _mg.ManagementPacks.GetManagementPack(SystemManagementPack.SystemCenter);
            ManagementPack Windows = _mg.ManagementPacks.GetManagementPack(SystemManagementPack.Windows);
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
            ManagementPack classMP = Class.GetManagementPack();
            List<ManagementPack> mplist = new List<ManagementPack>();
            mplist.Add(SysMP);
            mplist.Add(scl);
            mplist.Add(IGL);
            mplist.Add(WI);
            mplist.Add(Windows);
            mplist.Add(SWAL);
            mplist.Add(SNL);
            mplist.Add(classMP);
            foreach (ManagementPack m in mplist)
            {
                if (!mp.References.ContainsValue(m))
                {
                    try { mp.References.Add(m.Name.Replace('.', '_'), m); } catch {; }
                }
            }

            #endregion

            GroupPopulatorModuleType = _mg.Monitoring.GetModuleType("Microsoft.SystemCenter.GroupPopulator", scl);

            ManagementPackClass classWorkItemGroup = _mg.EntityTypes.GetClass("System.WorkItemGroup", WI);

            ManagementPackClass c = GetNewClass(mp, myQueueName, classWorkItemGroup);

            ManagementPackDiscovery d = GetNewDiscovery(mp, myDiscoveryName, c);

            ManagementPackRelationship relationshipForWorkItemGroup = AddRelationship(mp, Class, c);

            AddDiscoveryRelationship(d, relationshipForWorkItemGroup);

            AddLanguagePack(mp, c, d, Name, Description);

            #region Configuration

            string alias = String.Empty;
            StringBuilder sb = new StringBuilder();
            sb.Append("<RuleId>$MPElement$</RuleId>\r\n");
            sb.Append(String.Format("<GroupInstanceId>$MPElement[Name=\"{0}\"]$</GroupInstanceId>\r\n", myQueueName));
            sb.Append("<MembershipRules>\r\n");
            sb.Append("<MembershipRule>\r\n");
            sb.Append(String.Format("<MonitoringClass>$MPElement[Name=\"{0}!{1}\"]$</MonitoringClass>\r\n", Class.GetManagementPack().Name.Replace('.', '_'), Class.Name));
            sb.Append(String.Format("<RelationshipClass>$MPElement[Name=\"{0}\"]$</RelationshipClass>\r\n", myRelationshipName));


            sb.Append(convertedFilter);
            sb.Append("</MembershipRule>\r\n");
            sb.Append("</MembershipRules>\r\n");
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
                    if (ShouldProcess("Import " + myQueueName))
                    {
                        _mg.ManagementPacks.ImportManagementPack(mp);
                    }
                }
                catch (Exception e)
                {
                    ThrowTerminatingError(new ErrorRecord(e, "ImportGroupMP", ErrorCategory.InvalidResult, mp));
                }
            }
            if (PassThru)
            {
                if (ParameterSetName == "MP")
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
        private void AddDiscoveryRelationship(ManagementPackDiscovery d, ManagementPackRelationship r)
        {
            ManagementPackDiscoveryRelationship dr = new ManagementPackDiscoveryRelationship();
            dr.TypeID = r;
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
        private ManagementPackRelationship AddRelationship(ManagementPack m, ManagementPackClass targetClass, ManagementPackClass sourceClass)
        {
            ManagementPackRelationship r = new ManagementPackRelationship(m, myRelationshipName, ManagementPackAccessibility.Public);
            r.Accessibility = ManagementPackAccessibility.Public;
            r.Abstract = false;
            r.Base = SMHelpers.GetManagementPackRelationship(RelationshipTypes.System_WorkItemGroupContainsWorkItems, SMHelpers.GetManagementPack(ManagementPacks.System_WorkItem_Library, _mg), _mg);
            ManagementPackRelationshipEndpoint source = new ManagementPackRelationshipEndpoint(r, "ContainedByGroup");
            source.MinCardinality = 0;
            source.MaxCardinality = Int32.MaxValue;
            source.Type = sourceClass;
            r.Source = source;
            ManagementPackRelationshipEndpoint target = new ManagementPackRelationshipEndpoint(r, "GroupContains");
            target.MinCardinality = 0;
            target.MaxCardinality = Int32.MaxValue;
            target.Type = targetClass;
            r.Target = target;
            return r;
        }

    }
}