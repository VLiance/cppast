using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CwideContext
{
    [StructLayout(LayoutKind.Sequential)]
    struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct WINDOWINFO
    {
        public uint cbSize;
        public RECT rcWindow;
        public RECT rcClient;
        public uint dwStyle;
        public uint dwExStyle;
        public uint dwWindowStatus;
        public uint cxWindowBorders;
        public uint cyWindowBorders;
        public ushort atomWindowType;
        public ushort wCreatorVersion;

        public WINDOWINFO(Boolean? filler) : this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
        {
            cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
        }
    }

    class WinApi
    {
        public const int GWL_STYLE = -16;
        public const int SW_SHOWMAXIMIZED = 3;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool GetWindowInfo(IntPtr hwnd, out WINDOWINFO wi);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool AdjustWindowRect(ref RECT lpRect, int dwStyle, bool bMenu);

        [DllImport("user32.dll")]
        public static extern bool SetWindowText(IntPtr hWnd, string text);

        [DllImport( "USER32.DLL" )]
        public static extern int GetWindowText( IntPtr hWnd , StringBuilder lpString , int nMaxCount );


        [DllImport("User32.dll")]
        public static extern Int32 SendMessage( IntPtr hWnd,  int Msg,  int wParam,  [MarshalAs(UnmanagedType.LPStr)] string lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass,  string lpszWindow);





        public static Rectangle GetClientRect(IntPtr hWnd)
        {
            RECT rc;
            GetClientRect(hWnd, out rc);

            return new Rectangle(rc.Left, rc.Top, rc.Right - rc.Left, rc.Bottom - rc.Top);
        }

        public static void ResizeClientRectTo(IntPtr hWnd, Rectangle desired)
        {
            RECT size;
            size.Top = desired.Top;
            size.Left = desired.Left;
            size.Bottom = desired.Bottom;
            size.Right = desired.Right;

            var style = GetWindowLong(hWnd, GWL_STYLE);
            AdjustWindowRect(ref size, style, false);
            MoveWindow(hWnd, size.Left, size.Top, size.Right - size.Left, size.Bottom - size.Top, true);
        }



        
       

    public  static Process MainProcess =  Process.GetCurrentProcess();
    public static void KillProcessAndChildrens(int pid){

     try{

            Process proc = Process.GetProcessById(pid);

  //  MessageBox.Show("Kil: "+  proc.MainWindowTitle);

           // if(proc.Id != nDontKillId) {

        ManagementObjectSearcher searcher = new ManagementObjectSearcher
            ("Select * From Win32_Process Where ParentProcessID=" + pid);
        ManagementObjectCollection moc = searcher.Get();
        foreach (ManagementObject mo in moc)
        {
            KillProcessAndChildrens(Convert.ToInt32(mo["ProcessID"]));
        }
   
               //     MessageBox.Show("Kil: "+  proc.MainWindowTitle);
              //Program.fDebug("Kil: "+  proc.);
				if(proc.Id != MainProcess.Id  ) {
                        proc.Kill();
				}
//proc.CloseMainWindow();
          //  }


        }
        catch (Exception ex)
        {
            // Process already exited.
        }
     }


    }
 

}
