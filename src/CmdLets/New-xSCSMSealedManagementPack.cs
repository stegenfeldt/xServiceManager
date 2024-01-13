using System;
using System.Management.Automation;

namespace xServiceManager.Module
{
    //FIXME: This doesn't work with current .net version. Needs investigating.
    // /// <summary>
    // /// This class is an exectuable wrapper to seal a ManagementPack
    // /// </summary>
    // [Cmdlet(VerbsCommon.New, "xSCSMSealedManagementPack")]
    // public class NewSealedManagementPackCommand : PSCmdlet
    // {
    //     /// <summary>
    //     /// The main entry point for the application.
    //     /// </summary>
    //     private string _fullName;
    //     private string _keyfilePath;
    //     private string _companyName;
    //     private string _copyright;
    //     private string _outputDirectory;
    //     private SwitchParameter _delaySign;

    //     [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
    //     public string FullName
    //     {
    //         get { return _fullName; }
    //         set { _fullName = value; }
    //     }
    //     [Parameter(Mandatory = true)]
    //     public string KeyFilePath
    //     {
    //         get { return _keyfilePath; }
    //         set { _keyfilePath = value; }
    //     }
    //     [Parameter(Mandatory = true)]
    //     public string CompanyName
    //     {
    //         get { return _companyName; }
    //         set { _companyName = value; }
    //     }
    //     [Parameter(Mandatory = true)]
    //     public string Copyright
    //     {
    //         get { return _copyright; }
    //         set { _copyright = value; }
    //     }
    //     [Parameter(Mandatory = true)]
    //     public string OutputDirectory
    //     {
    //         get { return _outputDirectory; }
    //         set { _outputDirectory = value; }
    //     }
    //     public SwitchParameter DelaySign
    //     {
    //         get { return _delaySign; }
    //         set { _delaySign = value; }
    //     }

    //     protected override void ProcessRecord()
    //     {
    //         try
    //         {
    //             if (string.IsNullOrEmpty(FullName))
    //             {
    //                 throw new ArgumentNullException("FullName");
    //             }

    //             //is this an Xml file?
    //             if (!(FullName.EndsWith(FastAssemblyWriter.XmlExtension, StringComparison.OrdinalIgnoreCase) ||
    //                     FullName.EndsWith(FastAssemblyWriter.MpbExtension, StringComparison.OrdinalIgnoreCase))
    //                 )
    //             {
    //                 ThrowTerminatingError(new ErrorRecord(null, "Invalid file extension", ErrorCategory.InvalidOperation, FullName));
    //             }
    //             if (FullName.EndsWith(FastAssemblyWriter.MpbExtension, StringComparison.OrdinalIgnoreCase))
    //             {
    //                 FastAssemblyWriter.isMpb = true;
    //             }


    //             //create the assembly writer settings object
    //             FastAssemblyWriterSettings settings = new FastAssemblyWriterSettings(CompanyName, KeyFilePath, DelaySign);
    //             settings.Copyright = Copyright;
    //             settings.OutputDirectory = OutputDirectory;


    //             //write assembly file
    //             FastAssemblyWriter assemblywriter = new FastAssemblyWriter(settings);
    //             if (FastAssemblyWriter.isMpb)
    //             {
    //                 string outfile = assemblywriter.WriteMPB(FullName);
    //                 WriteVerbose("ManagementPack name is " + outfile);
    //             }
    //             else
    //             {
    //                 string outfile = assemblywriter.WriteManagementPack(FullName);
    //                 WriteVerbose("MPB name is " + outfile);
    //             }

    //         }
    //         catch (Exception e)
    //         {
    //             ThrowTerminatingError(new ErrorRecord(e, "foo", ErrorCategory.InvalidOperation, FullName));
    //         }

    //     }

    // }
}
