using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using PluginCore.Managers;
using ASCompletion.Context;
using ASCompletion.Model;
using PluginCore.Localization;
using PluginCore.Controls;
using PluginCore.Helpers;
using PluginCore;
using ASCompletion.Completion;
using System.Windows.Forms;
using ASCompletion;
using ProjectManager.Controls.TreeView;
using ProjectManager;
using ProjectManager.Projects;
using System.Drawing;
using System.Threading;
using ASCompletion.Settings;
using PluginCore.BBCode;
using static CwideContext.ProjectNode;
using FlashDevelop.Dialogs;
using System.Diagnostics;
using ScintillaNet;
using FlashDevelop.Managers;
using System.Resources;
using static CwideContext.WinApi;

namespace CwideContext
{

    public class Context : ASContext
    {



        public  List<ToolStripItem> MenuItems = new List<ToolStripItem>();
        public  ToolStripMenuItem MenuItem_GotoDecl =  null;
        public  ToolStripMenuItem MenuItem_SwapHeaderSource =  null;
        public string sMenuItem_GotoDecl =  "";


        public ToolStripComboBoxEx  CbTargetBuild =	null;
        public ToolStripComboBoxEx  CbSelectConfiguration =	null;

        public ToolStripButton btBuildProject = null;
        ToolStripComboBoxEx  TargetBuildSelector = null;

            //     oContext.CbTargetBuild.Items.Clear();
		      //    oContext.CbTargetBuild.Items.Add("aaaaaaaaaaaaaaaa");



        private void TargetBuildSelector_SelectedIndexChanged(object sender, EventArgs e) {
                Program.AddVar("BuildFile",  TargetBuildSelector.Text);
             //  TraceManager.Add("SELection!! "  +     TargetBuildSelector.Text);

        }

  




       internal void fEnableGotoDecl(string _sFile) {
              ///          TraceManager.Add("fEnableGotoDecl "  +_sFile);
             bIsPossibleGotoDecl = true;
            string _sFileName = _sFile;
            try { _sFileName = Path.GetFileName(_sFile); }catch(Exception e) { };

           // TraceManager.Add("_sFileName " + _sFile);
           if(MenuItem_GotoDecl != null) {
                MenuItem_GotoDecl.Enabled = true;
                MenuItem_GotoDecl.Text = "Goto \""+ _sFileName + "\"";
           }
           sGotoDecl_File = _sFile;
        }

        
        internal void fDisableGotoDecl()
        {
           bIsPossibleGotoDecl = false;
            if(MenuItem_GotoDecl != null) {
                MenuItem_GotoDecl.Enabled = false;
                MenuItem_GotoDecl.Text = sMenuItem_GotoDecl;
            }
        }


        
        public string sGotoDecl_File = "";
        public bool bIsPossibleGotoDecl = true;
         public  bool TryGotoDeclaration() {
            if(bIsPossibleGotoDecl) {
                GotoDeclaration(null,null);
                return true;
            }
            return false;
        }

        

        




        public  void SwapHeaderSource(object sender, EventArgs e) {
            string _sFilePath = fGetSwapFile(sCurrFileName);
              if(_sFilePath != "") {
                      fOpenFile( _sFilePath, false);
              }
        }


         public  void GotoDeclaration(object sender, EventArgs e) {
             TraceManager.Add("*GotoDeclaration!! " );
             string _sFilePath = fFoundFile(sGotoDecl_File);
             if(_sFilePath != "") {
                 fOpenFile( _sFilePath, false);
            }
        }

        
        public void fOpenFile(string _sFile, bool _bRestorFileState ) {
          // _sFile = Path.GetFullPath(_sFile);
               MainForm.OpenEditableDocument(  Path.GetFullPath(_sFile), false);
        }

        public  string fGetSwapFile(string _sFullPathFile) {
            //Get extention
             string _sExt =  Path.GetExtension(_sFullPathFile);
            string _sSearchExt = "";
            if(_sExt == null || _sExt.Length <= 0) { return "";}
            switch(_sExt) {
                case ".cpp":
                    _sSearchExt = ".h";
                 break;
                 case ".h":
                    _sSearchExt = ".cpp";
                 break;
            }
            string _sFile =  sCurrFileName.Substring(0, sCurrFileName.Length - _sExt.Length) + _sSearchExt;
             if(File.Exists(_sFile)) { //SameDirectory first
                 return _sFile; //Direct found
            }

           string _sClassPath  =  fFindClassPathFile(_sFile);
           if(_sClassPath != "") {
                string _sRelativePathResult = _sFile.Substring(_sClassPath.Length );
                  return fFoundFile(_sRelativePathResult);
           }
           return "";
        }
        
        public  string fFindClassPathFile(string _sFullPathFile) {
             ///----Find class path file----
            string _sLastFoundPath = "";//Take only longer path
             foreach (string _sPath in oCurrProject.oProject.Classpaths) {
                string _sFullPath = "";
                if(_sPath.Length > 1) {
                    if(_sPath[1] == ':'){ 
                        _sFullPath = _sPath + "\\";
                    }else {
                        _sFullPath = oCurrProject.oProject.Directory + "\\" + _sPath + "\\"; //Convert to Absolute
                    }
                }
                if(_sFullPathFile.Length >= _sFullPath.Length ) {
                    if( _sFullPathFile.Substring(0,_sFullPath.Length) == _sFullPath) { 
                        if(_sFullPath.Length > _sLastFoundPath.Length){
                            _sLastFoundPath = _sFullPath;
                        }
                    }
                }
            }
             return _sLastFoundPath;
        }

        public string fFoundFile(string _sFile) {

            string _sCurrentDirectoryFile = Path.GetDirectoryName(  sCurrFileName) + "\\" +  _sFile;
            if(File.Exists(_sCurrentDirectoryFile)) {
                return _sCurrentDirectoryFile;
            }
             foreach (string _sPath in oCurrProject.oProject.Classpaths) {
                string _sFullPath = "";
                if(_sPath.Length > 1) {
                    if(_sPath[1] == ':'){ 
                        _sFullPath = _sPath + "\\" +  _sFile;
                    }else {
                        _sFullPath = oCurrProject.oProject.Directory + "\\" + _sPath + "\\" +  _sFile;
                    }
                }
                if(File.Exists(_sFullPath)) {
                    return _sFullPath;
                }
               //    TraceManager.Add("Test "  + _sFullPath); 
            }
             return "";
        }








         public override bool IsFileValid{ get { return false; }  } //Don't use any build'in completion

         public override bool CanBuild {
            get { 
             return false; //True on .cwc*.bat?
            }
        }






           

public static bool bOnlyOne = false;
public void fCreateComboBox() {

	try {
		if(!bOnlyOne) {		
			bOnlyOne =true;

			
			settings.GetDefaultSDK(); //Set default Sdk if not set




			TargetBuildSelector = new ToolStripComboBoxEx();
			TargetBuildSelector.Name = "ViewSelector";
			TargetBuildSelector.ToolTipText = "Test";
			TargetBuildSelector.DropDownStyle = ComboBoxStyle.DropDownList;

			TargetBuildSelector.AutoSize = false;
			TargetBuildSelector.Width = 180;
			TargetBuildSelector.Margin = new Padding(1, 0, 0, 0);
			TargetBuildSelector.FlatStyle = PluginBase.MainForm.Settings.ComboBoxFlatStyle;
			TargetBuildSelector.Font = PluginBase.Settings.DefaultFont;
            TargetBuildSelector.Visible = false;
  TargetBuildSelector.FlatCombo.SelectedIndexChanged  += new System.EventHandler(TargetBuildSelector_SelectedIndexChanged);


// TargetBuildSelector.BackColor = Color.FromArgb(0xFFFFFF);

		//	PluginBase.MainForm.ToolStrip.Items.Add(TargetBuildSelector);
			//PluginBase.MainForm.ToolStrip.Items.Insert();

		//	PluginBase.MainForm.RegisterShortcutItem("ProjectMenu.TargetBuildSelector", Keys.Control | Keys.F8);
	//		PluginBase.MainForm.RegisterSecondaryItem("ProjectMenu.TargetBuildSelector", TargetBuildSelector);

 // toolBar.Items


//menus.BuildProject.Click += BuildProjectClick;

		bool bFound = false;
		for (int i =0; i < PluginBase.MainForm.ToolStrip.Items.Count; i++ ) {
					TraceManager.Add("Name ! : " + PluginBase.MainForm.ToolStrip.Items[i].Name);

			switch(PluginBase.MainForm.ToolStrip.Items[i].Name ) {
				case "TargetBuildSelector":
					PluginBase.MainForm.ToolStrip.Items.Insert(i+1,TargetBuildSelector);
				    bFound = true;
				break;
		
			}
			
		}
		if(!bFound) {
			PluginBase.MainForm.ToolStrip.Items.Add(TargetBuildSelector);
		}

		try {
		  CbTargetBuild =		(ToolStripComboBoxEx)(PluginBase.MainForm.ToolStrip.Items.Find("TargetBuildSelector", true)[0]);
    	}catch(Exception e) {}

       try {
		  CbSelectConfiguration =		(ToolStripComboBoxEx)(PluginBase.MainForm.ToolStrip.Items.Find("ConfigurationSelector", true)[0]);
    	}catch(Exception e) {}

        try {



            //  plugin.MenuItems...Find("",true);
		//  CbSelectConfiguration =		(ToolStripComboBoxEx)(PluginBase.MainForm.p.MenuItems.Items.Find("ConfigurationSelector", true)[0]);
    	}catch(Exception e) {}
        

        try {
		    btBuildProject =		(ToolStripButton)(PluginBase.MainForm.ToolStrip.Items.Find("BuildProject", true)[0]);
	     	btBuildProject.Click += delegate { StopBuild(); };
    	}catch(Exception e) {}
		
       //  CbTargetBuild.Visible = false;
		CbTargetBuild.Enabled = false;
	//	CbTargetBuild.Width = 400;
		CbTargetBuild.DropDownStyle = ComboBoxStyle.DropDownList;
	//	CbTargetBuild.Items.Clear();

        string [] fileEntries = Directory.GetFiles( oCurrProject.oProject.Directory);
        foreach(string _sFile in fileEntries) {
            string _sFileName = Path.GetFileName(_sFile);
                        TraceManager.Add("path " +   _sFileName);
            if(_sFileName.Length >= 4 && _sFileName.Substring(0,4).ToLower() == "make"  ){
                 //string _sName = _sFile.Substring(4);

                 TargetBuildSelector.Items.Add(_sFileName);
            }
         }
        if(TargetBuildSelector.Items.Count > 0) {
            TargetBuildSelector.SelectedIndex = 0;
         }
      //  PluginMain.oMainConsole.fSend("Make.bat");
      //  PluginMain.oMainConsole.fSend("Make2.bat");



         foreach (ToolStripItem item in plugin.MenuItems){ MenuItems.Add(item); } plugin.MenuItems.Clear();//Remove auto handle from ASCompletion

         foreach (ToolStripItem item in MenuItems){
                
              
                ItemData _oData =  (ItemData)item.Tag;
                if(_oData != null) {
                              //TraceManager.Add("*ToolStripItem* " + item.Name + " " + item.Text + " " + _oData.Id ); 
                         //     TraceManager.Add("*ToolStripItem* "  + _oData.Id ); 
                    switch(_oData.Id) {
                        case "none;SearchMenu.GotoDeclaration":
                               MenuItem_GotoDecl = (ToolStripMenuItem)item;
                               MenuItem_GotoDecl.Click +=  new EventHandler(GotoDeclaration);
                               sMenuItem_GotoDecl = MenuItem_GotoDecl.Text;
                         break;
                         case "none;SearchMenu.GotoTypeDeclaration":

                        break;

                    }
                            /*
                        TraceManager.Add("Data " + _oData.Id  );//SearchMenu.GotoDeclaration
                    if(item is ToolStripMenuItem ){
                        ToolStripMenuItem _oItem = (ToolStripMenuItem)item;
                        TraceManager.Add("*Shortcut! " + _oItem.ShortcutKeys  + " " + _oItem.ShortcutKeyDisplayString);

                    }*/
                }
                item.Enabled = false;
            }


       //  PluginBase.MainForm.EditorMenu.Items.Add(MenuItem_GotoDecl);
         //PluginBase.MainForm.EditorMenu.Items.Insert(0,MenuItem_GotoDecl);
        PluginBase.MainForm.TabMenu.Items.Insert(0, new ToolStripSeparator());
          
               
                 MenuItem_SwapHeaderSource = new ToolStripMenuItem("Swap Header/Source", PluginBase.MainForm.FindImage("99|9|3|-3"), new EventHandler(SwapHeaderSource));//TOdo
                 PluginBase.MainForm.TabMenu.Items.Insert(0,MenuItem_SwapHeaderSource);
                    //     PluginBase.MainForm.RegisterSecondaryItem("SearchMenu.GotoDeclaration", item);//Util?
                   // emenu.Items.Insert(4, item);
                 //   menuItems.Add(item);


                    


	}
	}catch(Exception e) {}
}



        internal void fAddError(string _sFullArg){
             string _sMainCallStack = "";
             string _sType = "";
             string _sSeverity= "";
             string _sMsg= "";
            //E:\_Project\_MyProject\AS3Engine\Engine\MyEngine.as(45): col: 4 Error: Access of undefined property aaaaaaaaaaa.
         //   |CallStack:*file:E:\TestProjectFD\TestCw\src\HelloWorld.cpp*line:8<|type:syntaxError|severity:errormsg:Invalid number of character '{' when these macros are defined: 'tPlatform_Windows=1;_WIN32=1;__GNUC__=4;__clang__=1;__llvm__=1;__x86_64__=1;tPlatform_Windows=1;_WIN32=1;__GNUC__=4;__clang__=1;__llvm__=1;__x86_64__=1'.
            string[] _aArg  = _sFullArg.Split('|');
            foreach(string _sArg in _aArg) {
                int _nIndexTag = _sArg.IndexOf(':'); if(_nIndexTag != -1) {
                    string _sTagArg = _sArg.Substring(_nIndexTag + 1);
                    switch(_sArg.Substring(0, _nIndexTag)) {
                        case "CallStack":
                            //Multiple source?
                           _sMainCallStack = fGetCallStack(_sTagArg);
                        break;
                         case "Type":
                            _sType = _sTagArg;
                         break ;
                         case "Severity":
                            _sSeverity = _sTagArg;
                         break ;
                          case "Msg":
                            _sMsg = _sTagArg;
                         break;
                    }
                }
            }


            
            
            //Add to result panel
            TraceManager.Add(_sMainCallStack + ":" + _sSeverity + ": " + _sMsg);
        }

        internal string fGetCallStack(string _sFullArg){
            string _sMainResult = "";
           string[] _aArg  = _sFullArg.Split('<');
           foreach(string _sArg in _aArg) {

                string _sFile = ""; 
                //int _nLine = 0; 
                string _sLine = ""; 

                 string[] _aSubArg  = _sArg.Split('*');
                foreach(string _sSubArg in _aSubArg) {
                      TraceManager.Add("_sSubArg: " + _sSubArg);
                     int _nIndexTag = _sSubArg.IndexOf(':'); if(_nIndexTag != -1) {

                        string _sTagArg = _sSubArg.Substring(_nIndexTag + 1);

                        switch(_sSubArg.Substring(0, _nIndexTag)) {
                            case "File":
                               _sFile = _sTagArg;
                                  TraceManager.Add("file: " + _sTagArg);
                            break;
                             case "Line":
                                _sLine = _sTagArg;
                                TraceManager.Add("_sLine: " + _sLine);
                             //   Int32.TryParse(_sTagArg, )
                            break;
                        }
                    }
                }

                _sMainResult = _sFile +  "(" + _sLine + ")";
                return _sMainResult; //Only first (for now)
           }

            return "";
        }



        public bool bCompletionListCreated = false;
        public void fCreateCompletionList() {
           if(!bCompletionListCreated) {
                try {
        
                    Completion.CreateControl(PluginBase.MainForm);
                    /*
                    codeTip = new CodeTip(PluginBase.MainForm);
                    simpleTip = new RichToolTip(PluginBase.MainForm);
                    callTip = new MethodCallTip(PluginBase.MainForm);*/
                }
                catch(Exception ex)
                {
                    ErrorManager.ShowError(/*"Error while creating editor controls.",*/ ex);
                }
            }
        }

        /*
         public static string fApplyCustomArg(string _sArgs){
            foreach(Argument _oArg in PluginBase.MainForm.CustomArguments) {
                if(_oArg.Key == _sKey)  {
                    _oArg.Value = _sValue;
                    return;
                }
            }
          return   _sArgs; 
        }*/


        internal static void fSetDefaultArgument() {
             fSetArgument("wPackageGuard_usr", "$(wFileType)_$(ProjectName)_$(wPackageUnderscore)$(FileName)", true);
             fSetArgument("wNamespaceConst", "$(FileName)_", true);
        }


       public static bool fDeleteArgument(string _sKey){
            foreach(Argument _oArg in PluginBase.MainForm.CustomArguments) {
                if(_oArg.Key == _sKey)  {
                    PluginBase.MainForm.CustomArguments.Remove(_oArg);
                    return true;
                }
            }
            return false;
        }
        public static string fGetArgument(string _sKey){
            foreach(Argument _oArg in PluginBase.MainForm.CustomArguments) {
                if(_oArg.Key == _sKey)  {
                    return _oArg.Value;
                }
            }
            return "";
        }


        public static void fSetArgument(string _sKey, string _sValue, bool _bIfNotExistOnly = false){
            foreach(Argument _oArg in PluginBase.MainForm.CustomArguments) {
                if(_oArg.Key == _sKey)  {
                    if(_bIfNotExistOnly) { return; }
                    _oArg.Value = _sValue;
                    return;
                }
            }
            PluginBase.MainForm.CustomArguments.Add(new Argument(_sKey,_sValue));
        }


       internal void fProjLoad() {
            fCreateComboBox();
            if(CbTargetBuild != null) CbTargetBuild.Visible = false;
            if(CbSelectConfiguration != null) CbSelectConfiguration.Visible = false;

            if(TargetBuildSelector  != null)TargetBuildSelector.Visible = true;

            fLauchCwc();
            fCreateCompletionList();

            
            RegisterFile.fRegisterAllFileType(false);
            fSetDefaultArgument();

        
        }

      

 
        internal void fProjClose() {
             if(CbTargetBuild != null) CbTargetBuild.Visible = true;
             if(CbSelectConfiguration != null) CbSelectConfiguration.Visible = true;

             if(TargetBuildSelector != null) TargetBuildSelector.Visible = false;

        }


        
        public static LauchTool oCwc = null;
        private void fLauchCwc() { //Start AST
            oCwc = new LauchTool();
        
		    oCwc.sWorkPath = PluginMain.CwcRootPath().Replace("\\", "/") + "/";
		    oCwc.dOut = new LauchTool.dIOut(fCwcOut);

           string _sProjectDir = PluginMain.fGetPrjDir();
             string _sLauchCmd = PluginMain.fGetCmdLauch();

            string _sPath = _sProjectDir + PluginMain.fGetFile(_sLauchCmd);
            TraceManager.Add("LauchPath!!"  + _sPath);
         //   oCwc.fLauchExe( oCwc.sWorkPath + "cwc.exe","-wMode IDE -wCloseOnId " + Process.GetCurrentProcess().Id); //_sPath is the current project
          //  oCwc.fLauchExe(_sPath,"-wMode IDE -wCloseOnId " + Process.GetCurrentProcess().Id); //_sPath is the current project
          
         //   _sPath = @"E:\TestProjectFD\TestCw\Build.bat";
           // oCwc.fLauchExe("cwc","-wMode IDE -wCloseOnId " + Process.GetCurrentProcess().Id + "| -wLauch " +_sPath + "|"); //_sPath is the current project
            oCwc.fLauchExe("cwc","-{wMode}= IDE -{wCloseOnId}= " + Process.GetCurrentProcess().Id + "| -#Lauch " +_sPath + "|"); //_sPath is the current project
  
          }

        public void fCwcOut(string _sOut)  {

	            TraceManager.Add("Ukn: " +  _sOut);

		    if(_sOut.Length > 4 && _sOut[0] == 'c' && _sOut[1] == 'w'  && _sOut[2] == 'c' && _sOut[3] == ':' ) {
			 //   Debug.fTrace("!!!!!!!!CWC: " + _sOut);
			    //Console.WriteLine("Cwc: " +  _sOut);
			    Msg.fRecewiveMsg(_sOut.Substring(4) );
		    }else

		    if(_sOut.Length > 7 && _sOut[0] == 'c' && _sOut[1] == 'w'  && _sOut[2] == 'c' && _sOut[3] == 'A' ) { //CwcAst:

			    //Console.WriteLine("Ast: " +  _sOut);
			    Msg.fRecewiveMsg(_sOut.Substring(7) );

		    }else {
			    //Console.WriteLine("Ukn: " +  _sOut);
    			//TraceManager.Add("Ukn: " +  _sOut);
		    }
        }	









       internal void fUpdate_BuildFiles() {
            

       }







		public void StopBuild()
		{
			TraceManager.Add("STOPBUILD!!!!!!!!");
			PluginMain.oProjectManager.UpdateUIStatus(ProjectManagerUIStatus.NotBuilding);
            oPluginMain.fStopBuild();
		}




		#region initialization
		new static readonly protected Regex re_CMD_BuildCommand =
            new Regex("@lnx[\\s]+(?<params>.*)", RegexOptions.Compiled | RegexOptions.Multiline);

        static private readonly Regex re_CWext =
            new Regex(".(lnx|cpp|c)[3-9]?", RegexOptions.Compiled);


        public static Context oSingleton;
        private ContextSettings langSettings;
        private List<InlineRange> cwRanges; // inlined CW ranges in HTML
        public ClassModel curClass;
        public ClassModel curFunc;
        public ClassModel curExp;

        public static ScintillaNet.ScintillaControl oCurSci;

        public ClassModel curFInfoClass;
        public ClassModel curFInfo;

        public string sCurrFileName;
        public FileModel oCurrFileModel;
    //    public ProjectNode oCurrProject;
        public ProjectNode oCurrProject; //New temp ?

 //  public ProjectNode node; //Cw oCurrProject
 //  public List<CwLibGroup> aLib = new List<CwLibGroup>(); //Cw oCurrProject



        public string sCurExp = "";
        public PluginMain oPluginMain;

            public Boolean    hasLevels = false;
            public string docType = "void";



        public override IContextSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        public Context(ContextSettings initSettings)
        {
			oCurrProject = new ProjectNode(this);
			
            oSingleton = this;
            langSettings = initSettings;
			classPath = new List<PathModel>();

            /* AS-LIKE OPTIONS */

   
            /* DESCRIBE LANGUAGE FEATURES */

            // language constructs
            features.hasPackages = false;
            features.hasImports = false;
            features.hasImportsWildcard = false;
            features.hasClasses = true;
            features.hasExtends = true;
            features.hasImplements = true;
            features.hasInterfaces = true;
            features.hasEnums = false;
            features.hasGenerics = false;
            features.hasEcmaTyping = false;
            features.hasVars = true;
            features.hasConsts = true;
            features.hasMethods = true;
            features.hasStatics = true;
            features.hasTryCatch = true;
            features.checkFileName = false;

            // allowed declarations access modifiers
            Visibility all = Visibility.Public | Visibility.Protected | Visibility.Private;
            features.classModifiers = all;
            features.varModifiers = all;
            features.methodModifiers = all;

            // default declarations access modifiers
            features.classModifierDefault = Visibility.Public;
            features.varModifierDefault = Visibility.Public;
            features.methodModifierDefault = Visibility.Public;

            // keywords
            features.dot = "->";
            features.voidKey = "void";
            features.objectKey = "Object";
            features.typesPreKeys = new string[] { "namespace", "new", "extends", "implements", "as" };
            features.codeKeywords = new string[] {
                "and", "or", "xor", "exception", "as", "break", "case", "continue", "declare", "default", 
                "do", "else", "elseif", "enddeclare", "endfor", "endforeach", "endif", "endswitch", 
                "endwhile", "for", "foreach", "global", "if", "new", "switch", "use", "while", 
                "try", "catch", "throw"
            };
            features.varKey = "var";
            features.functionKey = "function";
            features.staticKey = "static";
            features.publicKey = "public";
            features.privateKey = "private";
            features.intrinsicKey = "extern";

            /* INITIALIZATION */

            settings = initSettings;
            //BuildClassPath(); // defered to first use

		//	fCreateComboBox();
        }
        #endregion


		public List<CwLib> aRcList  = new List<CwLib>();
        internal bool bProjActive;

        #region classpath management
        /// <summary>
        /// Classpathes & classes cache initialisation
        /// </summary>
        public override void BuildClassPath()
        {
            ReleaseClasspath();
            started = true;
            if (langSettings == null) throw new Exception("BuildClassPath() must be overridden");
            if (contextSetup == null)
            {
                contextSetup = new ContextSetupInfos();
                contextSetup.Lang = settings.LanguageId;
                contextSetup.Platform = "Linx";
                contextSetup.Version = "5.0";
            }

            //
            // Class pathes
            //
  //          classPath = new List<PathModel>();
            // intrinsic language definitions
            if (langSettings.LanguageDefinitions != null)
            {
                string langPath = PathHelper.ResolvePath(langSettings.LanguageDefinitions);
                if (Directory.Exists(langPath)) AddPath(langPath);
            }

            // add external pathes
            List<PathModel> initCP = classPath;
            classPath = new List<PathModel>();
            if (contextSetup.Classpath != null)
            {
                foreach (string cpath in contextSetup.Classpath)
                    AddPath(cpath.Trim());
            }

            // add library
            AddPath(Path.Combine(PathHelper.LibraryDir, settings.LanguageId + "/classes"));
            // add user pathes from settings
            if (settings.UserClasspath != null && settings.UserClasspath.Length > 0)
            {
                foreach (string cpath in settings.UserClasspath) AddPath(cpath.Trim());
            }
            // add initial pathes
            foreach (PathModel mpath in initCP) AddPath(mpath);

            // parse top-level elements
            InitTopLevelElements();
            if (cFile != null) UpdateTopLevelElements();

            // add current temporaty path
            if (temporaryPath != null)
            {
                string tempPath = temporaryPath;
                temporaryPath = null;
                SetTemporaryPath(tempPath);
            }
            FinalizeClasspath();

         
        }


        public static void fCwcSend(string _sMsg){
            if(oCwc != null) {
              oCwc.fSend("cwc:" + _sMsg);
             }
        }

        /// <summary>
        /// Build the file DOM
        /// </summary>
        /// <param name="filename">File path</param>
        protected override void GetCurrentFileModel(string fileName)
        {
          

           //  fSetArgument("wPackageGuard","Testcc");
     //    Console.WriteLine("---------------------!!!!!!!!!!!!!---------");
            
            sCurrFileName = fileName;
         //   TraceManager.Add("-------GetCurrentFileModel-----: " + fileName);
          //  cFile = FileModel.Ignore;

            
            string ext = Path.GetExtension(fileName);
           // if (!re_CWext.IsMatch(ext))  //Not Linx / ignore
            if (!(PluginMain.bIsCpp || PluginMain.bIsCwc || PluginMain.bIsLinx) || !PluginMain.bCwcStarted )  //Not Linx / ignore
            {
       //         Console.WriteLine("-------NOT Linx-----: "  + fileName);
                cFile = FileModel.Ignore;
                UpdateContext(cLine);
            }
            else
            {
          //     Console.WriteLine("-------Im Linx---------");
          //      Console.WriteLine("-------SendMsg|GetClassInfo " + sCurrFileName);
         
                
            //    fCwcSend("GetClassInfo|" + sCurrFileName + "|");
                Msg.fSendToCppAst("*GetClassInfo|" + sCurrFileName + "|");
               // fCwcSend("ExtractClassFunc|" + sCurrFileName + "|");  //tempsss
                Msg.fSendToCppAst("*ExtractClassFunc|" + sCurrFileName + "|");  //tempsss
                PluginMain.fAnalyseScope(sCurrFileName, true);


                cFile = FileModel.Ignore;
                UpdateContext(cLine);
                /*
                cFile = new FileModel(fileName);
                cFile.Context = this;
                cFile.HasFiltering = true;
                //ASFileParser parser = new ASFileParser();
                //parser.ParseSrc(cFile, CurSciControl.Text);

                MemberModel member = new CwideMemberModel("oImg", "Tag?", FlagType.Variable | FlagType.Dynamic, Visibility.Public);
                member.LineTo = member.LineFrom = 30;

                cFile.Members.Add(member);
                TraceManager.Add("------- New FileModel---------");

                cLine = CurSciControl.CurrentLine;
                UpdateContext(cLine);
                */

               // fUpdateClassInfo();
            }
        }

        /// <summary>
        /// When selecting a node in the outline view
        /// </summary>
        /// <param name="node"></param>
        public override  void OnSelectOutlineNode(TreeNode node)
        {
  //TraceManager.Add("Ass  CLick :" + node.va);
            /*
            Type type = node.GetType();
while (type != null)
{
    TraceManager.Add(type.Name);
    type = type.BaseType;
}*/

            if (node is CWideMemberTreeNode)
            {


                CWideMemberTreeNode oNode = (CWideMemberTreeNode)node;


                CwideMemberModel _oMember = oNode.oMember;

           
            //    TraceManager.Add("My  CLick import:" + _oMember.Value);

                ClassModel aClass;
                // imports

              if (node.Tag as string == "class")
                {
                    aClass = Context.CurrentModel.GetClassByName(node.Text);
                    if (!aClass.IsVoid())
                    {
                        string name = (aClass.InFile.Version < 3) ? aClass.QualifiedName : aClass.Name;
                        ASComplete.LocateMember("(class|interface|abstract)", name, aClass.LineFrom);
                    }
                }
                else if (node.Tag != null && node.Tag is string)
                {
                   // TraceManager.Add("My  member:" + node.Tag + " " + oNode.oMember.Value);

                    if (_oMember.Flags == FlagType.Import || _oMember.Flags ==  FlagType.Extends)
                    {

                       // string _sPath = _oMember.Value.Replace("/", "\\");
						
                        string _sPath = _oMember.Value;
						
                      //  TraceManager.Add("My ---import:" + _sPath);

                        if (File.Exists(_sPath))
                        {
                        //    TraceManager.Add("Exist!!" + _oMember.LineFrom);
							
                           fOpenFile(oNode.oMember.Value, false);

                            ASComplete.LocateMember("(class|extension|interface|abstract)", _oMember.Name, _oMember.LineFrom);

                        }
                    }else{
	//Console.WriteLine("Mebmer !!!" );
                        string[] info = (node.Tag as string).Split('@');
                        int line;
                        if (info.Length == 2 && int.TryParse(info[1], out line))
                        {

				//		Console.WriteLine("Mebmer !!!:" +_oMember.Name );

							if(_oMember.oLinkedMember != null) { //Linked import file
								  string _sPath = _oMember.oLinkedMember.Value;
								//	Console.WriteLine("Mebmer _sPath!!!" + _sPath);
								   if (File.Exists(_sPath)){
										 fOpenFile( _oMember.oLinkedMember.Value, false);
									}
							}
							string _sPrecVar = "";
							if(!PluginMain.bIsCpp) {//Cpp permit all
								_sPrecVar = "(function|var|const|get|set|property|#region|namespace|,)";
							}
                            ASComplete.LocateMember(_sPrecVar, info[0], line);
                        }
                    }

                }
            }
        }

     

        public void fUpdateAllLibInfo(string _sArg)
        {


try {
            oCurrProject.oProject.Classpaths.Clear(); //Todo better way

            string[] _aArg = _sArg.Split('|');

	Debug.fTrace("------- ----!!!!!!! UpdateAllLibInfo !!!!!!!---------  " );

            string[] _aGroup = _aArg[0].Split(';');
            foreach (string _sGroup in _aGroup) {
				if(_sGroup != "") {
				  fUpdateGroupLibInfo(_sGroup);
				}
            }
			
            		TraceManager.Add("**********  Bef fRefreshTree  ************");
			//ProjectTreeView.Instance.BuildTree();
		//	Thread.Sleep(2000);
			fRefreshTree(true);
		
		TraceManager.Add("********** Aft fRefreshTree  ************");
		Debug.fTrace("**********  fRefreshTree  ************");

       //     CompileCpp.sOuputPath = _aArg[1];

}catch( Exception e ) {
	Debug.fTrace("errror: " +e.Message + " " + e.ToString());
}

        }


		 public void fRefreshTree(bool _bFull = false) {
           

            


       //      ModelsExplorer.Instance.UpdateTree();

    // plugin.Tre       Tree



            
		//	 PluginMain.SafeInvoke(delegate {  
				ProjectTreeView.Instance.RebuildTree();
         //    });




				if(_bFull) {
					var prefs = ProjectManager.PluginMain.Settings.GetPrefs(oCurrProject.oProject); //Restore build tree from saved config, must be after
					ProjectTreeView.Instance.ExpandedPaths = prefs.ExpandedPaths;
					//Reset at top
					Win32.SetScrollPos(ProjectTreeView.Instance, new Point()); 
					 if (ProjectTreeView.Instance.Nodes.Count > 0) ProjectTreeView.Instance.SelectedNode = ProjectTreeView.Instance.Nodes[0] as GenericNode;
				}
        

                

		}



        public void fUpdateGroupLibInfo(string _sGroupArg) {



			string[] _aGroupArg = _sGroupArg.Split('<');
			

			string _sGroupName = _aGroupArg[0];
			string _sGroupReadPath = _aGroupArg[1];
			string _sGroupWritePath = _aGroupArg[2];
			string _sGroupLibs = _aGroupArg[3];

			TraceManager.Add("------- -------- NEW _sGroup --------- : " + _sGroupName);
			Debug.fTrace("------- -------- NEW _sGroup --------- : " + _sGroupName);

	
			CwLibGroup _oGroup = new CwLibGroup(_sGroupName, _sGroupReadPath, _sGroupWritePath);
			//oCurrProject.aLib.Add(_oGroup);
			oCurrProject.aLib.Add(_oGroup);


//		TraceManager.Add("------- _sGroupArg --------- : " +  _sGroupArg);

			//TraceManager.Add("------- _sGroupLib --------- : " +  _sGroupLibs);
			
			string[] _aLibArg = _sGroupLibs.Split('*');
			foreach (string _sLibArg in _aLibArg) {
				if(_sLibArg != "") {
					string[] _aLib = _sLibArg.Split(',');
				
					string _sType = _aLib[0];
					
					switch(_sType) {
						case "Lib":
							TraceManager.Add("------- LibInfo --------- : " + _sLibArg);
		//					Debug.fTrace("------- LibInfo --------- : " + _sLibArg);
							fUpdateLibInfo(_oGroup, _aLib);
						break;
						case "Rc":
							fUpdateRcInfo(_oGroup, _aLib);
							TraceManager.Add("-------RCInfo --------- : " + _sLibArg);
						break;
					}
				}
			}
		}



        public void fUpdateLibInfo( CwLibGroup _oGroup, string[] _aLib) {


		
		//	return;
/*
            //string[] _aLib = _sLib.Split(',');
            if (_aLib.Length < 5)
            {
                return;
            }*/

            string _sName      = _oGroup.sName;
            string _sIdName    = _aLib[1];
            string _sReadOnly  = _aLib[2];
            string _sReadPath  = _aLib[3].Replace('\\','/').Replace('"', ' ').Trim(); ;
            string _sWriteName = _aLib[4].Replace('\\','/').Replace('"', ' ').Trim(); 
            string _sWritePath  = _aLib[5].Replace('\\','/').Replace('"', ' ').Trim(); 


           // PathModel _oPath = new PathModel(_sReadPath, this);
            //PathModel _oPath = new PathModel(_sReadPath, this);  
           // classPath.Add(_oPath);
                     // classPath.Add("aaawwwww");
           // FinalizeClasspath();
            
            TraceManager.Add("----- Lib_sName --------- : " + _sName);
            TraceManager.Add("----- Lib__sIdName --------- : " + _sIdName);
            TraceManager.Add("----- Lib_ sReadOnly --------- : " + _sReadOnly);
            TraceManager.Add("----- Lib_ _sReadPath --------- : " + _sReadPath);
            TraceManager.Add("----- Lib_ _sWriteName --------- : " + _sWriteName);
            TraceManager.Add("----- Lib_ _sWritePath --------- : " + _sWritePath);
			TraceManager.Add("-------Lib__sGroup --------- : " + _oGroup.sWritePath);
			TraceManager.Add("-------Lib__sGroupRead --------- : " + _oGroup.sReadPath);

            
           Debug.fTrace("----- Lib_sName --------- : " + _sName);
          Debug.fTrace("----- Lib__sIdName --------- : " + _sIdName);
            Debug.fTrace("----- Lib_ sReadOnly --------- : " + _sReadOnly);
            Debug.fTrace("----- Lib_ _sReadPath --------- : " + _sReadPath);
             Debug.fTrace("----- Lib_ _sWriteName --------- : " + _sWriteName);
             Debug.fTrace("----- Lib_ _sWritePath --------- : " + _sWritePath);
			  Debug.fTrace("-------Lib__sGroup --------- : " + _oGroup.sWritePath);
			  Debug.fTrace("-------Lib__sGroupRead --------- : " + _oGroup.sReadPath);


            
            string _sClassPath =_oGroup.sReadPath + _sReadPath;
             _sClassPath =   _sClassPath.Replace('/','\\');
            if(_sClassPath[_sClassPath.Length-1] == '\\') {
                    _sClassPath =   _sClassPath.Substring(0, _sClassPath.Length-1);
            }
            if(!oCurrProject.oProject.Classpaths.Contains(_sClassPath)) {
			    oCurrProject.oProject.Classpaths.Add(_sClassPath); //Required for searching
            }
         //   TraceManager.Add("--aadd CP: " + _sClassPath);

            

         //   oCurrProject.oProject.UpdateVars(false);


            CwLib _oLib = new CwLib();
			_oGroup.fAddLib(_oLib);

            _oLib.sName = _sName;
            _oLib.sIdName = _sIdName;
            _oLib.bReadOnly = false;
            if(_sReadOnly == "true"){
             _oLib.bReadOnly = true;
            }
            _oLib.sReadPath = _sReadPath.Replace('\\','/');
            _oLib.sWriteName = _sWriteName;
            _oLib.sFullWritePath = _sWritePath;
			_oLib.oGroup = _oGroup;

          //  new CwLibGroup(oCurrProject.aLib, _oLib);
			
           // oCurrProject.aLib.Add(_oLib);

            //Cw
          
        }


        public void fUpdateRcInfo( CwLibGroup _oGroup, string[] _aRc) {
			 string _sName      = _oGroup.sName;
            string _sIdName  = _aRc[1];
            string _sReadPath  = _aRc[2];
            string _sWritePath  = _aRc[3];
            string _sEmbed  = _aRc[4];

			TraceManager.Add("-------RC _sName --------- : " + _sName);
			TraceManager.Add("-------RC _sIdName --------- : " + _sIdName);
			TraceManager.Add("-------RC _sReadPath --------- : " + _sReadPath);
			TraceManager.Add("-------RC_sWritePath --------- : " + _sWritePath);
			TraceManager.Add("-------RC_s_sEmbed --------- : " + _sEmbed);

			TraceManager.Add("-------RC_sGroup --------- : " + _oGroup.sWritePath);
			TraceManager.Add("-------RC_sGroupRead --------- : " + _oGroup.sReadPath);



//TraceManager.Add("-------_oGroup --------- : " + _oGroup.sName;

			 CwLib _oLib = new CwLib();

			_oLib.eType = CwLib.Type.Rc;

			_oLib.sName = _sName;
			 _oLib.sIdName = _sIdName;
			_oLib.sReadPath = _sReadPath.Replace('\\','/');
            _oLib.sFullWritePath = _oGroup.sWritePath + _sWritePath;
            _oLib.sWritePath = _sWritePath;
			_oLib.oGroup = _oGroup;
			_oLib.sEmbedRc = _sEmbed;
			


			aRcList.Add(_oLib);
		
			_oGroup.fAddLib(_oLib);

		}



        public void fClassNotInBuild(string _sArg)
        {
            string[] _aArg = _sArg.Split('|');
            if (_aArg[0] == sCurrFileName)
            { //It's the current class continue ...
                cFile = new FileModel(sCurrFileName);
                cFile.Context = this;
                cFile.HasFiltering = true;
                oCurrFileModel = cFile;
                curClass = new ClassModel();
curClass.ExtendsType = "n/a";
                curClass.InFile = cFile;
                curClass.Name = "(Not in build list)";
                cFile.Classes.Add(curClass);


  //              ASContext.Context = this;
            }
        }

        public void fUpdateClassInfo(string _sArg)
        {
  
			if(cFile == null) {
				return;
			}

      TraceManager.Add("-------Update clas info|| New FileModel---------" + cFile.FileName);
               //      TraceManager.Add("!!!!!!!!!!! UpdateClassInfo --------");

            string[] _aArg = _sArg.Split('|');

			if(_aArg.Length < 1) { // minimal arg
                 TraceManager.Add("-------return--------" );
				return;
			}


// Console.WriteLine("---_aArg[0]" + _sArg);
// Console.WriteLine("---_aArg[0]" + _sArg);

//  Console.WriteLine("-*sCurrFileName" + sCurrFileName);
//  Console.WriteLine("-*sCurrFileName" + NormalizeFilename(_aArg[0]));

            if (NormalizeFilename(_aArg[0]) == sCurrFileName) { //It's the current class continue ...

                cFile = new FileModel(sCurrFileName);
                cFile.Context = this;
                cFile.HasFiltering = true;
                oCurrFileModel = cFile;

               // TraceManager.Add("!Perfect!");
                
              //  fUpdateCurrClassInfo(_aArg[1]);
                /*
                fUpdateExtendInfo(curClass, _aArg[2]);
                fUpdateImportInfo(curClass, _aArg[3]);
                fUpdateMemberInfo(curClass, _aArg[4]);
                fUpdateFunctionInfo(curClass, _aArg[5]);
                */

                curClass = new ClassModel();
                curClass.InFile = cFile;
			curClass.ExtendsType = "n/a";

              //  fAddClassInfo(curClass, _aArg[1], _aArg[2], _aArg[3], _aArg[4], _aArg[5]);
                fAddClassInfo(curClass, _aArg[1]);

                //cLine = CurSciControl.CurrentLine;
               // UpdateContext(cLine);

                cFile.Classes.Add(curClass);

                //For Local scope
                curFunc = new ClassModel();
                curFunc.Flags = FlagType.Function;
                curFunc.InFile = cFile;
                curFunc.Name = "Local - n/a";
			curFunc.ExtendsType = "n/a";
                sCurExp = "n/a";
                cFile.Classes.Add(curFunc);
                sCurExp = " n/a";

                
                ///For Local Expression
                curExp = new ClassModel();
                curExp.Flags = FlagType.Function;
                curExp.InFile = cFile;
                curExp.Name = "Exp - n/a";
	curExp.ExtendsType = "n/a";
              //  curExp.Members = new MemberList();

                cFile.Classes.Add(curExp);

                curFInfo = new ClassModel();
	curFInfo.ExtendsType = "n/a";
                curFInfoClass = new ClassModel();
	curFInfoClass.ExtendsType = "n/a";



           TraceManager.Add("-fUpdateOutline");
fUpdateOutline();



           TraceManager.Add("-----FINISH |---------------------");

            }else  {

                TraceManager.Add("------- Not same FileModel !!!--------- " +  NormalizeFilename(_aArg[0]) );
                TraceManager.Add("------- Not same FileModel !!!--------- " + sCurrFileName);

              
            }
        }

        
public void fUpdateOutline() { //Very Slow
       //     return;
	  //              ASContext.Context = this;

            //new thread?
            TraceManager.Add("---*RefreshView");
       
                 oCurrProject.RefreshView(oCurrFileModel);


}


        public void fAddClassInfo(ClassModel _oContainer, string _sData)
        {
  //  Console.WriteLine("------fAddClassInfo---------" +_sData);
  //  Console.WriteLine("------fAddClassInfo---------" );
            string[] _aData = _sData.Split('#');

            fUpdateCurrClassInfo(_oContainer, _aData[0]);
            fUpdateExtendInfo(_oContainer, _aData[1]);
            if (_oContainer != curExp)
            {
              fUpdateImportInfo( _aData[2]);
            }
            fUpdateMemberInfo(_oContainer, _aData[3]);
            fUpdateFunctionInfo(_oContainer, _aData[4]);
 //Console.WriteLine("-----End--------");
        }


        public void fAddNativeInfo(ClassModel _oContainer, string _sData)
        {
            string[] _aData = _sData.Split('#');

            fUpdateMemberInfo(_oContainer, _aData[0]);
            fUpdateFunctionInfo(_oContainer, _aData[1]);
        }
        


        public void fUpdateCurrClassInfo(ClassModel _oContainer, string _sArg)
        {
             string[] _aClass = _sArg.Split(':');


            // curClass.Comments = curComment;
            // var qtype = QualifiedName(model, token);
            // curClass.Type = qtype.Type;
            //  curClass.Template = qtype.Template;
            ///   curClass.Name = qtype.Name;
            // curClass.Constructor = (haXe) ? "new" : token;
            //  curClass.Flags = curModifiers;
            //  curClass.Access = (curAccess == 0) ? features.classModifierDefault : curAccess;
            // curClass.Namespace = curNamespace;
            //  curClass.LineFrom = (modifiersLine != 0) ? modifiersLine : curToken.Line;
            //   curClass.LineTo = curToken.Line;
            _oContainer.Flags = FlagType.Class;

            if (_oContainer == curExp)
            {
                _oContainer.Name = "Exp - " + _aClass[0] + " (" + sCurExp + ")";
                sCurExp = _aClass[0];
            }else{
                _oContainer.Name = _aClass[0];
            }


        }


        public void fUpdateImportInfo( string _sArg)
        {

            TraceManager.Add("fUpdateImportInfo: " + _sArg);
            string[] _aImportLine = _sArg.Split(';');
			oCurrProject.aImportsIndex = new CwideMemberModel[_aImportLine.Length];
            for (uint i = 0; i < _aImportLine.Length - 1; i++)
            {
                string[] _aImport = _aImportLine[i].Split(',');
                string _sImport = _aImport[0];
		//	TraceManager.Add(" _sImport : " + _sImport );
                //CwideMemberModel _import = new CwideMemberModel(_sImport.Substring(_sImport.LastIndexOf('.') + 1), _sImport, FlagType.Import, Visibility.Public);
                CwideMemberModel _import = new CwideMemberModel(_sImport, "", FlagType.Import, Visibility.Public);
               // _import.LineTo = _import.LineFrom = Int32.Parse(_aImport[1]);

                _import.LineTo = _import.LineFrom = Int32.Parse(_aImport[1])-1;
               // _import.Value = _aImport[2].Replace('/','\\');
                _import.Value = _aImport[2];
                /*
                    TraceManager.Add(" _importName : " + _import.Name );
                    TraceManager.Add(" _importType : " + _import.Type );
                    TraceManager.Add("_import.Value : " +     _import.Value );

                   TraceManager.Add(" _import.LineTo : " + _import.LineTo );
                   */
				if(_aImport.Length > 3) {
					_import.nIndex = UInt32.Parse(_aImport[3]);
					oCurrProject.aImportsIndex[_import.nIndex] = _import;
			   //     TraceManager.Add("_import.nIndex : " +_import.nIndex );
				}
				
                cFile.Imports.Add(_import);
            //    TraceManager.Add("path: " + _import.Value);
            }
        }

        public void fUpdateExtendInfo(ClassModel _oContainer, string _sArg)
        {
           // TraceManager.Add("Extends Arg : " + _sArg);
            string[] _aExtendLine = _sArg.Split(';');
            for (uint i = 0; i < _aExtendLine.Length - 1; i++)
            {
                string[] _aExtend = _aExtendLine[i].Split(',');
                string _sExtend = _aExtend[0];
                
                MemberModel _extend = new CwideMemberModel(_sExtend.Substring(_sExtend.LastIndexOf('.') + 1), _sExtend, FlagType.Extends, Visibility.Public);
                // _import.LineTo = _import.LineFrom = Int32.Parse(_aImport[1]);
                _extend.LineTo = _extend.LineFrom = Int32.Parse(_aExtend[1])-1;
                _oContainer.Members.Add(_extend);
                _extend.Value = _aExtend[2];
                curClass.ExtendsType = "Patate";
              

            }

        }



        public void fUpdateMemberInfo(ClassModel _oContainer, string _sArg)
        {
           // TraceManager.Add(_sArg);
            string[] _aMemberLine = _sArg.Split(';');
            for (uint i = 0; i < _aMemberLine.Length - 1; i++)
            {
                string[]  _aMember = _aMemberLine[i].Split(':');
                string _sMember = _aMember[0];
                string _sType = _aMember[1];
                string _sSharing = _aMember[3];

			

                int _eSharing;
                switch (_sSharing){
                    case "Pb":
                        _eSharing = (int)Visibility.Public;
                    break;
                    case "Pv":
                         _eSharing = (int)Visibility.Private;
                    break;
                    case "Pt":
                        _eSharing = (int)Visibility.Protected;
                    break;

                    default:
                         _eSharing = (int)Visibility.Public;
                    break;
                }


                CwideMemberModel member = new CwideMemberModel(_sMember, _sType, FlagType.Variable | FlagType.Dynamic, (Visibility)_eSharing);

				//link index import
				if(_aMember.Length > 4 ) {
					member.nIndex =  UInt32.Parse(_aMember[4]);
					//Get Import
					
					member.oLinkedMember =  oCurrProject.aImportsIndex[member.nIndex];
			//		member.Value  = member.oLinkedMember.Value;
				}


                member.LineTo = member.LineFrom = Int32.Parse(_aMember[2]) - 1;
                _oContainer.Members.Add(member);
            }
        }


        public void fAddParameter(MemberModel _oFunc, string _sAllParam)
        {
            _oFunc.Parameters = new List<MemberModel>();
            string[] _aParamList = _sAllParam.Split(',');
            foreach (string _sParam in _aParamList)
            {
                string[] _aParam = _sParam.Split(':');
                if (_aParam.Length > 1)
                {
                    MemberModel member = new CwideMemberModel(_aParam[0], _aParam[1], FlagType.Variable | FlagType.Dynamic, Visibility.Private);
                    member.LineTo = member.LineFrom = _oFunc.LineFrom;
             
                    _oFunc.Parameters.Add(member);
                }
              
            }
           // _oFunc.Parameters.Add(_aFunction[3]);
        }


        public void fUpdateFunctionInfo(ClassModel _oContainer, string _sArg)
        {
            //TraceManager.Add(_sArg);
            string[] _aFunctionLine = _sArg.Split(';');
            for (uint i = 0; i < _aFunctionLine.Length - 1; i++)
            {
                string[] _aFunction = _aFunctionLine[i].Split('&');
                string _sFunction= _aFunction[0];
               // TraceManager.Add("!!Func: " + _aFunction[2]);
                CwideMemberModel _oFunc = new CwideMemberModel( _sFunction, _aFunction[2], FlagType.Function, Visibility.Public);
                // _import.LineTo = _import.LineFrom = Int32.Parse(_aImport[1]);
                _oFunc.LineTo = _oFunc.LineFrom = Int32.Parse(_aFunction[1]) - 1;

               fAddParameter(_oFunc, _aFunction[3]);
	


				//link index import
				if(_aFunction.Length > 4 ) {
					_oFunc.nIndex =  UInt32.Parse(_aFunction[4]);
					//Get Import
					
					_oFunc.oLinkedMember =  oCurrProject.aImportsIndex[_oFunc.nIndex];
				//	_oFunc.Value  = _oFunc.oLinkedMember.Value;

				//	Console.WriteLine("addIndex:"  +_aFunction[3] );
			
				}

                _oContainer.Members.Add(_oFunc);


               //   topLevel.Members.Add(new CwideMemberModel(features.voidKey, "", FlagType.Class | FlagType.Intrinsic, Visibility.Public));
                /*
                member = new CwideMemberModel(inClass.Name, inClass.QualifiedName, FlagType.Constructor | FlagType.Function, Visibility.Public);
                GenerateFunction(
                    member,
                    Sci.CurrentPos, false, inClass);*/
            }

        }


        public Boolean fIsCurrentFile(string _sFile)
        {
            if (curFunc == null || curClass == null)
            {
                return false;
            }

            if (cFile.Classes.Count == 0) //has been reinitialised TODO better way?
            {
                cFile.Classes.Add(curClass);
                cFile.Classes.Add(curFunc);
                cFile.Classes.Add(curExp);
                
            }
            if (_sFile.Replace('/','\\').Trim() == sCurrFileName)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public void fLocalScopeOutside(string _sArg)
        {

            string[] _aArg = _sArg.Split('|');
            if (fIsCurrentFile(_aArg[0]))
            {
                curFunc.Members.Clear();
                curFunc.Name = "Local - n/a";
                sCurExp = " n/a";
                curFunc.LineFrom = 0;
                curFunc.LineTo = 0;
                
                curExp.Members.Clear();
                curExp.Name = "Exp - n/a";
curExp.ExtendsType = "n/a";
                curExp.LineFrom = 0;
                curExp.LineTo = 0;
                
 //               ASContext.Context = this;
                fUpdateOutline();
            }
        }



        /// <summary>
        /// ////////////////////////////////////////////////////
        /// ////////////////////////////////////////////////////
        /// </summary>
        /// <param name="_sArg"></param>

        public void fUpdateLocalScopeInfo(string _sArg) {
            TraceManager.Add("------- New Local scope ---------" + cFile.FileName);
            
            
            string[] _aArg = _sArg.Split('|');
               if (fIsCurrentFile(_aArg[0]))
               {
                
               TraceManager.Add("---okay------" +_aArg[0]);
          
                //It's the current class continue ...

               // cFile = new FileModel(sCurrFileName);
                //cFile.Context = this;
               // cFile.HasFiltering = true;

                //TraceManager.Add("!Perfect!");

                fUpdateLScopeFuncInfo(_aArg[1]);
              //  fUpdateLScopeFuncParamInfo(_aArg[2]);
                fUpdateLScopeInfo(curFunc, _aArg[2]);//Blocs Scope
               // TraceManager.Add("-------Scope lvl ---------" + _aArg[3]);
              //  TraceManager.Add("-------Bloc scope lvl ---------" + _aArg[3]);
                /*
                fUpdateExtendInfo(_aArg[2]);
                fUpdateImportInfo(_aArg[3]);
                fUpdateMemberInfo(_aArg[4]);
                fUpdateFunctionInfo(_aArg[5]);
                */



               // cLine = CurSciControl.CurrentLine;
               // UpdateContext(cLine);


 //               ASContext.Context = this;

                fUpdateOutline();

            }
            else
            {
                TraceManager.Add("xxxxxxx Not Same scope !! ----- " + cFile.FileName);
                TraceManager.Add("xxxxxxx Not Same scope !! ----- " + cFile.FileName);
            }
        }

        public void fUpdateLScopeFuncInfo(string _sFunc )
        {
            string[] _aFunc = _sFunc.Split(':');

            curFunc.Members.Clear();
            curExp.Members.Clear();
            curExp.Name = "Exp - n/a";
curExp.ExtendsType= "n/a";
            //cFile.Classes.re

         

            // curClass.Comments = curComment;
            // var qtype = QualifiedName(model, token);
            // curClass.Type = qtype.Type;
            //  curClass.Template = qtype.Template;
            ///   curClass.Name = qtype.Name;
            // curClass.Constructor = (haXe) ? "new" : token;
            //  curClass.Flags = curModifiers;
            //  curClass.Access = (curAccess == 0) ? features.classModifierDefault : curAccess;
            // curClass.Namespace = curNamespace;
            //  curClass.LineFrom = (modifiersLine != 0) ? modifiersLine : curToken.Line;
            //   curClass.LineTo = curToken.Line;

            curFunc.Name = "Local - " + _aFunc[0];
            sCurExp = _aFunc[0];

            curFunc.LineFrom = Int32.Parse(_aFunc[1]) - 1;
            curFunc.LineTo = Int32.Parse(_aFunc[2]) - 1;

            
            
            //  curFunc.Name = "- Local - " + _sFunc;

        }

        public void fUpdateLScopeInfo(ClassModel _oContainer,   string _sArg)
        {
            //TraceManager.Add("Param Arg : " + _sArg);
          //  string[] _aParamFull = _sArg.Split(',');
            string[] _aParamFull = _sArg.Split(';');
            for (uint i = 0; i < _aParamFull.Length - 1; i++)
            {
                string[] _aParam = _aParamFull[i].Split(':');
                string _sParamName = _aParam[0];
                string _sParamType = _aParam[1];

                MemberModel _extend = new CwideMemberModel(_sParamName, _sParamType, FlagType.Variable, Visibility.Public);
                _extend.LineTo = _extend.LineFrom = Int32.Parse(_aParam[2]) - 1;
                _oContainer.Members.Add(_extend);

            }
        }



        public void fUpdateExpScopeInfo(string _sArg)
        {
            TraceManager.Add("------- New Exp scope ---------" + cFile.FileName);
       

            string[] _aArg = _sArg.Split('|');
            if (fIsCurrentFile(_aArg[0]))
            {
                sCurExp = _aArg[1];
                
                switch(_aArg[2]){
                    case "Class":
                        fAddClassInfo(curExp, _aArg[3]);
                        break;

                    case "Native":
                        fAddNativeInfo(curExp, _aArg[3]);
                     break;
                 }

                //fAddClassInfo(curExp, _aArg[3]);
                //  ASContext.Context = this;

                    fUpdateOutline();
                  oPluginMain.fShowExp("", true);
               
            }
            else
            {
                //TraceManager.Add("xxxxxxx Exp not Same scope !! ---------" + cFile.FileName);
            }
        }




        public void fUpdateFuncInfo(string _sArg)
        {

            string[] _aArg = _sArg.Split('|');

            if (_aArg.Length > 2 && fIsCurrentFile(_aArg[0]))
            {
               // TraceManager.Add("------- New Class _sArg ---------" + _aArg[1]);
               // TraceManager.Add("------- New Func _sArg ---------" + _aArg[2]);


                fUpdateCurrClassInfo(curFInfoClass, _aArg[1]);
                //TraceManager.Add("------- New Class Name  ---------" + curFInfoClass.Name);

               fUpdateFunctionInfo(curFInfo, _aArg[2]);
                fShowFuncInfo();
            }


        }

        public void fShowFuncInfo()
        {
            TraceManager.Add("ShowFuncInfo " );

            /*
            MemberModel _oSelFunc = null;
            foreach (MemberModel _oGetFunc in curFInfo.Members)
            {
                _oSelFunc = _oGetFunc;
            }
            if (_oSelFunc != null)
            {
               // TraceManager.Add("------- New Func Info ---------" + cFile.FileName);
              //  ASExpr expr = ASComplete.GetExpression(oCurSci, PluginMain.nFuncTailPos, true);  //Todo PluginMain.nFuncTailPos better way?
                ASExpr expr = ASComplete.GetExpression(oCurSci, PluginMain.nFuncTailPos, true);  //Todo PluginMain.nFuncTailPos better way?
                ASComplete.FunctionContextResolved(oCurSci, expr, _oSelFunc, curFInfoClass, true);
            }

            */
            //ASExpr expr = ASComplete.GetExpression(oCurSci, oCurSci.CurrentPos, true);
            // expr.PositionExpression = PluginMain.nFuncTailPos;
            // expr.Position = PluginMain.nFuncTailPos;
         
        }

        public void fHideFuncInfo()
        {
            UITools.CallTip.Hide();
            /*
            TraceManager.Add("-------*** HIDE *********  ---------" );
            MemberModel _oSelFunc = null;
            foreach (MemberModel _oGetFunc in curFInfo.Members)
            {
                _oSelFunc = _oGetFunc;
            }
            if (_oSelFunc != null)
            {
                TraceManager.Add("------- New Func Name  ---------" + _oSelFunc.Name);
            }
            ASExpr expr = ASComplete.GetExpression(oCurSci, PluginMain.nFuncTailPos, true)
    
            ASComplete.FunctionContextResolved(oCurSci, expr, null, null, false);
        */
        }



        /// <summary>
        /// Refresh the file model
        /// </summary>
        /// <param name="updateUI">Update outline view</param>
        public override void UpdateCurrentFile(bool updateUI)
        {
    


        }



        /// <summary>
        /// Called if a FileModel needs filtering
        /// - define inline AS3 ranges
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public override string FilterSource(string fileName, string src)
        {
            //cwRanges = new List<InlineRange>();
           // return PhpFilter.FilterSource(src, cwRanges);
            TraceManager.Add("CW FilterSource str");
           // return "FilterSourceCw??";
            return "";
        }

        /// <summary>
        /// Called if a FileModel needs filtering
        /// - modify parsed model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public override void FilterSource(FileModel model)
        {
           // PhpFilter.FilterSource(model, cwRanges);
            TraceManager.Add("CW FilterSource");
        }
        #endregion

        #region class resolution
        /// <summary>
        /// Evaluates the visibility of one given type from another.
        /// Caller is responsible of calling ResolveExtends() on 'inClass'
        /// </summary>
        /// <param name="inClass">Completion context</param>
        /// <param name="withClass">Completion target</param>
        /// <returns>Completion visibility</returns>
        public override Visibility TypesAffinity(ClassModel inClass, ClassModel withClass)
        {
            // same file
            if (withClass != null && inClass.InFile == withClass.InFile)
                return Visibility.Public | Visibility.Protected | Visibility.Private;
            // inheritance affinity
            ClassModel tmp = inClass;
      tmp.ExtendsType = "n/a";
            while (!tmp.IsVoid())
            {
                if (tmp == withClass)
                    return Visibility.Public | Visibility.Protected;
                tmp = tmp.Extends;
            }
            // same package
            if (withClass != null && inClass.InFile.Package == withClass.InFile.Package)
                return Visibility.Public;
            // public only
            else
                return Visibility.Public;
        }


        /// <summary>
        /// Return the full project classes list
        /// </summary>
        /// <returns></returns>
        public override MemberList GetAllProjectClasses()
        {
            return base.GetAllProjectClasses();
        }

        /// <summary>
        /// Retrieves a class model from its name
        /// </summary>
        /// <param name="cname">Class (short or full) name</param>
        /// <param name="inClass">Current file</param>
        /// <returns>A parsed class or an empty ClassModel if the class is not found</returns>
        public override ClassModel ResolveType(string cname, FileModel inFile)
        {
            return base.ResolveType(cname, inFile);
        }

        /// <summary>
        /// Prepare intrinsic known vars/methods/classes
        /// </summary>
        protected override void InitTopLevelElements()
        {
            TraceManager.Add("CW InitTopLevelElements");
            string filename = "toplevel" + settings.DefaultExtension;
            topLevel = new FileModel(filename);

            // search top-level declaration
            foreach (PathModel aPath in classPath)
                if (File.Exists(Path.Combine(aPath.Path, filename)))
                {
                    filename = Path.Combine(aPath.Path, filename);
                    topLevel = GetCachedFileModel(filename);
                    break;
                }

            if (File.Exists(filename))
            {
                // copy declarations as file-level (ie. flatten class)
                /*ClassModel tlClass = topLevel.GetPublicClass();
                if (!tlClass.IsVoid() && tlClass.Members.Count > 0)
                {
                    topLevel.Members = tlClass.Members;
                    tlClass.Members = null;
                    topLevel.Classes = new List<ClassModel>();
                }*/
            }
            // not found
            else
            {
                //ErrorHandler.ShowInfo("Top-level elements class not found. Please check your Program Settings.");
            }

            // special variables
            topLevel.Members.Add(new CwideMemberModel("oImg", "", FlagType.Variable, Visibility.Public));
            //  topLevel.Members.Add(new CwideMemberModel("$this", "", FlagType.Variable, Visibility.Public));
            
            topLevel.Members.Add(new CwideMemberModel("self", "", FlagType.Variable, Visibility.Public));
            topLevel.Members.Add(new CwideMemberModel("parent", "", FlagType.Variable, Visibility.Public));
            topLevel.Members.Sort();
            foreach (MemberModel member in topLevel.Members)
                member.Flags |= FlagType.Intrinsic;
        }

        public override void CheckModel(bool onFileOpen)
        {
            if (!File.Exists(cFile.FileName))
            {
                // refresh model
                base.CheckModel(onFileOpen);
            }
        }

        /// <summary>
        /// Update intrinsic known vars
        /// </summary>
        protected override void UpdateTopLevelElements()
        {
            MemberModel special;
            //special = topLevel.Members.Search("$this", 0, 0);
            special = topLevel.Members.Search("oImg", 0, 0);
            if (special != null)
            {
                if (!cClass.IsVoid()) special.Type = cClass.Name;
                else special.Type = (cFile.Version > 1) ? features.voidKey : docType;
            }
            special = topLevel.Members.Search("self", 0, 0);
            if (special != null)
            {
                if (!cClass.IsVoid()) special.Type = cClass.Name;
                else special.Type = (cFile.Version > 1) ? features.voidKey : docType;
            }
            special = topLevel.Members.Search("parent", 0, 0);
            if (special != null)
            {
                cClass.ResolveExtends();
                ClassModel extends = cClass.Extends;
                if (!extends.IsVoid()) special.Type = extends.Name;
                else special.Type = (cFile.Version > 1) ? features.voidKey : features.objectKey;
            }
        }

        /// <summary>
        /// Retrieves a package content
        /// </summary>
        /// <param name="name">Package path</param>
        /// <param name="lazyMode">Force file system exploration</param>
        /// <returns>Package folders and types</returns>
        public override FileModel ResolvePackage(string name, bool lazyMode)
        {
            return base.ResolvePackage(name, lazyMode);
        }
        #endregion

        #region command line compiler

        //static public string TemporaryOutputFile;

        /// <summary>
        /// Retrieve the context's default compiler path
        /// </summary>
        public override string GetCompilerPath()
        {
            // to be implemented
            TraceManager.Add("CW GetCompilerPath");
            return null;
        }

        /// <summary>
        /// Check current file's syntax
        /// </summary>
        public override void CheckSyntax()
        {
            // to be implemented
            TraceManager.Add("CW Check syntax");
        }
        /*
        override public bool CanBuild
        {
            get { return false; }
        }*/

        /// <summary>
        /// Run compiler in the current files's base folder with current classpath
        /// </summary>
        /// <param name="append">Additional comiler switches</param>
        public override void RunCMD(string append)
        {
            TraceManager.Add("CW RunCMD");
            // to be implemented
        }

        /// <summary>
        /// Calls RunCMD with additional parameters taken from the file's doc tag
        /// </summary>
        public override bool BuildCMD(bool failSilently)
        {
            TraceManager.Add("CW BuildCMD");
            // to be implemented
            return true;
        }
        #endregion

        /*
        public override void OnMouseHover(ScintillaNet.ScintillaControl sci, int position)
        {

            TraceManager.Add("CW OnMouseHover");
        }*/



        /// <summary>
        /// Update the class/member context for the given line number.
        /// Be carefull to restore the context after calling it with a custom line number
        /// </summary>
        /// <param name="line"></param>
        public override void UpdateContext(int line)
        {
//CbTargetBuild.Items.Add("aaaaaaaaaaaaaaaa");

	//	CbTargetBuild.DropDownStyle = ComboBoxStyle.DropDownList;


            try
            {
                //TraceManager.Add("MyUpdateContext/UpdateOutline");
                if (cFile == FileModel.Ignore)
                {
                    SetTemporaryPath(null);
                    return;
                }
                else
                {
                    if (SetTemporaryPath(NormalizePath(cFile.GetBasePath())))
                    {
                        PathModel tPath = classPath[0];
                        tPath.AddFile(cFile);
                    }
                }

                if (cFile.OutOfDate) UpdateCurrentFile(true);


                /*
                ASResult ctx = GetDeclarationAtLine(line);
                if (ctx.InClass != cClass)
                {
                    cClass = ctx.InClass;
                    // update "this" and "super" special vars
                    UpdateTopLevelElements();
                }
                cMember = ctx.Member;

                // in package or after
                bool wasInPrivate = inPrivateSection;
                inPrivateSection = cFile.PrivateSectionIndex > 0 && line >= cFile.PrivateSectionIndex;
                if (wasInPrivate != inPrivateSection)
                {
                    completionCache.IsDirty = true;
                    completionCache.Classname = null;
                }

                // rebuild completion cache
                if (completionCache.IsDirty && IsFileValid &&
                    (completionCache.Package != cFile.Package
                    || completionCache.Classname != cFile.GetPublicClass().Name))
                    RefreshContextCache(null);
                */

            }
            catch { }
            }








        /*
        public void fVerifyCppBuildList(string _sAll)
        {
            //TraceManager.Add("------- New Build list---------" + cFile.FileName);

            string[] _aArg = _sAll.Split('|');

            if (_aArg.Length >= 2)
            {
                string _sOutputDir = _aArg[0];
                string _sArg = _aArg[1];

                string[] _aFiles = _sArg.Split('?');

                TraceManager.Add("-------  Output  ---------" + _aFiles.Length);
                TraceManager.Add("-------  _sArg  ---------" + _sArg);

                foreach (string _sPath in _aFiles) {
                    if (_sPath != "")
                    {
                  //      TraceManager.Add("-------  Verify Build Cpp ---------" + _sPath);
                         CompileCpp.fVerifyCompileFile(_sOutputDir, _sPath);
                    }
                }
                TraceManager.Add("-------  End  ---------" );
            }
        }
        */


        /*
        public void fRequireList(string _sAll)
        {
            //TraceManager.Add("------- New Build list---------" + cFile.FileName);

            string[] _aArg = _sAll.Split('|');

            if (_aArg.Length >= 2)
            {
                string _sOutputDir = _aArg[0];
                string _sArg = _aArg[1];

                string[] _aFiles = _sArg.Split('?');

                TraceManager.Add("-------  Require Output  ---------" + _aFiles.Length);
                TraceManager.Add("------- Require_sArg  ---------" + _sArg);

                foreach (string _sVal in _aFiles)
                {
                    if (_sVal != "")
                    {
                        TraceManager.Add("-------  Require Cpp ---------" + _sVal);
                       // CompileCpp.fCompileFile(_sOutputDir, _sPath);
                    }
                }
                TraceManager.Add("-------  End  ---------");
            }
        }*/


    }
}
