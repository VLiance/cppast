using ProjectManager.Projects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CwideContext
{
    class ProjectArg : FileUtils
    {

       // private  string sTab = "";
       // StreamWriter writer;
        Project oProject;
		string sDefaultArg = "";
		string sListDir = "";
		string sLinker = "";

        public ProjectArg(Project _oProject ,string _sGroupList, string _sProjectName,  string _sOutputDir, string _sOutBranch, List<string> _aFileList, List<string> _aIncludeDir, string _sPlatform, string _sOpLibList)
        {

/*
			oProject = _oProject;
			
            string _sPrjFile = _sOutputDir + _sOutBranch +_sProjectName + ".args";
            //CompileCpp.fCreateDirectoryRecursively(Path.GetDirectoryName(_sPrjFile));

			fIniFile(_sPrjFile);



			string[] _aCompilerArg = CompileCpp.sCompilerFlag.Split(new string[] { " -" }, StringSplitOptions.RemoveEmptyEntries);
			foreach(string _sArg in _aCompilerArg) {	
				  sDefaultArg += "-" + _sArg + " ";
			}


			sListDir += "-I . "; //Current dir
            foreach (string _sDir in _aIncludeDir){
				sListDir += "-I " + _sDir + " ";
            }
			
			

			string[] _aLinkerArg = CompileCpp.sLinkerFlag.Split(new string[] { " -" }, StringSplitOptions.RemoveEmptyEntries);
			foreach(string _sArg in _aLinkerArg) {
				  sLinker += "-" + _sArg + " ";
			}

			fAdd(sDefaultArg + sListDir);
				
			fClose();

*/
        }

/*
        public void fSubTab() {
            sTab = sTab.Substring(0, sTab.Length - 1);
        }
        public void fAddTab()  {
            sTab += '\t';
        }

        public void fAdd(string _sLine){
            writer.WriteLine(sTab + _sLine );
        } */

    }
}
