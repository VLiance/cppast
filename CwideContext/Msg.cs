using CwideContext;
using PluginCore;
using PluginCore.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CwideContext
{
	class Msg{

        public static int nHandleCppAst = -1;
        
        public static void fSendToCppAst(string _sMsg)   {
            if(nHandleCppAst > 0) {
             MsgProcess.fSendMsg( _sMsg,nHandleCppAst);
            }
        }




        public static void fRecewiveMsg(string _sMsg) 
        {


            /*
            if( PluginMain.pluginUI == null) {
                TraceManager.Add("Error Msg :" + _sMsg);
            }    
               PluginMain.pluginUI.BeginInvoke((MethodInvoker)delegate {		
               */
        //     });

		//	 PluginMain.SafeInvoke(delegate {  


// TraceManager.Add("fRecewiveMsg Thread:" +     Thread.CurrentThread.ManagedThreadId.ToString());

   //if ((PluginBase.MainForm as Form).InvokeRequired){  //MainThread


Debug.fTrace("**fRecewiveMsg : " + _sMsg);
                (PluginBase.MainForm as Form).BeginInvoke((MethodInvoker)delegate() {
           
          
 
            

/*
 TraceManager.Add("fRecewiveMsgMainForm Thread:" +     Thread.CurrentThread.ManagedThreadId.ToString());


Debug.fTrace("**fRecewiveMsg : " + _sMsg);
Debug.fTrace("**fRecewiveMsg : " + _sMsg[0]);
Debug.fTrace("**fRecewiveMsg : " + _sMsg[1]);
*/

     try {


                if (_sMsg[1] == ':')
                {
                    switch (_sMsg[0])
                    {
                          case 'C':
                            fManageConsole(_sMsg.Substring(3));
                            break;


                        case 'E':
                            fManageError(_sMsg.Substring(3));
                            break;

                        case 'A':

                            fManageAssist(_sMsg.Substring(3));
                            break;
                        default:
                            break;

                    }
                }


      }catch(Exception ex) {  Debug.fTrace("Error: " + ex.Message);   TraceManager.Add("Error: " + ex.Message);   }



      });


    }

        private static void fManageConsole(string _sMsg)
        {
            TraceManager.Add("Receive:" + _sMsg);
        }

        private static void fManageError(string _sMsg)
        {
      //      Output.SelectionColor = Color.Blue;
           // Output.Text += "Error: " + _sMsg;
      //      Output.AppendText("Error: " + _sMsg);
            TraceManager.Add(_sMsg);
        }

        private static void fManageAssist(string _sMsg)
        {

   // Console.WriteLine("-----------------------A: " + _sMsg);
try { 


         TraceManager.Add("A: " + _sMsg);
            int _nIndex = _sMsg.IndexOf("|");
            if (_nIndex <= 0) {
                return;
            }
            string _sArg = _sMsg.Substring(_nIndex + 1).Trim();
            _sMsg = _sMsg.Substring(0, _nIndex);

      //      Output.AppendText(_sMsg);
  ///   Debug.fTrace("------------------TESt: " + _sMsg);

            switch (_sMsg)
            {

                 case "Ready": //Receive handle

					//TraceManager.Add("--Ready: " + _sArg);
                        string[] _aData = _sArg.Split('|');
                        if(_aData.Length>0){
                            Int32.TryParse( _aData[0], out nHandleCppAst);
					        TraceManager.Add("--Found: " + nHandleCppAst);
                        }
                    // MsgProcess.fSendMsg("GetLibInfo|");

				 break;


				 case "Lnx2Cpp":
					TraceManager.Add("Lnx2Cpp: " + _sArg);
				 break;

                case "FinishExtract":
					PluginMain.bCwcStarted = true;
              //      Output.AppendText("!!!!I finish!!!!");
 //   Console.WriteLine("------------------!!!: FinishExtract");
                    //TraceManager.Add("-------SendMsg|GetClassInfo " + pluginMain.oContext.sCurrFileName);
                    MsgProcess.fSendMsg("GetLibInfo|");
                    MsgProcess.fSendMsg("ExtractClassFunc|" + PluginMain.oContext.sCurrFileName + "|");
                    MsgProcess.fSendMsg("GetClassInfo|" + PluginMain.oContext.sCurrFileName + "|");

                    //  PluginMain.fAnalyseScope(pluginMain.oContext.sCurrFileName, true);
                    break;

                case "LibInfo":


					Debug.fTrace("------- ----!!!!!!! LibInfo !!!!!!!---------  " );
               //     Output.AppendText("LibInfo! " + _sArg);
                    PluginMain.oContext.fUpdateAllLibInfo(_sArg);
                    // pluginMain.oContext.fUpdateClassInfo(_sArg);
                    break;


                case "ClassInfo":

//Console.WriteLine("GetClassInfo" + _sArg);
//                    PluginMain.fAnalyseScope(pluginMain.oContext.sCurrFileName, true);
     
                    PluginMain.oContext.fUpdateClassInfo(_sArg);
                    break;


                case "LocalScopeInfo":
                //    Output.AppendText("LocalScopeVar!!!");
                    PluginMain.oContext.fUpdateLocalScopeInfo(_sArg);
                    break;

                case "RelScope":
                  //  Output.AppendText("RelScope!!!");
                    PluginMain.oContext.fUpdateExpScopeInfo(_sArg);
                    break;

                case "FuncInfo":
                 //   Output.AppendText("FuncInfo!!!");
                    PluginMain.oContext.fUpdateFuncInfo(_sArg);
                    break;

                case "LocalScopeOut":
                  //  Output.AppendText("LScopeOutside!");
                    PluginMain.oContext.fLocalScopeOutside(_sArg);
                    break;



                case "ErrorNotFoundClass":
                    PluginMain.oContext.fClassNotInBuild(_sArg);
                    break;

                case "CppBuildList":
                   // CompileCpp.fCppBuildList(pluginMain.oContext.oCurrProject, pluginMain.oContext, _sArg);
                    break;
                    /*
                case "VerifyCppBuildList":
                    pluginMain.oContext.fVerifyCppBuildList(_sArg);
                    break;
                */
               /*
                case "RequireList":
                    pluginMain.oContext.fRequireList(_sArg);
                    break;*/

                case "Handle":

                    PluginMain.nHandle = (uint)MsgProcess.getWindowId("", "cwcc: " + _sArg + " Trial Version");
                    //  pluginMain.nHandle = (uint)MsgProcess.getWindowId("", "cwcc: 0 Trial Version");
                    MsgProcess.nHandle = PluginMain.nHandle;

                 //   Output.AppendText(_sArg);
                    //TraceManager.Add("Search: " + "cwcc: " + _sArg + " Trial Version");
                   // TraceManager.Add("FoundHandle: " + pluginMain.nHandle);
                    //pluginMain.nHandle = UInt32.Parse(_sArg);


             //       MsgProcess.sendWindowsStringMessage(pluginMain.nHandle, 0, "hello");

                    if (PluginMain.nHandle != 0)
                    {
                        PluginMain.bHandle = true;
                    }

                    break;

                     case   "ErrMsg":
                         
                          PluginMain.oContext.fAddError(_sArg);

                     break;

                default:
               //     Output.AppendText("\n\r");
             //       Output.AppendText(_sMsg);
                    break;

            }
   }catch(Exception ex) {  
Debug.fTrace("Error: " + ex.Message);   
 //throw new System.ArgumentException("Parameter cannot be null", "original");  
 }

        }     


	}
}
