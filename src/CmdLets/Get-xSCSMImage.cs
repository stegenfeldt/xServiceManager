using System;
using System.IO;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMImage", DefaultParameterSetName = "Name")]
    public class GetSCSMImageCommand : PresentationCmdletBase
    {

        private SwitchParameter _listOnly;
        [Parameter]
        public SwitchParameter ListOnly
        {
            get { return _listOnly; }
            set { _listOnly = value; }
        }
 
        private IList<ManagementPackImage> list;
        private string _currentDirectory;
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            list = _mg.Resources.GetResources<ManagementPackImage>();
            _currentDirectory = SessionState.Path.CurrentFileSystemLocation.Path;
            if (ListOnly)
            {
                foreach (string p in Name)
                {
                    //WildcardPattern pattern = new WildcardPattern(p, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
                    Regex r = new Regex(p, RegexOptions.IgnoreCase);
                    foreach (ManagementPackImage i in list)
                    {
                        if (r.Match(i.FileName).Success && !i.HasNullStream)
                        {
                            WriteObject(i);
                        }
                    }
                }
            }
        }
        protected override void ProcessRecord()
        {
            if (ListOnly)
            {
                return;
            }
            foreach (string p in Name)
            {
                //WildcardPattern pattern = new WildcardPattern(p, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
                Regex r = new Regex(p, RegexOptions.IgnoreCase);
                foreach (ManagementPackImage v in list)
                {
                    if (r.Match(v.FileName).Success && !v.HasNullStream)
                    {
                        if (ShouldProcess(v.FileName))
                        {
                            try
                            {
                                string outputFile = String.Format("{0}\\{1}", _currentDirectory, v.FileName);
                                WriteVerbose("output filename: " + outputFile);
                                Stream s = _mg.Resources.GetResourceData(v.Id);
                                byte[] b = new byte[s.Length];
                                s.Read(b, 0, (int)s.Length);
                                s.Close();
                                s.Dispose();
                                FileStream fs = new FileStream(outputFile, FileMode.Create);
                                fs.Write(b, 0, b.Length);
                                fs.Close();
                                fs.Dispose();
                                WriteObject(SessionState.InvokeCommand.InvokeScript("Get-ChildItem " + outputFile));
                            }
                            catch (Exception e)
                            {
                                WriteError(new ErrorRecord(e, "Save Image", ErrorCategory.InvalidOperation, v));
                            }
                        }
                    }
                }
            }
        }
    }
}
