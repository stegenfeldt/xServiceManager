using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Security;
using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.New, "xSCSMUserRole")]
    public class NewSCSMUserRoleCommand : SMCmdletBase
    {
        # region Private Properties
        private string _displayname;
        private string _description;
        private Profile _profile;
        private ManagementPackElement[] _objects;
        private EnterpriseManagementObject[] _scsmusers;
        private String[] _users;
        private ManagementPackTemplate[] _templates;
        private ManagementPackClass[] _classes;
        private ManagementPackView[] _views;
        private ManagementPackConsoleTask[] _consoletasks;
        private Boolean _alltemplates;
        private Boolean _allconsoletasks;
        private Boolean _allviews;
        private Boolean _allclasses;
        private Boolean _allobjects;

        # endregion Private Properties

        #region Parameters

        [Parameter(ValueFromPipeline = false, Mandatory = true)]
        public String DisplayName
        {
            get { return _displayname; }
            set { _displayname = value; }
        }

        [Parameter(ValueFromPipeline = false, Mandatory = true)]
        public Profile Profile
        {
            get { return _profile; }
            set { _profile = value; }
        }

        [Parameter(ValueFromPipeline = false, Mandatory = false)]
        public String Description
        {
            get { return _description; }
            set { _description = value; }
        }

        [Parameter(ValueFromPipeline = false, Mandatory = false)]
        public ManagementPackElement[] Objects
        {
            get { return _objects; }
            set { _objects = value; }
        }

        [Parameter(ValueFromPipeline = false, Mandatory = false)]
        public ManagementPackTemplate[] Templates
        {
            get { return _templates; }
            set { _templates = value; }
        }

        [Parameter(ValueFromPipeline = false, Mandatory = false)]
        public ManagementPackClass[] Classes
        {
            get { return _classes; }
            set { _classes = value; }
        }

        [Parameter(ValueFromPipeline = false, Mandatory = false)]
        public ManagementPackView[] Views
        {
            get { return _views; }
            set { _views = value; }
        }

        [Parameter(ValueFromPipeline = false, Mandatory = false)]
        public ManagementPackConsoleTask[] ConsoleTasks
        {
            get { return _consoletasks; }
            set { _consoletasks = value; }
        }

        [Parameter(ValueFromPipeline = false, Mandatory = false)]
        public EnterpriseManagementObject[] SCSMUsers
        {
            get { return _scsmusers; }
            set { _scsmusers = value; }
        }

        [Parameter(ValueFromPipeline = false, Mandatory = false)]
        public String[] Users
        {
            get { return _users; }
            set { _users = value; }
        }

        [Parameter(ValueFromPipeline = false, Mandatory = false)]
        public SwitchParameter AllTemplates
        {
            get { return _alltemplates; }
            set { _alltemplates = value; }
        }

        [Parameter(ValueFromPipeline = false, Mandatory = false)]
        public SwitchParameter AllObjects
        {
            get { return _allobjects; }
            set { _allobjects = value; }
        }

        [Parameter(ValueFromPipeline = false, Mandatory = false)]
        public SwitchParameter AllClasses
        {
            get { return _allclasses; }
            set { _allclasses = value; }
        }

        [Parameter(ValueFromPipeline = false, Mandatory = false)]
        public SwitchParameter AllViews
        {
            get { return _allviews; }
            set { _allviews = value; }
        }

        [Parameter(ValueFromPipeline = false, Mandatory = false)]
        public SwitchParameter AllConsoleTasks
        {
            get { return _allconsoletasks; }
            set { _allconsoletasks = value; }
        }

        #endregion Parameters

        protected override void BeginProcessing()
        {
            //This will set the _mg which is the EnterpriseManagementGroup object for the connection to the server
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            //Create a new user role and set its properties based on what the user passed in
            UserRole ur = new UserRole()
            {
                DisplayName = _displayname,
                Profile = _profile,
                Name = SMHelpers.MakeMPElementSafeUniqueIdentifier("UserRole")
            };
            if (_description != null) { ur.Description = _description; };

            ManagementPackClass classUser = SMHelpers.GetManagementPackClass(ClassTypes.Microsoft_AD_User, SMHelpers.GetManagementPack(ManagementPacks.Microsoft_Windows_Library, _mg), _mg);

            //Add the users
            if (_scsmusers != null)
            {
                foreach (EnterpriseManagementObject emo in _scsmusers)
                {
                    ur.Users.Add(emo[classUser, ClassProperties.System_Domain_User__Domain] + "\\" + emo[classUser, ClassProperties.System_Domain_User__UserName]);
                }
            }

            if (_users != null)
            {
                foreach (String user in _users)
                {
                    ur.Users.Add(user);
                }
            }

            //Set the security scopes
            if (_alltemplates) { ur.Scope.Templates.Add(UserRoleScope.RootTemplateId); }
            else { if (_templates != null) { foreach (ManagementPackTemplate template in _templates) { ur.Scope.Templates.Add(template.Id); } } }

            if (_allobjects) { ur.Scope.Objects.Add(UserRoleScope.RootObjectId); }
            else { if (_objects != null) { foreach (ManagementPackElement emo in _objects) { ur.Scope.Objects.Add(emo.Id); } } }

            if (_allclasses) { ur.Scope.Classes.Add(UserRoleScope.RootClassId); }
            else { if (_classes != null) { foreach (ManagementPackClass mpclass in _classes) { ur.Scope.Classes.Add(mpclass.Id); } } }

            if (_allconsoletasks) { ur.Scope.ConsoleTasks.Add(UserRoleScope.RootConsoleTaskId); }
            else { if (_consoletasks != null) { foreach (ManagementPackConsoleTask consoletask in _consoletasks) { ur.Scope.ConsoleTasks.Add(consoletask.Id); } } }

            if (_allviews)
            {
                Pair<Guid, Boolean> pairView = new Pair<Guid, Boolean>(UserRoleScope.RootViewId, false);
                ur.Scope.Views.Add(pairView);
            }
            else
            {
                if (_views != null)
                {
                    foreach (ManagementPackView view in _views)
                    {
                        if (view != null)
                        {
                            Pair<Guid, Boolean> pairView = new Pair<Guid, Boolean>(view.Id, false);
                            ur.Scope.Views.Add(pairView);
                        }
                    }
                }
            }
            _mg.Security.InsertUserRole(ur);
        }
    }

}