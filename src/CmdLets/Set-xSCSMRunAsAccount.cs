using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Security;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Set, "xSCSMRunAsAccount", SupportsShouldProcess = true)]
    public class SetRunAsAccountsCommand : SMCmdletBase
    {
        /* Created by Travis Wright (twright@microsoft.com, radtravis@hotmail.com) Jan 12 2011
         * 
         * REALLY LONG EXPLANATION OF HOW RUN AS ACCOUNTS WORK IN SERVICE MANAGER
         * This long explanation is provided for anybody that ever has to do something like this again
         * so they don't have to go through the pain I did to figure this out.  There is no way you could 
         * figure this out without first hand knowledge of how the product was built and access to the product
         * source code for reference.
         * 
         * In the System Center common platform which underlies SCSM, SCOM, and SCE there is a concept of a "Run As Account".
         * 
         * There are really three things at play here though:
         *      There is a "Run As Account" (aka SecureData) 
         *      There is a "Run As Profile" (aka SecureReference)
         *      There is a "Run As Account override" (aka SecureReferenceOverride)
         *      
         * In SCOM and SCE you can see all of these concepts in the UI, but in SCSM we obscured this because it wasnt necessary so you only see "Run As Accounts".
         * 
         * For the rest of this explanation I will only talk in coding terms since that's what we are doing here.  Just be aware of the mapping to the terms in the UI.
         * 
         * SecureData
         * ====================
         * A SecureData is where we store the credentials of a given "account".  There are different types of SecureData:
         * WindowsCredentialSecureData - stores domain\username & password for a Windows/AD user account
         * SimpleCredentialSecureData - stores simple login/password credentials for non-Windows stuff like Unix,SNMP traps etc, 
         * 
         * For our purposes we typically just need to worry about WindowsCredentialSecureData.
         * 
         * When a SecureData object is created it is stored on the CredentialManagerSecureStorage table in the database.  The password is encrypted using a fancy
         * public/private/symmetric key system.  I won't go into that.
         * 
         * SecureData objects cannot be declared in management packs.  They only exist in the database and therefore are not transportable from one management group to another.
         * 
         * 
         * SecureReference
         * =====================
         * SecureReferences can be declared in management packs.  They are declared in a section inside of TypeDefinitions (just before ModuleTypes if it exists)
         * <TypeDefinitions>
                <SecureReferences>
                    <SecureReference ID="ExchangeAdmin" Accessibility="Public" Context="System!System.Entity"/>
                </SecureReferences>
            </TypeDefinitions>

         * A SecureReference is a level of indirection between where the SecureData is defined and where it is used.  It allows a management pack author to create a "placeholder" 
         * for where an administrator needs to provide some specific credentials.  For example, let's say that I have a management pack that monitors Exchange.  In order to monitor Exchange
         * effectively I need to have some of my rules and monitors running under the security context of a user that is an Exchange administrator.  So - I could create a SecureReference called 
         * ExchangeAdmin and then tell different ModuleTypes in my MP that when then run they need to run under the SecureData that is associated with the ExchangeAdmin SecureReference like this:
         
         * <WriteActionModuleType ID="blah" Accessibility="Public" RunAs="ExchangeAdmin" Batching="false">
         * 
         * Now when the customer administrator imports this management he needs to specify which SecureData will be associated with that SecureReference.
         * 
         * In SCOM and SCE that is easy to do because we made that possible in the UI.  In SCSM we tried to hide the complexity of this level of indirection thinking that most people would never
         * need a custom Run As Account outside of using them in Connectors.  We handle all of this logic for the customer in the Connector wizards so it is simple.
         * 
         * In SCSM there is a 1:1 mapping of a SecureData to a SecureReference but the infrastructure allows for more than one SecureData to be associated to a SecureReference.
         * 
         * This cmdlet (Set-SCSMRunAsAccount) will provide the administrator a means to set the SecureData for a given SecureReference.  The cmdlet assumes the same 1:1 mapping we have for all other SecureReferences.
         * 
         * This cmdlet assumes that a SecureReference has already been defined in a management pack and imported into SCSM and now the administrator is going to just run Get-SCSMRunAsAccount | Set-SCSMRunAsAccount 
         * to set the SecureData for that SecureReference.
         * 
         * SecureReferences are stored on the SecureReference table in the database.
         * 
         * SecureReferenceOverride
         * =======================
         * 
         * A SecureReferenceOverride tells Service Manager to use a particular SecureData for a particular context.  For example, given the Exchange scenario above, let's say that I wanted to use
         * CredentialA (mydomain\user1) for some of my exchange servers and CredentialB (mydomain\user2) for the other Exchange servers. I can do that by using SecureReferenceOverrides.
         * 
         * This level of control is really only needed in SCOM situations.
         * 
         * In SCSM we just need to tell SCSM to use the same SecureData everywhere so this cmdlet will set the scope of the override to System.Entity.
         * 
         * SecureReferenceOverrides are stored in MPs and on the SecureReferenceOverride table in the database.
         * 
         * Conclusion
         * =======================
         * This cmdlet does 3 things:
         * 1) Creates a WindowsCredentialSecureData provided the credentials that are passed in.
         * 2) Creates a SecureReferenceOverride to tell SCSM to use the provided WindowsCredentialSecureData everywhere
         * 3) Grants all the health services (aka System Center Management services) permission to download the credentials and decrypt the password.
         * 
         */

        private WindowsCredentialSecureData _credentialSecureData = new WindowsCredentialSecureData();
        private PSCredential _credential;
        private ManagementPackSecureReference[] _secureReferences = null;

        //The user needs to pipe a ManagementPackSecureReference from Get-SCSMRunAsAccount.
        [Parameter(ValueFromPipeline = true,
                    Mandatory = true,
                    ParameterSetName = "ByObject")]
        public ManagementPackSecureReference[] SecureReferences
        {
            get { return _secureReferences; }
            set { _secureReferences = value; }
        }

        //The user must supply a PSCredential.  For example:
        //$cred = Get-Credential  <-- This will pop up a dialog the user can enter credentials into.  The password will be stored in a SecureString in $cred.
        //Then the user can call Get-SCSMRunAsAccount -Name "My Run As Account" | Set-SCSMRunAsAccount $cred
        [Parameter(Position = 0,
                    Mandatory = true)]
        public PSCredential WindowsCredential
        {
            get { return _credential; }
            set { _credential = value; }
        }

        protected override void BeginProcessing()
        {
            //This will set the _mg which is the EnterpriseManagementGroup object for the connection to the server
            base.BeginProcessing();

            //Set the name of the secure data equal to the domain\username provided in the credential passed in.
            _credentialSecureData.Name = _credential.UserName;

            //Set the properties of the WindowsCredentialSecureData object
            _credentialSecureData.UserName = _credential.GetNetworkCredential().UserName;
            _credentialSecureData.Domain = _credential.GetNetworkCredential().Domain;
            _credentialSecureData.Data = _credential.Password;

            //Saving the WindowsCredentialSecureData.  This goes into the CredentialManagerSecureStorage table in the database.
            if (ShouldProcess(_credentialSecureData.Domain + "\\" + _credentialSecureData.UserName))
            {
                _mg.Security.InsertSecureData(_credentialSecureData);
            }

        }

        protected override void ProcessRecord()
        {
            foreach (ManagementPackSecureReference secureReference in _secureReferences)
            {
                //First get the MP that the Secure Reference that was passed in is stored in so that we can create some SecureReferenceOverrides in it.
                ManagementPack mpSecureReferenceMP = secureReference.GetManagementPack();

                //Before we create a new SecureReferenceOverride we need to check to see if one already exists.
                bool boolSecureRefOverrideAlreadyExists = false;

                //Loop through each Override in the MP...
                ManagementPackElementCollection<ManagementPackOverride> listOverrides = mpSecureReferenceMP.GetOverrides();
                foreach (ManagementPackOverride mpOverride in listOverrides)
                {
                    //...if it is a ManagementPackSecureReferenceOverride...
                    if (mpOverride is ManagementPackSecureReferenceOverride)
                    {
                        //...then cast it to a ManagementPackSecureReferenceOverride...
                        ManagementPackSecureReferenceOverride mpSecRefOverride = mpOverride as ManagementPackSecureReferenceOverride;
                        //...and then compare it to the SecureReference that was passed in...
                        if (mpSecRefOverride.SecureReference.Id == secureReference.Id)
                        {
                            //...if it is the same one then get a list of all the SecureData objects so we can compare with those...
                            IList<SecureData> secureDataList = _mg.Security.GetSecureData();
                            foreach (SecureData secureData in secureDataList)
                            {
                                //...by comparing the SecureStorageID of each of the existing and the .Value of the SecureData we just created...
                                if (String.Compare
                                            (BitConverter.ToString(secureData.SecureStorageId, 0, secureData.SecureStorageId.Length).Replace("-", ""),
                                                mpSecRefOverride.Value,
                                                StringComparison.Ordinal
                                            ) == 0
                                   )
                                {
                                    //...and if you find a match...
                                    WindowsCredentialSecureData windowsCred = secureData as WindowsCredentialSecureData;
                                    if (windowsCred != null)
                                    {
                                        //...then set the bool to true so we know that there is already a SecureReferenceOverride with this same exact SecureData
                                        // so we dont need to create a new SecureReferenceOverride in this case.
                                        boolSecureRefOverrideAlreadyExists = true;
                                    }
                                }
                            }
                        }
                    }
                }

                //Do we need to create a new SecureReferenceOverride?
                if (!boolSecureRefOverrideAlreadyExists)
                {
                    //Yes, we need to create a new SecureReferenceOverride...

                    //First create the SecureReferenceOverride object by setting its ID
                    ManagementPackSecureReferenceOverride secureOverride = new ManagementPackSecureReferenceOverride(mpSecureReferenceMP, String.Format("SecureReferenceOverride.{0}", Guid.NewGuid().ToString("N")));

                    //Then tell it that it's scope is for all objects by setting the class context to System.Entity
                    secureOverride.Context = _mg.EntityTypes.GetClass(SystemClass.Entity);

                    //Set the SecureReference equal to the SecureReference that was passed in.
                    secureOverride.SecureReference = secureReference;

                    //Give it a display name - doesnt need to be anything fancy since it doesnt show anywhere in the UI.
                    secureOverride.DisplayName = "SecureReferenceOverride_" + Guid.NewGuid().ToString();

                    //Convert to a byte array
                    secureOverride.Value = BitConverter.ToString(_credentialSecureData.SecureStorageId, 0, _credentialSecureData.SecureStorageId.Length).Replace("-", "");

                    //Now allow this SecureData to be downloaded to all the management servers
                    ApprovedHealthServicesForDistribution<EnterpriseManagementObject> approved = new ApprovedHealthServicesForDistribution<EnterpriseManagementObject>();
                    approved.Result = ApprovedHealthServicesResults.All;

                    //Tell SCSM that we are going to update (or submit new) this SecureReferenceOverride
                    secureReference.Status = ManagementPackElementStatus.PendingUpdate;

                    //Post it to the database.  This will show up on the SecureReferenceOverride table in the database and in the <Overrides> section of the MP XML
                    string secureReferenceInfo = secureReference.Name;
                    if (secureReference.DisplayName != null)
                    {
                        secureReferenceInfo = secureReference.DisplayName;
                    }

                    if (ShouldProcess(secureReferenceInfo))
                    {
                        _mg.Security.SetApprovedHealthServicesForDistribution<EnterpriseManagementObject>(_credentialSecureData, approved);
                        mpSecureReferenceMP.AcceptChanges();
                    }
                }
            }
        }
    }

}