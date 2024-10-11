using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PluginCore.Utilities;
using PluginCore.Managers;
using PluginCore.Helpers;
using PluginCore;
using System.Diagnostics;
using System.Management;
using ProjectManager.Projects;
/*
public  static class Debug {
public  static void fTrace(string _sMsg) {
#if DEBUG
Console.WriteLine(_sMsg);
#endif
}
}*/



namespace CwideContext{

  public  static class Program {


	//public static LauchTool oCwc = null;

        public  static Process MainProcess =  Process.GetCurrentProcess();
        public static readonly Object oLockOutPut = new Object();
        public static int nDontKillId = 0;


        private static List<BuildEventInfo> aVars = new List<BuildEventInfo>();

        public static void AddVar(string name, string value) {
             foreach (BuildEventInfo _oArg in aVars) {
               if(_oArg.Name == name){ //Already exist
                    _oArg.Value = value; //Update value
                    return; 
                }
              }
            aVars.Add(new BuildEventInfo(name, value));

        }
          public static string ResolveVars(string _sToResolve) {
             foreach (BuildEventInfo _oArg in aVars) {
                _sToResolve = _sToResolve.Replace(_oArg.FormattedName, _oArg.Value);
              }
            return _sToResolve;
        }





	static  void InitializeEmptyProject(){

/*
		if(!Directory.Exists( "IDE" )) {
			Directory.CreateDirectory("IDE");
		}
		if(!Directory.Exists( "Plugins" )) {
			Directory.CreateDirectory("IDE/Plugins");
		}
		if(!Directory.Exists( "IDE/Settings" )) {
			Directory.CreateDirectory("IDE/Settings");
		}
		if(!File.Exists( "IDE/Settings/MainMenu.xml" )) {
			 File.Create( "IDE/Settings/MainMenu.xml").Close();
		}

		if(!Directory.Exists( "IDE/Templates" )) {
			Directory.CreateDirectory("IDE/Templates");
		}
*/


	}

/*
        [STAThread]
        static void Main(String[] arguments)
        {
			InitializeEmptyProject();

            if (Win32.ShouldUseWin32()) 
            {
                if (SingleInstanceApp.AlreadyExists)
                {
                    Boolean reUse = Array.IndexOf(arguments, "-reuse") > -1;
                    if (!MultiInstanceMode || reUse) SingleInstanceApp.NotifyExistingInstance(arguments);
                    else RunSimacodeWithErrorHandling(arguments, false);
                }
                else {
                    RunSimacodeWithErrorHandling(arguments, true);
         
                   
                }

            }
            else // For other platforms
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                MainForm.IsFirst = true;
                MainForm.Arguments = arguments;
                MainForm mainForm = new MainForm();
                Application.Run(mainForm);
            }

    //        Process.GetCurrentProcess().Kill(); //Kill all thread
     //      Environment.Exit(0);
			fQuit();
        }
*/

/*
        /// <summary>
        /// Run Cwide and catch any unhandled exceptions.
        /// </summary>
        ///  
        [STAThread]
        static void RunSimacodeWithErrorHandling(String[] arguments, Boolean isFirst)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm.IsFirst = isFirst;
            MainForm.Arguments = arguments;
            MainForm mainForm = new MainForm();
            SingleInstanceApp.NewInstanceMessage += delegate(Object sender, Object message)
            {
                MainForm.Arguments = message as String[];
                mainForm.ProcessParameters(message as String[]);
            };
            try
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, e)
                 => FatalExceptionObject(e.ExceptionObject);

                Application.ThreadException += (sender, e)
                => FatalExceptionHandler.Handle(e.Exception);

                // whatever you need/want here

                SingleInstanceApp.Initialize();
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                FatalExceptionHandler.Handle(ex);
                MessageBox.Show("There was an unexpected problem while running Cwide: " + ex.Message, "Error");
            }
            finally
            {
                SingleInstanceApp.Close();
            }
        }


        static void FatalExceptionObject(object exceptionObject) {
            var huh = exceptionObject as Exception;
            if (huh == null) {
              huh = new NotSupportedException(
                "Unhandled exception doesn't derive from System.Exception: "
                 + exceptionObject.ToString()
              );
            }
           // FatalExceptionHandler(huh);
          }


*/

/*
        /// <summary>
        /// Checks if we should run in multi instance mode.
        /// </summary>
        public static Boolean MultiInstanceMode
        {
            get 
            {
                String file = Path.Combine(PathHelper.AppDir2, "_.multi");
                return File.Exists(file);
            }
        }

        private static class FatalExceptionHandler
        {
            internal static void Handle(Exception exception)
            {
                Console.Write("Fatal exception doesn't derive from System.Exception: " + exception.Message);
                 throw new NotImplementedException();
            }
        }
*/

      static public void fQuit() {
			Debug.fTrace("-------------QUIIIITT----------");
/*
			if(oCwc != null) {
				oCwc.fSend("wQuit");
			}	*/


             KillProcessAndChildren( MainProcess.Id);

              Environment.Exit(1);
            

        }

      
    private static void KillProcessAndChildren(int pid){
		KillProcessAndChildrens( pid);
	}


    private static void KillProcessAndChildrens(int pid){

     try {
            Process proc = Process.GetProcessById(pid);
            if(  proc.Id != nDontKillId) {

					ManagementObjectSearcher searcher = new ManagementObjectSearcher
						("Select * From Win32_Process Where ParentProcessID=" + pid);
					ManagementObjectCollection moc = searcher.Get();
					foreach (ManagementObject mo in moc)
					{
						KillProcessAndChildrens(Convert.ToInt32(mo["ProcessID"]));
					}
   
								
					   //   Debug.fTrace("Kil: "+  proc.);
					    if(proc.Id != MainProcess.Id ) {
						
					//	Debug.fTrace("--Kill: "+  proc.MainWindowTitle.ToString());
							try {

								//proc.Kill();
							//	proc.Kill();
                                proc.CloseMainWindow();

							}catch(Exception e) {
								Debug.fTrace("Exeption: " + e.Message);
		
							} catch {
								// native exception
							}
						}
            }
        }
        catch (Exception ex)
        {
		   MessageBox.Show("**Exeption**: ");
            // Process already exited.
        }
     }



    }
    
}
