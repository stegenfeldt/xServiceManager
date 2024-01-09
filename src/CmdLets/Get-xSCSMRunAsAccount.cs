using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMRunAsAccount")]
    public class GetRunAsAccountsCommand : SMCmdletBase
    {

        private ActionAccountSecureData aasd = null;
        private IList<ManagementPackOverride> overrides = null;
        private Hashtable sdHash = null;
        private Regex r;
        private string _name = ".*";
        [Parameter(Position = 0)]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            r = new Regex(Name, RegexOptions.IgnoreCase);
            overrides = _mg.Overrides.GetOverrides();
            sdHash = new Hashtable();
            foreach (SecureDataHealthServiceReference sr in _mg.Security.GetSecureDataHealthServiceReferences())
            {
                aasd = _mg.Security.GetSecureData(sr.SecureDataId) as ActionAccountSecureData;
                if (aasd != null)
                {
                    break;
                }
            }

            foreach (ManagementPackOverride mpOverride in overrides)
            {
                ManagementPackSecureReferenceOverride secRefOverride = mpOverride as ManagementPackSecureReferenceOverride;
                if (secRefOverride != null)
                {
                    Guid secrefid = secRefOverride.SecureReference.Id;
                    int i = 0, x = 0;
                    byte[] bytes = new byte[(secRefOverride.Value.Length) / 2];
                    while (secRefOverride.Value.Length > i + 1)
                    {
                        long lngDecimal = Convert.ToInt32(secRefOverride.Value.Substring(i, 2), 16);
                        bytes[x] = Convert.ToByte(lngDecimal);
                        i = i + 2;
                        ++x;
                    }
                    SecureData secureData = _mg.Security.GetSecureData(bytes);
                    WindowsCredentialSecureData credential = secureData as WindowsCredentialSecureData;
                    if (credential != null)
                    {
                        if (!sdHash.ContainsKey(secrefid))
                        {
                            sdHash.Add(secrefid, String.Format(CultureInfo.InvariantCulture, "{0}\\{1}", credential.Domain, credential.UserName));
                        }
                    }
                    else
                    {
                        ActionAccountSecureData actionAccount = secureData as ActionAccountSecureData;
                        if (actionAccount != null)
                        {
                            if (!sdHash.ContainsKey(secrefid))
                            {
                                sdHash.Add(secrefid, String.Format(CultureInfo.InvariantCulture, "{0}\\{1}", actionAccount.Domain, actionAccount.UserName));
                            }
                        }
                    }
                }
            }
        }

        // This is a bit fragile and relies on current SM 1.0 behaviors
        // and definitions
        protected bool GetIsVisible(ManagementPackSecureReference sr)
        {
            // bail right away if we don't have any categories
            if (sr.GetCategories().Count == 0) { return false; }
            foreach (ManagementPackCategory c in sr.GetCategories())
            {
                try
                {
                    string s = _mg.EntityTypes.GetEnumeration(c.Value.Id).Name;
                    // This is fragile - changes in the underlying system can cause
                    // misbehavior
                    if (s == "VisibleToUser") { return true; }
                }
                catch {; }
            }
            return false;
        }

        protected override void EndProcessing()
        {
            foreach (ManagementPackSecureReference sr in _mg.Security.GetSecureReferences())
            {
                if (r.Match(sr.Name).Success || r.Match(sr.DisplayName).Success)
                {
                    bool IsVisible = GetIsVisible(sr);
                    PSObject o = new PSObject(sr);
                    o.Members.Add(new PSNoteProperty("DomainUser", GetUserName(sr)));
                    o.Members.Add(new PSNoteProperty("IsVisible", IsVisible));
                    o.Members.Add(new PSNoteProperty("ManagementPack", sr.GetManagementPack().FriendlyName));
                    WriteVerbose(GetUserName(sr));
                    WriteObject(o);
                }
            }

        }

        protected string GetUserName(ManagementPackSecureReference sr)
        {
            if (sdHash.ContainsKey(sr.Id))
            {
                return sdHash[sr.Id].ToString();
            }
            if (aasd != null)
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}\\{1}", aasd.Domain, aasd.UserName);
            }
            return "unknown";
        }

    }

}