using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Automation;
using CwideContext;
using System.Threading;
using System.Runtime.InteropServices;
using PluginCore.Managers;
using System.Text;

namespace ConsoleControl
{
    public partial class TabConsole : UserControl
    {
        public Process process;
        public int cwcHandle = 0;
       public IntPtr cmdHandle;
        AutomationElement window;
        Size realSize;

        ConsoleColor backColor = ConsoleColor.Black;
        ConsoleColor foreColor = ConsoleColor.White;
        List<string> commandsToDo = new List<string>();
        string lastWorkingDir;
        string cmd;
        string arg;

        public event EventHandler Exited;

        public string WorkingDirectory
        {
            set
            {
                lastWorkingDir = value;

                if (process == null)
                {
                    Create();
                }
                else
                {
                    SendString("cd \"" + value + "\"");
                    SendString("cls");
                }
            }
        }

        public ConsoleColor ConsoleBackColor
        {
            get
            {
                return backColor;
            }
            set
            {
                if (backColor == value)
                    return;

                backColor = value;

                var trimmedFore = foreColor.ToString("X").TrimStart('0');
                var trimmedBack = backColor.ToString("X").TrimStart('0');
                if (trimmedFore == "")
                    trimmedFore = "0";
                if (trimmedBack == "")
                    trimmedBack = "0";

                SendString("color " + trimmedBack + trimmedFore);
                SendString("cls");
            }
        }

        public ConsoleColor ConsoleForeColor
        {
            get
            {
                return foreColor;
            }
            set
            {
                if (foreColor == value)
                    return;

                foreColor = value;

                var trimmedFore = foreColor.ToString("X").TrimStart('0');
                var trimmedBack = backColor.ToString("X").TrimStart('0');
                if (trimmedFore == "")
                    trimmedFore = "0";
                if (trimmedBack == "")
                    trimmedBack = "0";

                SendString("color " + trimmedBack + trimmedFore);
                SendString("cls");
            }
        }

        /// <summary>
        /// Returns the actual size of the console window's client area
        /// (the width of a console window only changes by specific values)
        /// </summary>
        public Size ActualSize
        {
            get
            {
                return realSize;
            }
        }

        public Process Process
        {
            get
            {
                return process;
            }
        }

        /// <summary>
        /// Creates a new ConsoleControl
        /// </summary>
        /// <param name="init"></param>
        /// <param name="workingDirectory"></param>
        public TabConsole(string command, string _arg, bool init = true, string workingDirectory = null)
        {
           // InitializeComponent();
            InitializeComponent2();
            SetStyle(ControlStyles.Selectable, true);

            lastWorkingDir = workingDirectory;
            cmd = command;
            arg = _arg;

            if (init)
                Create();
        }

        /// <summary>
        /// Cancel the currently running process
        /// </summary>
        public void Cancel()
        {
            if (process != null)
            {
                if (!process.HasExited)
                {
                    WinApi.KillProcessAndChildrens(process.Id);
                   // SendString("^(c)", false);
                 //  TraceManager.Add("KillConsole");//Kill Tree
               //     process.Kill();
                }
                
                process = null;
            }
        }


        public bool bStarted = false;
        /// <summary>
        /// Creates the console window if it was closed / not created yet
        /// </summary>
        public void Create()
        {
            if (process != null && !process.HasExited)
            {
                return;
            }

            try  {

            //   new Point =  pnlClipping.PointToScreen(new Point(0,0));
             //   string _sConsolePath = "J:\\Download\\Cwc-master(3)\\Cwc-master\\cwc.exe";
                
                string _sConsolePath = cmd;
                string  _sConsolePath_conv =  _sConsolePath.Replace("/","_");

                TraceManager.Add("PAAHT : " + _sConsolePath_conv);
               MsgProcess.fGetConsoleValue(_sConsolePath_conv);
               MsgProcess.fSaveConsolePosition(_sConsolePath_conv, -15000,-15000,50,50);//Make Console invisible -> out of sceen

     
                   

                process = new Process();

               // process.StartInfo.FileName = cmd;
                process.StartInfo.FileName = _sConsolePath;
                
               process.StartInfo.Arguments =  "-{wConnectHandle}=" + Handle.ToString() +  " -{wCloseOnId}=" +  Process.GetCurrentProcess().Id + " | " + arg;
             //  process.StartInfo.Arguments = arg + " -wConnectHandle " + pnlClipping_Menu.Handle.ToString();
          //   process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            //    process.StartInfo.UseShellExecute = false;
          //      process.StartInfo.CreateNoWindow = true;


         //      process.StartInfo.Arguments = arg + " -wMenuHandle " + pnlClipping_Menu.Handle.ToString();
              //  MessageBox.Show(pnlClipping_Menu.Handle.ToString());
                           //    process.StartInfo.Arguments = arg ;
               //process.StartInfo.CreateNoWindow = true;


                if (lastWorkingDir != null)
                    process.StartInfo.WorkingDirectory = lastWorkingDir;
                process.StartInfo.UseShellExecute = false;
                
              
            //    process.StartInfo.RedirectStandardInput = true;

                process.EnableRaisingEvents = true;
                process.Exited += Process_Exited;

                process.Start();
                bStarted = true;
                //Wait for cmd window
                while (process.MainWindowHandle == IntPtr.Zero)
                {
                    process.Refresh();
                }
                cmdHandle = process.MainWindowHandle;
              //  window = System.Windows.Automation.AutomationElement.FromHandle(cmdHandle);
                WinApi.SetParent(cmdHandle, pnlClipping.Handle);

               // SendString("cls");
                ResizeConsole();
                // Remove border and whatnot
               // SetWindowLong(cmdHandle, GWL_STYLE, WS_VISIBLE);

                MsgProcess.fRestoreConsole(_sConsolePath_conv);//Make Console invisible



             //   fWaitAnswer();



            }
            catch
            {
            }
        }



    void fWaitAnswer() {
         //   IntPtr _nHandle = pnlClipping_Menu.Handle;
            IntPtr _nHandle = Handle;
        Thread waitThread = new Thread(new ThreadStart(() =>  {  
                while(true) {
                    Thread.Sleep(50);
                    //   main.CreatConsolePanel("cwc");
                    int length= 50;
                    StringBuilder builder = new StringBuilder( length );
                    WinApi.GetWindowText(_nHandle , builder , length + 1 ); //Work

                    if(builder.ToString() != ""){
                        int _nToHandle = 0;
                        Int32.TryParse(builder.ToString(),out _nToHandle );
                       cwcHandle = (int)_nToHandle;

                         //  TraceManager.Add("Send to asshole");
                         // fSend("P:Connected");
                           MsgProcess.fSendMsg("P:Connected", (int)_nToHandle);
                        break;
                    }

            }
            }));  
        waitThread.Start();

    }



         public const int WM_COPYDATA = 0x004A;
        public struct COPYDATASTRUCT
        {
		   public Int32 dwData;
            public Int32 cbData;
            public IntPtr lpData;
        }

   protected override void WndProc(ref Message m) {
            switch (m.Msg)  {

                // program receives WM_COPYDATA Message from target app
                case WM_COPYDATA:
                    if (m.Msg == WM_COPYDATA) {
						 Object thisLock = new Object();
						 lock (thisLock) {

					//	Debug.fTrace("WM_COPYDATA ");
                            TraceManager.Add("Receive WM_COPYDATA!! ");
						// get the data
						COPYDATASTRUCT cds = new COPYDATASTRUCT();
						cds = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam,
						typeof(COPYDATASTRUCT));

	
								if (cds.cbData > 2) //2 to remove carriage return
								{
									byte[] data = new byte[cds.cbData  ];
									Marshal.Copy(cds.lpData, data, 0, cds.cbData );
								//	data[data.Length - 1 ] = 0;
								//	try{this.BeginInvoke((MethodInvoker)delegate { try {

								Encoding unicodeStr = Encoding.UTF8; //Not UTF8 ?? Seem work
								string _sReceivedText = new string(unicodeStr.GetChars(data ));
								_sReceivedText = _sReceivedText.Substring(0,_sReceivedText.Length-2);

								fRecewiveMsg(_sReceivedText);

							}

						m.Result = (IntPtr)1;
						}
                    }
                    break;

                default:
                    break;
            }
            base.WndProc(ref m);
        }

        private void fRecewiveMsg(string _sReceived) {
            if(_sReceived.Length > 2 &&_sReceived[1] == ':' ) { 
                switch(_sReceived[0]) {
                    case 'C': //Commande
                      fPerformCommand(_sReceived.Substring(2));
                    break;
                }
            }
         
            TraceManager.Add("Receive!!!! " +_sReceived );
        }

    

        private const int GWL_STYLE = (-16);
      private const int WS_VISIBLE = 0x10000000;


 [DllImport("user32.dll", SetLastError = true)]
static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);





        /// <summary>
        /// Sends a String to the embedded console window
        /// </summary>
        /// <param name="str">The string to send</param>
        /// <param name="execute">if true, a "\r" is appended to the given string</param>
        public void SendString(string str, bool execute = true)
        {
            if (execute)
                str += "\r";
            ProcessCommandCache();
            try
            {
                RunCommandWithoutCache(str);
            }
            catch
            {
                commandsToDo.Add(str);
            }
        }

        
		public  void fSend(string _sMsg) {
            if(cwcHandle != 0 &&  process != null && !process.HasExited) {
                    try {
                          MsgProcess.fSendMsg(_sMsg, (int)cwcHandle);
                    }catch(Exception e) { }
              }
            
		}

        private void ProcessCommandCache()
        {
            while (commandsToDo.Count > 0)
            {
                var toDo = commandsToDo[0];
                
                try
                {
                    RunCommandWithoutCache(toDo);
                    commandsToDo.RemoveAt(0);
                }
                catch
                {
                    break;
                }
            }
        }

        private void RunCommandWithoutCache(string cmd)
        {
           // cmdHandle

             WinApi.SendMessage(cmdHandle , 0x000C, 0, cmd);


           //process.Handle.SendMessage("a");

          //  SendKeys.Send("a");
            /*

               this.BeginInvoke((MethodInvoker)delegate {	
            if(window != null) {
                   	
                           window.SetFocus();

                
	
                    ///        SendKeys.SendWait(cmd);

         



                //TODO: prevent user from clicking around while doing this
      //          SendKeys.SendWait(cmd);
                PluginUI.pluginUI.fSenkey("test");
            }
            Focus();
               });*/
        }

        private void ResizeConsole()
        {
            
            WinApi.ShowWindow(cmdHandle, WinApi.SW_SHOWMAXIMIZED);
            
            WinApi.ResizeClientRectTo(cmdHandle, new Rectangle(new Point(0,0), new Size(Size.Width,Size.Height-22)));
            
            SetRealConsoleSize();

            var tooWide = realSize.Width > Width;
            var tooHigh = realSize.Height > Height;
            if (tooWide || tooHigh)
            {
                int newWidth = realSize.Width;
                int newHeight = realSize.Height;
                if (tooWide)
                {
                    newWidth -= 8;
                }
                if (tooHigh)
                {
                    newHeight -= 12;
                }

                WinApi.ResizeClientRectTo(cmdHandle, new Rectangle(0, 0, newWidth, newHeight));
                SetRealConsoleSize();
            }

            pnlClipping_Menu.Size = new Size( Width, 25 );

          //  pnlClipping.Size = new Size( realSize.Width, realSize.Height - 25 );
            pnlClipping.Size = new Size( realSize.Width, realSize.Height );
        }

        private void SetRealConsoleSize()
        {
            WINDOWINFO info;
            WinApi.GetWindowInfo(cmdHandle, out info);

            var leftBorder = info.rcClient.Left - info.rcWindow.Left;
            var topBorder = info.rcClient.Top - info.rcWindow.Top;
            var rightBorder = (int)info.cxWindowBorders;
            var bottomBorder = (int)info.cyWindowBorders;

            var width = info.rcWindow.Right - info.rcWindow.Left;
            var height = info.rcWindow.Bottom - info.rcWindow.Top;

            realSize.Width = width - leftBorder - rightBorder;
            realSize.Height = height - topBorder - bottomBorder;
        }

        private void CmdPanel_Resize(object sender, EventArgs e)
        {
            ResizeConsole();
           //   SendKeys.Send("aaaaaaa");
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            cmdHandle = IntPtr.Zero;
            if (Exited != null)
                Exited(sender, e);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if(process != null) {
                try { 
            process.Kill();//TODO better way?
                }catch(Exception e) { }
            }
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            Cancel();

            base.Dispose(disposing);
        }

        private void CmdPanel_Enter(object sender, EventArgs e)
        {
            /*
            try
            {
                window.SetFocus();
            }
            catch { }
            */
            ProcessCommandCache();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // TabConsole
            // 
            this.Name = "TabConsole";
            this.Load += new System.EventHandler(this.ConsoleControl_Load);
            this.Click += new System.EventHandler(this.TabConsole_Click);
            this.ResumeLayout(false);

        }

        private void ConsoleControl_Load(object sender, EventArgs e)
        {

        }

        private void TabConsole_Click(object sender, EventArgs e)
        {
          
        }






         private void fPerformCommand(string _sCommandes) {
              string[]  _aCommandes =   _sCommandes.Split(';');
            foreach(string _sCommande in _aCommandes) {
                string[]  _aPart =   _sCommande.Split(':');
                if(_aPart.Length >= 2 ) {
                     switch(_aPart[0]){
                        case "wMsgHandle":
                            TraceManager.Add("Found wMsgHandle:" +_aPart[1]);
                   
                             Int32 _nCwcHandle= 0;
                              if(  Int32.TryParse(_aPart[1], out _nCwcHandle))  {
                                   cwcHandle = _nCwcHandle;
                               }

                        break;
                          case "wMenuHandle":
                              TraceManager.Add("Found wMenuHandle:" +_aPart[1]);
                                Int32 _nHandle= 0;

                              if(  Int32.TryParse(_aPart[1], out _nHandle))  {
                                //  WinApi.SetParent((IntPtr)_nHandle, pnlClipping.Handle);
                                  WinApi.SetParent((IntPtr)_nHandle, Handle);
                                 TraceManager.Add("SetParent wMenu:" +_aPart[1] + " to " +  Handle );
                               }
                                fSend("C:ShowMenu");
                           
                        break;
                            

                    }

                }

            }

           
        }














        /*
private void InitializeComponent()
{
this.SuspendLayout();
// 
// ConsoleControl
// 
this.Name = "ConsoleControl";
this.Click += new System.EventHandler(this.ConsoleControl_Click);
this.ResumeLayout(false);

}

private void ConsoleControl_Click(object sender, EventArgs e)
{
MessageBox.Show("aaaaaa");
SendKeys.Send("aaaaaaa");
}*/
    }
}
