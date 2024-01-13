using System;
using System.IO;
using System.Resources;
using System.IO.Compression;

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

}
