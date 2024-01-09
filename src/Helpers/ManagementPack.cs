using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Security;
using System.Threading;
using System.Resources;
using System.Xml.XPath;
using Microsoft.CSharp;
using System.Xml.Schema;
using System.Globalization;
using System.IO.Compression;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.Xml.Serialization;
using System.Security.Permissions;
using System.Management.Automation;
using Microsoft.EnterpriseManagement;

namespace xServiceManager.Module
{

    /// <summary>
    /// This class is used to create sealed ManagementPacks on disk(.DLL files)
    /// </summary>
    public class FastAssemblyWriter
    {
        #region Constructors
        public FastAssemblyWriter(FastAssemblyWriterSettings settings)
        {
            #region Validate Settings
            //validate settings
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            //validate keyfile string
            if (string.IsNullOrEmpty(settings.KeyFilePath))
            {
                throw new ArgumentNullException("settings");
            }

            //validate that the specified keyfile exists
            FileInfo finfo = null;
            try
            {
                finfo = new FileInfo(settings.KeyFilePath);
            }
            catch (Exception)
            {
                throw new InvalidOperationException("settings");
            }

            if (finfo == null || (finfo.Exists == false))
            {
                throw new FileNotFoundException("Specified Keyfile does not exits. Cannot find file : " + settings.KeyFilePath);
            }
            #endregion

            this._settings = settings;
        }
        #endregion


        #region Private Members
        private FastAssemblyWriterSettings _settings = null;
        #endregion

        //FIXME: WriteMPB is not compatible with newer .net
        // #region WriteMPB
        // public string WriteMPB(string fileName)
        // {
        //     //validate mp
        //     if (fileName == null)
        //     {
        //         throw new ArgumentNullException("fileName");
        //     }

        //     // since this is an MPB, just write it out

        //     #region Read and Compress ManagementPack Xml
        //     //get MP contents
        //     byte[] buffer = File.ReadAllBytes(fileName);

        //     //stream for compressed output
        //     MemoryStream msout = new MemoryStream();

        //     //compress the buffer copy
        //     using (MemoryStream msin = new MemoryStream(buffer))
        //     {

        //         //compress the management pack contents to memory
        //         Compress(msin, msout);
        //     }
        //     #endregion

        //     #region Write Assembly
        //     //write the assembly resource file
        //     WriteResource(msout.ToArray());

        //     //Create the Assembly
        //     string mpoutputfilename = String.Format("Sealed_{0}", fileName);
        //     return (CreateAssemblyFile(this._settings, mpoutputfilename));
        //     #endregion
        // }
        // #endregion

        // #region WriteManagementPack Method
        // /// <summary>
        // /// This method creates a sealed ManagementPack (signed .NET Assembly with .DLL file extension)
        // /// in the specified directory. 
        // /// 
        // /// The file name for the output file obtained from the Manifest section of ManagementPack (value of the <ID> tag)
        // /// (and is not changeable)
        // /// </summary>
        // /// <param name="mp">The ManagementPack to write as an assembly</param>
        // public string WriteManagementPack(string fileName)
        // {
        //     //validate mp
        //     if (fileName == null)
        //     {
        //         throw new ArgumentNullException("fileName");
        //     }

        //     #region Get Information from MP
        //     //open document
        //     XPathDocument doc = null;
        //     doc = new XPathDocument(fileName);
        //     XPathNavigator nav = doc.CreateNavigator();

        //     string mpoutputfilename = string.Empty;

        //     //read Name name
        //     XPathNavigator namenav = nav.SelectSingleNode("/ManagementPack/Manifest/Identity/ID");
        //     if (namenav != null)
        //     {
        //         //set the Version field
        //         mpoutputfilename = namenav.Value;
        //     }
        //     else
        //     {
        //         throw new ItemNotFoundException("Cannot read ManagementPack Name from the file specified");
        //     }

        //     //read version
        //     XPathNavigator versionnav = nav.SelectSingleNode("/ManagementPack/Manifest/Identity/Version");
        //     if (versionnav != null)
        //     {
        //         //set the Version field
        //         this._settings.AssemblyVersion = new Version(versionnav.Value);
        //     }
        //     else
        //     {
        //         throw new ItemNotFoundException("Cannot read Management Pack Version from the file specified");
        //     }

        //     //read product name
        //     XPathNavigator productnav = nav.SelectSingleNode("/ManagementPack/Manifest/Name");
        //     if (productnav != null)
        //     {
        //         //set the Version field
        //         this._settings.ProductName = productnav.Value;
        //     }
        //     else
        //     {
        //         throw new ItemNotFoundException("Cannot read ManagementPack Name from the file specified");
        //     }
        //     #endregion

        //     #region Read and Compress ManagementPack Xml
        //     //get MP contents
        //     string mpcontents = nav.OuterXml;

        //     //store in a byte buffer
        //     byte[] buffer = Encoding.Unicode.GetBytes(mpcontents);

        //     //stream for compressed output
        //     MemoryStream msout = new MemoryStream();

        //     //compress the buffer copy
        //     using (MemoryStream msin = new MemoryStream(buffer))
        //     {

        //         //compress the management pack contents to memory
        //         Compress(msin, msout);
        //     }
        //     #endregion

        //     #region Write Assembly
        //     //write the assembly resource file
        //     WriteResource(msout.ToArray());

        //     //Create the Assembly
        //     return (CreateAssemblyFile(this._settings, mpoutputfilename));
        //     #endregion
        // }
        // #endregion

        #region Helper Methods
        private static void Compress(Stream inputstream, Stream outputstream)
        {
            //compress in 4k chunks
            byte[] buffer = new byte[ChunkSize];
            int n;
            using (GZipStream gzipCompressionStream = new GZipStream(outputstream, CompressionMode.Compress))
            {
                //read in chunks from the inputstream
                while ((n = inputstream.Read(buffer, 0, ChunkSize)) != 0)
                {
                    gzipCompressionStream.Write(buffer, 0, n);
                }
            }
        }

        //write out the resource file
        private static void WriteResource(byte[] mpbytes)
        {
            //write out the resource
            using (ResourceWriter resourcewriter = new ResourceWriter(FastAssemblyWriter.AssemblyManagementPackResourcePackageName))
            {
                resourcewriter.AddResource(FastAssemblyWriter.AssemblyManagementPackResourceName, mpbytes);
            }
        }

        //FIXME: CreateAssemblyFile is not compatible with newer .net
        // //create assembly
        // // [SerializableAttribute]
        // // [PermissionSetAttribute(SecurityAction.InheritanceDemand, Name = "FullTrust")]
        // // [PermissionSetAttribute(SecurityAction.LinkDemand, Name = "FullTrust")]
        // // [PermissionSetAttribute(SecurityAction.Assert, Name = "LinkDemand")] // old stuff, not sure if necessary in modern .net
        // private static string CreateAssemblyFile(FastAssemblyWriterSettings settings, string mpoutputname)
        // {
        //     #region Compiler options
        //     //set options for assembly creation
        //     CompilerParameters options = new CompilerParameters
        //     {
        //         GenerateExecutable = false,
        //         GenerateInMemory = false,
        //         TreatWarningsAsErrors = false
        //     };
        //     options.EmbeddedResources.Add(AssemblyManagementPackResourcePackageName);
        //     if (isMpb)
        //     {
        //         options.OutputAssembly = Path.Combine(settings.OutputDirectory, mpoutputname + FastAssemblyWriter.MpbExtension);
        //     }
        //     else
        //     {
        //         options.OutputAssembly = Path.Combine(settings.OutputDirectory, mpoutputname + AssemblyExtension);
        //     }

        //     StringBuilder compilerOptions = new StringBuilder();

        //     if (settings.DelaySign)
        //     {
        //         compilerOptions.Append("/DelaySign+ ");
        //     }
        //     compilerOptions.AppendFormat("/keyfile:{0}", settings.KeyFilePath);
        //     options.CompilerOptions = compilerOptions.ToString();
        //     #endregion

        //     #region Assembly Attributes
        //     string[] sources = new string[1];
        //     //create the formatted csharp code string that sets all assembly attributes
        //     string AssemblyDescriptionSubstitutedSourceCode =
        //         String.Format(CultureInfo.CurrentCulture, AssemblyAttributesCodeTemplate, settings.AssemblyVersion.ToString(),
        //         settings.CompanyName,
        //         settings.ProductName,
        //         settings.Copyright);

        //     //set source csharp code to this formatted string
        //     sources[0] = AssemblyDescriptionSubstitutedSourceCode;
        //     #endregion

        //     #region Build Assembly
        //     //create a new code provider
        //     CSharpCodeProvider codeProvider = new CSharpCodeProvider();

        //     //run build
        //     CompilerResults cr = codeProvider.CompileAssemblyFromSource(options, sources);

        //     //any errors during compilation?
        //     if (cr.Errors.Count > 0)
        //     {
        //         // Display compilation errors
        //         StringBuilder fullErrorText = new StringBuilder();

        //         foreach (CompilerError error in cr.Errors)
        //         {
        //             fullErrorText.AppendLine(error.ToString());
        //         }

        //         // throw new InvalidOperationException("ManagementPack sealing failed with error :" + fullErrorText.ToString());
        //         throw new InvalidOperationException(mpoutputname);
        //     }

        //     codeProvider.Dispose();
        //     return (options.OutputAssembly);
        //     #endregion
        // }

        internal const int ChunkSize = 4096;
        internal const string AssemblyManagementPackResourceName = "ManagementPack";
        internal const string AssemblyManagementPackResourcePackageName = "MPResources.resources";

        private static string AssemblyAttributesCodeTemplate =
                @"using System.Reflection;
                  using System.Runtime.CompilerServices;
                  [assembly: AssemblyVersion(""{0}"")]
                  [assembly: AssemblyCompany(""{1}"")]
                  [assembly: AssemblyProduct(""{2}"")]
                  [assembly: AssemblyCopyright(""{3}"")]";

        #endregion

        #region File Extension constants
        internal static string XmlExtension = ".xml";
        internal static string MpbExtension = ".mpb";
        internal static string AssemblyExtension = ".mp";
        internal static bool isMpb = false;
        #endregion

    }

    /// <summary>
    /// This class is used as input to ManagementPackAssemblyWriter - to seal ManagementPacks
    /// Settings that can be specified for the ManagementPackAssemblyWriter
    /// </summary>
    public class FastAssemblyWriterSettings
    {
        /// <summary>
        /// Constructor to create a ManagementPackAssemblyWriterSettings 
        /// </summary>
        /// <param name="companyName">Name of the company</param>
        /// <param name="keyFilePath">The Keypair file (.snk) that has a key pair for signing the ManagementPack assembly. Usually created with sn.exe utility from .NET Framework</param>
        /// <param name="productName">Name of the Product that this ManagementPack Monitors (including version of the product) Example: Microsoft SQL Server 2005 ManagementPack</param>
        /// <param name="delaySign">Should the assemlby be delay signed?</param>
        public FastAssemblyWriterSettings(string companyName, string keyFilePath, bool delaySign)
        {
            //validate company name
            if (string.IsNullOrEmpty(companyName))
            {
                throw new ArgumentNullException("companyName");
            }
            this.companyName = companyName;

            //validate keyfile path
            if (string.IsNullOrEmpty(keyFilePath))
            {
                throw new ArgumentNullException("keyFilePath");
            }
            this.keyFilePath = keyFilePath;

            //initalize the output directory to be the default (current) directory
            this.OutputDirectory = ".";

            this.delaySign = delaySign;
        }


        #region Properties

        /// <summary>
        /// A value of the CompanyName Assembly attribute - to set on the sealed ManagementPack Assembly
        /// </summary>
        public string CompanyName
        {
            get { return this.companyName; }
        }

        /// <summary>
        /// A value of the ProductName Assembly attribute - to set on the sealed ManagementPack Assembly
        /// </summary>
        public string ProductName
        {
            get { return this.productName; }
            internal set { this.productName = value; }
        }
        /// <summary>
        /// A value of the Copyright Assembly attribute - to set on the sealed ManagementPack Assembly
        /// </summary>
        public string Copyright
        {
            get { return copyright; }
            set { this.copyright = value; }
        }

        /// <summary>
        /// Path to the Key pair to sign the ManagementPack with
        /// </summary>
        public string KeyFilePath
        {
            get { return this.keyFilePath; }
        }

        /// <summary>
        /// DelaySign flag
        /// </summary>
        public bool DelaySign
        {
            get { return (this.delaySign); }
            set { this.delaySign = value; }
        }

        /// <summary>
        /// The directory to write the assembly out to
        /// </summary>
        public string OutputDirectory
        {
            get { return (this.outputDirectory); }
            set
            {
                this.outputDirectory = value;

                //validate that the specified output directory exists
                DirectoryInfo info = null;
                try
                {
                    info = new DirectoryInfo(this.outputDirectory);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Output directory" + this.OutputDirectory + " is not accessible", e);
                }

                if (info == null || (info.Exists == false))
                {
                    throw new DirectoryNotFoundException("Output directory " + this.outputDirectory + " does not exist");
                }
            }
        }

        /// <summary>
        /// The version of the Assembly being written
        /// </summary>
        internal Version AssemblyVersion
        {
            get { return (this.version); }
            set { this.version = value; }
        }


        #endregion

        #region Private Fields
        private string outputDirectory;
        private string companyName;
        private string productName;
        private string copyright;
        private string keyFilePath;
        private bool delaySign;
        private Version version;
        #endregion

    }

}
