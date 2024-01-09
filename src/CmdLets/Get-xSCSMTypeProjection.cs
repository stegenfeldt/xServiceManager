using System;
using System.Globalization;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;
using System.Reflection;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get, "xSCSMTypeProjection")]
    public class GetSMTypeProjectionCommand : EntityTypeHelper
    {
        private Guid _id = Guid.Empty;
        [Parameter]
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private SwitchParameter _noAdapt;
        [Parameter]
        public SwitchParameter NoAdapt
        {
            get { return _noAdapt; }
            set { _noAdapt = value; }
        }

        protected override void ProcessRecord()
        {
            if (Id != Guid.Empty)
            {
                try { WriteObject(_mg.EntityTypes.GetTypeProjection(Id)); }
                catch (ObjectNotFoundException e) { WriteError(new ErrorRecord(e, "TypeProjection not found", ErrorCategory.ObjectNotFound, Id)); }
                catch (Exception e) { WriteError(new ErrorRecord(e, "Unknown error", ErrorCategory.NotSpecified, Id)); }
            }
            else
            {
                Regex r = new Regex(Name, RegexOptions.IgnoreCase);
                foreach (ManagementPackTypeProjection o in _mg.EntityTypes.GetTypeProjections())
                {
                    if (r.Match(o.Name).Success)
                    {
                        if (NoAdapt)
                        {
                            WriteObject(o);
                        }
                        else
                        {
                            PSObject AP = AdaptProjection(o);
                            WriteObject(AP);
                        }
                    }
                }
            }
        }
        public PSObject AdaptProjection(ManagementPackTypeProjection projection)
        {
            PSObject o = new PSObject();
            o.Members.Add(new PSNoteProperty("__base", projection));
            o.Members.Add(new PSScriptMethod("GetAsXml", ScriptBlock.Create("[xml]($this.__base.CreateNavigator().OuterXml)")));
            o.TypeNames.Insert(0, projection.GetType().FullName);
            o.TypeNames.Insert(0, projection.Name);
            Type T = projection.GetType();
            foreach (PropertyInfo pi in T.GetProperties())
            {
                // no need to catch - just get what you can
                // I've seen problems with Item
                try
                {
                    o.Members.Add(new PSNoteProperty(pi.Name, T.InvokeMember(pi.Name, BindingFlags.GetProperty, null, projection, null, CultureInfo.CurrentCulture)));
                }
                catch
                {
                    ;
                }
            }
            return o;
        }

    }
}