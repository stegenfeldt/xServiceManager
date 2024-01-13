using System;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.Management.Automation;
using System.Collections.ObjectModel;
using Microsoft.EnterpriseManagement.Packaging;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Configuration.IO;

namespace xServiceManager.Module
{
    //TODO: Decide if this really is necessary in 2019+
    // [Cmdlet(VerbsData.Import,"xSCSMManagementPack", SupportsShouldProcess=true)]
    // public class ImportManagementPackCommand : SMCmdletBase
    // {
    //     // Parameters
    //     private string _fullname;
    //     [Parameter(ParameterSetName="FullName",Position=0,Mandatory=true,ValueFromPipelineByPropertyName=true)]
    //     public string FullName
    //     {
    //         get { return _fullname; }
    //         set { _fullname = value; }
    //     }

    //     private ManagementPack _managementpack;
    //     [Parameter(ParameterSetName="MPInstance",Position=0,Mandatory=true,ValueFromPipeline=true)]
    //     public ManagementPack ManagementPack
    //     {
    //         get { return _managementpack; }
    //         set { _managementpack = value; }
    //     }

    //     private SwitchParameter _noTokenCheck;
    //     [Parameter()]
    //     public SwitchParameter NoTokenCheck
    //     {
    //         set { _noTokenCheck = value; }
    //         get { return _noTokenCheck; }
    //     }

    //     private SwitchParameter _noSealCheck;
    //     [Parameter()]
    //     public SwitchParameter NoSealCheck
    //     {
    //         set { _noSealCheck = value; }
    //         get { return _noSealCheck; }
    //     }

    //     private SwitchParameter _noVersionCheck;
    //     [Parameter()]
    //     public SwitchParameter NoVersionCheck
    //     {
    //         set { _noVersionCheck = value; }
    //         get { return _noVersionCheck; }
    //     }

    //     private List<ManagementPack> InstalledMPs;
    //     private List<ManagementPack> InstallationOrder;
    //     private List<ManagementPack> MPsToInstall;
    //     private Hashtable FailureHash = new Hashtable();
    //     private ManagementPackBundleReader mpbr;
        
    //     protected override void BeginProcessing()
    //     {

    //         base.BeginProcessing();
    //         WriteDebug("InstalledMPs");
    //         InstalledMPs = new List<ManagementPack>();
    //         foreach(ManagementPack p in _mg.ManagementPacks.GetManagementPacks())
    //         {
    //             InstalledMPs.Add(p);
    //         }
    //         WriteDebug("InstallationOrder");
    //         InstallationOrder = new List<ManagementPack>();
    //         WriteDebug("MPsToInstall");
    //         MPsToInstall = new List<ManagementPack>();
    //         // We may need this, just created it 
    //         mpbr = ManagementPackBundleFactory.CreateBundleReader();
    //     }

    //     protected override void ProcessRecord()
    //     {
    //         if ( ManagementPack != null )
    //         {
    //             MPsToInstall.Add( ManagementPack );
    //         }
    //         if ( FullName != null )
    //         {
    //             ProviderInfo pi;
    //             foreach (string providerPath in GetResolvedProviderPathFromPSPath(FullName, out pi))
    //             {
    //                 FileInfo mpFile = ResolvePath(providerPath);
    //                 // FileInfo mpFile = new FileInfo(FullName);
    //                 if (mpFile.Exists)
    //                 {
    //                     if (mpFile.Extension.ToUpperInvariant() == ".MPB")
    //                     {
    //                         WriteVerbose("MPB: " + mpFile.FullName);
    //                         ManagementPackBundle mpb = mpbr.Read(mpFile.FullName, _mg);
    //                         if (ShouldProcess(mpFile.FullName))
    //                         {
    //                             _mg.ManagementPacks.ImportBundle(mpb);
    //                         }
    //                     }
    //                     else if (mpFile.Extension.ToUpperInvariant() == ".XML" || mpFile.Extension.ToUpperInvariant() == ".MP")
    //                     {
    //                         foreach (ManagementPack mp in MpFromFullName(mpFile.FullName))
    //                         {
    //                             MPsToInstall.Add(mp);
    //                         }
    //                     }
    //                     else
    //                     {
    //                         WriteError(new ErrorRecord(new FileLoadException(mpFile.Name),
    //                             "Cannot Import " + mpFile.Name + ". REASON: Bad extension",
    //                             ErrorCategory.InvalidType,
    //                             mpFile.Name));
    //                     }
    //                 }
    //             }
    //         }
    //     }

    //     protected override void EndProcessing()
    //     {
    //         GetInstallationOrder();
    //         if ( MPsToInstall.Count > 0 )
    //         {
    //             foreach ( ManagementPack mp in MPsToInstall)
    //             {
    //                 WriteError(new ErrorRecord(new FileLoadException(mp.Name),
    //                     "Cannot Import " + mp.Name + ". REASON: " + FailureHash[mp], 
    //                     ErrorCategory.ObjectNotFound, 
    //                     mp.Name));
    //             }
    //         }
    //         foreach(ManagementPack mp in InstallationOrder)
    //         {
    //             if(ShouldProcess("Import management pack " + mp.Name))
    //             {
    //                 _mg.ManagementPacks.ImportManagementPack(mp);
    //             }
    //         }
    //     }
    //     // TODO: REDO FOR COLLECTION
    //     private FileInfo ResolvePath(string potentialFile)
    //     {
    //         // this handles the case that we got a fullpath
    //         if ( File.Exists(potentialFile)) { return new FileInfo(potentialFile); }
    //         else
    //         {
    //             // go look for the file
    //             ProviderInfo pid;
    //             Collection<string> results = GetResolvedProviderPathFromPSPath(potentialFile, out pid);
    //             if ( results == null || results.Count == 0)
    //             {
    //                 // ok - return a fileinfo object, 
    //                 // we know it's bad, but the code above will handle
    //                 return new FileInfo(potentialFile);
    //             }
    //             // BLECH!!!
    //             return new FileInfo(results[0]);
    //         }
    //     }

    //     private void GetInstallationOrder()
    //     {
    //         int offset = 0;
    //         string ReasonForFailure = "";
    //         while(offset < MPsToInstall.Count)
    //         {
    //             WriteDebug("OFFSET: " + offset + ", COUNT: " + MPsToInstall.Count);
    //             List<ManagementPackReference> mpr = new List<ManagementPackReference>();
    //             // List<string>keys = (List<string>)MPsToInstall[offset].References.Keys;
    //             // foreach(string key in keys) { mpr.Add(MPsToInstall[offset].References[key]); }
    //             foreach(string key in MPsToInstall[offset].References.Keys)
    //             {
    //                 mpr.Add(MPsToInstall[offset].References[key]);
    //             }
    //             bool OkToAdd = true;
    //             foreach(ManagementPackReference reference in mpr )
    //             {
    //                 if ( ! CheckReferenceIsInstalled(reference) ) 
    //                 { 
    //                     ReasonForFailure = String.Format(CultureInfo.CurrentCulture, "Referenced ManagementPack '{0}' is not installed", reference.Name);
    //                     OkToAdd = false;
    //                     break;
    //                 }
    //                 else
    //                 { 
    //                     if ( ! DoVersionCheck(reference) )
    //                     {
    //                         ReasonForFailure = String.Format(CultureInfo.CurrentCulture, "Version Mismatch '{0}' <> '{1}'",MPsToInstall[offset].Version,reference.Version);
    //                         OkToAdd = false;
    //                         break;
    //                     }
    //                     if ( ! CheckIsSealed(reference) )
    //                     {
    //                         ReasonForFailure = String.Format(CultureInfo.CurrentCulture, "Referenced ManagementPack '{0}' is unsealed", reference.Name);
    //                         OkToAdd = false;
    //                         break;
    //                     }
    //                     if ( ! DoKeyTokenCheck(reference) )
    //                     {
    //                         ReasonForFailure = String.Format(CultureInfo.CurrentCulture, "KeyToken Mismatch '{0}' <> '{1}'",MPsToInstall[offset].KeyToken,reference.KeyToken);
    //                         OkToAdd = false;
    //                         break;
    //                     }
    //                 }
    //             }
    //             if ( OkToAdd ) 
    //             { 
    //                 InstallationOrder.Add(MPsToInstall[offset]); 
    //                 InstalledMPs.Add(MPsToInstall[offset]);
    //                 MPsToInstall.Remove(MPsToInstall[offset]);
    //                 offset = 0;
    //             }
    //             else
    //             {
    //                 if ( FailureHash.ContainsKey(MPsToInstall[offset]))
    //                 {
    //                     FailureHash[MPsToInstall[offset]] = ReasonForFailure;
    //                 }
    //                 else
    //                 {
    //                     FailureHash.Add(MPsToInstall[offset], ReasonForFailure);
    //                 }
    //                 offset++;
    //             }
    //         }
    //     }

    //     private ManagementPack GetManagementPackByName(string name)
    //     {
    //         foreach(ManagementPack mp in InstalledMPs)
    //         {
    //             if ( mp.Name == name ) { return mp; }
    //         }
    //         return null;
    //     }

    //     private bool CheckReferenceIsInstalled(ManagementPackReference reference) 
    //     { 
    //         if ( GetManagementPackByName(reference.Name) != null) { return true; } else { return false; }
    //     }
    //     private bool DoVersionCheck(ManagementPackReference reference) 
    //     { 
    //         if ( NoVersionCheck ) { return true; }
    //         if ( (GetManagementPackByName(reference.Name).Version >= reference.Version) )
    //         {
    //             return true; 
    //         }
    //         else
    //         {
    //             return false;
    //         }
    //     }
    //     private bool CheckIsSealed(ManagementPackReference reference) 
    //     { 
    //         if ( NoSealCheck ) { return true; }
    //         if ( GetManagementPackByName(reference.Name).Sealed )
    //         {
    //             return true; 
    //         }
    //         else
    //         {
    //             return false;
    //         }
    //     }
    //     private bool DoKeyTokenCheck(ManagementPackReference reference) 
    //     { 
    //         if ( NoTokenCheck ) { return true; }
    //         if ( GetManagementPackByName(reference.Name).KeyToken == reference.KeyToken )
    //         {
    //         return true; 
    //         }
    //         else
    //         {
    //         return false;
    //         }
    //     }

    //     private List<ManagementPack> MpFromFullName(string FullName)
    //     {
    //         ProviderInfo providerInfo;
    //         FileInfo _mpFile;
    //         ManagementPack _theMp;
    //         List<ManagementPack> mplist = new List<ManagementPack>();
    //         foreach(string file in GetResolvedProviderPathFromPSPath(FullName,out providerInfo))
    //         {
    //             // TODO: Ensure FullName
    //             // Be sure to bail before the MG is created, if the FileInfo
    //             // can't be created, there's no reason to continue
    //             _mpFile = new FileInfo(file);
    //             if ( _mpFile.Exists )
    //             {
    //                 // Build and import the MP
    //                 try
    //                 {
    //                     ManagementPackFileStore mpStore = new ManagementPackFileStore();
    //                     mpStore.AddDirectory(_mpFile.Directory);
    //                     _theMp = new ManagementPack(_mpFile.FullName, mpStore);
    //                     mplist.Add( _theMp );
    //                     // MPsToInstall.Add( _theMp );
    //                     // _mg.ManagementPacks.ImportManagementPack(_theMp);
    //                 }
    //                 catch (Exception e)
    //                 {
    //                     ThrowTerminatingError(
    //                             new ErrorRecord(e, "ManagementPack creation failed",
    //                                 ErrorCategory.NotSpecified, _mpFile.FullName)
    //                             );
    //                 }
    //             }
    //             else
    //             {
    //                 WriteError(
    //                         new ErrorRecord(new FileNotFoundException(file),
    //                             "Import Failed",
    //                             ErrorCategory.ObjectNotFound, FullName)
    //                         );
    //             }
    //         }
    //         return mplist;
    //     }

    // }
}
