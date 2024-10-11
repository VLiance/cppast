using System;
using System.Collections;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI;
using PluginCore;
using System.Runtime.InteropServices;

using System.Text;

using System.Drawing;



using PluginCore.Managers;
using PluginCore.Helpers;

namespace CwideContext
{
    public class PluginUI : UserControl
    {
        public const int WM_COPYDATA = 0x004A;
        public struct COPYDATASTRUCT
        {
		   public Int32 dwData;
            public Int32 cbData;
            public IntPtr lpData;
        }

		public Queue myQ = new Queue();
	

        private RichTextBox richTextBox;
        public static PluginMain pluginMain;
        public static PluginUI pluginUI;
        
        public PluginUI(PluginMain _pluginMain)
        {
     //       this.InitializeComponent();
            pluginMain = _pluginMain;
            pluginUI = this;
        }

        /// <summary>
        /// Accessor to the RichTextBox
        /// </summary>
        public RichTextBox Output
        {
            get { return this.richTextBox; }
        }
        
        #region Windows Forms Designer Generated Code

        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent() 
        {
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // richTextBox
            // 
            this.richTextBox.DetectUrls = false;
            this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox.Location = new System.Drawing.Point(0, 0);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.Size = new System.Drawing.Size(280, 352);
            this.richTextBox.TabIndex = 0;
            this.richTextBox.Text = "";
            this.richTextBox.TextChanged += new System.EventHandler(this.richTextBox_TextChanged);
            // 
            // PluginUI
            // 
            this.Controls.Add(this.richTextBox);
            this.Name = "PluginUI";
            this.Size = new System.Drawing.Size(280, 352);
            this.ResumeLayout(false);

        }

        #endregion

       public void fSenkey(string _sTest) {

               this.BeginInvoke((MethodInvoker)delegate {		
		
             SendKeys.SendWait(_sTest);
             });

        }


        protected override void WndProc(ref Message m) {
            switch (m.Msg)  {

                // program receives WM_COPYDATA Message from target app
                case WM_COPYDATA:
                    if (m.Msg == WM_COPYDATA) {
						 Object thisLock = new Object();
						 lock (thisLock) {

						Debug.fTrace("WM_COPYDATA ");
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
								//	TraceManager.Add("****************" + _sReceivedText);
							
								 myQ.Enqueue(_sReceivedText );
								 Queue mySyncdQ = Queue.Synchronized( myQ );
								 PluginMain.SafeInvoke(delegate {  //Not syre

										string _sReceived = (string)mySyncdQ.Dequeue();
												
											//	Output.SelectionColor = Color.Red;
												//  Output.Text += _sReceivedText;
												Msg.fRecewiveMsg(_sReceived);
											//		Output.AppendText("\r\n");

									}); 
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



        private void richTextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
