using System;
using System.IO;

namespace xServiceManager.Module
{

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
