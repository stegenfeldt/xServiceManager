using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Security;
using System;
using System.Linq;
using System.Management.Automation;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Set, "xSCSMUserRole", DefaultParameterSetName = "name", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class SetSCSMUserRoleCommand : SMCmdletBase
    {
        # region Private Properties
        private string _displayname;
        private string _description;
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
        private Guid[] _id;
        private string[] _name;
        private UserRole[] _userroles;

        # endregion Private Properties

        #region Parameters

        [Parameter(Position = 0, ParameterSetName = "id", ValueFromPipelineByPropertyName = true)]
        public Guid[] Id
        {
            get { return _id; }
            set { _id = value; }
        }

        [Parameter(Position = 0, ParameterSetName = "name")]
        public String[] Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [Parameter(ValueFromPipeline = false)]
        public String DisplayName
        {
            get { return _displayname; }
            set { _displayname = value; }
        }

        [Parameter(ValueFromPipeline = false)]
        public String Description
        {
            get { return _description; }
            set { _description = value; }
        }

        [Parameter(ValueFromPipeline = false)]
        public ManagementPackElement[] Objects
        {
            get { return _objects; }
            set { _objects = value; }
        }

        [Parameter(ValueFromPipeline = false)]
        public ManagementPackTemplate[] Templates
        {
            get { return _templates; }
            set { _templates = value; }
        }

        [Parameter(ValueFromPipeline = false)]
        public ManagementPackClass[] Classes
        {
            get { return _classes; }
            set { _classes = value; }
        }

        [Parameter(ValueFromPipeline = false)]
        public ManagementPackView[] Views
        {
            get { return _views; }
            set { _views = value; }
        }

        [Parameter(ValueFromPipeline = false)]
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

        [Parameter(ValueFromPipeline = false)]
        public SwitchParameter AllTemplates
        {
            get { return _alltemplates; }
            set { _alltemplates = value; }
        }

        [Parameter(ValueFromPipeline = false)]
        public SwitchParameter AllObjects
        {
            get { return _allobjects; }
            set { _allobjects = value; }
        }

        [Parameter(ValueFromPipeline = false)]
        public SwitchParameter AllClasses
        {
            get { return _allclasses; }
            set { _allclasses = value; }
        }

        [Parameter(ValueFromPipeline = false)]
        public SwitchParameter AllViews
        {
            get { return _allviews; }
            set { _allviews = value; }
        }

        [Parameter(ValueFromPipeline = false)]
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
            if (_userroles == null)
            {
                PowerShell powerShell = PowerShell.Create();

                powerShell.AddCommand("Import-Module")
                    .AddParameter("Assembly",
                          System.Reflection.Assembly.GetExecutingAssembly());
                powerShell.Invoke();
                powerShell.Commands.Clear();

                PSVariable DefaultComputer = SessionState.PSVariable.Get("SMDefaultComputer");
                if (DefaultComputer != null)
                {
                    powerShell.AddScript(string.Format("$SMDefaultComputer = '{0}';", DefaultComputer.Value));
                    powerShell.Invoke();
                    powerShell.Commands.Clear();
                }

                powerShell.AddCommand("Get-SCSMUserRole");

                if (_id != null)
                {
                    powerShell.AddParameter("Id", _id);
                    _userroles = powerShell.Invoke<UserRole>().ToArray();
                }
                if (_name != null)
                {
                    powerShell.AddParameter("Name", _name);
                    _userroles = powerShell.Invoke<UserRole>().ToArray();
                }
            }
            foreach (UserRole ur in _userroles)
            {
                if (!ur.IsSystem)
                {
                    if (ShouldProcess(ur.DisplayName))
                    {
                        if (DisplayName != null)
                        {
                            ur.DisplayName = _displayname;
                        }

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
                        if (_alltemplates)
                        {
                            if (!ur.Scope.Templates.Contains(UserRoleScope.RootTemplateId)) { ur.Scope.Templates.Add(UserRoleScope.RootTemplateId); }
                        }
                        else
                        {
                            if (_templates != null)
                            {
                                foreach (ManagementPackTemplate template in _templates)
                                {
                                    if (!ur.Scope.Templates.Contains(template.Id)) { ur.Scope.Templates.Add(template.Id); }
                                }
                            }
                        }

                        if (_allobjects)
                        {
                            if (!ur.Scope.Objects.Contains(UserRoleScope.RootObjectId)) { ur.Scope.Objects.Add(UserRoleScope.RootObjectId); }
                        }
                        else
                        {
                            if (_objects != null)
                            {
                                foreach (ManagementPackElement emo in _objects)
                                {
                                    if (!ur.Scope.Objects.Contains(emo.Id)) { ur.Scope.Objects.Add(emo.Id); }
                                }
                            }
                        }

                        if (_allclasses)
                        {
                            if (!ur.Scope.Classes.Contains(UserRoleScope.RootClassId)) { ur.Scope.Classes.Add(UserRoleScope.RootClassId); }
                        }
                        else
                        {
                            if (_classes != null)
                            {
                                foreach (ManagementPackClass mpclass in _classes)
                                {
                                    if (!ur.Scope.Classes.Contains(mpclass.Id)) { ur.Scope.Classes.Add(mpclass.Id); }
                                }
                            }
                        }

                        if (_allconsoletasks)
                        {
                            if (!ur.Scope.ConsoleTasks.Contains(UserRoleScope.RootConsoleTaskId)) { ur.Scope.ConsoleTasks.Add(UserRoleScope.RootConsoleTaskId); }
                        }
                        else
                        {
                            if (_consoletasks != null)
                            {
                                foreach (ManagementPackConsoleTask consoletask in _consoletasks)
                                {
                                    if (!ur.Scope.ConsoleTasks.Contains(consoletask.Id)) { ur.Scope.ConsoleTasks.Add(consoletask.Id); }
                                }
                            }
                        }

                        if (_allviews)
                        {
                            Pair<Guid, Boolean> pairView = new Pair<Guid, Boolean>(UserRoleScope.RootViewId, false);
                            if (!ur.Scope.Views.Contains(pairView))
                            { ur.Scope.Views.Add(pairView); }
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
                                        if (!ur.Scope.Views.Contains(pairView))
                                        { ur.Scope.Views.Add(pairView); }
                                    }
                                }
                            }
                        }
                        ur.Update();
                    }
                }
            }
        }
    }

}