using ProjectManager.Projects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CwideContext
{
    class ProjectBuildAll : FileUtils
    {

       // private  string sTab = "";
       // StreamWriter writer;
        Project oProject;


        public ProjectBuildAll(Project _oProject ,string _sGroupList, string _sProjectName,  string _sOutputDir, string _sOutBranch, List<string> _aFileList, List<string> _aIncludeDir, string _sPlatform, string _sOpLibList)
        {
			oProject = _oProject;
            
            string _sPrjFile = _sOutputDir + _sOutBranch + _sProjectName + "_BuildAll.cpp";
            //CompileCpp.fCreateDirectoryRecursively(Path.GetDirectoryName(_sPrjFile));

			fIniFile(_sPrjFile);

			int nCount = 0;
			string[] _aGroupList = _sGroupList.Split('?');
			foreach (string _sCompileGroup in _aGroupList)  {
				nCount ++;
				if (_sCompileGroup != "") {
					fAdd("#include \"" +  _sCompileGroup +  "\"");
				}
			}

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
