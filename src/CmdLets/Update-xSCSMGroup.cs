using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Configuration.IO;
using Microsoft.EnterpriseManagement.ConnectorFramework;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    /// <summary>
    /// This cmdlet updates a group
    /// </summary>
    /// 
    /*  Commenting out until it is completed
    [Cmdlet(VerbsCommon.Update, "xSCSMGroup")]
    public class UpdateSCGroupCommand : ObjectCmdletHelper
    {
        private EnterpriseManagementGroupObject _group;
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public EnterpriseManagementGroupObject Group
        {
            get { return _group; }
            set { _group = value; }
        }
        override protected void ProcessRecord()
        {
            ThrowTerminatingError(new ErrorRecord(new NotImplementedException(), "Not yet", ErrorCategory.InvalidOperation, this));
        }

    }
    */ 
}