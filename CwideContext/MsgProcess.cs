using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Diagnostics;
using PluginCore.Managers;
using System.Threading;
using Microsoft.Win32;

namespace CwideContext
{

    public class MsgProcess
    {
        [DllImport("User32.dll")]
        private static extern int RegisterWindowMessage(string lpString);

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern Int32 FindWindow(String lpClassName, String lpWindowName);

        //For use with WM_COPYDATA and COPYDATASTRUCT
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        //For use with WM_COPYDATA and COPYDATASTRUCT
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern bool PostMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern bool SendNotifyMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(int hWnd, int Msg, int wParam, int lParam);

        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(int hWnd, int Msg, int wParam, int lParam);

        delegate void SendAsyncProc(int hwnd, uint uMsg, int dwData, int lResult);

        [DllImport("user32.dll")]
        private static extern bool SendMessageCallback(
        int hWnd,
        uint Msg,
        int wParam,
        ref COPYDATASTRUCT lParam,
        SendAsyncProc lpCallback, //Call back delegate used
        int dwData); 


        [DllImport("User32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(int hWnd);


        public class Worker
        {
            // This method will be called when the thread is started. 
            public void DoWork()
            {
                while (!_shouldStop)
                {
                    Debug.fTrace("worker thread: working...");
                    Thread.Sleep(1);
                }
                Debug.fTrace("worker thread: terminating gracefully.");
            }
            public void RequestStop()
            {
                _shouldStop = true;
            }
            // Volatile is used as hint to the compiler that this data 
            // member will be accessed by multiple threads. 
            private volatile bool _shouldStop;
        }


        public const int WM_USER = 0x400;
        public const int WM_COPYDATA = 0x4A;

        public static uint nHandle = 0;

        //Used for WM_COPYDATA for string messages
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }

        public static void fStartWorker()
        {
/*
            Worker workerObject = new Worker();
            Thread workerThread = new Thread(workerObject.DoWork);
            workerThread.Start();
*/
        }



        public static void fSendMsg(string msg, int _nToHandle = 0)
        {
           	if(_nToHandle == -1){
                  return;
            }
			if(_nToHandle == 0){
                _nToHandle = (int)nHandle;
                 if(_nToHandle == 0){
                        return;
                  }
            }
           


             int result = 0;
            string _sMsg = (string)msg + "\0\0\0\0"; //Unicode Safe 
			//  Debug.fTrace("Static thread procedure. Data='{0}'", data);
			byte[] sarr = System.Text.Encoding.Default.GetBytes(_sMsg);
			int len = sarr.Length;
			COPYDATASTRUCT cds;
			//cds.dwData = (IntPtr)0;
			cds.dwData = (IntPtr)5;
			cds.lpData = _sMsg;
			cds.cbData = len + 1;

  Thread SendMsg = new Thread(new ThreadStart(() =>  {  

         //       TraceManager.Add("Send " + (int)_nToHandle + "  Msg " + _sMsg);
			result = SendMessage((int)_nToHandle, WM_COPYDATA, 0, ref cds);//!! TODO Possibility to hang !!
            //For that reason you should always use SendNotifyMessage or SendMessageTimeout when you use HWND_BROADCAST, or are otherwise sending messages to windows owned by other processes.
            //There is a SendMessageTimeout which will limit the amount of time your application blocks while waiting for the receiver to accept.
	    	//Another workaround is to launch multiple threads and have them deliver multiple messages at once (i.e. to it in parallel). Then if one of the receivers is hung, you don't kill your entire app.
		}));  SendMsg.Start();   
             
            /*
			if(PluginMain.bCwcStarted ) {
				Console.WriteLine("Send : " + msg);

				PluginMain.oCwc.fSend("cwc:" + msg);
				//PluginMain.oCwc.fSend("asssssssssss");
			}*/


			/*
			string newMsg = msg;
            Thread newThread = new Thread(MsgProcess.DoWork);
            newThread.Start(newMsg);
*/
		//	DoWork(msg);
        }

        /*
        public static void DoWork(object data)
        {
			try {
            int result = 0;
				string _sMsg = (string)data;
			  //  Debug.fTrace("Static thread procedure. Data='{0}'", data);
				byte[] sarr = System.Text.Encoding.Default.GetBytes(_sMsg + "\0");
				int len = sarr.Length;
				COPYDATASTRUCT cds;
				cds.dwData = (IntPtr)0;
				cds.lpData = _sMsg;
				cds.cbData = len + 1;
                	TraceManager.Add("Send " + (int)nHandle);
                	TraceManager.Add("Msg " + _sMsg);
				result = SendMessage((int)nHandle, WM_COPYDATA, 0, ref cds);
			}
			catch
			{
				TraceManager.Add("Fail SendMessage!!");
			}

        }*/




        public static int sendWindowsStringMessage(uint hWnd, int wParam, string msg)
        {
            int result = 0;
            bool bResult = false;
            if (hWnd > 0)
            {
                byte[] sarr = System.Text.Encoding.Default.GetBytes(msg  + "\0");
                int len = sarr.Length;
                COPYDATASTRUCT cds;
                cds.dwData = (IntPtr)0;
                cds.lpData = msg;
                cds.cbData = len + 1;

               result = SendMessage((int)hWnd, WM_COPYDATA, 0, ref cds);

             //   bResult = SendNotifyMessage((int)hWnd, WM_COPYDATA, wParam, ref cds);

             //  bResult = PostMessage((int)hWnd, WM_COPYDATA, wParam, ref cds);


               // SendMessageCallback((int)hWnd, WM_COPYDATA, wParam, ref cds);



                //SendMessageCallback((IntPtr)hWnd, WM_COPYDATA, IntPtr.Zero, IntPtr.Zero, del, UIntPtr.Zero);
                /*
                SendAsyncProc del = new SendAsyncProc(SendMessage_Callback);
                 bResult = SendMessageCallback((int)hWnd, WM_COPYDATA, wParam, ref cds, del, 0);
                if (!bResult)
                {
                    TraceManager.Add("failed!");

                }*/


            }

            return result;
        }

        public static int sendWindowsMessage(int hWnd, int Msg, int wParam, int lParam)
        {
            int result = 0;

            if (hWnd > 0)
            {
                result = SendMessage(hWnd, Msg, wParam, lParam);
            }

            return result;
        }




        public static int getWindowId(string className, string windowName)
        {
            //return FindWindow(className, windowName)
                return FindWindow(null, windowName);
        }
        /*
        internal static void sendWindowsStringMessage(uint nHandle, int p1, string p2)
        {
            throw new NotImplementedException();
        }*/


        private static void SendMessage_Callback(int hwnd, uint uMsg, int dwData, int lResult)
        {
            //TraceManager.Add("Receive!");
            // Here is your callback!
            return;
        }


[DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);



/*
String App_Exe = "MyApp.exe";
String App_Path = "%localappdata%;

 SetAssociation_User("myExt", App_Path + App_Exe, App_Exe);
*/

/*
public static void SetAssociation_User(string Extension, string OpenWith, string KeyName, string FileDescription = "")
{
    // The stuff that was above here is basically the same

    // Delete the key instead of trying to change it
    CurrentUser = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\" + Extension, true);
    CurrentUser.DeleteSubKey("UserChoice", false);
    CurrentUser.Close();

    // Tell explorer the file association has been changed
    SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
}
*/
    public static int nConsole_Position = 0;
    public static int _nConsole_Size = 0;


        public static void  fRestoreConsole(string _sPath){
 

            RegistryKey regKey = fGetRegkey( _sPath);
	    	if (regKey != null){
                //           TraceManager.Add("*SetValue-WindowPosition " +  nConsole_Position );
			    regKey.SetValue("WindowPosition", nConsole_Position);
      
            }
        }


      	public static void  fGetConsoleValue(string _sPath){
			RegistryKey regKey = fGetRegkey(_sPath);
			if (regKey != null){
				try{

					object _oKey = regKey.GetValue("WindowPosition");
					if(_oKey != null){
						nConsole_Position = (int)_oKey;
                            //           TraceManager.Add("*GetValue-WindowPosition " +  nConsole_Position );
					}
                    /*
	                object _oKey = regKey.GetValue("WindowSize");
					if(_oKey != null){
						nConsole_Position = (int)_oKey;
					}
                    */


				}catch(Exception e){};
					//Console.WriteLine(_nWindowPlacement);
			}
		}


      public static void  fSaveConsolePosition(string _sPath,Int32 x, Int32 y, Int32 _nWidth, Int32 _nHeight){
 
				RegistryKey regKey = fGetRegkey( _sPath);
					if (regKey != null){
					
							//Save position & size
					//		Int32 x; Int32 y; Int32 width; Int32 height; Int32 ClientWidth; Int32 ClientHeight;
					//		GetWindowPosition(out x, out y, out width, out height, out ClientWidth, out ClientHeight );


					//	int _nWidth = Console.WindowWidth;
					//	int _nHeight = Console.WindowHeight;
						
						ushort _nX = (ushort)x;
						ushort _nY = (ushort)y;
						regKey.SetValue("WindowPosition", (_nY << 16) | _nX);
						//regKey.SetValue("WindowSize", (_nHeight << 16) | _nWidth);
						
						
						//ushort _nBuffWidth = (ushort)Console.BufferWidth;
						//ushort _nBuffHeight = (ushort)Console.BufferHeight;
						//regKey.SetValue("ScreenBufferSize",(_nBuffHeight << 16) | _nBuffWidth);

               }

            }


        public static RegistryKey  fGetRegkey(string _sPath){
			try{
				//string _sCurrenKey = "";
			//	if(oParentProcess.ProcessName == "cmd"){
			//		_sCurrenKey = "%SystemRoot%_system32_cmd.exe";
			//	}else{
					_sPath = _sPath.Replace('\\','_');
			//	}
				string _sKey = @"Console\" + _sPath;
				RegistryKey _oKey =  Registry.CurrentUser.OpenSubKey(_sKey,true);
				if(_oKey == null){
					Registry.CurrentUser.CreateSubKey(_sKey);
					 _oKey =  Registry.CurrentUser.OpenSubKey(_sKey,true);
				}
				return _oKey;

			}catch(Exception e){};
			
			
			return null;
		}






//Win10 ?
 public static void SetAssociation_User(string Extension, string OpenWith, string ExecutableName)
 {
    try {
                using (RegistryKey User_Classes = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Classes\\", true))
                using (RegistryKey User_Ext = User_Classes.CreateSubKey("." + Extension))
                using (RegistryKey User_AutoFile = User_Classes.CreateSubKey(Extension + "_auto_file"))
                using (RegistryKey User_AutoFile_Command = User_AutoFile.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command"))
                using (RegistryKey ApplicationAssociationToasts = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\ApplicationAssociationToasts\\", true))
                using (RegistryKey User_Classes_Applications = User_Classes.CreateSubKey("Applications"))
                using (RegistryKey User_Classes_Applications_Exe = User_Classes_Applications.CreateSubKey(ExecutableName))
                using (RegistryKey User_Application_Command = User_Classes_Applications_Exe.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command"))
                using (RegistryKey User_Explorer = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\." + Extension))
                using (RegistryKey User_Choice = User_Explorer.OpenSubKey("UserChoice"))
                {
                    User_Ext.SetValue("", Extension + "_auto_file", RegistryValueKind.String);
                    User_Classes.SetValue("", Extension + "_auto_file", RegistryValueKind.String);
                    User_Classes.CreateSubKey(Extension + "_auto_file");
                    User_AutoFile_Command.SetValue("", "\"" + OpenWith + "\"" + " \"%1\"");

if(ApplicationAssociationToasts != null) {
	ApplicationAssociationToasts.SetValue(Extension + "_auto_file_." + Extension, 0);
	ApplicationAssociationToasts.SetValue(@"Applications\" + ExecutableName + "_." + Extension, 0);
}

                    User_Application_Command.SetValue("", "\"" + OpenWith + "\"" + " \"%1\"");
                    User_Explorer.CreateSubKey("OpenWithList").SetValue("a", ExecutableName);
                    User_Explorer.CreateSubKey("OpenWithProgids").SetValue(Extension + "_auto_file", "0");
                    if (User_Choice != null) User_Explorer.DeleteSubKey("UserChoice");
                    User_Explorer.CreateSubKey("UserChoice").SetValue("ProgId", @"Applications\" + ExecutableName);
                }
                SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
            }
            catch (Exception excpt)
            {
                //Your code here
            }
        }




 // [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
  //public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

    }
}
