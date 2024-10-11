using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using WeifenLuo.WinFormsUI.Docking;
using CwideContext.Resources;
using PluginCore.Localization;
using PluginCore.Utilities;
using PluginCore.Managers;
using PluginCore.Helpers;
using PluginCore;
using System.Diagnostics;
using PluginCore.Controls;
using System.Collections.Generic;
using ASCompletion.Model;
using ASCompletion.Completion;
using ASCompletion.Context;
using ScintillaNet;
using System.Threading;
using ProjectManager.Controls.TreeView;
using ProjectManager;
using ProjectManager.Projects;
using FlashDevelop;
using CwideContext.Projects;
using System.Collections;
using FlashDevelop.Dialogs;
using ConsolePanel.Gui;
using Microsoft.Win32;
using ConsoleControl;

public  static class Debug {
  public  static void fTrace(string _sMsg) {
#if DEBUG
		Console.WriteLine(_sMsg);
		//TraceManager.Add(_sMsg);
#endif
	}
}

namespace CwideContext{

 

    public class PluginMain : IPlugin  {

        public static   TabConsole oMainConsole = null;
         private Image image;
        private DockContent cmdPanelDockContent;
        private TabbedConsole tabView;



         private void Initialize_Console() {
           oMainConsole =   CreateConsolePanel(PluginMain.CwSetting.GetCwcPath());
        //   oMainConsole =   CreateConsolePanel(@"E:\_Project\TestGUI\CwcGUI\CwcGUI\bin\Debug\CwcGUI.exe");
            
            CreateMenuItem_Console();

        }

        private void CreateMenuItem_Console()
        {
             string label = "Cwide Plugin";
            ToolStripMenuItem viewMenu = (ToolStripMenuItem)PluginBase.MainForm.FindMenuItem("ViewMenu");
            ToolStripMenuItem cmdItem = new ToolStripMenuItem(label, image, OpenCmdPanel);

            viewMenu.DropDownItems.Add(cmdItem);
        }
           private void OpenCmdPanel(object sender, EventArgs e)
        {
            CreateConsolePanel(PluginMain.CwSetting.GetCwcPath());
            //cmdPanelDockContent.Show();
        }
        private void InitBasics_Console(){
              image = PluginBase.MainForm.FindImage("57");
        }

        private void CreatePluginPanel_Console(){
            tabView = new TabbedConsole(this);
            cmdPanelDockContent = PluginBase.MainForm.CreateDockablePanel(tabView, pluginGuid, image, DockState.DockBottom);
            cmdPanelDockContent.Text = "CWide";
        }


        public TabConsole fIsConsoleExist(string  _sTitle){
            foreach(var item in tabView.tabConsoleMap) {
                TabPage _oPage =  item.Key;
                TabConsole _oConsole = item.Value;
                if(_oConsole.Text == _sTitle) {
                  TabControl _oTabCtrl=  (TabControl)_oPage.Parent;
                    _oTabCtrl.SelectedTab = _oPage;
                   // TraceManager.Add("Already Exist " +_sTitle);
                    //Send build cmd
                    //_oConsole.fSend("C:Build");
                    return _oConsole;
                }
            }
            return null;

        }





        
        internal void fStopBuild() {
            
          TabConsole _oSelConsole = fIsConsoleExist(sRunningConsoleTitle);
          if(_oSelConsole != null){
                _oSelConsole.fSend("C:StopBuild");
           }
        }
        public string sRunningConsoleTitle = "";

        public ConsoleControl.TabConsole CreateConsolePanel(string _sPath, string _sArg = "",string  _sName = "")
        {

             string _sTitle = "";
          if(_sName == "") {
                 string _sFileName = Path.GetFileName(_sPath);
                 _sTitle = _sFileName;
            }else{
                 _sTitle = _sName;
            }
          sRunningConsoleTitle = _sTitle;

          TabConsole _oSelConsole = fIsConsoleExist(_sTitle);
          if(_oSelConsole != null){
                _oSelConsole.fSend("C:StartBuild");
                return _oSelConsole;
           }

          /*
            foreach(var item in tabView.tabConsoleMap) {
                TabPage _oPage =  item.Key;
                TabConsole _oConsole = item.Value;
                if(_oConsole.Text == _sTitle) {
                  TabControl _oTabCtrl=  (TabControl)_oPage.Parent;
                    _oTabCtrl.SelectedTab = _oPage;
                    TraceManager.Add("Already Exist " +_sTitle);
                    //Send build cmd
                    _oConsole.fSend("C:Build");
                    return _oConsole;
                }
            }
            */




                   TraceManager.Add("CreateConsolePanel " + _sPath + " "  + _sArg);

            cmdPanelDockContent.Show();

           //var cmdPanel = new ConsoleControl.ConsoleControl("cmd", false);
            var cmdPanel = new TabConsole(_sPath, _sArg,false);
     
          //  cmdPanel.Text = "Console";
         
           cmdPanel.Text =_sTitle;

       //     cmdPanel.ConsoleBackColor = settingObject.BackgroundColor;
      //      cmdPanel.ConsoleForeColor = settingObject.ForegroundColor;

            cmdPanel.Exited += delegate  {
                if (tabView.InvokeRequired) {
                    tabView.Invoke((MethodInvoker)delegate
                    {
                        if (!PluginBase.MainForm.ClosingEntirely)
                            tabView.RemoveConsole(cmdPanel);
                    });
                }else {
                    if (!PluginBase.MainForm.ClosingEntirely)
                        tabView.RemoveConsole(cmdPanel);
                }
            };

            cmdPanel.Create();
            tabView.AddConsole(cmdPanel);
            return cmdPanel;
        }


		

	  public static void SetStatusBar(string text) { 
		PluginBase.MainForm.StatusLabel.Text = " " + text; 
		}

		public static string CwcRootPath() {
				string _sCwcRootPath =  PluginBase.CurrentProject.CurrentSDK;
				if(_sCwcRootPath  == null) { //Set default path
					_sCwcRootPath = PathHelper.ToolDir + "/Cwc/";
				}
				return _sCwcRootPath;
		}


        public  static void SafeInvoke( Action updater, bool forceSynchronous = true){
			if (pluginUI == null){
				throw new ArgumentNullException("uiElement");
			}
			if (pluginUI.InvokeRequired){
				if (forceSynchronous){
					pluginUI.Invoke((Action)delegate { SafeInvoke( updater, forceSynchronous); });
				}else{
					pluginUI.BeginInvoke((Action)delegate { SafeInvoke( updater, forceSynchronous); });
				}
			}else{    
				if (pluginUI.IsDisposed){
					throw new ObjectDisposedException("Control is already disposed.");
				}
				updater();
			}
		}
       public static PluginUI pluginUI;
       public static Context oContext = null;
	   public static  ProjectManager.PluginMain  oProjectManager = null;
	   public static  ASCompletion.PluginMain  oASCompletion = null;

       public static uint nHandle = 0;
       public static Boolean bHandle = false;

        private String pluginName = "CwideContext";
        private String pluginGuid = "84ac7fab-421b-1f38-a985-72a03534f731";
        private String pluginHelp = "www.cwc-repo.com";



        private String pluginDesc = "Cwide C++ / Cw developpemnt";
        private String pluginAuth = "Maeiky";
        private String associatedSyntax = "cwc"; // ie. coloring syntax file name
        private String associatedSyntaxCpp = "Cpp"; // ie. coloring syntax file name Importat -> get classinfo

        private String settingFilename;
        public static ContextSettings CwSetting;
       // private Settings settingObject;
        private DockContent pluginPanel;

 

        private Image pluginImage;
 




        static public Boolean bIsLinx = false;
        static public Boolean bIsCpp = false;
        static public Boolean bIsCwc = false;

        public int nPosStartCmpl = 0;
        public Boolean bOnCompletion = false;
        public int nCurTailPos = 0;


        ////Maeiky
        public ProcessRunner oCwRunner;
        public static bool bClosing = false;

        public int nLastCharPosition = 0;
        
        public static LauchTool oCwc = null;
        public static bool bCwcStarted = false;






	static bool _bOne = true;


        #region Required Properties

        /// <summary>
        /// Api level of the plugin
        /// </summary>
        public Int32 Api
        {
            get { return 1; }
        }

        /// <summary>
        /// Name of the plugin
        /// </summary> 
        public String Name
        {
            get { return this.pluginName; }
        }

        /// <summary>
        /// GUID of the plugin
        /// </summary>
        public String Guid
        {
            get { return this.pluginGuid; }
        }

        /// <summary>
        /// Author of the plugin
        /// </summary> 
        public String Author
        {
            get { return this.pluginAuth; }
        }

        /// <summary>
        /// Description of the plugin
        /// </summary> 
        public String Description
        {
            get { return this.pluginDesc; }
        }

        /// <summary>
        /// Web address for help
        /// </summary> 
        public String Help
        {
            get { return this.pluginHelp; }
        }

        /// <summary>
        /// Object that contains the settings
        /// </summary>
        [Browsable(false)]
        public Object Settings
        {
            get { return CwSetting; }
        }
        
        #endregion
        
        #region Required Methods
        
        /// <summary>
        /// Initializes the plugin
        /// </summary>
        public void Initialize()
        {
            try
            {
                this.InitBasics();
                this.LoadSettings();
                this.AddEventHandlers();
                this.InitLocalization();
                this.CreatePluginPanel();
                this.CreateMenuItem();
                Initialize_Console();
            }
            catch { }
        }

   

        /// <summary>
        /// Disposes the plugin
        /// </summary>
        public void Dispose()
        {
            this.SaveSettings();
        }


		public void fFileSwitch() {
			 //  if (PluginBase.MainForm.CurrentDocument.SciControl == null)      {      Debug.fTrace("ret: " + e.Type); return;   }

       

                            TraceManager.Add("..ConfigurationLanguage");
                            PluginBase.MainForm.CurrentDocument.SciControl.Cancel();
                            String lang = PluginBase.MainForm.CurrentDocument.SciControl.ConfigurationLanguage;
                            TraceManager.Add("..is : " + PluginBase.MainForm.CurrentDocument.SciControl.ConfigurationLanguage);
							 if (lang == "cpp"){
								bIsCpp = true;
							}
                             if (lang == "cwc"){
								bIsCwc = true;
							}
							
                            if (lang == "linx")
                            {
                                //Set a bool
                                bIsLinx = true;
                                if (oContext != null)
                                {
                                   // oContext.oCurrProject.oProject = ProjectManager.PluginMain.activeProject;
                                    oContext.oCurrProject.oProject = (Project)PluginBase.CurrentProject;

								
                                }
                            }
                            else
                            {
                                bIsLinx = false;
                            }

                            string fileName = PluginBase.MainForm.CurrentDocument.FileName;
                          //  pluginUI.Output.Text += fileName + "\r\n";
                            Console.WriteLine("Switched to " + fileName); // tracing to output panel
		}
		
        
        /// <summary>
        /// Handles the incoming events
        /// </summary>
        public void HandleEvent(Object sender, NotifyEvent e, HandlingPriority prority)
        {
				 if(bClosing){
					return;
				}

			// for some reason we have to do this on the next message loop for the tree
			// state to be restored properly.
			try{    

			SafeInvoke(delegate {  //Not syre
                
       //   TraceManager.Add("Event Command! " ); 
               
		//Debug.fTrace("*Event: " + e.Type);






            if (e.Type == EventType.UIStarted) {
			//	Debug.fTrace("----- NEW Devo CONTEXT --------");
             //   TraceManager.Add("Event UIStart");
				try {
					oProjectManager = (ProjectManager.PluginMain) PluginBase.MainForm.FindPlugin("30018864-fadd-1122-b2a5-779832cbbf23");
				}catch(Exception _e) {
					oProjectManager = null;
				}

				try {
					oASCompletion = (ASCompletion.PluginMain) PluginBase.MainForm.FindPlugin("078c7c1a-c667-4f54-9e47-d45c0e835c4e");
				}catch(Exception _e) {
					oASCompletion = null;
				}

TraceManager.Add("---" + PathHelper.AppDir );
string _sExeName = System.AppDomain.CurrentDomain.FriendlyName;

int _nIndex = _sExeName.IndexOf("vshost");
if(_nIndex > 0) {
	_sExeName = _sExeName.Substring(0,_nIndex) + "exe"; 
}
string _sExePath = PathHelper.AppDir + "\\";
TraceManager.Add("+++-" +_sExeName);

//ystem.AppDomain.CurrentDomain.FriendlyName
MsgProcess.SetAssociation_User("cwproj",_sExePath + _sExeName, _sExeName);


                oContext = new Context(CwSetting);
                oContext.oPluginMain = this;

				ProjectManager.Helpers.ProjectCreator.AppendProjectType("project.cwproj", typeof(CwProject)) ;


                // Associate this context with a file type
                ASCompletion.Context.ASContext.RegisterLanguage(oContext, associatedSyntax);
                ASCompletion.Context.ASContext.RegisterLanguage(oContext, associatedSyntaxCpp);

			
				fFileSwitch();

            }
            else if (oContext != null && !bClosing)
            {
                try
                {

                    switch (e.Type)
                    {
                           case  EventType.FileTemplate:
                                Context.fDeleteArgument("wPackageGuard");
                                Context.fDeleteArgument("wFolderPath");
                                
                                TraceManager.Add("EventType.FileTemplate"); 
                        	break;

                                case EventType.ProcessArgs:
                                     //  string fileName = Path.GetFileNameWithoutExtension(lastFileFromTemplate);
                                     TraceManager.Add("Event ProcessArgs"); 

                            	break;

                        case EventType.Completion: //Disable BasicCompletion
                         //   TraceManager.Add("Event Completion"); 
                            // if (XMLComplete.Active)

                            if (bIsLinx || bIsCpp || PluginMain.bIsCwc)
                            {

                                e.Handled = true;
                            }
                          // Debug.fTrace("ret: " + e.Type); return;
						break;
                            
                        case EventType.UIStarted:
                   
							/*
                            TraceManager.Add("Event UIStart");

						
                            oContext = new Context(CwSetting);
                            oContext.oPluginMain = this;

                            // Associate this context with a file type
                            ASCompletion.Context.ASContext.RegisterLanguage(oContext, associatedSyntax);
                            ASCompletion.Context.ASContext.RegisterLanguage(oContext, associatedSyntaxCpp);

						*/

                            break;

                        // Catches FileSwitch event and displays the filename it in the PluginUI.

                        case EventType.SyntaxChange:
                        case EventType.FileSwitch:
                            TraceManager.Add("Event FileSwitch/SyntaxChange"); 

							fFileSwitch();
                         
                            break;


                        case EventType.FileSave:
                            TraceManager.Add("Event FileSave"); 
                            ITabbedDocument doc = PluginBase.MainForm.CurrentDocument;
                            if (!doc.IsEditable) return;
                               Msg.fSendToCppAst("*Reload|" +  doc.FileName  + "|");
                            //  Context.fCwcSend("Reload|" +  doc.FileName  + "|");


                            if (bHandle) //???
                            {
                                //MsgProcess.fSendMsg("FileSave|" + doc.FileName.Substring(0, doc.FileName.Length - 2));
                                //  MsgProcess.fSendMsg("FileSave|" + doc.FileName);

                             //   MsgProcess.fSendMsg("FileSave|" + doc.FileName + "|");

                              

                                 PluginBase.MainForm.CallCommand("PluginCommand", "ResultsPanel.ClearOutputFile," + doc.FileName);

                                //MsgProcess.sendWindowsStringMessage(nHandle, 0, "hello");

                            }

                            // TraceManager.Add("FileSave to "+ doc.FileName ); // tracing to output panel
                            //if (ASContext.Context.Settings.CheckSyntaxOnSave) CheckSyntax(null, null);
                            // ASContext.Context.RemoveClassCompilerCache();

                            break;

                        // Catches Project change event and display the active project path
                        case EventType.Command:
						   	DataEvent de = (DataEvent)e;
                            string cmd = de.Action;
                        //    TraceManager.Add("Event Command: " + cmd); 
                            TraceManager.Add("Event Command: " + cmd); 
							
							fNewCommand(cmd, de);
						 break;

                        case EventType.Keys:
                            //TraceManager.Add("Event Keys");
                              e.Handled = HandleKeys(((KeyEvent)e).Value,(KeyEvent)e);
                           //       e.ProcessKey = false;
                            break;


                        case EventType.UIRefresh:

                           // if (sender is CwideContext.MainForm)
                            if (sender is FlashDevelop.MainForm)
                            {
                                if (oContext != null && oContext.oCurrProject != null)
                                {

                                    fAnalyseScope(oContext.sCurrFileName);
									
									if(_bOne) { //Ugliest patch to show loading
										_bOne = false;
										ProjectTreeView.Instance.RebuildTree();
									}

                                }
                            }

                            break;

                        case EventType.UIClosing:
                            bClosing = true;
							
                            TraceManager.Add("Event Closing"); 
                              Context.fCwcSend("Quit|");
							/*
							if(oCwc != null) {
								oCwc.fSend("wQuit");
							}*/
                            break;

                        default:
                            TraceManager.Add("Event default"); 
                            break;

                    }

                }
                catch (Exception ex)
                {

                    TraceManager.Add("Error, events : " + ex.Message + " : " + ex.Data + " : " + ex.InnerException + " : " + ex.HelpLink);
                }

            }

			
			 });}catch {};

	//	Debug.fTrace("ret: " + e.Type);
        }

        private bool HandleKeys(Keys key,KeyEvent e)
        {
          
           // TraceManager.Add("Key event!!! " + key);
              if (e.Value == PluginBase.MainForm.GetShortcutItemKeys("SearchMenu.GotoDeclaration")) {
                  if(oContext.TryGotoDeclaration()) {
                    
                    e.Handled = true;
                    e.ProcessKey = false;
                }
                    
                }
              if ( key == Keys.Left) {
                   ITabbedDocument doc = PluginBase.MainForm.CurrentDocument;
                  ScintillaControl sci = doc.SciControl;
                  sci.CharLeft();
            }
             if ( key == Keys.Right) {
                    ITabbedDocument doc = PluginBase.MainForm.CurrentDocument;
                  ScintillaControl sci = doc.SciControl;
                  sci.CharRight();
            }
              if ( key == Keys.Up) {
                Completion.KeyUp();
            }
             if ( key == Keys.Down) {
                Completion.KeyDown();
            }
      
            // hide if pressing Esc or Ctrl+Key combination
           // if (lockedSciControl == null || !lockedSciControl.IsAlive || key == Keys.Escape
            if ( key == Keys.Return || key == Keys.Escape || ((Control.ModifierKeys & Keys.Control) != 0 && Control.ModifierKeys != (Keys.Control|Keys.Alt)) ) {
                if (key == (Keys.Control | Keys.C) || key == (Keys.Control | Keys.A)) {
                    return false; // let text copy in tip
                }

                if ( key == Keys.Return) {
                   Completion.ValidateSelection();
                    e.Handled = true;
                     e.ProcessKey = false;
                     return true;
                }

                Completion.Hide((char)27);
                return false;
            }
              return false;
        }

        public static string fGetPrjDir()  {
            return Path.GetDirectoryName(PluginMain.oContext.oCurrProject.oProject.ProjectPath ).Replace('\\', '/').Trim() + "/";	
        }
        public static string fGetCmdLauch()  {
            string _sCmd = PluginBase.MainForm.ProcessArgString( oContext.oCurrProject.oProject.TestMovieCommand).Trim().Replace('\\', '/');
            return  Program.ResolveVars(_sCmd); 
        }
        public static string fGetFile(string _sCmd)  {

            int _nFindDot = _sCmd.IndexOf('.');
            if(_nFindDot < 0) {
                _nFindDot = 0;
            }

            int _nFindSpace = _sCmd.IndexOf(' ',_nFindDot);
			if(_nFindSpace <= 0) {
				_nFindSpace =  _sCmd.Length;
			}
		    return _sCmd.Substring(0,_nFindSpace);
        }

		private void fNewCommand(string cmd, DataEvent de)
		{
					switch (cmd) {



           


							case "ProjectManager.RunWithAssociatedIDE":
							{
								///TraceManager.Add("----RunWithAssociatedIDE!!!!");
								///data["command"] = command;
								///data["project"] = project;
								///data["runOutput"] = runOutput;
								///data["releaseMode"] = releaseMode;
									
										
								Hashtable hashData = (Hashtable)de.Data;
								if( (string)hashData["command"] =="Cwc") {
									
					 //     string _sLauchCmd = PluginBase.MainForm.ProcessArgString( oContext.oCurrProject.oProject.TestMovieCommand).Trim();
                //    _sLauchCmd = oContext.oCurrProject.oProject.FixDebugReleasePath(_sLauchCmd);


								//	TraceManager.Add("----Run CWC: " +  CwcRootPath()+  " "+  _sLauchCmd);

								//	  oProjectManager.p.pluginUI.Menu.TestAllProjects.Click += delegate { TestBuild(); };
								
			
									oProjectManager.UpdateUIStatus(ProjectManagerUIStatus.Building);
									oProjectManager.UpdateUIStatus((ProjectManagerUIStatus)999); //Put invalid data to Remove Popup (Sure stop build & killprocesss)

								//	oProjectManager.UpdateUIStatus(ProjectManagerUIStatus.NotBuilding);	
									
								//	oC project.TestMovieCommand 
									string _sProjectDir = fGetPrjDir();
                                    string _sLauchCmd = fGetCmdLauch();
									
									fStartCwc(oContext.oCurrProject.oProject,  _sProjectDir ,	_sLauchCmd);

									SetStatusBar("Build Started: " + _sProjectDir +  	_sLauchCmd);
											
									de.Handled = true; //-> Build success start
										
								}
							}break;


									case "ProjectManager.BuildingProject":
                            {
                                TraceManager.Add("----BuildingProject");
                       //         MsgProcess.fSendMsg("Compile|" + oContext.oCurrProject.TargetBuild + "|");
                               // CompileCpp.gblCompileCwTime.Reset();
                              //  CompileCpp.gblCompileCwTime.Start();
                            }break;

									case "ProjectManager.TestingProject":
                            {
                                TraceManager.Add("----TestingProject: " + oContext.oCurrProject.oProject.TargetBuild );
								
                                PluginBase.MainForm.CallCommand("PluginCommand", "ResultsPanel.ClearResults");

                                // MsgProcess.fSendMsg("Compile|" + oContext.oCurrProject.TargetBuild + "|"  + "NoBuild" + "|");
                            //    MsgProcess.fSendMsg("Compile|" + oContext.oCurrProject.TargetBuild + "|");
                              //  CompileCpp.gblCompileCwTime.Reset();
                               // CompileCpp.gblCompileCwTime.Start();

                            }break;


									case "ProjectManager.BuildComplete": {
											TraceManager.Add("----BuildComplete");
							}break;
							case "ProjectManager.CleanProject": {
											TraceManager.Add("----CleanProject");
							}break;

							case "ProjectManager.BuildFailed":
                            {
                                TraceManager.Add("---BuildFailed");
                            }break;
							case "ProjectManager.BuildStop":
                            {
                                TraceManager.Add("---BuildStop");
                         //       CompileCpp.fStopAll();
                            }break;



							case "ProjectManager.BeforeSave": {
											TraceManager.Add("----BeforeSave");
							}break;
							case "ProjectManager.OpenVirtualFile": {
											TraceManager.Add("----BeforeSave");
							}break;


							case "ProjectManager.CreateNewFile" :
                            {
                                  // fSetArgument("PackageGuard","TestBB");                                
                                if(oContext != null) { 
								    Hashtable hashData = (Hashtable)de.Data;
                  
                                    string _sSourcePath =  (string)hashData["templatePath"] ;
                                    string _sToDir =  (string)hashData["inDirectory"] ;
                                    
                                       /*
                                    if(_sTemplatePath != null && _sTemplatePath != "") {
						    	          oContext.sCurrFileName =_sTemplatePath;
                                     } */

                                    //Create template ajuste custom var
                                   string _sFile = _sToDir  + "\\";
                                   string _sRelativePathResult = "";
                                   string _sClassPath  = oContext.fFindClassPathFile(_sFile);
                                   if(_sClassPath != "") {
                                       _sRelativePathResult = _sFile.Substring(_sClassPath.Length );
                                   }
                                     string sFileType = "";
                                    if(_sSourcePath.Length > 3) {
                                         _sSourcePath =  _sSourcePath.Replace(".fdt", ""); 
                                           sFileType = Path.GetExtension(_sSourcePath);
                                            if(sFileType.Length>1) {sFileType = sFileType.Substring(1).ToUpper();}//Remove dot
                                    }

                                    Program.AddVar("wFileType",  sFileType);
                                    Program.AddVar("wPackageUnderscore",   _sRelativePathResult.Replace('\\', '_') );
                                       
                                    Context.fSetDefaultArgument();
                                  
                                    Context.fSetArgument("wPackageGuard",   Program.ResolveVars(Context.fGetArgument("wPackageGuard_usr")));
                                    Context.fSetArgument("wFolderPath",  _sRelativePathResult.Replace('\\', '/') );
                                    
                                   
                                    
                                   //  info["templatePath"] = templatePath;
                                    //  info["inDirectory"] = inDirectory;
                                    TraceManager.Add("---CreateNewFile---");
								    oContext.fRefreshTree();
                               }

                            }break;
									case "ProjectManager.UserRefreshTree":
                            {
                                TraceManager.Add("---Refresh");
								oContext.fRefreshTree();

                            }break;
                       
							 case "ProjectManager.TestAndSkip"   :{
								 TraceManager.Add("----TestingProject");
                                PluginBase.MainForm.CallCommand("PluginCommand", "ResultsPanel.ClearResults");


								


							//	CompileCpp.gblCompileCwTime.Reset();
                         //       CompileCpp.gblCompileCwTime.Start();

                        //         MsgProcess.fSendMsg("Compile|" + oContext.oCurrProject.TargetBuild + "|"  + "NoBuild" + "|");
         
                            }break;




                           case "ProjectManager.Project":
                            {
                 
                                IProject project = PluginBase.CurrentProject;

                                TraceManager.Add("-Projet LOOOOOOOAAAAAAAAAAAAAAAAADDDDDD " );
                            
                                    if(project != null && project.GetType() == typeof(CwProject)) {
                                            //CWide Poject!!!!!!!!
                                           ///TraceManager.Add("CWide Poject!!!!!!!!" );       
                                           if (oContext != null) {
                                                    
                                                oContext.oCurrProject.oProject = (Project)PluginBase.CurrentProject;
                                                oContext.bProjActive = true;
                                                 oContext.fProjLoad();
									            //ProjectTreeView.Instance.RebuildTree();
                                           }
                                   
                                     }else if (oContext != null) {
                                        if( oContext.bProjActive ) {
                                                oContext.fProjClose();
                                        }
                                     }
                            
                              


                                if (project == null)
                                {
                                    TraceManager.Add("---project is null");
                                   // pluginUI.Output.Text += "Project closed.\r\n";
                                }
                                else
                                {
                    
								
								
									//  TraceManager.Add("- ----- START CWC ---------");
									//	fStartCwc(project);


										
                                   // pluginUI.Output.Text += "Project open: " + project.ProjectPath + "\r\n";
                                   // TraceManager.Add("---project open");
                                //    fStartCwc(project);
                                }
								
                           

                            } break;

								default:
									  TraceManager.Add("- ---UnknowCmd --------- " + cmd);
									break;
				
						}
		}

		#endregion

		#region Custom Methods

		/// <summary>
		/// Initializes important variables
		/// </summary>
		public void InitBasics()
        {
            String dataPath = Path.Combine(PathHelper.DataDir, "CwideContext");
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            this.settingFilename = Path.Combine(dataPath, "Settings.fdb");
            this.pluginImage = PluginBase.MainForm.FindImage("100");
            InitBasics_Console();

     }



        private void fStartCwc(IProject _oProject, string _sPrjDir,string _sLauch){
		

			string _sPath = _oProject.ProjectPath;
			Process currentProcess = Process.GetCurrentProcess();
			string _sPocId = currentProcess.Id.ToString();
			string _sHandle = pluginUI.Handle.ToString();

/*
			BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)
            {*/
			//	Thread.Sleep(50);

				// Get the current process.
				/*
				ProcessStartInfo startInfo = new ProcessStartInfo();
				startInfo.FileName = "H:/_MyPoject/_MyEngine/_LinxCompiler/cwc.exe";
				startInfo.UseShellExecute = true;
				startInfo.Arguments = "-L XXX -P Ala -A " + currentProcess.Id + ";" + pluginUI.Handle;
				startInfo.RedirectStandardOutput = false;
				startInfo.WorkingDirectory = "H:/_MyPoject/_MyEngine/_LinxCompiler/";
	startInfo.WindowStyle = ProcessWindowStyle.Hidden;//startInfo.

				//Start the process
				Process proc = Process.Start(startInfo);
				*/
/*
				string[] _aPath = _sPath.Split('.');
				if (_aPath.Length > 0)
				{*/


				//	oCwRunner = new ProcessRunner();
				//	oCwRunner.WorkingDirectory = PathHelper.Compilerdir;
			   //     oCwRunner.RedirectInput = true;
					//  ascRunner.Run(JvmConfigHelper.GetJavaEXE(jvmConfig), cmd, true);

			   //     oCwRunner.Output += ascRunner_Output;
				//    oCwRunner.Error += ascRunner_Error;

					//	Debug.fTrace("----- Start CWC  -----: " + PathHelper.Compilerdir);
					
					  

					//	oCwRunner.Run(PathHelper.Compilerdir +  "/cwc.exe", "-C " + _aPath[0] + ".cwe" + " -L XXX -P Ala -A " + _sPocId + ";" + _sHandle, true); //A for assist
				//		oCwRunner.Run(_sPath, "", true); //A for assist

					oCwc = new LauchTool();
					oCwc.sWorkPath = CwcRootPath().Replace("\\", "/") + "/";
					oCwc.dOut = new LauchTool.dIOut(fCwcOut);


            //CwideArg


            //Program.AddVar("BuildFile", "Makit.yy");
          //  _sCwcPath = Program.ResolveVars(_sCwcPath);

            /*
            //Apply my custum vars
         BuildEventVars vars = new BuildEventVars(null);

            vars.additional.Clears();
         vars.AddVar("Entry", "Makit.yy");
        foreach (BuildEventInfo _oArg in vars.GetVars()) {
                _sCwcPath = _sCwcPath.Replace(_oArg.FormattedName, _oArg.Value);
        }
        */


        TraceManager.Add("----- Start CWC  -----: " + _sLauch);
      //  TraceManager.Add("----- _sCwcPath  -----: " + _sCwcPath);
        TraceManager.Add("-----CwcRootPath -----: " +oCwc.sWorkPath );
    //    TraceManager.Add("-----_sCwcArg -----: " +_sCwcArg );



            
          //  List<Argument> arguments = ArgumentDialog.CustomArguments;
        //    arguments.Add(new Argument("aaaa","bb") );
            

	//	oCwc= _oCwcExe;
	//TraceManager.Add("-----------Path  -----: " );

				//	_oCwcExe.sWorkPath =  PathHelper.Compilerdir;
				//	_oCwcExe.bVisible =  true;
				//	oCwc.fLauchExe(_sCwcPath,"-wMode IDE -wCloseOnId " + _sCwcArg); //_sPath is the current project


              // string _sFileName = Path.GetFileName(_sCwcPath);
               string _sFileName = fGetFile(_sLauch);
          

		//			oCwc.fLauchExe(_sCwcPath,"-wMode IDE -wCloseOnId " + _sPocId); //_sPath is the current project
                CreateConsolePanel(PluginMain.CwSetting.GetCwcPath(), @" -#Lauch " + _sPrjDir + _sFileName + " |",  _sFileName );
             


                
				//	_oCwcExe.fLauchExe(_sPath,"");
				//	Cwide.Program.oCwc = oCwc;

				// }
/*
		  });
         bw.RunWorkerAsync();
*/
        }


	   public void fCwcOut(string _sOut)  {

	TraceManager.Add("Ukn: " +  _sOut);

		if(_sOut.Length > 4 && _sOut[0] == 'c' && _sOut[1] == 'w'  && _sOut[2] == 'c' && _sOut[3] == ':' ) {
			Debug.fTrace("!!!!!!!!CWC: " + _sOut);
			//Console.WriteLine("Cwc: " +  _sOut);
			Msg.fRecewiveMsg(_sOut.Substring(4) );
		}else

		if(_sOut.Length > 7 && _sOut[0] == 'c' && _sOut[1] == 'w'  && _sOut[2] == 'c' && _sOut[3] == 'A' ) { //CwcAst:

			//Console.WriteLine("Ast: " +  _sOut);
			Msg.fRecewiveMsg(_sOut.Substring(7) );

		}else {
			//Console.WriteLine("Ukn: " +  _sOut);
//			TraceManager.Add("Ukn: " +  _sOut);
		}
			

	   //this.BeginInvoke((MethodInvoker)delegate {		
		// });
}







        private void ascRunner_Output(object sender, string line)
        {
            TraceManager.AddAsync(line, 0);
        }

        private void ascRunner_Error(object sender, string line)
        {
                 TraceManager.AddAsync(line, -3);
        }





        /// <summary>
        /// Adds the required event handlers
        /// </summary> 
        public void AddEventHandlers()
        {
            // Set events you want to listen (combine as flags)
            EventManager.AddEventHandler(this, EventType.FileSwitch | EventType.SyntaxChange | EventType.Command | EventType.UIStarted | EventType.UIRefresh | EventType.FileSave | EventType.UIClosing | EventType.Completion | EventType.Keys |  EventType.FileTemplate | EventType.ProcessArgs );
            UITools.Manager.OnCharAdded += new UITools.CharAddedHandler(OnChar);
            UITools.Manager.OnMouseHover += new UITools.MouseHoverHandler(OnMouseHover);
            UITools.Manager.OnTextChanged += new UITools.TextChangedHandler(OnTextChanged);
            UITools.CallTip.OnUpdateCallTip += new MethodCallTip.UpdateCallTipHandler(OnUpdateCallTip);
            UITools.Tip.OnUpdateSimpleTip += new RichToolTip.UpdateTipHandler(OnUpdateSimpleTip);
           // CompletionList.OnInsert += new InsertedTextHandler(ASComplete.HandleCompletionInsert);
        }

        /// <summary>
        /// Initializes the localization of the plugin
        /// </summary>
        public void InitLocalization()
        {
            LocaleVersion locale = PluginBase.MainForm.Settings.LocaleVersion;
            switch (locale)
            {
                /*
                case LocaleVersion.fi_FI : 
                    // We have Finnish available... or not. :)
                    LocaleHelper.Initialize(LocaleVersion.fi_FI);
                    break;
                */
                default : 
                    // Plugins should default to English...
                    LocaleHelper.Initialize(LocaleVersion.en_US);
                    break;
            }
            this.pluginDesc = LocaleHelper.GetString("Info.Description");
        }

        /// <summary>
        /// Creates a menu item for the plugin and adds a ignored key
        /// </summary>
        public void CreateMenuItem()
        {
            /*
            ToolStripMenuItem viewMenu = (ToolStripMenuItem)PluginBase.MainForm.FindMenuItem("ViewMenu");
            viewMenu.DropDownItems.Add(new ToolStripMenuItem(LocaleHelper.GetString("Label.ViewMenuItem"), this.pluginImage, new EventHandler(this.OpenPanel), this.CwSetting.SampleShortcut));
            PluginBase.MainForm.IgnoredKeys.Add(this.CwSetting.SampleShortcut);*/
        }

        /// <summary>
        /// Creates a plugin panel for the plugin
        /// </summary>
        public void CreatePluginPanel()
        {
            pluginUI = new PluginUI(this);
    //        pluginUI.Text = LocaleHelper.GetString("Title.PluginPanel");
       //     pluginPanel = PluginBase.MainForm.CreateDockablePanel(pluginUI, this.pluginGuid, this.pluginImage, DockState.DockRight);
            CreatePluginPanel_Console();
        }

        /// <summary>
        /// Loads the plugin settings
        /// </summary>
        public void LoadSettings()
        {
            CwSetting = new ContextSettings();
            if (!File.Exists(this.settingFilename)) this.SaveSettings();
            else
            {
                Object obj = ObjectSerializer.Deserialize(this.settingFilename, CwSetting);
                CwSetting = (ContextSettings)obj;
            }
        }

        /// <summary>
        /// Saves the plugin settings
        /// </summary>
        public void SaveSettings()
        {
            ObjectSerializer.Serialize(this.settingFilename, CwSetting);
        }

        /// <summary>
        /// Opens the plugin panel if closed
        /// </summary>
        public void OpenPanel(Object sender, System.EventArgs e)
        {
         //   this.pluginPanel.Show();
        }

        #endregion



        #region Event handlers

        /// <summary>
        /// Display completion list or calltip info
        /// </summary>
        private void OnChar(ScintillaNet.ScintillaControl Sci, int Value)
        {

          //  Sci.KeyWords(1, "AA BBB CCC");
  
           TraceManager.Add("char " + Value); //Not good
            /*
            if (!bOnCompletion)
            {
                bOnCompletion = true;
                nPosStartCmpl = Sci.CurrentPos;
            }*/
            //ASComplete.GetWordLeft(Sci, ref nPosStartCmpl);

            // smart focus token
            //if (!features.externalCompletion)
            //AutoselectDotToken(classScope, tail);
            
        }


        public static bool bFileModified = false;
        private void OnTextChanged(ScintillaNet.ScintillaControl sender, int position, int length, int linesAdded)
        {

            //CW+
            ITabbedDocument document = DocumentManager.FindDocument(sender);
          //  if (!FileInspector.IsCWave(document.FileName))  {
            if (!bIsCpp)  {
                TraceManager.Add("---!!! No -OnTextChanged  " );
                Console.WriteLine("---!!! No -OnTextChanged  " );
                return;
            }



MsgProcess.fSendMsg("P:Connected", (int)1380598);




            bFileModified = true;

           /*
            BUUUGG Warning
            sender.StyleSetBold(1, true);
            sender.Colourise(position - 3, position);
            sender.StartStyling(position-3, 0);
            sender.StyleSetBold(1, true);
            sender.SetStyling(5, 1);
            */
          //  sender.s(position - 3, 0);

            if (linesAdded > 0){ //No autocompletion on new line
                return;
            }
            int _nOriginalPos = position;
          

            TraceManager.Add("------OnTextChanged  " + linesAdded);
            try {
        
                if (oContext != null && oContext.curClass != null)
                {

                    int tailPos = position;
                    if (length < 0){
                        tailPos--;
                    }else{
                        sender.CurrentPos++;
                        tailPos += length - 1;
                    }

                    int _nPos = tailPos;
                
                    string tail = GetWordLeft(sender, ref tailPos);
                    
                    if (length > 0)
                    {
                        sender.SelectionStart = tailPos + 1;
                    }
              
                    int _nPosStartFuncParam = tailPos;
                    if (fIsInFunction(sender, ref _nPosStartFuncParam))
                    {
                        TraceManager.Add("--HandleFunctionCompletion");

                        fFunctionCompletion(sender, _nPosStartFuncParam);
                       // return;
                    }
                    else
                    {
                        oContext.fHideFuncInfo();
                    }
             
                    if (sender.CharAt(_nPos) == '.')
                    {
                        nCurTailPos = tailPos;
                        fGetRealtiveScope(sender, ref tailPos, "");
                        sender.GotoPos(_nOriginalPos);
                        return;
                    }
                    else
                    {
                        if ( tailPos != nCurTailPos )
                        {
                            TraceManager.Add("--OKOK GEtRel");
                            nCurTailPos = tailPos;
                            if (sender.CharAt(tailPos) == '.')
                            {
                                fGetRealtiveScope(sender, ref tailPos, tail);

                               // TraceManager.Add("LINE CHANGE: " + sender.CharAt(tailPos) + " : " + tail);
                                //MsgProcess.fSendMsg("LineChange|" + oContext.CurrentFile + "|" + oContext.CurrentLine + "|" + "0");
                                sender.GotoPos(_nOriginalPos);
                                return;
                            }else{
                                if(oContext.curExp != null && oContext.curExp.Members != null){
                                    oContext.curExp.Members.Clear();
                                  //  ASContext.Context = oContext; //Refresh
									oContext.fUpdateOutline();
                                }
                            }
                        }
                    }
                    TraceManager.Add("Noraml");
                    nCurTailPos = tailPos;
                    TraceManager.Add("Tail: " + tail);
    

                    if (sLastLetter == '.')
                    {
                        fAnalyseRealtiveScope(sender, ref tailPos, tail);
                      
                        sender.GotoPos(_nOriginalPos);
                          fShowExp(tail, true);
                        return;
                    }else if (tail.Length == 0) {
                        sender.GotoPos(_nOriginalPos);
                        Completion.Hide();
                        return;
                    }


           
                   // sender.CurrentPos = tailPos;
                    sender.GotoPos(_nOriginalPos);
                    if(length == 1 || length == - 1) { //Not on Copy paste
                       fShowExp(tail);
                    }else {
                        Completion.Hide();
                    }
                   
                }

              }
            catch (Exception ex)
            {

                TraceManager.Add("Error, OnTextChanged");
            }
           }

        //private MethodCallTip callTip;

        public static int nFuncTailPos = 0; //Todo PluginMain.nFuncTailPos better way?
        public void fFunctionCompletion(ScintillaNet.ScintillaControl sender, int _nTailPos)
        {
            
            int _nPrePos = _nTailPos;
            _nPrePos--;
             string  _sPreTail = GetWordLeft(sender, ref _nPrePos);


             string _sExpScope = fGetFullTail(sender, ref _nPrePos, _sPreTail);

            TraceManager.Add("PreTail: " + _sExpScope);

            if (_nTailPos != nFuncTailPos) //already sended
            {
                nFuncTailPos = _nTailPos;
                 Msg.fSendToCppAst("*GetFuncInfo|" + oContext.sCurrFileName + "|" + nLastLine + "|" + _sExpScope + "|");
            }
            else
            {
                oContext.fShowFuncInfo();
            }

           // ASExpr expr = ASComplete.GetExpression(oCurSci, _nTailP, true);
          //  ASComplete.FunctionContextResolved(oCurSci, expr, _oSelFunc, curFInfoClass, true);

            /*
            callTip = new MethodCallTip(PluginBase.MainForm);

            callTip.CallTipShow(sender, 2, "(asdasd, asdf )", true);
             callTip.PositionControl(sender);
   
            callTip.CallTipSetHlt(0 + 1, 5, true);

            ASComplete.calltipMember = oContext.curFunc;
            ASComplete.calltipDef = "(aaa,test)";
            ASComplete.ShowCalltip(sender, 0, true);
           // UITools.CallTip.toolTipRTB.Te
        
            UITools.CallTip.CallTipShow(sender,2, "(asdasd, asdf )", true);
            UITools.CallTip.CallTipSetHlt(0 + 1, 5, true);
            UITools.CallTip.PositionControl(sender);*/
          //  CompletionList.UpdateTip(sender, null);

        }

        public void fShowExp(string _sTail, bool bSubExp = false){
            //TraceManager.Add("Shpw Tail: " + _sTail);
            if(_sTail.Length == 0) {
               Completion.Hide();
                return;
            }

            if (oContext.curClass == null)
            {
                return;
            }
            if (oContext.curFunc == null)
            {
                return;
            }
            

            MemberList mix = new MemberList();
            if (bSubExp)
            {
                 if ( oContext.curExp.Members != null)
                  {
                         mix.Add(oContext.curExp.Members);
                 }
            }
            else
            {
                if (oContext.curClass.Members != null )
                 {
                     mix.Add(oContext.curClass.Members);
                }

                if (oContext.curFunc.Members != null )
                {
                mix.Add(oContext.curFunc.Members);
                }
              
            }
         


            // show
            List<ICompletionListItem> list = new List<ICompletionListItem>();
            foreach (MemberModel member in mix)
            {
                if ((member.Flags & FlagType.Template) > 0)
                    list.Add(new TemplateItem(member));
                else
                    list.Add(new MemberItem(member));
            }



 //           CompletionList.ShowNx(list, false, _sTail); TODO
            Completion.ShowNx(list, false, _sTail);// TODO
        }



        public string sLastExpScope = "";

        //GetWordLeft must be used before with the sane exp
        private string fGetFullTail(ScintillaNet.ScintillaControl Sci, ref int position, string _sTail)
        {
            while (sLastLetter == '.'){
                position--;
                _sTail = GetWordLeft(Sci, ref position) + '.' + _sTail;
            }
            return _sTail;
        }
        

        private void fGetRealtiveScope(ScintillaNet.ScintillaControl Sci, ref int position, string _sTail)
        {



          _sTail = fGetFullTail(Sci, ref position, _sTail);

            //TraceManager.Add("fGetRealtiveScopeTail :" + _sTail);
            /*
            while (sLastLetter == '.'){
                position--;
                _sTail = GetWordLeft(Sci, ref position) + '.' + _sTail;
            }*/


            //      TraceManager.Add("----GetRelScope|" + oContext.sCurrFileName + "|" + nLastLine + "|" + _sTail + "|");

            string _sExpScope = oContext.sCurrFileName + "|" + nLastLine  + "|" + _sTail ;

            if(oContext.curExp.Members.Count == 0 || sLastExpScope != _sExpScope){ // TODO vefifie if it was cleared insted
                 Msg.fSendToCppAst("*GetRelScope|" + _sExpScope + "|");
                sLastExpScope = _sExpScope;
                TraceManager.Add("-----******** fGetRelScope :" + _sExpScope);

            }else {

              //  ASContext.Context = oContext;
				oContext.fUpdateOutline();

                oContext.oPluginMain.fShowExp("", true);
            }

            oContext.curExp.Members.Clear();
         
           // Sci.WordEndPosition(position, true);

        }

        private void fAnalyseRealtiveScope(ScintillaNet.ScintillaControl Sci, ref int position, string _sTail)
        {
            TraceManager.Add("Anlayse : " + _sTail);
        }


        
        public static int nLastLine = 0;
        public static void fAnalyseScope(string _sFile, bool _bForce = false)   {

            try
            { //Workaround
                if ( Globals.SciControl != null) {
                
                    ScintillaControl _oSci = Globals.SciControl;
                    Context.oCurSci = _oSci;
                    ITabbedDocument document = DocumentManager.FindDocument(_oSci);
                    if (_oSci != null && document != null && document.IsEditable)
                    {


                        int _nLine = _oSci.CurrentLine + 1;
                        if (nLastLine != _nLine || _bForce)
                        { //Line change
                            Completion.Hide();
                        
            
                            
                            if(bFileModified) {
                              bFileModified = false;


                                //TODO only if we have 0 build errrors
                                    DataEvent de = new DataEvent(EventType.Command,"ResultsPanel.ClearResults", null);
                                 EventManager.DispatchEvent(pluginUI, de);


                             //Context.fCwcSend("Reparse|" + _sFile + "|" + 0 + "|" + _oSci.Text.Length + "|" + _oSci.Text.Replace('\n','\r'));
                            Msg.fSendToCppAst("*Reparse|" + _sFile + "|" + 0 + "|" + _oSci.Text.Length + "<" + _oSci.Text );
                          //  Msg.fSendToCppAst("*Reparse|" + _sFile + "|" + 0 + "|" + _oSci.Text.Length + "<" + _oSci.Text);
                                
                                //tempsss
                           //   Context.fCwcSend("Reparse|" + _sFile + "|" + 0 + "|" + _oSci.Text.Length + "|" + _oSci.Text);  //tempsss
                            }

                           fLineChange(_oSci, _nLine );
                            TraceManager.Add("_AnalyseScopeLine : " + _nLine);


                        //    Context.fCwcSend("LineChange|" + _sFile + "|" + _nLine + "|" + nLastLine + "|");  //tempsss
                            Msg.fSendToCppAst("*LineChange|" + _sFile + "|" + _nLine + "|" + nLastLine + "|");  //tempsss
                            nLastLine = _nLine;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void fLineChange(ScintillaControl _oSci, int _nLine)   {
              oContext.fDisableGotoDecl();
            string _sLine = _oSci.GetLine(_nLine-1).Trim();
            if(_sLine.Length > 3) {   //#include "
                 if(_sLine[0] == '#') { //Directive line
                     if(_sLine.Length > 8 && _sLine.Substring(0, 8) == "#include") {
                        //Include directive
                       // MessageBox.Show("Include directive!!");
                    //    TraceManager.Add("****----------Include directive!!");
                           int _nStartIndex = _sLine.IndexOf('\"');
                           int _nLastIndex = _sLine.LastIndexOf('\"')-1;
                           if(_nStartIndex != -1 && _nLastIndex > _nStartIndex ) {
                                fIncludeDirective(_sLine.Substring(_nStartIndex+1,_nLastIndex -_nStartIndex ));
                        }
                    }
                }
            }
        }
       public static void fIncludeDirective(string _sFile)   {
            if(_sFile != "") {

             TraceManager.Add("****----------Include directive!! " + _sFile);
                oContext.fEnableGotoDecl(_sFile);

            }
        }



        private void OnMouseHover(ScintillaNet.ScintillaControl sci, int position)
        {
            /*
            if (!ASContext.Context.IsFileValid)
                return;

            lastHoverPosition = position;

            // get word at mouse position
            int style = sci.BaseStyleAt(position);
            if (!ASComplete.IsTextStyle(style))
                return;
            position = sci.WordEndPosition(position, true);
            ASResult result = ASComplete.GetExpressionType(sci, position);

            // set tooltip
            if (!result.IsNull())
            {
                string text = ASComplete.GetToolTipText(result);
                if (text == null) return;
                // show tooltip
                UITools.Tip.ShowAtMouseLocation(text);
            }*/
           // TraceManager.Add("MouseOver");
        }

        private void OnUpdateCallTip(ScintillaNet.ScintillaControl sci, int position)
        {/*
            if (ASComplete.HasCalltip())
            {
                int pos = sci.CurrentPos - 1;
                char c = (char)sci.CharAt(pos);
                if ((c == ',' || c == '(') && sci.BaseStyleAt(pos) == 0)
                    sci.Colourise(0, -1);
                ASComplete.HandleFunctionCompletion(sci, false, true);
            }*/
            TraceManager.Add("UpdateCallTIp");
        }

        private void OnUpdateSimpleTip(ScintillaNet.ScintillaControl sci, Point mousePosition)
        {/*
            if (UITools.Tip.Visible)
                OnMouseHover(sci, lastHoverPosition);*/
            TraceManager.Add("UpdateSimpleTIp");
        }

        #endregion

        private static char sLastLetter = '\0';
        static public string GetWordLeft(ScintillaNet.ScintillaControl Sci, ref int position)
        {
            sLastLetter = '\0';
            // get the word characters from the syntax definition
            string characterClass = ScintillaNet.ScintillaControl.Configuration.GetLanguage(Sci.ConfigurationLanguage).characterclass.Characters;

            string word = "";
            char c;
            while (position >= 0)
            {

                c = (char)Sci.CharAt(position);
                sLastLetter = c;
                if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_'))
                {
                    break;
                }
                word = c + word;
                position--;
            }
            return word;
        }



        static public bool fIsInFunction(ScintillaNet.ScintillaControl Sci, ref int position)
        {
           // int _nCurrLine = Sci.CurrentLine;
            int _nStartLine = Sci.PositionFromLine(Sci.CurrentLine);
               // TraceManager.Add("Posistion:"  + _nStartLine);
            if ((char)Sci.CharAt(position) == '(')
            {
                return true;
            }

            int _nLevel = 0;
            char c = (char)Sci.CharAt(position);
            //while (Sci.CurrentLine == _nCurrLine){
            while (position > _nStartLine) { 


                 if (c == '('   )
                {
                    if (_nLevel <= 0)
                    {
                        //TraceManager.Add("FOUND:" + position);
                        position++;
                        return true;
                     }
                    _nLevel--;
                }else if (c == ')'   )
                {
                    _nLevel++;
                }

                c = (char)Sci.CharAt(position);
                position--;
            }
            // position++;
          //  TraceManager.Add("Not found:" + position);
            return false ;
        }

    }



    /// <summary>
    /// Template completion list item
    /// </summary>
    public class TemplateItem : MemberItem
    {
        public TemplateItem(MemberModel oMember) : base(oMember) { }

        override public string Description
        {
            get
            {
                if (ASComplete.HasSnippet(member.Name))
                    member.Comments = "[i](" + TextHelper.GetString("Info.InsertKeywordSnippet") + ")[/i]";
                return base.Description;
            }
        }
    }

    public class MemberItem : ICompletionListItem
    {
        protected MemberModel member;
        private int icon;

        public MemberItem(MemberModel oMember)
        {
            member = oMember;
            icon = ASCompletion.PluginUI.GetIcon(member.Flags, member.Access);
        }

        public string Label
        {
            get { return member.FullName; }
        }

        public virtual string Description
        {
            get
            {
                return ClassModel.MemberDeclaration(member) + ASDocumentation.GetTipDetails(member, null);
            }
        }

        public Bitmap Icon
        {
            get { return (Bitmap)ASContext.Panel.GetIcon(icon); }
        }

        public string Value
        {
            get
            {
                if (member.Name.IndexOf('<') > 0)
                {
                    if (member.Name.IndexOf(".<") > 0)
                        return member.Name.Substring(0, member.Name.IndexOf(".<"));
                    else return member.Name.Substring(0, member.Name.IndexOf('<'));
                }
                return member.Name;
            }
        }

        public override string ToString()
        {
            return Label;
        }
    }


     

    
}
