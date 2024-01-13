using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    public sealed class ConnectionHelper
    {
        private static Hashtable ht;
        private static object locker = new object();
        public static List<string> GetMGList()
        {
            return GetMGList(".*");
        }
        public static List<string> GetMGList(string re)
        {
            List<string> l = new List<string>();
            Regex r = new Regex(re, RegexOptions.IgnoreCase);
            foreach (string k in ht.Keys)
            {
                if (r.Match(k).Success)
                {
                    l.Add(k.ToString());
                }
            }
            return l;
        }
        public static EnterpriseManagementGroup GetMG(string computerName)
        {
            if (ht == null) { ht = new Hashtable(StringComparer.OrdinalIgnoreCase); }
            if (ht.ContainsKey(computerName))
            {
                return (EnterpriseManagementGroup)ht[computerName];
            }
            else
            {
                return null;
            }
        }
        public static EnterpriseManagementGroup GetMG(string computerName, PSCredential credential, string threeLetterWindowsLanguageName)
        {
            lock (locker)
            {
                string sessionId = GenereateUniqSessionId(computerName, threeLetterWindowsLanguageName, credential);
                if (ht == null) { ht = new Hashtable(StringComparer.OrdinalIgnoreCase); }
                if (!ht.ContainsKey(sessionId))
                {
                    EnterpriseManagementGroup emg;
                    EnterpriseManagementConnectionSettings settings = new EnterpriseManagementConnectionSettings(computerName);
                    settings.ThreeLetterWindowsLanguageName = threeLetterWindowsLanguageName;

                    if (credential != null)
                    {

                        settings.UserName = credential.GetNetworkCredential().UserName;
                        settings.Domain = credential.GetNetworkCredential().Domain;
                        settings.Password = credential.Password;

                    }

                    emg = new EnterpriseManagementGroup(settings);
                    try
                    {
                        ht.Add(sessionId, emg);
                    }
                    catch (Exception er)
                    {
                        throw new Exception(string.Format("Unable to add new session {0}", sessionId), er);
                    }
                }
                if (!((EnterpriseManagementGroup)ht[sessionId]).IsConnected)
                {
                    ((EnterpriseManagementGroup)ht[sessionId]).Reconnect();
                }
                return ht[sessionId] as EnterpriseManagementGroup;
            }
        }

        public static void SetMG(EnterpriseManagementGroup emg)
        {
            if (ht == null) { ht = new Hashtable(StringComparer.OrdinalIgnoreCase); }
            string sessionId = GenereateUniqSessionId(emg.ConnectionSettings);
            if (!ht.ContainsKey(sessionId))
            {
                ht.Add(sessionId, emg);
            }
        }
        public static void RemoveMG(EnterpriseManagementConnectionSettings settings)
        {
            string sessionId = GenereateUniqSessionId(settings);
            if (ht == null) { ht = new Hashtable(StringComparer.OrdinalIgnoreCase); }
            if (ht.ContainsKey(sessionId))
            {
                ht.Remove(sessionId);
            }
        }
        private ConnectionHelper() {; }

        private static string GenereateUniqSessionId(string serverName, string threeLetterWindowsLanguageName, string userName, string domain)
        {
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(domain))
                return serverName + "_" + threeLetterWindowsLanguageName + "_" + domain + "_" + userName;
            else
                return serverName + "_" + threeLetterWindowsLanguageName;
        }

        private static string GenereateUniqSessionId(string serverName, string threeLetterWindowsLanguageName, PSCredential credential)
        {
            if (credential != null)
                return serverName + "_" + threeLetterWindowsLanguageName + "_" + credential.GetNetworkCredential().Domain + "_" + credential.GetNetworkCredential().UserName;
            else
                return serverName + "_" + threeLetterWindowsLanguageName;
        }

        private static string GenereateUniqSessionId(EnterpriseManagementConnectionSettings settings)
        {
            if (!string.IsNullOrEmpty(settings.UserName) && !string.IsNullOrEmpty(settings.Domain))
                return settings.ServerName + "_" + settings.ThreeLetterWindowsLanguageName + "_" + settings.Domain + "_" + settings.UserName;
            else
                return settings.ServerName + "_" + settings.ThreeLetterWindowsLanguageName;
        }
    }
}
