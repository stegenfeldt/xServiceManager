using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    #region SCSMView cmdlets

    [Cmdlet(VerbsCommon.New, "xSCSMView")]
    public class NewSMViewCommand : ObjectCmdletHelper
    {
        #region parameters

        private ManagementPackFolder _Folder = null;
        private String _DisplayName = null;
        private String _Criteria = null;
        private ManagementPackClass _Class = null;
        private ManagementPack _ManagementPack = null;
        private SwitchParameter _PassThru;
        private ManagementPackTypeProjection _Projection;
        private Column[] _columns;
        private ManagementPackImage _image;
        private KeyValuePair<string, ManagementPackReference>[] _ManagementPackReferences;

        [Parameter(Position = 0,
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The folder of the view.")]

        public ManagementPackFolder Folder
        {
            get { return _Folder; }
            set { _Folder = value; }
        }

        [Parameter(Position = 1,
        Mandatory = false,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The display name of the view.")]

        public String DisplayName
        {
            get { return _DisplayName; }
            set { _DisplayName = value; }
        }

        [Parameter(Position = 2,
        Mandatory = false,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The target class of the view.")]

        public ManagementPackClass Class
        {
            get { return _Class; }
            set { _Class = value; }
        }


        [Parameter(Position = 3,
        Mandatory = false,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The management pack to store the view in.")]

        public ManagementPack ManagementPack
        {
            get { return _ManagementPack; }
            set { _ManagementPack = value; }
        }

        [Parameter(Position = 4,
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The view criteria.")]

        public String Criteria
        {
            get { return _Criteria; }
            set { _Criteria = value; }
        }

        [Parameter(Position = 5,
        Mandatory = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "The columns to show.")]

        public Column[] Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        [Parameter(Position = 6, 
        Mandatory = false,
        ValueFromPipeline = false,
        HelpMessage = "The tree image for the view")]
        public ManagementPackImage Image
        {
            get { return _image; }
            set { _image = value; }
        }

        [Parameter(Position = 7,
        Mandatory = false,
        ValueFromPipeline = false,
        HelpMessage = "The management pack references to add to be used in the criteria XML")]
        public KeyValuePair<string,ManagementPackReference>[] ManagementPackReference
        {
            get { return _ManagementPackReferences; }
            set { _ManagementPackReferences = value; }
        }

        [Parameter(Position = 8,
        Mandatory = false,
        ValueFromPipeline = false,
        HelpMessage = "The type projection to be used.")]

        public ManagementPackTypeProjection Projection
        {
            get { return _Projection; }
            set { _Projection = value; }
        }

        [Parameter]
        public SwitchParameter PassThru
        {
            get { return _PassThru; }
            set { _PassThru = value; }
        }

        #endregion

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();

                ManagementPackView view = new ManagementPackView(ManagementPack, SMHelpers.MakeMPElementSafeUniqueIdentifier("View"), ManagementPackAccessibility.Internal);
                view.DisplayName = _DisplayName;     //Set the display name according to what the user specified
                view.Target = _Class;                //Set the class according to what the user specified

                //TODO: Parameterize these later
                view.Visible = true;
                view.Accessibility = ManagementPackAccessibility.Public;
                view.Enabled = true;
                view.Category = "NotUsed";

                //Set the parent folder that was passed in
                ManagementPackFolderItem folderitem = new ManagementPackFolderItem(view, _Folder);

                //Add the management pack references to the MP
                if(_ManagementPackReferences != null)
                {
                    foreach (KeyValuePair<string,ManagementPackReference> kvp in _ManagementPackReferences)
                    {
                        _ManagementPack.References.Add(kvp);
                    }
                }

                //Get the Grid view type and set it for the view
                ManagementPack mpConsole = SMHelpers.GetManagementPack(ManagementPacks.Microsoft_EnterpriseManagement_ServiceManager_UI_Console, _mg);
                view.TypeID = mpConsole.GetViewType("GridViewType");

                #region DataAdapters

                DataAdapter daEMO = new DataAdapter();
                DataAdapter daAdvancedList = new DataAdapter();

                if (_Projection != null)
                {
                    daEMO.Name = "dataportal:EnterpriseManagementObjectAdapter";
                    daEMO.Type = "Microsoft.EnterpriseManagement.UI.SdkDataAccess.DataAdapters.EnterpriseManagementObjectProjectionAdapter";
                }
                else
                {
                    daEMO.Name = "dataportal:EnterpriseManagementObjectProjectionAdapter";
                    daEMO.Type = "Microsoft.EnterpriseManagement.UI.SdkDataAccess.DataAdapters.EnterpriseManagementObjectAdapter";
                }

                daEMO.Assembly = "Microsoft.EnterpriseManagement.UI.SdkDataAccess";

                daAdvancedList.Name = "viewframework://Adapters/AdvancedList";
                daAdvancedList.Assembly = "Microsoft.EnterpriseManagement.UI.ViewFramework";
                daAdvancedList.Type = "Microsoft.EnterpriseManagement.UI.ViewFramework.AdvancedListSupportAdapter";

                Collection<DataAdapter> collDataAdpaters = new Collection<DataAdapter>();
                collDataAdpaters.Add(daEMO);
                collDataAdpaters.Add(daAdvancedList);

                #endregion DataAdapters

                view.Configuration = CreateViewConfiguration(collDataAdpaters, _columns);

                foreach (Column column in _columns)
                {
                    ManagementPackStringResource mpsr = new ManagementPackStringResource(view.GetManagementPack(), column.DisplayNameId);
                    mpsr.DisplayName = column.DisplayNameString;
                }

                //Set the image
                if (_image != null)
                {
                    ManagementPackElementReference<ManagementPackImage> viewIconReference = (ManagementPackElementReference<ManagementPackImage>)_mg.Resources.GetResource<ManagementPackImage>(_image.Name,_image.GetManagementPack());
                    ManagementPackImageReference imageref = new ManagementPackImageReference(view, viewIconReference, view.GetManagementPack());
                }

                view.GetManagementPack().AcceptChanges();

                if (PassThru)
                {
                    //Pass the new object to the pipeline
                    WriteObject(view);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "NewView", ErrorCategory.InvalidOperation, DisplayName));
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }

        private string CreateViewConfiguration(Collection<DataAdapter> collDataAdapters, Column[] collColumns)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement elConfiguration = xmldoc.CreateElement("Configuration");
            xmldoc.AppendChild(elConfiguration);

            CreateDataXmlNode(ref xmldoc, collDataAdapters);

            CreatePresentationXmlNode(ref xmldoc, collColumns);

            StringWriter sw = new StringWriter();
            XmlTextWriter tw = new XmlTextWriter(sw);

            xmldoc.WriteTo(tw);

            string strConfiguration = sw.ToString();

            //Strip off the dummy document element.            
            strConfiguration = strConfiguration.Replace("<Configuration>", "");
            strConfiguration = strConfiguration.Replace("</Configuration>", "");

            return (strConfiguration);
        }

        private void CreateDataXmlNode(ref XmlDocument xmldoc, Collection<DataAdapter> collDataAdapters)
        {
            XmlElement elData = xmldoc.CreateElement("Data");
            xmldoc.DocumentElement.AppendChild(elData);  //Add as child of "Configuration" document element for now.
            XmlElement elAdapters = xmldoc.CreateElement("Adapters");

            foreach (DataAdapter da in collDataAdapters)
            {
                XmlElement xmlnodeDataAdapter = CreateDataAdapterNode(ref xmldoc, da);
                elAdapters.AppendChild(xmlnodeDataAdapter);
            }

            elData.AppendChild(elAdapters);

            XmlElement elItemsSource = xmldoc.CreateElement("ItemsSource");
            XmlElement elAdvancedListSupportClass = xmldoc.CreateElement("AdvancedListSupportClass");

            //AdvancedListSupportClass attributes
            XmlAttribute attrXmlnsALSC = xmldoc.CreateAttribute("xmlns");
            XmlAttribute attrXmlnsAvALSC = xmldoc.CreateAttribute("xmlns:av");
            XmlAttribute attrXmlnsXALSC = xmldoc.CreateAttribute("xmlns:x");
            XmlAttribute attrXmlnsSALSC = xmldoc.CreateAttribute("xmlns:s");
            XmlAttribute attrDataTypeName = xmldoc.CreateAttribute("DataTypeName");
            XmlAttribute attrAdapterName = xmldoc.CreateAttribute("AdapterName");
            XmlAttribute attrFullUpdateAdapter = xmldoc.CreateAttribute("FullUpdateAdapter");
            XmlAttribute attrDataSource = xmldoc.CreateAttribute("DataSource");
            XmlAttribute attrFullUpdateFrequency = xmldoc.CreateAttribute("FullUpdateFrequency");
            XmlAttribute attrStreaming = xmldoc.CreateAttribute("Streaming");
            XmlAttribute attrIsRecurring = xmldoc.CreateAttribute("IsRecurring");
            XmlAttribute attrRecurrenceFrequency = xmldoc.CreateAttribute("RecurrenceFrequency");

            //TODO: Parameterize these someday
            attrXmlnsALSC.Value = "clr-namespace:Microsoft.EnterpriseManagement.UI.ViewFramework;assembly=Microsoft.EnterpriseManagement.UI.ViewFramework";
            attrXmlnsAvALSC.Value = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            attrXmlnsXALSC.Value = "http://schemas.microsoft.com/winfx/2006/xaml";
            attrXmlnsSALSC.Value = "clr-namespace:System;assembly=mscorlib";
            attrDataTypeName.Value = "";
            attrAdapterName.Value = "viewframework://Adapters/AdvancedList";
            attrDataSource.Value = "mom:ManagementGroup";
            attrFullUpdateFrequency.Value = "1";
            attrStreaming.Value = "true";
            attrIsRecurring.Value = "true";
            attrRecurrenceFrequency.Value = "{x:Static s:Int32.MaxValue}";
            if(_Projection != null)
                attrFullUpdateAdapter.Value = "dataportal:EnterpriseManagementObjectProjectionAdapter";
            else
                attrFullUpdateAdapter.Value = "dataportal:EnterpriseManagementObjectAdapter";

            elAdvancedListSupportClass.Attributes.Append(attrXmlnsALSC);
            elAdvancedListSupportClass.Attributes.Append(attrXmlnsAvALSC);
            elAdvancedListSupportClass.Attributes.Append(attrXmlnsSALSC);
            elAdvancedListSupportClass.Attributes.Append(attrXmlnsXALSC);
            elAdvancedListSupportClass.Attributes.Append(attrDataTypeName);
            elAdvancedListSupportClass.Attributes.Append(attrAdapterName);
            elAdvancedListSupportClass.Attributes.Append(attrFullUpdateAdapter);
            elAdvancedListSupportClass.Attributes.Append(attrDataSource);
            elAdvancedListSupportClass.Attributes.Append(attrFullUpdateFrequency);
            elAdvancedListSupportClass.Attributes.Append(attrStreaming);
            elAdvancedListSupportClass.Attributes.Append(attrIsRecurring);
            elAdvancedListSupportClass.Attributes.Append(attrRecurrenceFrequency);

            XmlElement elAdvancedListSupportClassParameters = xmldoc.CreateElement("AdvancedListSupportClass.Parameters");

            XmlElement elQueryParameter = xmldoc.CreateElement("QueryParameter");

            XmlAttribute attrParameter = xmldoc.CreateAttribute("Parameter");
            XmlAttribute attrValue = xmldoc.CreateAttribute("Value");

            ManagementPackElement element;

            if (_Projection != null)
            {
                attrParameter.Value = "TypeProjectionId";
                element = _Projection;
            }
            else
            {
                attrParameter.Value = "ManagementPackClassId";
                element = _Class;
            }

            ManagementPack elementMP = element.GetManagementPack();
            
            string strMPAlias = string.Empty;

            if (elementMP != _ManagementPack)
            {
                //If the class/type projection is not in the same management pack as where we are creating the view then try to either use an existing MP reference or create a new one
                foreach (KeyValuePair<string,ManagementPackReference> mpref in _ManagementPack.References)
                {
                    if (mpref.Value.Id == elementMP.Id)
                    {
                        //Found an existing reference so just use that alias
                        strMPAlias = mpref.Key;
                        break;
                    }
                }
                
                if (strMPAlias == string.Empty)
                {
                    //We didnt find the MP reference in the set of existing references, let's create one..
                    strMPAlias = elementMP.Name.Replace(".", "_");
                    ManagementPackReference mpref = new ManagementPackReference(elementMP);
                    _ManagementPack.References.Add(strMPAlias, mpref);
                }
            }

            if(strMPAlias == string.Empty)
                attrValue.Value = String.Format("$MPElement[Name='{0}']$", element.Name);
            else
                attrValue.Value = String.Format("$MPElement[Name='{0}!{1}']$", strMPAlias, element.Name);

            elQueryParameter.Attributes.Append(attrParameter);
            elQueryParameter.Attributes.Append(attrValue);

            elAdvancedListSupportClassParameters.AppendChild(elQueryParameter);

            elAdvancedListSupportClass.AppendChild(elAdvancedListSupportClassParameters);

            elItemsSource.AppendChild(elAdvancedListSupportClass);

            elData.AppendChild(elItemsSource);

            XmlDocumentFragment fragCriteria = xmldoc.CreateDocumentFragment();
            fragCriteria.InnerXml = _Criteria;

            elData.AppendChild(fragCriteria);
        }

        private void CreatePresentationXmlNode(ref XmlDocument xmldoc, Column[] collColumns)
        {
            XmlElement elPresentation = xmldoc.CreateElement("Presentation");
            xmldoc.DocumentElement.AppendChild(elPresentation);  //Add as child of "Configuration" document element for now.
            XmlElement elColumns = xmldoc.CreateElement("Columns");
            XmlElement elViewStrings = xmldoc.CreateElement("ViewStrings");

            XmlElement elColumnCollection = xmldoc.CreateElement("mux", "ColumnCollection", "http://schemas.microsoft.com/SystemCenter/Common/UI/Views/GridView");

            XmlAttribute attrXmlnsCC = xmldoc.CreateAttribute("xmlns");
            attrXmlnsCC.Value = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            
            XmlAttribute attrXmlnsSCC = xmldoc.CreateAttribute("xmlns:s");
            attrXmlnsSCC.Value = "clr-namespace:System;assembly=mscorlib";
            
            XmlAttribute attrXmlnsXCC = xmldoc.CreateAttribute("xmlns:x");
            attrXmlnsXCC.Value = "http://schemas.microsoft.com/winfx/2006/xaml";
            
            XmlAttribute attrXmlnsDataCC = xmldoc.CreateAttribute("xmlns:data");
            attrXmlnsDataCC.Value = "clr-namespace:Microsoft.EnterpriseManagement.UI.SdkDataAccess.Common;assembly=Microsoft.EnterpriseManagement.UI.SdkDataAccess";

            elColumnCollection.Attributes.Append(attrXmlnsCC);
            elColumnCollection.Attributes.Append(attrXmlnsSCC);
            elColumnCollection.Attributes.Append(attrXmlnsXCC);
            elColumnCollection.Attributes.Append(attrXmlnsDataCC);

            foreach (Column column in collColumns)
            {
                XmlElement elColumn = CreateColumnNode(ref xmldoc, column);
                elColumnCollection.AppendChild(elColumn);

                XmlElement elViewString = CreateViewStringNode(ref xmldoc, column);
                elViewStrings.AppendChild(elViewString);
            }

            elColumns.AppendChild(elColumnCollection);
            elPresentation.AppendChild(elColumns);

            elPresentation.AppendChild(elViewStrings);
        }

        private XmlElement CreateDataAdapterNode(ref XmlDocument xmldoc, DataAdapter da)
        {

            XmlElement elAdapter = xmldoc.CreateElement("Adapter");

            XmlAttribute attrAdapterName = xmldoc.CreateAttribute("AdapterName");
            attrAdapterName.Value = da.Name;
            elAdapter.Attributes.Append(attrAdapterName);

            XmlElement elAdapterAssembly = xmldoc.CreateElement("AdapterAssembly");
            elAdapterAssembly.InnerText = da.Assembly;

            XmlElement elAdapterType = xmldoc.CreateElement("AdapterType");
            elAdapterType.InnerText = da.Type;

            elAdapter.AppendChild(elAdapterAssembly);
            elAdapter.AppendChild(elAdapterType);

            return (elAdapter);
        }

        private XmlElement CreateColumnNode(ref XmlDocument xmldoc, Column column)
        {
            XmlElement elColumn = xmldoc.CreateElement("mux","Column","http://schemas.microsoft.com/SystemCenter/Common/UI/Views/GridView");

            XmlAttribute attrName = xmldoc.CreateAttribute("Name");
            XmlAttribute attrDisplayMemberBinding = xmldoc.CreateAttribute("DisplayMemberBinding");
            XmlAttribute attrWidth = xmldoc.CreateAttribute("Width");
            XmlAttribute attrDisplayNameId = xmldoc.CreateAttribute("DisplayName");
            XmlAttribute attrProperty = xmldoc.CreateAttribute("Property");
            XmlAttribute attrDataType = xmldoc.CreateAttribute("DataType");

            attrName.Value = column.Name;
            attrDisplayMemberBinding.Value = column.DisplayMemberBinding;
            attrWidth.Value = column.Width;
            attrDisplayNameId.Value = column.DisplayNameId;
            attrProperty.Value = column.Property;
            attrDataType.Value = column.DataType;

            elColumn.Attributes.Append(attrName);
            elColumn.Attributes.Append(attrDisplayMemberBinding);
            elColumn.Attributes.Append(attrWidth);
            elColumn.Attributes.Append(attrDisplayNameId);
            elColumn.Attributes.Append(attrProperty);
            elColumn.Attributes.Append(attrDataType);

            return (elColumn);
        }

        private XmlElement CreateViewStringNode(ref XmlDocument xmldoc, Column column)
        {
            XmlElement elViewString = xmldoc.CreateElement("ViewString");
            XmlAttribute attrId = xmldoc.CreateAttribute("ID");
            attrId.Value = column.DisplayNameId;
            elViewString.Attributes.Append(attrId);
            elViewString.InnerText = "$MPElement[Name=\"" + column.DisplayNameId + "\"]$";
            return (elViewString);
        }
    }

#endregion
#region SCSMFolder cmdlets

#endregion
}
