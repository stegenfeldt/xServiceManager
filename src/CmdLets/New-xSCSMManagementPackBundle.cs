using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Packaging;
using Microsoft.EnterpriseManagement.Configuration;
using xServiceManager.MPBMaker;

namespace xServiceManager.Module
{
    //FIXME: Not working in newer .net
    // // Implementation of New-SCSMManagementPackBundle
    // // Creates a new management pack bundle (MPB)
    // // New-SCManagementPackBundle already exist in Microsoft.EnterpriseManagement.Core.Cmdlets, so name changed
    // [Cmdlet(VerbsCommon.New, "xSCSMManagementPackBundle")]
    // public class NewManagementPackBundleCommand : SMCmdletBase
    // {
    //     private string _name = "";
    //     [Parameter(Position=0,Mandatory = true, HelpMessage = "Name of MPB file to create")]
    //     public string Name
    //     {
    //         get { return _name; }
    //         set { _name = value; }
    //     }

    //     private string[] _mpFileNames = { };
    //     [Parameter(Mandatory=true, HelpMessage="Full path to management pack(s), XML or MP allowed")]
    //     public string[] ManagementPackFiles
    //     {
    //         get { return _mpFileNames; }
    //         set { _mpFileNames = value; }
    //     }

    //     private string _outDir = "";
    //     [Parameter(Mandatory = false, HelpMessage = "Folder where new MPB will be created. Current directory by default")]
    //     public string OutputDir
    //     {
    //         get { return _outDir; }
    //         set { _outDir = value; }
    //     }

    //     protected override void ProcessRecord()
    //     {
    //         List<FileInformation> _mpsList = new List<FileInformation>();

    //         if (System.IO.File.Exists(this.Name))
    //         {
    //             new ArgumentException("The Name parameter must be a bandle Name, not a file name");
    //         }

    //         foreach (string file in this.ManagementPackFiles)
    //         {
    //             if (System.IO.File.Exists(file) && (new string[] { ".xml", ".mp" }).Contains((new System.IO.FileInfo(file)).Extension))
    //                 _mpsList.Add(new FileInformation(file));
    //             else
    //             {
    //                 this.WriteWarning(string.Format("Management pack {0} not found or not .mp either .xml", file));
    //             }
    //         }

    //         if (_mpsList.Count > 0)
    //         {
    //             this.WriteVerbose("\tStart BuildBundel process...");
                

    //             ManagementPackBundle newBandle = ManagementPackBundleFactory.CreateBundle();
    //             if (string.IsNullOrEmpty(this.OutputDir))
    //                 this.WriteVerbose("\tOutput folder not specified");

    //             foreach (FileInformation mp in _mpsList)
    //             {
    //                 this.WriteVerbose("\tProcess management pack " + mp.FileName + " ...");
    //                 //string[] includePaths = new string[] { @"c:\Program Files (x86)\Microsoft System Center\Service Manager 2010 Authoring\Library\" };
    //                 ManagementPack mpObj = new ManagementPack(mp.FullPath);
    //                 newBandle.AddManagementPack(mpObj);
    //                 ManagementPackElementCollection<ManagementPackResource> allResources = mpObj.GetResources<ManagementPackResource>();
    //                 foreach (ManagementPackResource resource in allResources)
    //                 {
    //                     this.WriteVerbose("\tProcess resource file " + resource.FileName + " ...");
    //                     FileInfo[] foundedFiles = mp.Info.Directory.GetFiles(resource.FileName);
    //                     FileInfo fileToAdd = foundedFiles.Length > 0 ? foundedFiles[0] : null;
    //                     if (fileToAdd != null)
    //                     {
    //                         Stream curStr = null;
    //                         try
    //                         {
    //                             curStr = fileToAdd.Open(FileMode.Open, FileAccess.Read);
    //                             newBandle.AddResourceStream(mpObj, resource.Name, curStr, ManagementPackBundleStreamSignature.Empty);
    //                         }
    //                         catch (Exception er)
    //                         {
    //                             if (curStr != null)
    //                             {
    //                                 curStr.Close();
    //                                 curStr.Dispose();
    //                             }
    //                             Console.WriteLine("\tCouldn't process file: " + resource.FileName);
    //                             Console.WriteLine("\tError: " + er.Message);
    //                             return;
    //                         }
    //                     }
    //                     else
    //                     {
    //                         new Exception("Resource file not found: " + resource.FileName);
    //                     }
    //                 }

    //             }
    //             try
    //             {
    //                 ManagementPackBundleWriter bundleWriter = ManagementPackBundleFactory.CreateBundleWriter(string.IsNullOrEmpty(this.OutputDir) ? _mpsList[0].Info.DirectoryName : this.OutputDir);
    //                 string ret = bundleWriter.Write(newBandle, this.Name);

    //                 Console.WriteLine("");
    //                 Console.WriteLine("\tBundle successfully saved!");
    //                 Console.WriteLine("");
    //                 WriteObject(new FileInfo(ret));
    //             }
    //             catch (Exception er)
    //             {
    //                 throw er;
    //             }
    //             finally
    //             {
    //                 foreach (ManagementPack mp in newBandle.ManagementPacks)
    //                 {
    //                     foreach (Stream str in newBandle.GetStreams(mp).Values)
    //                     {
    //                         str.Close();
    //                         str.Dispose();
    //                     }
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             new Exception("Error: Management packs not found.");
    //         }
    //     }
    // }
}
