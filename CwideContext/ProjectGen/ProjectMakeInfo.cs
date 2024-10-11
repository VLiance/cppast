using ProjectManager.Projects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CwideContext
{
    class ProjectMakeInfo : FileUtils
    {

       // private  string sTab = "";
       // StreamWriter writer;
        Project oProject;
		string sDefaultArg = "";
		string sListDir = "";
		string sLinker = "";

        public ProjectMakeInfo(Project _oProject ,string _sGroupList, string _sProjectName,  string _sOutputDir, string _sOutBranch, List<string> _aFileList, List<string> _aIncludeDir, string _sPlatform, string _sOpLibList)
        {
			oProject = _oProject;
			
			
            
            string _sPrjFile = _sOutputDir + _sOutBranch + "_" +_sProjectName + "_HowToBuild.txt";
            //CompileCpp.fCreateDirectoryRecursively(Path.GetDirectoryName(_sPrjFile));

			fIniFile(_sPrjFile);


            //this code segment write data to file.
           // FileStream fs1 = new FileStream(_sPrjFile, FileMode.OpenOrCreate, FileAccess.Write);
           // writer = new StreamWriter(fs1);


            fAdd("//How to compile " + _sProjectName);
			fAdd("//Open Command Prompt in this folder (cmd.exe)");
			fAdd("//Put your compilater 'bin' folder in the 'Path' of your 'Environement Variables', here we use mingw32, but you can replace it by anything");
			fAdd("//Enter these commands (Copy/Paste all to cmd.exe):");
			fAdd("//---- Only one file method ----");
			
			fAdd("mingw32-g++.exe " + "@" + _sProjectName + ".args -x c++ -c " +  _sProjectName + "_BuildAll.cpp " +  " -o " + _sProjectName + ".o");
			fAdd("mingw32-g++.exe " + "-o " +_sProjectName  + ".exe " + _sProjectName + ".o" +  sLinker);

			fAdd("");
			fAdd("//----- Grouped file method ----");
			/*
            sDefaultArg += "-march=pentium ";
            sDefaultArg += "-std=c++11 ";
            sDefaultArg += "-g ";
            sDefaultArg += "-Wreturn-type ";
            sDefaultArg += "-DGZ_tNo_FreeType ";
            sDefaultArg += "-DGZ_t" + _sPlatform + " ";
            sDefaultArg += "-DGZ_tOverplace=\\\"[" + _sOpLibList + "]\\\" ";
			sDefaultArg += "-DGZ_tTakeEmbedRcOnDrive ";
			sDefaultArg += "-DGZ_tMonothread ";
			sDefaultArg += "-DSTBI_NO_STDIO ";

			sListDir += "-I . "; //Current dir
            foreach (string _sDir in _aIncludeDir){
				sListDir += "-I " + _sDir + " ";
            }
			
			if(_sPlatform == "Windows") {
				sLinker += "-lgdi32 ";
			}*/

			int nCount = 0;
			string _sLinkFiles = "";
			string[] _aGroupList = _sGroupList.Split('?');
			foreach (string _sCompileGroup in _aGroupList)  {
				nCount ++;
				if (_sCompileGroup != "") {
					//fAdd("mingw32-g++.exe " + sDefaultArg + sListDir + "-x c++ -c " +  _sCompileGroup +  " -o " + nCount.ToString() + ".o");
					fAdd("mingw32-g++.exe " + "@" + _sProjectName + ".args -x c++ -c " +  _sCompileGroup +  " -o " + nCount.ToString() + ".o");
					_sLinkFiles += nCount.ToString() + ".o ";
				}
			}

			fAdd("mingw32-g++.exe " + "-o " +_sProjectName  + ".exe " + _sLinkFiles +  sLinker);
			fAdd("");
				
			fClose();
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
