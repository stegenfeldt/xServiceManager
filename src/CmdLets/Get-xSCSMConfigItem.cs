using System;
using System.Text;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    /// <summary>
    /// Represents a cmdlet to retrieve Service Manager configuration items based on their display name.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "xSCSMConfigItem", SupportsShouldProcess = true)]
    public class SCSMConfigItemGet : SMCmdletBase
    {
        private String _DisplayName = null;
        private String _TargetProjection = "8ab27adb-13b1-2b7b-56e6-91598417cbee";

        [Parameter(Position = 0,
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The display name of the config item.")]

        [ValidateNotNullOrEmpty]
        public string DisplayName
        {
            get { return _DisplayName; }
            set { _DisplayName = value; }
        }

        [Parameter(Position = 1,
        Mandatory = false,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The display name of the config item.")]

        [ValidateNotNullOrEmpty]
        public string TargetProjection
        {
            get { return _TargetProjection; }
            set { _TargetProjection = value; }
        }

        protected override void ProcessRecord()
        {
            ManagementPackTypeProjection targetProjection = _mg.EntityTypes.GetTypeProjection(new Guid(TargetProjection));

            ManagementPack trgProjMp = targetProjection.GetManagementPack();

            ManagementPack systemMp = _mg.ManagementPacks.GetManagementPack(SystemManagementPack.System);

            WriteVerbose("Starting to build search criteria...");
            List<string> criterias = new List<string>();

            // Define the query criteria string. 
            // This is XML that validates against the Microsoft.EnterpriseManagement.Core.Criteria schema.              
            StringBuilder configCriteria = new StringBuilder(String.Format(@"
                <Criteria xmlns=""http://Microsoft.EnterpriseManagement.Core.Criteria/"">
                  <Reference Id=""System.Library"" PublicKeyToken=""{0}"" Version=""{1}"" Alias=""targetMp"" />
                      <Expression>", systemMp.KeyToken, systemMp.Version.ToString()));

            if (this._DisplayName != null)
            {
                WriteVerbose(string.Format("Adding \"DisplayName like {0}\" to search criteria", this.DisplayName));
                criterias.Add(@"<SimpleExpression>
                                    <ValueExpressionLeft>
                                    <Property>$Context/Property[Type='targetMp!System.ConfigItem']/DisplayName$</Property>
                                    </ValueExpressionLeft>
                                    <Operator>Like</Operator>
                                    <ValueExpressionRight>
                                    <Value>" + this.DisplayName + @"</Value>
                                    </ValueExpressionRight>
                                </SimpleExpression>");
            }

            if (criterias.Count > 1)
            {
                for (int i = 0; i < criterias.Count; i++)
                {
                    criterias[i] = "<Expression>" + criterias[i] + "</Expression>";
                }
            }

            if (criterias.Count > 1)
            {
                configCriteria.AppendLine("<And>");
            }

            foreach (var item in criterias)
            {
                configCriteria.AppendLine(item);
            }

            if (criterias.Count > 1)
            {
                configCriteria.AppendLine("</And>");
            }

            configCriteria.AppendLine(@"</Expression>
                </Criteria>");

            WriteDebug("Search criteria: " + configCriteria.ToString());

            // Define the criteria object by using one of the criteria strings.
            ObjectProjectionCriteria criteria = new ObjectProjectionCriteria(configCriteria.ToString(),
                targetProjection, _mg);

            // For each retrieved type projection, display the properties.
            List<EnterpriseManagementObjectProjection> result = new List<EnterpriseManagementObjectProjection>();
            foreach (EnterpriseManagementObjectProjection projection in
                _mg.EntityObjects.GetObjectProjectionReader<EnterpriseManagementObject>(criteria, ObjectQueryOptions.Default))
            {
                WriteVerbose(String.Format("Adding config item \"{0}\" to the pipeline", projection.Object.DisplayName));
                WriteObject(projection, false);
            }
        }
    }
}