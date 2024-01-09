using System;
using System.Management.Automation;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.ConnectorFramework;
using Microsoft.EnterpriseManagement.Common;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.New, "xSCSMIncident", SupportsShouldProcess = true)]
    public class SCSMIncidentNew : SMCmdletBase
    {
        #region Parameters
        private String _Title = null;
        private String _Description = null;
        private String _Impact = null;
        private String _Urgency = null;
        private String _Status = "active";
        private String _Classification = null;
        private String _Source = "console";
        private EnterpriseManagementObjectProjection[] _AffectedCIs;
        private String _AffectedUser = null;
        private String _SupportGroup = null;


        [Parameter(Position = 0,
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The title of the incident.")]

        [ValidateNotNullOrEmpty]
        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }

        [Parameter(Position = 1,
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The description of the incident")]

        [ValidateNotNullOrEmpty]
        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        [Parameter(Position = 2,
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The Impact of the incident (Low/Medium/High)")]

        [ValidateNotNullOrEmpty]
        [ValidateSet("Low","Medium","High")]
        public string Impact
        {
            get { return _Impact; }
            set { _Impact = value; }
        }

        [Parameter(Position = 3,
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The Urgency of the incident (Low/Medium/High)")]

        [ValidateNotNullOrEmpty]
        [ValidateSet("Low","Medium","High")]
        public string Urgency
        {
            get { return _Urgency; }
            set { _Urgency = value; }
        }

        [Parameter(Position = 4,
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The \"Classification\" of the incident (X/Y/Z)")]

        [ValidateNotNullOrEmpty]
        public string Classification
        {
            get { return _Classification; }
            set { _Classification = value; }
        }

        [Parameter(Position = 5,
        Mandatory = false,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The \"Status\" of the incident (X/Y/Z)")]

        [ValidateNotNullOrEmpty]
        public string Status
        {
            get { return _Status; }
            set { _Status = value; }
        }

        [Parameter(Position = 6,
        Mandatory = false,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The \"Source\" of the incident (Console/Portal/X/Y/Z)")]
        [ValidateNotNullOrEmpty]
        public string Source
        {
            get { return _Source; }
            set { _Source = value; }
        }

        /*
        [Parameter(Position = 7,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Provide if you don't want the cmdlet to work against localhost.")]
        [ValidateNotNullOrEmpty]
        public string ServerName
        {
            get { return _ServerName; }
            set { _ServerName = value; }
        }
        */

        [Parameter(Position = 8,
        Mandatory = false,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The \"Affected User\" of the incident. Syntax: \"Domain\\userid\"")]
        [ValidateNotNullOrEmpty]
        public string AffectedUser
        {
            get { return _AffectedUser; }
            set { _AffectedUser = value; }
        }

        [Parameter]
        public string SupportGroup
        {
            get { return _SupportGroup; }
            set { _SupportGroup = value; }
        }

        [Parameter(
           Mandatory = false,
           ValueFromPipeline = true)]
        public EnterpriseManagementObjectProjection[] AffectedCIs
        {
            get { return this._AffectedCIs; }
            set { _AffectedCIs = value; }
        }

        private SwitchParameter _passThru;
        [Parameter]
        public SwitchParameter PassThru
        {
            get { return _passThru; }
            set { _passThru = value; }
        }

        private IncrementalDiscoveryData idd;
        private int batchSize = 200;
        private int toCommit = 0;
        private SwitchParameter _bulk;
        [Parameter]
        public SwitchParameter Bulk
        {
            get { return _bulk; }
            set { _bulk = value; }
        }

        private DateTime? _createdDate;
        [Parameter]
        public DateTime? CreatedDate
        {
            get { return _createdDate; }
            set { _createdDate = value; }
        }

        #endregion

        private string incidentPrefix = "IR{0}";
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if ( Bulk )
            {
                idd = new IncrementalDiscoveryData();
            }
            // If you can't get the prefix, just prepend 'IR'
            try
            {
                ManagementPackClass incidentSettings = SMHelpers.GetManagementPackClass("System.WorkItem.Incident.GeneralSetting", SMHelpers.GetManagementPack("ServiceManager.IncidentManagement.Library", _mg), _mg );
                EnterpriseManagementObject incidentSettingsInstance = _mg.EntityObjects.GetObject<EnterpriseManagementObject>(incidentSettings.Id, ObjectQueryOptions.Default);
                incidentPrefix = incidentSettingsInstance[null, "PrefixForId"].Value.ToString() + "{0}";
            }
            catch
            {
                // do nothing - incidentPrefix was already set
                ;
            }
            
        }

        protected override void ProcessRecord()
        {
            try
            {
                ManagementPackClass clsIncident = SMHelpers.GetManagementPackClass(ClassTypes.System_WorkItem_Incident, SMHelpers.GetManagementPack(ManagementPacks.System_WorkItem_Incident_Library, _mg), _mg);

                EnterpriseManagementObjectProjection incidentProjection = new EnterpriseManagementObjectProjection(_mg, clsIncident);

                WriteVerbose("Setting basic properties");
                incidentProjection.Object[clsIncident, "Id"].Value = incidentPrefix;

                incidentProjection.Object[clsIncident, "Title"].Value = this.Title;

                if ( CreatedDate != null )
                {
                    incidentProjection.Object[clsIncident, "CreatedDate"].Value = this.CreatedDate;
                }

                SMHelpers.UpdateIncident(_mg, clsIncident, incidentProjection,
                    this.Impact, this.Urgency, this.Status, this.Classification, this.Source, this.SupportGroup, null, null, this.Description, null);


                if (AffectedCIs!=null)
                {
                    WriteVerbose("Adding affected CIs");
                    foreach (var item in AffectedCIs)
                    {
                        WriteVerbose(string.Format("Adding {0} as affected configuration item.", item.Object.DisplayName));
                        SMHelpers.AddAffectedCI(incidentProjection, item.Object, _mg);
                    }
                }                

                if (AffectedUser != null)
                {
                    WriteVerbose(string.Format("Adding {0} as affected configuration item.", AffectedUser));
                    SMHelpers.AddAffectedUser(incidentProjection, this.AffectedUser, _mg);
                }

                // a bulk operation
                // do in batches of toCommit (set above)
                if ( Bulk )
                {
                    toCommit++;
                    idd.Add(incidentProjection);
                    if ( toCommit >= batchSize )
                    {
                        idd.Commit(_mg);
                        idd = new IncrementalDiscoveryData();
                        toCommit = 0;
                    }
                }
                else
                {
                    incidentProjection.Commit();
                }

                if ( PassThru )
                {
                    //Pass the new object to the pipeline
                    WriteObject(incidentProjection);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "NewIncident", ErrorCategory.InvalidOperation, Title));
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            if ( Bulk && toCommit > 0)
            {
                idd.Commit(_mg);
            }
        }

        private void RegisterNewIncident(EnterpriseManagementGroup emg, ManagementPackClass clsIncident, EnterpriseManagementObject AffectedCIs)
        {
            
        }
    }
}
