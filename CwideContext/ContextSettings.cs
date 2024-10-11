using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.ComponentModel;
using PluginCore.Localization;
using ASCompletion.Settings;
using PluginCore;
using System.Windows.Forms;


namespace CwideContext
{
    public delegate void ClasspathChangedEvent();

    [Serializable]
    public class ContextSettings : IContextSettings, InstalledSDKOwner
    {

        private Int32 sampleNumber = 69;
        private String sampleText = "CWide plugin";
        private Keys sampleShortcut = Keys.Control | Keys.F1;

        /// <summary> 
        /// Get and sets the sampleText
        /// </summary>
        [Description("A CWide string setting."), DefaultValue("This is a CWide plugin.")]
        public String SampleText
        {
            get { return this.sampleText; }
            set { this.sampleText = value; }
        }

        /// <summary> 
        /// Get and sets the sampleNumber
        /// </summary>
        [Description("A CWide integer setting."), DefaultValue(69)]
        public Int32 SampleNumber
        {
            get { return this.sampleNumber; }
            set { this.sampleNumber = value; }
        }

        /// <summary> 
        /// Get and sets the sampleShortcut
        /// </summary>
        [Description("A sample shortcut setting."), DefaultValue(Keys.Control | Keys.F1)]
        public Keys SampleShortcut
        {
            get { return this.sampleShortcut; }
            set { this.sampleShortcut = value; }
        }







        public event ClasspathChangedEvent OnClasspathChanged;

        #region IContextSettings Documentation

        const string LANGUAGE_WEBSITE = "http://Cw.net/manual";
        const string DEFAULT_DOC_COMMAND = "http://www.google.com/search?q=$(ItmTypPkg)+$(ItmTypName)+$(ItmName)+site:" + LANGUAGE_WEBSITE;
        protected string documentationCommandLine = DEFAULT_DOC_COMMAND;

        [DisplayName("Documentation Command Line")]
        [LocalizedCategory("ASCompletion.Category.Documentation"), LocalizedDescription("ASCompletion.Description.DocumentationCommandLine"), DefaultValue(DEFAULT_DOC_COMMAND)]
        public string DocumentationCommandLine
        {
            get { return documentationCommandLine; }
            set { documentationCommandLine = value; }
        }

        #endregion

        #region IContextSettings Members

        const bool DEFAULT_CHECKSYNTAX = false;
        const bool DEFAULT_COMPLETIONENABLED = true;
        const bool DEFAULT_GENERATEIMPORTS = false;
        const bool DEFAULT_PLAY = false;
        const bool DEFAULT_LAZYMODE = false;
        const bool DEFAULT_LISTALL = true;
        const bool DEFAULT_QUALIFY = true;
        const bool DEFAULT_FIXPACKAGEAUTOMATICALLY = true;

        protected bool checkSyntaxOnSave = DEFAULT_CHECKSYNTAX;
        private bool lazyClasspathExploration = DEFAULT_LAZYMODE;
        protected bool completionListAllTypes = DEFAULT_LISTALL;
        protected bool completionShowQualifiedTypes = DEFAULT_QUALIFY;
        protected bool completionEnabled = DEFAULT_COMPLETIONENABLED;
        protected bool generateImports = DEFAULT_GENERATEIMPORTS;
        protected bool playAfterBuild = DEFAULT_PLAY;
        protected bool fixPackageAutomatically = DEFAULT_FIXPACKAGEAUTOMATICALLY;
        protected string[] userClasspath = null;

        [Browsable(false)]
        public string LanguageId
        {
            get { return "Linx"; }
        }

  [Browsable(false)]
        public string DefaultExtension
        {
            get { return ".cpp"; }
        }



/*
        [Browsable(false)]
        public string DefaultExtension
        {
            get { return ".lnx"; }
        }*/

        [Browsable(false)]
        public string CheckSyntaxRunning
        {
            get { return TextHelper.GetString("Info.CwRunning"); }
        }

        [Browsable(false)]
        public string CheckSyntaxDone
        {
            get { return TextHelper.GetString("Info.CwDone"); }
        }

        [DisplayName("Check Syntax On Save")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.CheckSyntaxOnSave"), DefaultValue(DEFAULT_CHECKSYNTAX)]
        public bool CheckSyntaxOnSave
        {
            get { return checkSyntaxOnSave; }
            set { checkSyntaxOnSave = value; }
        }

        [DisplayName("User Classpath")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.UserClasspath")]
        public string[] UserClasspath
        {
            get { return userClasspath; }
            set
            {
                userClasspath = value;
                FireChanged();
            }
        }

        [DisplayName("Enable Completion")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.CompletionEnabled"), DefaultValue(DEFAULT_COMPLETIONENABLED)]
        public bool CompletionEnabled
        {
            get { return completionEnabled; }
            set { completionEnabled = value; }
        }

        [DisplayName("Generate Imports")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.GenerateImports"), DefaultValue(DEFAULT_GENERATEIMPORTS)]
        public bool GenerateImports
        {
            get { return generateImports; }
            set { generateImports = value; }
        }

        /// <summary>
        /// In completion, show all known types in project
        /// </summary>
        [DisplayName("List All Types In Completion")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.CompletionListAllTypes"), DefaultValue(DEFAULT_LISTALL)]
        public bool CompletionListAllTypes
        {
            get { return completionListAllTypes; }
            set { completionListAllTypes = value; }
        }

        /// <summary>
        /// In completion, show qualified type names (package + type)
        /// </summary>
        [DisplayName("Show QualifiedTypes In Completion")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.CompletionShowQualifiedTypes"), DefaultValue(DEFAULT_QUALIFY)]
        public bool CompletionShowQualifiedTypes
        {
            get { return completionShowQualifiedTypes; }
            set { completionShowQualifiedTypes = value; }
        }

        /// <summary>
        /// Defines if each classpath is explored immediately (PathExplorer) 
        /// </summary>
        [DisplayName("Lazy Classpath Exploration")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.LazyClasspathExploration"), DefaultValue(DEFAULT_LAZYMODE)]
        public bool LazyClasspathExploration
        {
            get { return lazyClasspathExploration; }
            set { lazyClasspathExploration = value; }
        }

        [DisplayName("Play After Build")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.PlayAfterBuild"), DefaultValue(DEFAULT_PLAY)]
        public bool PlayAfterBuild
        {
            get { return playAfterBuild; }
            set { playAfterBuild = value; }
        }

        [DisplayName("Fix Package Automatically")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.FixPackageAutomatically"), DefaultValue(DEFAULT_FIXPACKAGEAUTOMATICALLY)]
        public bool FixPackageAutomatically
        {
            get { return fixPackageAutomatically; }
            set { fixPackageAutomatically = value; }
        }

        #endregion

        #region Language specific members

        private string intrinsicPath;

        [DisplayName("Intrinsic Definitions")]
        [DefaultValue("Library\\Cw\\intrinsic")]
        [LocalizedCategory("ASCompletion.Category.Language"), LocalizedDescription("CwContext.Description.IntrinsicDefinitions")]
        public string LanguageDefinitions
        {
            get { return intrinsicPath; }
            set
            {
                if (value == intrinsicPath) return;
                intrinsicPath = value;
                FireChanged();
            }
        }

        #endregion

        #region Interface Implementations

   // List<InstalledSDK> allSdks = new List<InstalledSDK>();
/*
        public InstalledSDK GetDefaultSDK()
        {
            return null;
        }
*/



	 public InstalledSDK SetDefaultSDK()
        {	
		   if (installedSDKs == null || installedSDKs.Length == 0) {
				 var sdk = new InstalledSDK(this);
                    sdk.Path = "Default(Tools/Cwc/)";
                    sdk.Name = "Cwc";
					sdk.Version = "Default";
                    //sdk. = "Cwc/";
                //    installedSDKs[installedSDKs.Length] = sdk;
					installedSDKs = new InstalledSDK[1];
				 installedSDKs[0] = sdk;				
			}
			
			return installedSDKs[0];
		}

		  protected InstalledSDK[] installedSDKs;
        public InstalledSDK GetDefaultSDK()
        {
            if (installedSDKs == null || installedSDKs.Length == 0) {
              //  return InstalledSDK.INVALID_SDK;
                return SetDefaultSDK();
			}

            foreach (InstalledSDK sdk in installedSDKs) {
                if (sdk.IsValid) return sdk;
			}
           // return InstalledSDK.INVALID_SDK;
            return  SetDefaultSDK();
        }

         public string GetCwcPath()
        {
            string _sPath = GetDefaultSDK().Path;
            _sPath = _sPath.Replace('\\','/');
            if(_sPath[_sPath.Length - 1]!= '/') {
                _sPath += "/";
            }
             _sPath += "cwc.exe";
            return _sPath;

        }



      

        public bool ValidateSDK(InstalledSDK sdk)
        {
            return true;
        }

     //[LocalizedCategory("ASCompletion.Category.Language"), DefaultValueAttribute("Path to the Cwc compiler.")]
       // [LocalizedCategory("ASCompletion.Category.Language"), LocalizedDescription("HaXeContext.Description.HaXePath")]

        [DisplayName("Installed CWide SDKs")]
        [LocalizedCategory("ASCompletion.Category.Language"), LocalizedDescription("CwideContext.Description.CwidePath")]
        public InstalledSDK[] InstalledSDKs
        {
            get { return installedSDKs; }
            set
            {
                installedSDKs = value;
                FireChanged();
            }
        }
/*
        [Browsable(false)]
        public InstalledSDK[] InstalledSDKs
        {
            get { return new InstalledSDK[0]; }
            set {  }
        }
*/

        #endregion
        
        [Browsable(false)]
        private void FireChanged()
        {
            if (OnClasspathChanged != null) OnClasspathChanged();
        }

    }

}
