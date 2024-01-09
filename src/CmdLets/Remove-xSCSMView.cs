using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    [Cmdlet(VerbsCommon.Remove, "xSCSMView", SupportsShouldProcess = true)]
    public class RemoveSCSMView : PresentationCmdletBase
    {
        private ManagementPackView _view;

        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public ManagementPackView View
        {
            get { return _view; }
            set { _view = value; }
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            ManagementPackView view = _mg.Presentation.GetView(_view.Id);
            ManagementPack mp = view.GetManagementPack();
            view.Status = ManagementPackElementStatus.PendingDelete;
            string viewInfo = view.Name;
            if(view.DisplayName != null)
            {
                viewInfo = view.DisplayName;
            }

            if(ShouldProcess(viewInfo))
            {
                mp.AcceptChanges();
            }
        }
    }
}
