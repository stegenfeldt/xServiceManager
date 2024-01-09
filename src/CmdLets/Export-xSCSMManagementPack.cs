using System;
using System.IO;
using System.Xml;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Configuration.IO;

namespace xServiceManager.Module
{
    // Implementation of Export-ManagementPack
    [Cmdlet(VerbsData.Export, "xSCSMManagementPack", SupportsShouldProcess = true)]
    public class ExportManagementPackCommand : PSCmdlet
    {
        // Private data
        // Parameters
        private ManagementPack _mp;
        private string _outputFileName;
        [Parameter(Position = 1, Mandatory = true, ValueFromPipeline = true)]
        public ManagementPack ManagementPack
        {
            get { return _mp; }
            set { _mp = value; }
        }

        private DirectoryInfo _target;
        [Parameter(Position = 0, Mandatory = true)]
        public DirectoryInfo TargetDirectory
        {
            get { return _target; }
            set { _target = value; }
        }
        protected override void BeginProcessing()
        {
            if (!TargetDirectory.Exists)
            {
                ThrowTerminatingError(
                        new ErrorRecord((new ItemNotFoundException()), "Target Directory does not exist",
                            ErrorCategory.ObjectNotFound, TargetDirectory.FullName)
                        );
            }
        }
        protected override void ProcessRecord()
        {
            ExportPack();
        }
        private void ExportPack()
        {
            if (ShouldProcess("Export Management Pack " + ManagementPack.Name))
            {
                try
                {
                    WriteVerbose("exporting " + ManagementPack.Name);
                    _outputFileName = TargetDirectory + "/" + ManagementPack.Name + ".xml";
                    Stream mpStream = new FileStream(_outputFileName, FileMode.Create);
                    XmlWriter writer = XmlWriter.Create(mpStream);
                    ManagementPackXmlWriter mpWriter = new ManagementPackXmlWriter(writer);
                    mpWriter.WriteManagementPack(ManagementPack);
                    mpStream.Close();
                }
                catch (Exception e)
                {
                    WriteError(
                            new ErrorRecord(e, "Failed to export",
                                ErrorCategory.WriteError, _outputFileName)
                            );
                }
            }
        }
    }
}
