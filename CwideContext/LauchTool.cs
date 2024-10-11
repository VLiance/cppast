using PluginCore.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
//using cwc.Update;

namespace CwideContext {
    public class LauchTool {

         //public MainForm oForm = null; //Delegate?

        public bool bExeLauch = false;
        public bool bStopAll = false;
        public bool bHasError = false;
        public bool bVisible  = false;

        public string sExePath = "";
        public string sWorkPath = "";
        public bool bOutput  = true;
        public string sSourceFile  = "";
        public string sTarget  = "";

      public   delegate void dIExit(LauchTool _oTool);
      public   delegate void dIOut(string _sOut);

       public   dIExit dExit = null; 
       public   dIOut dOut = null; 


      public  Process ExeProcess = null;


        public void fLauchExe(string _sExePath, string _sArg, string _sSourceFile = "", string _sTarget= "", bool _bDontKill = false) {
			  sTarget =  _sTarget;
                sSourceFile = _sSourceFile;

             //   string _sArg = "";
                bExeLauch = true;


                BackgroundWorker bw = new BackgroundWorker();

                bw.DoWork += new DoWorkEventHandler(
                delegate(object o, DoWorkEventArgs args) {


                    sExePath = _sExePath;
					if(sWorkPath == "") {
						sWorkPath = _sExePath;
					}

                    ProcessStartInfo processStartInfo = new ProcessStartInfo(_sExePath, _sArg);
                    processStartInfo.UseShellExecute = false;

//Debug.fTrace("Arguments: " +   processStartInfo.Arguments );

                    ExeProcess = new Process();
                   ExeProcess.EnableRaisingEvents = true;
                  ExeProcess.Exited += new EventHandler(fExited);
                   


                     if(bOutput) {
						
                        processStartInfo.CreateNoWindow = !bVisible;
                        processStartInfo.UseShellExecute = false;
                        processStartInfo.RedirectStandardOutput = true;
                        processStartInfo.RedirectStandardError = true;
						processStartInfo.RedirectStandardInput = true;

                        
                        ExeProcess.OutputDataReceived += (sender, e) => {
                                if (e.Data != null)  {
                                    fAppOutput(this, e.Data);
                                }
                            };
                        ExeProcess.ErrorDataReceived += (sender, e) => {
                            if (e.Data != null)  {
                                fAppOutput(this, e.Data);
                            }
                        };
                    }

                    ExeProcess.StartInfo = processStartInfo;
                   processStartInfo.WorkingDirectory = Path.GetDirectoryName(sWorkPath); 
 

	TraceManager.Add("----- _sExePath  -----: " +  _sExePath);
	TraceManager.Add("-----WorkingDirectory -----: " +     processStartInfo.WorkingDirectory);

                    bool processStarted = false;

                    if (bStopAll) {
                        bExeLauch = false;
                        return;
                    }

                    try {
                        if (bHasError){
                            return;
                        }
						
						Debug.fTrace("ExeStart : " + processStartInfo.FileName);
                        processStarted = ExeProcess.Start();
                         if(_bDontKill) {
                            Program.nDontKillId = ExeProcess.Id;
                        }


                        if(bOutput) {
                             ExeProcess.BeginOutputReadLine();
                             ExeProcess.BeginErrorReadLine();
                        }
  //    Thread.Sleep(100);
               

                        while (!ExeProcess.HasExited){
                            Thread.Sleep(1);
   

                            if (bStopAll) {
                                break;
                            }
                        }

                        if(dExit != null) {
                             dExit(this);
                        }
                        if(_bDontKill && ExeProcess.Id == Program.nDontKillId) {
                            Program.nDontKillId = 0;
                        }


                        /*
                        if(oForm != null) {
                            oForm.fLauchEnd();
                        }*/

                    }  catch (Exception ex){ }

                    bExeLauch = false;

                      //           Debug.fTrace("--------------------------- 7z finish -------------------------");

                });
                bw.RunWorkerAsync();

        }

		   public  void fSend(string _sMsg) {
         //   TraceManager.Add("Send " + _sMsg);
                 Thread SendMsg = new Thread(new ThreadStart(() =>  {  
					ExeProcess.StandardInput.WriteLine(_sMsg);
                }));  SendMsg.Start();   
                
			}

        public static void fAppOutput(LauchTool _oThis,string _sOut) {

             lock(Program.oLockOutPut) {
				
				     if(_oThis.dOut != null) {
                             _oThis.dOut(_sOut);
                    }else {
							  /*
							if(_sOut.Length > 3 && _sOut[3] == '%') {
							  Console.Write( "\r\r" + _sOut.Substring(4,_sOut.Length-4) );
							}else { */
	//						 Debug.fTrace(_sOut );
						   // }
					}
             
            }
        }

      private void fExited(object sender, System.EventArgs e) {
           lock(Program.oLockOutPut) {
                Debug.fTrace(" -- Finish --" );
             //   Debug.fTrace("Exit time:    {0}\r\n" +  "Exit code:    {1}\r\nElapsed time: {2}", ExeProcess.ExitTime, ExeProcess.ExitCode);
          }
		

			
    }



    }
}
