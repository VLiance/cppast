using Microsoft.Win32;
using PluginCore.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CwideContext
{
    class RegisterFile {


        static string sExeCwc_Name = "Cwc.";

        static string sExeIDE =  PathHelper.AppDir + "\\FlashDevelop.exe";
        static string sExeIDE_Name = "FlashDevelop.CWide.";

         public static string  fCwcRoot()  {
            return PluginMain.CwcRootPath() + "\\";
        }


        public static string  fCwcExe()  {
                return fCwcRoot() + "cwc.exe";
       }
       public static string  fIconPath(string _sExt)  {
                return fCwcRoot() + "Tools\\icon\\"+ _sExt + "File.ico";
       }

       public static void fRegisterAllFileType( bool  _bForce = false)  {
             fRegisterFileType(sExeIDE,sExeIDE_Name, "c",  "C src file",_bForce);
             fRegisterFileType(sExeIDE,sExeIDE_Name,"cpp", "C++ src file",_bForce);
             fRegisterFileType(sExeIDE,sExeIDE_Name, "h",  "Header scr file",_bForce);
             fRegisterFileType(sExeIDE,sExeIDE_Name, "hh", "Header of Header src file",_bForce);
             fRegisterFileType(fCwcExe(),sExeCwc_Name, "cwMake", "Header of Header src file",_bForce);
             fRegisterFileType(sExeIDE,sExeCwc_Name, "cwc", "Header of Header src file",_bForce);
             //   fRefreshTree(); //Update ICONs
        }

        public static void fRegisterFileType(string _sExePath,string _sNameExe, string _sExt, string _sDesc, bool  _bForce = false) {
            string _sIconPath = fIconPath(_sExt);
            if(File.Exists(_sIconPath)) {
                if (_bForce || !IsAssociated("." + _sExt)) {
                    //string _sExePath = PathHelper.AppDir + "\\FlashDevelop.exe";
                    Associate("." + _sExt, _sNameExe + _sExt,_sDesc,_sIconPath, _sExePath);
                }
            }
        }



        // Associate file extension with progID, description, icon and application
        public static void Associate(string extension,  string progID, string description, string icon, string application)
        {
            Registry.ClassesRoot.CreateSubKey(extension).SetValue("", progID);
            if (progID != null && progID.Length > 0)
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(progID))
                {
                    if (description != null)
                        key.SetValue("", description);
                    if (icon != null)
                        key.CreateSubKey("DefaultIcon").SetValue("", ToShortPathName(icon));
                    if (application != null)
                        key.CreateSubKey(@"Shell\Open\Command").SetValue("",  ToShortPathName(application) + " \"%1\" %*");
                }
        }

        // Return true if extension already associated in registry
        public static bool IsAssociated(string extension){
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(extension, false);
            if (key != null) {

                    string _sVal = (string)key.GetValue("");
                    if(_sVal == null || _sVal.Length == 0) {return false; }
                   return true;
            }
             return false;
        }
 
        [DllImport("Kernel32.dll")]
        private static extern uint GetShortPathName(string lpszLongPath,  [Out] StringBuilder lpszShortPath, uint cchBuffer);
 
        // Return short path format of a file name
        private static string ToShortPathName(string longName)
        {
            StringBuilder s = new StringBuilder(1000);
            uint iSize = (uint)s.Capacity;
            uint iRet = GetShortPathName(longName, s, iSize);
            return s.ToString();
        }

    }
}
