using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Common;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{

    [Cmdlet(VerbsCommon.Set, "xSCSMIncident", SupportsShouldProcess = true)]
    public class SCSMIncidentSet : SMCmdletBase
    {
        #region Parameters
        private String _ID = null;
        private String _Description = null;
        private String _Impact = null;
        private String _Urgency = null;
        private String _Status = null;
        private String _Comment = null;
        private String _UserComment = null;
        private String _Classification = null;
        private String _Source = null;
        private String _AttachmentPath = null;
        private String _SupportGroup = null;
        // private String _ServerName = "localhost";
        private EnterpriseManagementObjectProjection[] _InputObject;
        private ManagementPackClass clsIncident;
        private ManagementPack systemMp;
        private bool hasChanged = false;
        [Parameter(Position = 0,
            ParameterSetName = "ID",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The id of the incident to update.")]

        [ValidateNotNullOrEmpty]
        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        [Parameter(Position = 0,
           ParameterSetName = "InputObject",
           Mandatory = true,
           ValueFromPipeline = true)]
        public EnterpriseManagementObjectProjection[] InputObject
        {
            get { return this._InputObject; }
            set { _InputObject = value; hasChanged = true; }
        }

        [Parameter(Position = 1,
            HelpMessage = "The description of the incident")]

        [ValidateNotNullOrEmpty]
        public string Description
        {
            get { return _Description; }
            set { _Description = value; hasChanged = true; }
        }

        [Parameter(Position = 2,
            HelpMessage = "The Impact of the incident (Low/Medium/High)")]

        [ValidateNotNullOrEmpty]
        [ValidateSet("Low","Medium","High")]
        public string Impact
        {
            get { return _Impact; }
            set { _Impact = value; hasChanged = true; }
        }

        [Parameter(Position = 3,
            HelpMessage = "The Urgency of the incident (Low/Medium/High)")]

        [ValidateNotNullOrEmpty]
        [ValidateSet("Low","Medium","High")]
        public string Urgency
        {
            get { return _Urgency; }
            set { _Urgency = value; hasChanged = true; }
        }

        [Parameter(Position = 4,
            HelpMessage = "The Status of the incident (Active/Pending/Resolved/Closed)")]
        [ValidateNotNullOrEmpty]
        public string Status
        {
            get { return _Status; }
            set { _Status = value; hasChanged = true; }
        }

        [Parameter(Position = 5,
            HelpMessage = "A comment that will be added to the action log")]
        [ValidateNotNullOrEmpty]
        public string Comment
        {
            get { return _Comment; }
            set { _Comment = value; hasChanged = true; }
        }

        [Parameter(HelpMessage = "A user comment that will be added to the action log")]
        public string UserComment
        {
            get { return _UserComment; }
            set { _UserComment = value; hasChanged = true; }
        }

        [Parameter(Position = 6,
            HelpMessage = "A path to the file you want to attach to the incident")]
        [ValidateNotNullOrEmpty]
        public string AttachmentPath
        {
            get { return _AttachmentPath; }
            set { _AttachmentPath = value; hasChanged = true; }
        }

        [Parameter(Position = 7, HelpMessage = "Incident source")]
        [ValidateNotNullOrEmpty]
        public string Source
        {
            get { return _Source; }
            set { _Source = value; hasChanged = true; }
        }

        [Parameter(Position = 8, HelpMessage = "Incident classification")]
        [ValidateNotNullOrEmpty]
        public string Classification
        {
            get { return _Classification; }
            set { _Classification = value; hasChanged = true; }
        }

        [Parameter]
        public string SupportGroup
        {
            get { return _SupportGroup; }
            set { _SupportGroup = value; hasChanged = true; }
        }


        #endregion
        private ManagementPack workitemMp;
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            workitemMp = _mg.ManagementPacks.GetManagementPacks(new ManagementPackCriteria("Name = 'System.WorkItem.Library'")).First();
        }

        protected override void ProcessRecord()
        {
            // If nothing has changed, stop
            if ( ! hasChanged ) 
            { 
                ThrowTerminatingError(
                    new ErrorRecord(
                        new ArgumentException("No values have changed, please provide new values for the incident"), 
                        "Incident", 
                        ErrorCategory.InvalidOperation, 
                        "NoNewValues")
                    );
            }
            try
            {
                systemMp = _mg.ManagementPacks.GetManagementPack(SystemManagementPack.System);

                try
                {
                    clsIncident = SMHelpers.GetManagementPackClass(ClassTypes.System_WorkItem_Incident, SMHelpers.GetManagementPack(ManagementPacks.System_WorkItem_Incident_Library, _mg), _mg);
                }
                catch
                {
                    try
                    {
                        // last ditch try to get a class (this happens if a debug build is used
                        clsIncident = _mg.EntityTypes.GetClasses(new ManagementPackClassCriteria("Name = 'System.WorkItem.Incident'")).First();
                    }
                    catch (Exception e)
                    {
                        ThrowTerminatingError(new ErrorRecord(e, "badclass", ErrorCategory.ObjectNotFound, "System.WorkItem.Incident"));
                    }
                    
                }

                int i = 1;
                if (InputObject != null)
                {
                    foreach (var item in this.InputObject)
                    {
                        ProgressRecord prog = new ProgressRecord(1, "Updating " + item.Object.DisplayName, String.Format("{0} of {1}", i, this.InputObject.Length.ToString()));
                        WriteProgress(prog);
                        SMHelpers.UpdateIncident(_mg,clsIncident,item,this.Impact,this.Urgency,this.Status,this.Classification,this.Source,this.SupportGroup, this.Comment,this.UserComment,this.Description,this.AttachmentPath);
                        i++;
                    }                    
                }
                else
                {
                    WriteDebug("No input object passed");
                    if (this._ID!=null)
                    {
                        UpdateNamedIncident(_mg, this._ID);
                    }
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(ex, "BadSet", ErrorCategory.InvalidOperation, InputObject));
            }
        }

        private void UpdateNamedIncident(EnterpriseManagementGroup emg, string id)
        {
            // Define the type projection that you want to query for.
            // This example queries for type projections defined by the System.WorkItem.Incident.ProjectionType
            // type in the ServiceManager.IncidentManagement.Library management pack.
            ManagementPackTypeProjection incidentTypeProjection = SMHelpers.GetManagementPackTypeProjection(TypeProjections.System_WorkItem_Incident_ProjectionType, SMHelpers.GetManagementPack(ManagementPacks.ServiceManager_IncidentManagement_Library, emg), emg);

            // Define the query criteria string. 
            // This is XML that validates against the Microsoft.EnterpriseManagement.Core.Criteria schema.              
            string incidentCriteria = String.Format(@"
                <Criteria xmlns=""http://Microsoft.EnterpriseManagement.Core.Criteria/"">
                  <Reference Id=""System.WorkItem.Library"" PublicKeyToken=""{0}"" Version=""{1}"" Alias=""WorkItem"" />
                      <Expression>
                            <SimpleExpression>
                              <ValueExpressionLeft>
                                <Property>$Context/Property[Type='WorkItem!System.WorkItem']/Id$</Property>
                              </ValueExpressionLeft>
                              <Operator>Equal</Operator>
                              <ValueExpressionRight>
                                <Value>" + _ID + @"</Value>
                              </ValueExpressionRight>
                            </SimpleExpression>
                      </Expression>
                </Criteria>
                ", workitemMp.KeyToken, workitemMp.Version.ToString());


            WriteDebug(incidentCriteria);
            // Define the criteria object by using one of the criteria strings.
            ObjectProjectionCriteria criteria = new ObjectProjectionCriteria(incidentCriteria, incidentTypeProjection, emg);

            EnterpriseManagementObjectProjection emop = null;

            // For each retrieved type projection, display the properties.
            foreach (EnterpriseManagementObjectProjection projection in
                emg.EntityObjects.GetObjectProjectionReader<EnterpriseManagementObject>(criteria, ObjectQueryOptions.Default))
            {
                emop = projection;
            }

            if (emop != null)
            {
                SMHelpers.UpdateIncident(emg, clsIncident, emop, this.Impact, this.Urgency, this.Status, this.Classification, this.Source, this.SupportGroup, this.Comment, this.UserComment, this.Description, this.AttachmentPath);
            }
            else
            {
                WriteError(new ErrorRecord(new ObjectNotFoundException(_ID), "Incident not found", ErrorCategory.ObjectNotFound, criteria));
                return;
            }
        }

        
    }
}
