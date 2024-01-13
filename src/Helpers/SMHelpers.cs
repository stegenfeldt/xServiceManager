using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Text;

namespace xServiceManager.Module
{
    public static class SMHelpers
    {
        public static bool GuidTryParse(string s, out Guid result)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            Regex format = new Regex(
                "^[A-Fa-f0-9]{32}$|" +
                "^({|\\()?[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}(}|\\))?$|" +
                "^({)?[0xA-Fa-f0-9]{3,10}(, {0,1}[0xA-Fa-f0-9]{3,6}){2}, {0,1}({)([0xA-Fa-f0-9]{3,4}, {0,1}){7}[0xA-Fa-f0-9]{3,4}(}})$");
            Match match = format.Match(s);
            if (match.Success)
            {
                result = new Guid(s);
                return true;
            }
            else
            {
                result = Guid.Empty;
                return false;
            }
        }

        public static ManagementPackEnumeration GetEnum(string identifier, ManagementPackEnumeration etype)
        {
            // Try really hard to find an enumeration based on a parent enumeration
            // Match in the following order:
            //   ID
            //   Name
            //   The last token of the Name after the last "."
            //   The DisplayName
            //   A regex match of the name
            //   A regex match of the DisplayName
            // If any of those is found, return the first one you succeed in matching
            // if you still can't find a match, then throw and provide a helping message

            if (etype == null) { throw new ArgumentException("Base Enumeration is null"); }
            try
            {
                Regex r = new Regex(identifier, RegexOptions.IgnoreCase);
                Regex removeToDot = new Regex(".*\\.");
                foreach (ManagementPackEnumeration e in etype.ManagementGroup.EntityTypes.GetChildEnumerations(etype.Id, TraversalDepth.Recursive))
                {
                    if (String.Compare(identifier, e.Id.ToString(), true) == 0) { return e; }
                    if (String.Compare(identifier, e.Name, true) == 0) { return e; }
                    if (String.Compare(identifier, removeToDot.Replace(e.Name, ""), true) == 0) { return e; }
                    if (String.Compare(identifier, e.DisplayName, true) == 0) { return e; }
                    if (r.Match(e.Name).Success == true) { return e; }
                    if (r.Match(e.DisplayName).Success == true) { return e; }
                }
            }
            catch
            {
                ;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("Could not find '" + identifier + "' in enumeration '");
            sb.Append(etype.DisplayName);
            sb.Append("'. Allowed values are:\n");
            foreach (ManagementPackEnumeration e in etype.ManagementGroup.EntityTypes.GetChildEnumerations(etype.Id, TraversalDepth.Recursive))
            {
                sb.Append("\t '" + e.DisplayName + "' (" + e.Name + ")\n");
            }
            throw new ArgumentException(sb.ToString());
        }

        public static ManagementPackEnumeration GetEnum(string identifier, EnterpriseManagementGroup emg)
        {
            // This one is very strict - it will only match the name and really targeted at helper methods
            foreach (ManagementPackEnumeration e in emg.EntityTypes.GetEnumerations())
            {
                if (string.Compare(e.Name, identifier, true) == 0)
                {
                    return e;
                }
            }
            return null;
        }

        public static int DeterminePriority(ManagementPackEnumeration Urgency, ManagementPackEnumeration Impact, EnterpriseManagementGroup _mg)
        {
            // give it a good try, if something goes wrong, just return 5
            try
            {
                EnterpriseManagementGroup emg = Urgency.ManagementGroup;
                ManagementPackClass settingClass = SMHelpers.GetManagementPackClass(ClassTypes.System_WorkItem_Incident_GeneralSetting, SMHelpers.GetManagementPack(ManagementPacks.ServiceManager_IncidentManagement_Library, _mg), _mg);
                EnterpriseManagementObject settings = emg.EntityObjects.GetObject<EnterpriseManagementObject>(settingClass.Id, ObjectQueryOptions.Default);
                XmlDocument x = new XmlDocument();
                string xmlstring = settings[null, "PriorityMatrix"].Value as string;

                x.LoadXml(xmlstring);
                string u = Urgency.Id.ToString();
                string i = Impact.Id.ToString();
                string xpath = String.Format("/Matrix/U[@Id='{0}']/I[@Id='{1}']/P", u, i);
                XmlNode s = x.SelectSingleNode(xpath);
                string ts = s.InnerText;
                return Int32.Parse(ts);

            }
            catch
            {
                // Try to do something sensible, compute some sensible defaults
                try
                {
                    Dictionary<string, int> matrix = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
                    matrix.Add("Low", 1);
                    matrix.Add("Medium", 2);
                    matrix.Add("High", 3);
                    int v = (3 * (matrix[Urgency.DisplayName] - 1)) + matrix[Impact.DisplayName];
                    return v;
                }
                catch // nevermind, return 5
                {
                    return 5;
                }
            }
        }

        public static void UpdateIncident(EnterpriseManagementGroup emg, ManagementPackClass clsIncident, EnterpriseManagementObjectProjection emop,
            string impact, string urgency, string status, string classification, string source, string supportGroup, string comment, string userComment, string description, string attachmentPath)
        {
            FileStream item = null;

            try
            {

                // get the BaseEnums, these are used later!
                ManagementPackEnumeration impactBase = SMHelpers.GetEnum("System.WorkItem.TroubleTicket.ImpactEnum", emg);
                ManagementPackEnumeration urgencyBase = SMHelpers.GetEnum("System.WorkItem.TroubleTicket.UrgencyEnum", emg);
                ManagementPackEnumeration statusBase = SMHelpers.GetEnum("IncidentStatusEnum", emg);
                ManagementPackEnumeration classificationBase = SMHelpers.GetEnum("IncidentClassificationEnum", emg);
                ManagementPackEnumeration sourceBase = SMHelpers.GetEnum("IncidentSourceEnum", emg);
                ManagementPackEnumeration supportGroupBase = SMHelpers.GetEnum("IncidentTierQueuesEnum", emg);

                ManagementPackEnumeration impactEnum = null;
                ManagementPackEnumeration urgencyEnum = null;
                ManagementPackEnumeration statusEnum = null;
                ManagementPackEnumeration classificationEnum = null;
                ManagementPackEnumeration sourceEnum = null;
                ManagementPackEnumeration supportGroupEnum = null;

                //If impact supplies, update the current value
                if (impact != null)
                {
                    impactEnum = SMHelpers.GetEnum(impact, impactBase);
                    emop.Object[clsIncident, "Impact"].Value = impactEnum.Id;
                }

                //If value supplied, update the current value
                if (urgency != null)
                {
                    urgencyEnum = SMHelpers.GetEnum(urgency, urgencyBase);
                    emop.Object[clsIncident, "Urgency"].Value = urgencyEnum.Id;
                }

                //If value supplied, update the current value
                if (status != null)
                {
                    statusEnum = SMHelpers.GetEnum(status, statusBase);
                    emop.Object[clsIncident, "Status"].Value = statusEnum.Id;
                    if (String.Compare(status, "Closed", true) == 0)
                    {
                        emop.Object[clsIncident, "ClosedDate"].Value = DateTime.Now.ToUniversalTime();
                    }
                    else if (String.Compare(status, "Resolved") == 0)
                    {
                        emop.Object[clsIncident, "ResolvedDate"].Value = DateTime.Now.ToUniversalTime();
                    }
                }

                //If source supplied, update the current value
                if (source != null)
                {
                    sourceEnum = SMHelpers.GetEnum(source, sourceBase);
                    emop.Object[clsIncident, "Source"].Value = sourceEnum.Id;
                }

                //If classification supplied, update the current value
                if (classification != null)
                {
                    classificationEnum = SMHelpers.GetEnum(classification, classificationBase);
                    emop.Object[clsIncident, "Classification"].Value = classificationEnum.Id;
                }

                if (supportGroup != null)
                {
                    supportGroupEnum = SMHelpers.GetEnum(supportGroup, supportGroupBase);
                    emop.Object[clsIncident, "TierQueue"].Value = supportGroupEnum.Id;
                }

                //Description supplied, update the current value
                if (description != null)
                {
                    emop.Object[clsIncident, "Description"].Value = description;
                }

                //If comment supplied, update the current value
                if (comment != null)
                {
                    ManagementPackClass analystCommentClass = GetManagementPackClass(ClassTypes.System_WorkItem_TroubleTicket_AnalystCommentLog, GetManagementPack(ManagementPacks.System_WorkItem_Library, emg), emg);
                    CreatableEnterpriseManagementObject analystComment = new CreatableEnterpriseManagementObject(emg, analystCommentClass);
                    analystComment[analystCommentClass, "Id"].Value = Guid.NewGuid().ToString();// "comment-" + DateTime.Now.ToString(); //Had to change from date time dependent since the speed was an issues
                    analystComment[null, "Comment"].Value = comment;
                    analystComment[null, "EnteredBy"].Value = EnterpriseManagementGroup.CurrentUserName;
                    analystComment[null, "EnteredDate"].Value = DateTime.Now.ToUniversalTime();
                    ManagementPackRelationship incidentEmbedsAnalystComment = SMHelpers.GetManagementPackRelationship(RelationshipTypes.System_WorkItem_TroubleTicketHasAnalystComment, GetManagementPack(ManagementPacks.System_WorkItem_Library, emg), emg);
                    emop.Add(analystComment, incidentEmbedsAnalystComment.Target);
                }

                //If comment supplied, update the current value
                if (userComment != null)
                {
                    ManagementPackClass userCommentClass = GetManagementPackClass(ClassTypes.System_WorkItem_TroubleTicket_UserCommentLog, GetManagementPack(ManagementPacks.System_WorkItem_Library, emg), emg);
                    CreatableEnterpriseManagementObject userCommentObject = new CreatableEnterpriseManagementObject(emg, userCommentClass);
                    userCommentObject[userCommentClass, "Id"].Value = Guid.NewGuid().ToString();// "comment-" + DateTime.Now.ToString(); //Had to change from date time dependent since the speed was an issues
                    userCommentObject[null, "Comment"].Value = userComment;
                    userCommentObject[null, "EnteredBy"].Value = EnterpriseManagementGroup.CurrentUserName;
                    userCommentObject[null, "EnteredDate"].Value = DateTime.Now.ToUniversalTime();
                    ManagementPackRelationship incidentEmbedsUserComment = SMHelpers.GetManagementPackRelationship(RelationshipTypes.System_WorkItem_TroubleTicketHasUserComment, SMHelpers.GetManagementPack(ManagementPacks.System_WorkItem_Library, emg), emg);
                    emop.Add(userCommentObject, incidentEmbedsUserComment.Target);
                }

                //If file path for attachment supplied, attach file
                if (attachmentPath != null)
                {
                    ManagementPackClass fileAttachmentClass = GetManagementPackClass(ClassTypes.System_FileAttachment, GetManagementPack(ManagementPacks.System_SupportingItem_Library, emg), emg);
                    CreatableEnterpriseManagementObject fileAttachment = new CreatableEnterpriseManagementObject(emg, fileAttachmentClass);

                    item = new FileStream(attachmentPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                    string fileName = Path.GetFileName(attachmentPath);

                    fileAttachment[fileAttachmentClass, "Id"].Value = Guid.NewGuid().ToString();
                    fileAttachment[fileAttachmentClass, "DisplayName"].Value = fileName;
                    fileAttachment[fileAttachmentClass, "Description"].Value = fileName;
                    fileAttachment[fileAttachmentClass, "Extension"].Value = Path.GetExtension(attachmentPath);
                    fileAttachment[fileAttachmentClass, "Size"].Value = (int)item.Length;
                    fileAttachment[fileAttachmentClass, "AddedDate"].Value = DateTime.Now.ToUniversalTime();
                    fileAttachment[fileAttachmentClass, "Content"].Value = item;

                    ManagementPackRelationship workItemHasFileAttachment = SMHelpers.GetManagementPackRelationship(
                        RelationshipTypes.System_WorkItemHasFileAttachment,
                        SMHelpers.GetManagementPack(ManagementPacks.System_WorkItem_Library, emg), emg);
                    emop.Add(fileAttachment, workItemHasFileAttachment.Target);

                    //File add Action log
                    ManagementPackClass logCommentClass = GetManagementPackClass(
                        ClassTypes.System_WorkItem_TroubleTicket_ActionLog, GetManagementPack(ManagementPacks.System_WorkItem_Library, emg), emg);
                    CreatableEnterpriseManagementObject logComment = new CreatableEnterpriseManagementObject(emg, logCommentClass);

                    logComment[logCommentClass, "Id"].Value = Guid.NewGuid().ToString();
                    logComment[logCommentClass, "Description"].Value = fileName;
                    logComment[logCommentClass, "EnteredDate"].Value = DateTime.Now.ToUniversalTime();

                    ManagementPackEnumeration enumeration = GetEnum(Enumerations.SystemWorkItemActionLogEnumFileAttached, emg);

                    logComment[logCommentClass, "ActionType"].Value = enumeration;
                    logComment[logCommentClass, "EnteredBy"].Value = Environment.UserName;
                    logComment[logCommentClass, "Title"].Value = enumeration.DisplayName;

                    ManagementPackRelationship fileAttachmentComment = SMHelpers.GetManagementPackRelationship(
                        RelationshipTypes.System_WorkItem_TroubleTicketHasActionLog,
                        GetManagementPack(ManagementPacks.System_WorkItem_Library, emg), emg);

                    emop.Add(logComment, fileAttachmentComment.Target);
                }

                if (urgencyEnum != null && impactEnum != null)
                {
                    int priority = SMHelpers.DeterminePriority(urgencyEnum, impactEnum, emg);
                    emop.Object[clsIncident, "Priority"].Value = priority;
                }

                //Commit all changes
                emop.Commit();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (item != null)
                {
                    item.Close();
                    item.Dispose();
                }
            }
        }

        public static void AddAffectedCI(EnterpriseManagementObjectProjection projection, EnterpriseManagementObject affectedCI,
            EnterpriseManagementGroup mg)
        {
            //Set the created by user to the user who used the web form.
            ManagementPackRelationship affectedItem = SMHelpers.GetManagementPackRelationship(RelationshipTypes.System_WorkItemAboutConfigItem, GetManagementPack(ManagementPacks.System_WorkItem_Library, mg), mg);
            projection.Add(affectedCI, affectedItem.Target);
        }

        public static void AddAffectedUser(EnterpriseManagementObjectProjection projection, string userIdentifier,
            EnterpriseManagementGroup mg)
        {
            EnterpriseManagementObject user = GetUser(mg, userIdentifier);

            //Set the created by user to the user who used the web form.
            ManagementPackRelationship affectedUser = SMHelpers.GetManagementPackRelationship(RelationshipTypes.System_WorkItemAffectedUser, GetManagementPack(ManagementPacks.System_WorkItem_Library, mg), mg);
            projection.Add(user, affectedUser.Target);
        }

        public static EnterpriseManagementObject GetUser(EnterpriseManagementGroup mg, string userIdentifier)
        {
            ManagementPack mpWindows = mg.ManagementPacks.GetManagementPack(SystemManagementPack.Windows);
            ManagementPackClass userClass = GetManagementPackClass(ClassTypes.Microsoft_AD_User, GetManagementPack(ManagementPacks.Microsoft_Windows_Library, mg), mg);

            //Create the criteria XML and criteria object
            string userCriteria = CreateUserCriteriaXml(mg, userIdentifier);
            EnterpriseManagementObjectCriteria criteria = new EnterpriseManagementObjectCriteria(userCriteria, userClass, mpWindows, mg);

            //Retrieve the user that corresponds to the criteria
            IEnumerable<EnterpriseManagementObject> users =
               (IEnumerable<EnterpriseManagementObject>)mg.EntityObjects.GetObjectReader<EnterpriseManagementObject>(criteria, ObjectQueryOptions.Default);

            //Get the enumerator
            IEnumerator<EnterpriseManagementObject> enumUsers = users.GetEnumerator();
            while (enumUsers.MoveNext())
            {
                return enumUsers.Current;
            }

            //If no user was found, throw an exception
            throw new Exception("No user with user identified by: " + userIdentifier + " found in Service Manager");
        }

        public static string CreateUserCriteriaXml(EnterpriseManagementGroup mg, string userIdentifier)
        {
            ManagementPack mpWindows = mg.ManagementPacks.GetManagementPack(SystemManagementPack.Windows);

            string userCriteria = string.Empty;
            // Check the format of the userName to make sure we create the correct filter
            if (userIdentifier.StartsWith("CN="))
            {
                // This is XML that validates against the Microsoft.EnterpriseManagement.Core.Criteria schema.                  
                userCriteria = String.Format(@"
                <Criteria xmlns=""http://Microsoft.EnterpriseManagement.Core.Criteria/"">
                   <Expression>
                    <SimpleExpression>
                      <ValueExpressionLeft>
                        <Property>$Target/Property[Type='Microsoft.AD.User']/DistinguishedName$</Property>
                      </ValueExpressionLeft>
                      <Operator>Equal</Operator>
                      <ValueExpressionRight>
                        <Value>{0}</Value>
                      </ValueExpressionRight>
                    </SimpleExpression>
                  </Expression>
                </Criteria>
                ", userIdentifier);
            }
            else if (userIdentifier.StartsWith("S-1-5"))
            {
                // This is XML that validates against the Microsoft.EnterpriseManagement.Core.Criteria schema.                  
                userCriteria = String.Format(@"
                <Criteria xmlns=""http://Microsoft.EnterpriseManagement.Core.Criteria/"">
                   <Expression>
                    <SimpleExpression>
                      <ValueExpressionLeft>
                        <Property>$Target/Property[Type='Microsoft.AD.User']/SID$</Property>
                      </ValueExpressionLeft>
                      <Operator>Equal</Operator>
                      <ValueExpressionRight>
                        <Value>{0}</Value>
                      </ValueExpressionRight>
                    </SimpleExpression>
                  </Expression>
                </Criteria>
                ", userIdentifier);
            }
            else
            {
                string[] userData = userIdentifier.Split('\\');
                // This is XML that validates against the Microsoft.EnterpriseManagement.Core.Criteria schema.                  
                userCriteria = String.Format(@"
                <Criteria xmlns=""http://Microsoft.EnterpriseManagement.Core.Criteria/"">
                   <Expression>
                    <And>
                    <Expression>
                    <SimpleExpression>
                      <ValueExpressionLeft>
                        <Property>$Target/Property[Type='Microsoft.AD.User']/UserName$</Property>
                      </ValueExpressionLeft>
                      <Operator>Equal</Operator>
                      <ValueExpressionRight>
                        <Value>{1}</Value>
                      </ValueExpressionRight>
                    </SimpleExpression>
                    </Expression>
                    <Expression>
                    <SimpleExpression>
                      <ValueExpressionLeft>
                        <Property>$Target/Property[Type='Microsoft.AD.User']/Domain$</Property>
                      </ValueExpressionLeft>
                      <Operator>Equal</Operator>
                      <ValueExpressionRight>
                        <Value>{0}</Value>
                      </ValueExpressionRight>
                    </SimpleExpression>
                    </Expression>
                    </And>
                  </Expression>
                </Criteria>
                ", userData[0], userData[1]);
            }

            return userCriteria;
        }

        public static string MakeMPElementSafeUniqueIdentifier(string strElementType)
        {
            string[] strDisallowedCharacters = new string[] { "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "_", "-", "+", "=", "{", "}", "[", "]", "|", "\\", ":", "?", "/", "<", ">", ".", "~", ",", "'", "\"", " " };
            string strSafeUniqueIdentifier = String.Format("{0}.{1}", strElementType, Guid.NewGuid().ToString("N"));
            foreach (string strDisallowedCharacter in strDisallowedCharacters)
            {
                strSafeUniqueIdentifier = strSafeUniqueIdentifier.Replace(strDisallowedCharacter, "");
            }
            return (strSafeUniqueIdentifier);
        }

        public static ManagementPack GetManagementPack(string id, EnterpriseManagementGroup emg)
        {
            ManagementPackCriteria mpcriteria = new ManagementPackCriteria(String.Format("Name = '{0}'", id));
            IList<ManagementPack> mps = emg.ManagementPacks.GetManagementPacks(mpcriteria);
            //assuming there is only one - return the first one regardless
            if (mps.Count > 0)
                return (mps[0]);
            else
                return null;
        }

        public static ManagementPackRelationship GetManagementPackRelationship(string id, ManagementPack mp)
        {
            ManagementPackRelationshipCriteria mprcriteria = new ManagementPackRelationshipCriteria(String.Format("Name = '{0}'", id));
#if ( _SERVICEMANAGER_R2_ )
            IList<ManagementPackRelationship> mprelationships = mp.Store.EntityTypes.GetRelationshipClasses(mprcriteria);
#else
            MethodInfo method = mp.GetType().GetMethod("GetManagementGroupObject", BindingFlags.NonPublic | BindingFlags.Instance);
            EnterpriseManagementGroup emg = method.Invoke(mp, null) as EnterpriseManagementGroup;
            IList<ManagementPackRelationship> mprelationships = emg.EntityTypes.GetRelationshipClasses(mprcriteria);
#endif
            foreach (ManagementPackRelationship mprelationship in mprelationships)
            {
                if (mprelationship.GetManagementPack().Id == mp.Id)
                    return mprelationship;
            }
            //Didnt find any matches
            return (null);
        }

        public static ManagementPackRelationship GetManagementPackRelationship(string id, ManagementPack mp, EnterpriseManagementGroup emg)
        {
            ManagementPackRelationshipCriteria mprcriteria = new ManagementPackRelationshipCriteria(String.Format("Name = '{0}'", id));
            IList<ManagementPackRelationship> mprelationships = emg.EntityTypes.GetRelationshipClasses(mprcriteria);
            foreach (ManagementPackRelationship mprelationship in mprelationships)
            {
                if (mprelationship.GetManagementPack().Id == mp.Id)
                    return mprelationship;
            }
            //Didnt find any matches
            return (null);
        }

        public static ManagementPackClass GetManagementPackClass(string id, ManagementPack mp)
        {
            ManagementPackClassCriteria mpccriteria = new ManagementPackClassCriteria(String.Format("Name = '{0}'", id));
#if ( _SERVICEMANAGER_R2_ )
            IList<ManagementPackClass> mpclasses = mp.Store.EntityTypes.GetClasses(mpccriteria);
#else
            MethodInfo method = mp.GetType().GetMethod("GetManagementGroupObject", BindingFlags.NonPublic | BindingFlags.Instance);
            EnterpriseManagementGroup emg = method.Invoke(mp, null) as EnterpriseManagementGroup;
            IList<ManagementPackClass> mpclasses = emg.EntityTypes.GetClasses(mpccriteria);
#endif
            foreach (ManagementPackClass mpclass in mpclasses)
            {
                if (mpclass.GetManagementPack().Id == mp.Id)
                    return (mpclass);
            }
            //Didn't find any matches
            return (null);
        }

        public static ManagementPackClass GetManagementPackClass(string id, ManagementPack mp, EnterpriseManagementGroup emg)
        {
            ManagementPackClassCriteria mpccriteria = new ManagementPackClassCriteria(String.Format("Name = '{0}'", id));
            IList<ManagementPackClass> mpclasses = emg.EntityTypes.GetClasses(mpccriteria);
            foreach (ManagementPackClass mpclass in mpclasses)
            {
                if (mpclass.GetManagementPack().Id == mp.Id)
                    return (mpclass);
            }
            //Didn't find any matches
            return (null);
        }

        public static ManagementPackTypeProjection GetManagementPackTypeProjection(string id, ManagementPack mp)
        {
            ManagementPackTypeProjectionCriteria mptpcriteria = new ManagementPackTypeProjectionCriteria(String.Format("Name = '{0}'", id));
#if ( _SERVICEMANAGER_R2_ )
            IList<ManagementPackTypeProjection> mptps = mp.Store.EntityTypes.GetTypeProjections(mptpcriteria);
#else
            MethodInfo method = mp.GetType().GetMethod("GetManagementGroupObject", BindingFlags.NonPublic | BindingFlags.Instance);
            EnterpriseManagementGroup emg = method.Invoke(mp, null) as EnterpriseManagementGroup;
            IList<ManagementPackTypeProjection> mptps = emg.EntityTypes.GetTypeProjections(mptpcriteria);
#endif
            foreach (ManagementPackTypeProjection mptp in mptps)
            {
                if (mptp.GetManagementPack().Id == mp.Id)
                    return (mptp);
            }
            //Didn't find any matches
            return (null);
        }

        public static ManagementPackTypeProjection GetManagementPackTypeProjection(string id, ManagementPack mp, EnterpriseManagementGroup emg)
        {
            ManagementPackTypeProjectionCriteria mptpcriteria = new ManagementPackTypeProjectionCriteria(String.Format("Name = '{0}'", id));
            IList<ManagementPackTypeProjection> mptps = emg.EntityTypes.GetTypeProjections(mptpcriteria);
            foreach (ManagementPackTypeProjection mptp in mptps)
            {
                if (mptp.GetManagementPack().Id == mp.Id)
                    return (mptp);
            }
            //Didn't find any matches
            return (null);
        }

        public static ManagementPackObjectTemplate GetObjectTemplateFromRequestOffering(EnterpriseManagementObjectProjection requestOffering)
        {
            string TemplateIdentifier = requestOffering.Object[null, "TargetTemplate"].Value.ToString();
            string TemplateName = TemplateIdentifier.Split('|')[3];
            ManagementPackObjectTemplateCriteria c = new ManagementPackObjectTemplateCriteria("Name = '" + TemplateName + "'");
            return requestOffering.Object.ManagementGroup.Templates.GetObjectTemplates(c)[0];
        }
    }
}
