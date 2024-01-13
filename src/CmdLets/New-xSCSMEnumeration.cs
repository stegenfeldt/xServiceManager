using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.New, "xSCSMEnumeration", SupportsShouldProcess = true)]
    public class AddSCSMEnumerationCommand : SMCmdletBase
    {
        #region Parameters
        private ManagementPackEnumeration _parent;
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public ManagementPackEnumeration Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }
        private String _name;
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private String _displayName;
        [Parameter]
        public String DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }
        private Double _ordinal;
        [Parameter(Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public Double Ordinal
        {
            get { return _ordinal; }
            set { _ordinal = value; }
        }
        private ManagementPack _mp;
        [Parameter(Position = 3, Mandatory = true, ParameterSetName = "MP")]
        [Alias("mp")]
        public ManagementPack ManagementPack
        {
            get { return _mp; }
            set { _mp = value; }
        }
        private String _mpName;
        [Parameter(Position = 3, Mandatory = true, ParameterSetName = "MPName")]
        [Alias("MPName")]
        public String ManagementPackName
        {
            get { return _mpName; }
            set { _mpName = value; }
        }
        private SwitchParameter _passThru;
        [Parameter]
        public SwitchParameter PassThru
        {
            get { return _passThru; }
            set { _passThru = value; }
        }
        #endregion
        EnterpriseManagementGroup emg;
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            emg = Parent.ManagementGroup;
            if (!emg.IsConnected)
            {
                emg.Reconnect();
            }
            if (ParameterSetName == "MPName")
            {
                WildcardPattern wp = new WildcardPattern(ManagementPackName, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
                int mpMatchCount = 0;
                foreach (ManagementPack m in emg.ManagementPacks.GetManagementPacks())
                {
                    if ((wp.IsMatch(m.Name) || wp.IsMatch(m.DisplayName)) && m.Sealed)
                    {
                        mpMatchCount++;
                        ManagementPack = m;
                    }
                }
                if (mpMatchCount == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(new ObjectNotFoundException(ManagementPackName + " could not be found"), "No MP", ErrorCategory.ObjectNotFound, ManagementPackName));
                }
                else if (mpMatchCount > 1)
                {
                    ThrowTerminatingError(new ErrorRecord(new ObjectNotFoundException(ManagementPackName + " matched multiple mps"), "Multiple MP", ErrorCategory.ObjectNotFound, ManagementPackName));
                }
            }
            if (ManagementPack.Sealed)
            {
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(ManagementPack.Name + " is sealed"), "Sealed MP", ErrorCategory.InvalidOperation, ManagementPack));
            }
        }
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            ManagementPackEnumeration e = new ManagementPackEnumeration(ManagementPack, Name, ManagementPackAccessibility.Public);
            e.Ordinal = Ordinal;
            e.Parent = Parent;
            ManagementPack ParentMP = Parent.GetManagementPack();
            if (DisplayName != null) { e.DisplayName = DisplayName; }

            if (ShouldProcess(e.Name))
            {
                if (!ManagementPack.References.ContainsValue(ParentMP))
                {
                    WriteVerbose("Adding reference to " + ParentMP.Name);
                    // Errors here are not fatal
                    // but could be later (The MP may not have the appropriate references)
                    try { ManagementPack.References.Add(ParentMP.Name.Replace('.', '_'), ParentMP); } catch {; }
                }
                ManagementPack.AcceptChanges();
            }
            if (PassThru)
            {
                WriteObject(e);
            }
        }
    }
}