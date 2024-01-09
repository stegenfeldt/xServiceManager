using System;
using System.Reflection;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Get,"xSCSMManagementPackElement")]
    public class GetManagementPackElementCommand : SMCmdletBase
    {
        // Parameters

        private Guid _id;
        [Parameter(Position=0,Mandatory=true,ValueFromPipelineByPropertyName=true)]
        public Guid Id
        {
            get {return _id; }
            set { _id = value; }
        }

        protected override void ProcessRecord()
        {
            Type t = Type.GetType("Microsoft.EnterpriseManagement.Configuration.ManagementPackElementReference`1, Microsoft.EnterpriseManagement.Core, Version=7.0.5000.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            Type [] Targs = { _mg.GetType(), typeof(Guid) };
            Type t2 = t.MakeGenericType(typeof(ManagementPackElement));
            ConstructorInfo ci = t2.GetConstructor(BindingFlags.NonPublic|BindingFlags.Instance,null,Targs,null);
            object[] myargs = { _mg, Id };
            ManagementPackElement r = ((ManagementPackElementReference<ManagementPackElement>)ci.Invoke(myargs)).GetElement();
            WriteObject(r);
        }
    }
}
