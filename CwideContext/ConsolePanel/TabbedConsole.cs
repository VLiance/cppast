using ConsoleControl;
using CwideContext;
using PluginCore.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ConsolePanel.Gui
{
    public partial class TabbedConsole : UserControl
    {
        //private List<CmdPanel> consoles;
        private PluginMain main;

        private TabConsole oLastConsole;

        public ICollection<ConsoleControl.TabConsole> Consoles
        {
            get
            {
                return consoleTabMap.Keys;
            }
        }

        public Dictionary<ConsoleControl.TabConsole, TabPage> consoleTabMap;
        public Dictionary<TabPage, ConsoleControl.TabConsole> tabConsoleMap;

        public TabbedConsole(PluginMain plugin)
        {
            InitializeComponent();
            InitializeComponent2();

            main = plugin;
            consoleTabMap = new Dictionary<ConsoleControl.TabConsole, TabPage>();
            tabConsoleMap = new Dictionary<TabPage, ConsoleControl.TabConsole>();

            btnNew.Image = PluginCore.PluginBase.MainForm.FindImage16("33");
        }

        public void AddConsole(TabConsole console)
        {
            oLastConsole = console;
            var tabPage = new TabPage(console.Text);
            console.Dock = DockStyle.Fill;
            tabPage.Controls.Add(console);

            tabConsoles.TabPages.Add(tabPage);
            tabConsoles.SelectTab(tabPage);
            consoleTabMap.Add(console, tabConsoles.SelectedTab);
            tabConsoleMap.Add(tabConsoles.SelectedTab, console);
        }

        public void RemoveConsole(ConsoleControl.TabConsole console)
        {
            if (consoleTabMap.ContainsKey(console))
            {
                console.Cancel();

                var page = consoleTabMap[console];
                tabConsoles.TabPages.Remove(page);
                consoleTabMap.Remove(console);
                tabConsoleMap.Remove(page);
            }
        }

        private void tabConsoles_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                for (int i = 0; i < tabConsoles.TabCount; i++)
                {
                    if (tabConsoles.GetTabRect(i).Contains(e.Location))
                    {
                        RemoveConsole(tabConsoleMap[tabConsoles.TabPages[i]]);
                    }
                }
                
            }
        }




        private void btnNew_Click(object sender, EventArgs e){

            /*
            WinApi.SetWindowText(oLastConsole.process.MainWindowHandle, "asssss");
            WinApi.SetWindowText(oLastConsole.process.Handle, "AWWWWWWDDDDDD");
           // oLastConsole.process.cmdHandle
           oLastConsole.process.Refresh();
        //        WinApi.SendMessage( oLastConsole.process.MainWindowHandle, 0x000C, 0, "AWW");
          //      WinApi.SendMessage( oLastConsole.process.Handle, 0x000C, 0, "AWW");

           TraceManager.Add("Title: " + oLastConsole.process.MainWindowTitle);
           TraceManager.Add("aass: " + oLastConsole.process.MainModule.ModuleName);
           TraceManager.Add("aass: " + oLastConsole.process.MainModule.BaseAddress);
           TraceManager.Add("aass: " + oLastConsole.process.MainModule.EntryPointAddress);
           TraceManager.Add("aass: " + oLastConsole.process.MainModule.FileName);
           TraceManager.Add("aass: " + oLastConsole.process.ProcessName);
           TraceManager.Add("aass: " + oLastConsole.process.Handle);
           TraceManager.Add("aass: " +          oLastConsole.pnlClipping_Menu.Name);
         //  TraceManager.Add("aass: " +          oLastConsole.pnlClipping_Menu.Handle);

         //   main.CreatConsolePanel("cwc");
         int length= 100;
          StringBuilder builder = new StringBuilder( length );
         //  WinApi.GetWindowText( oLastConsole.process.Handle , builder , length + 1 ); //Work
           WinApi.GetWindowText(oLastConsole.pnlClipping_Menu.Handle , builder , length + 1 ); //Work
            TraceManager.Add("Testaa: " +         builder);
            */
            TraceManager.Add("GetDefaultSDK().Path: " + PluginMain.CwSetting.GetCwcPath());
            
            main.CreateConsolePanel(PluginMain.CwSetting.GetCwcPath());
        }

        private void InitializeComponent2()
        {
            this.SuspendLayout();
            // 
            // TabbedConsole
            // 
            this.Name = "TabbedConsole";
            this.Load += new System.EventHandler(this.TabbedConsole_Load);
            this.ResumeLayout(false);

        }

        private void TabbedConsole_Load(object sender, EventArgs e)
        {

        }
    }
}
