using System;
using System.Xml;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Monitoring;

namespace xServiceManager.Module
{
    //FIXME xSCSMTaskResult is not working after porting to new .net
    // [Cmdlet(VerbsCommon.Get, "xSCSMTaskResult", DefaultParameterSetName = "Guid")]
    // public class GetSMTaskResultCommand : ObjectCmdletHelper
    // {
    //     private Guid _id = Guid.Empty;
    //     [Parameter(ParameterSetName = "Guid", Position = 0)]
    //     public Guid BatchId
    //     {
    //         get { return _id; }
    //         set { _id = value; }
    //     }
    //     private TaskResultCriteria _criteria = null;
    //     [Parameter(ParameterSetName = "Criteria", Position = 0)]
    //     public TaskResultCriteria Criteria
    //     {
    //         get { return _criteria; }
    //         set { _criteria = value; }
    //     }

    //     // since the TaskRuntime moved to the ServiceManagementGroup, we need to create one of those
    //     // from our current connection
    //     private ServiceManagementGroup smg;
    //     protected override void BeginProcessing()
    //     {
    //         base.BeginProcessing();
    //         ServiceManagementConnectionSettings cSetting = new ServiceManagementConnectionSettings(_mg.ConnectionSettings.ServerName);
    //         cSetting.UserName = _mg.ConnectionSettings.UserName;
    //         cSetting.Domain = _mg.ConnectionSettings.Domain;
    //         cSetting.Password = _mg.ConnectionSettings.Password;
    //         smg = new ServiceManagementGroup(cSetting);
    //     }
    //     protected override void ProcessRecord()
    //     {
    //         if (BatchId == Guid.Empty && Criteria == null)
    //         {
    //             foreach (TaskResult r in smg.TaskRuntime.GetTaskResults())
    //             {
    //                 PSObject o = new PSObject(r);
    //                 try
    //                 {
    //                     XmlDocument x = new XmlDocument();
    //                     x.LoadXml(r.Output);
    //                     o.Members.Add(new PSNoteProperty("OutputXML", x));
    //                 }
    //                 catch
    //                 {
    //                     WriteVerbose("Cannot cast output to XML, ignoring");
    //                 }
    //                 WriteObject(o);
    //             }
    //         }
    //         else if (BatchId != Guid.Empty)
    //         {
    //             WriteObject(smg.TaskRuntime.GetTaskResultsByBatchId(BatchId));
    //         }
    //         // If someone provides us a filter, we'll use that instead of a criteria
    //         else if (Criteria != null)
    //         {
    //             foreach (TaskResult r in smg.TaskRuntime.GetTaskResults(Criteria))
    //             {
    //                 WriteObject(r);
    //             }
    //         }
    //     }
    // }
}
