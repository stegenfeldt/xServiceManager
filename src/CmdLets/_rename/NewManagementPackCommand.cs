using System;
using System.IO;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Configuration.IO;

namespace xServiceManager.Module
{
    // Implementation of New-ManagementPack
    // This does not import an MP, just creates an MP object based on a file
    // which can then be used by Export-ManagementPack
    [Cmdlet(VerbsCommon.New,"xSCSMManagementPack",DefaultParameterSetName="FromFile")]
    public class NewManagementPackCommand : SMCmdletBase
    {
        // Private data
        private FileInfo _mpFile;
        private ManagementPack _mp;
        // Parameters
        private string _fullname;
        [Parameter(Position=0,Mandatory=true,ValueFromPipelineByPropertyName=true,ParameterSetName="FromFile")]
        public string FullName
        {
            get { return _fullname; }
            set { _fullname = value; }
        }

        private string _managementPackName;
        [Parameter(Mandatory=true,ParameterSetName="NoFile")]
        [Alias("Name")]
        public string ManagementPackName
        {
            get { return _managementPackName; }
            set { _managementPackName = value; }
        }

        private string _friendlyName;
        [Parameter(ParameterSetName = "NoFile")]
        public string FriendlyName
        {
            get { return _friendlyName; }
            set { _friendlyName = value; }
        }

        private string _displayName;
        [Parameter(ParameterSetName = "NoFile")]
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }
        /*
        private EnterpriseManagementGroup _emg;
        [Parameter(ParameterSetName = "NoFile", Mandatory = true)]
        public EnterpriseManagementGroup EMG
        {
            get { return _emg; }
            set { _emg = value; }
        }
         * */
        private Version _version;
        [Parameter(ParameterSetName = "NoFile")]
        public Version Version
        {
            get { return _version; }
            set { _version = value; }
        }

        private SwitchParameter _verify;
        [Parameter]
        public SwitchParameter Verify
        {
            get { return _verify; }
            set { _verify = value; }
        }

        private SwitchParameter _passThru;
        [Parameter]
        public SwitchParameter PassThru
        {
            get { return _passThru; }
            set { _passThru = value; }
        }

        private List<String> _dependencyDirectory;
        [Parameter]
        private List<String> DependencyDirectory
        {
            get { return _dependencyDirectory; }
            set { _dependencyDirectory = value; }
        }
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (Version == null)
            {
                Version = new Version("1.0.0.0");
            }
            DependencyDirectory = new List<String>();
        }

        private List<ManagementPack> ImportMPFromFile()
        {
            List<ManagementPack> mpList = new List<ManagementPack>();
            ProviderInfo providerInfo;
            foreach(string file in GetResolvedProviderPathFromPSPath(FullName,out providerInfo))
            {
                // TODO: Ensure FullName
                // Be sure to bail before the MG is created, if the FileInfo
                // can't be created, there's no reason to continue
                _mpFile = new FileInfo(file);
                if ( _mpFile.Exists )
                {
                    // Build the MP
                    try
                    {
                        ManagementPackFileStore mpStore = new ManagementPackFileStore();
                        mpStore.AddDirectory(_mpFile.Directory);
                        if ( DependencyDirectory != null )
                        {
                            foreach(string dir in DependencyDirectory ) { mpStore.AddDirectory(dir); }
                        }
                        _mp = new ManagementPack(_mpFile.FullName, mpStore);
                        mpList.Add(_mp);
                    }
                    catch (Exception e)
                    {
                        WriteError( new ErrorRecord(e, "ManagementPack creation failed", ErrorCategory.NotSpecified, _mpFile.FullName) );
                    }
                    // OK, we have an MP, call verify if needed
                    if ( Verify )
                    {
                        try
                        {
                            _mp.Verify();
                        }
                        catch (Exception e)
                        {
                            WriteError( new ErrorRecord(e, "Verification of management pack failed", ErrorCategory.NotSpecified, _mpFile.FullName));
                        }
                    }
                }
                else
                {
                    WriteError( new ErrorRecord(new FileNotFoundException(file), "Failed to create management pack", ErrorCategory.ObjectNotFound, FullName));
                }
            }
            return mpList;
        }

        private ManagementPack ImportMPFromName()
        {
            if (FriendlyName == null)
            {
                FriendlyName = "Friendly name for " + ManagementPackName;
            }
            _mp = new ManagementPack(ManagementPackName, FriendlyName, Version, _mg);
            ManagementPack p;
            p = _mg.ManagementPacks.GetManagementPack(SystemManagementPack.System);
            _mp.References.Add(p.Name.Replace('.', '_'), p);
            p = _mg.ManagementPacks.GetManagementPack(SystemManagementPack.Windows);
            _mp.References.Add(p.Name.Replace('.', '_'), p);
            p = _mg.ManagementPacks.GetManagementPack(SystemManagementPack.SystemCenter);
            _mp.References.Add(p.Name.Replace('.', '_'), p);
            if ( DisplayName != null )
            {
                _mp.DisplayName = DisplayName;
            }
            else
            {
                _mp.DisplayName = ManagementPackName;
            }
            _mp.AcceptChanges();
            _mg.ManagementPacks.ImportManagementPack(_mp);
            return _mp;
        }

        protected override void ProcessRecord()
        {
            if ( ParameterSetName == "FromFile")
            {
                foreach (ManagementPack m in ImportMPFromFile())
                {
                    if (PassThru)
                    {
                        WriteObject(m);
                    }
                }
            }
            else
            {
                ManagementPack m = ImportMPFromName();
                if (PassThru)
                {
                    WriteObject(m);
                }
            }

        }
    }
}
