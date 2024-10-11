using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Diagnostics;
using ProjectManager.Projects;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ProjectManager.Projects.AS3;
using PluginCore.Managers;
using ProjectManager.Controls.TreeView;
using ProjectManager.Controls;

namespace CwideContext
{
   
    public class LibPathNode : ProjectManager.Controls.TreeView.WatcherNode
    {
        public string classpath;
        public CwLib oLib;
      
        /*
        public class LibNode : LibPathNode
        {
            public LibNode(Project project, string classpath, string text, CwLib _oLib)
                : base(project, classpath, text)
            {
                oLib = _oLib;

                if (text != Text)
                {
                    ToolTipText = text;
                }
                else
                {
                    ToolTipText = "";
                }
            }

            public override void Refresh(bool recursive)
            {
                base.Refresh(recursive);

                if (!IsInvalid)
                {
                    ImageIndex = Icons.ProjectClasspath.Index;
                }
                else
                {
                    ImageIndex = Icons.ProjectClasspathError.Index;
                }

                SelectedImageIndex = ImageIndex;

                NotifyRefresh();
            }
        }*/



        public LibPathNode(Project project, string classpath, string text, CwLib _oLib)
            : base(classpath)
        {
            oLib = _oLib;
            isDraggable = false;
            isRenamable = false;

            this.classpath = classpath;
				

		
            // shorten text
            string[] excludes = ProjectManager.PluginMain.Settings.FilteredDirectoryNames;
            char sep = Path.DirectorySeparatorChar;
            string[] parts = text.Split(sep);
            List<string> label = new List<string>();
            Regex reVersion = new Regex("^[0-9]+[.,-][0-9]+");

            if (parts.Length > 0)
            {
                for (int i = parts.Length - 1; i > 0; --i)
                {
                    String part = parts[i] as String;
                    if (part != "" && part != "." && part != ".." && Array.IndexOf(excludes, part.ToLower()) == -1)
                    {
                        if (Char.IsDigit(part[0]) && reVersion.IsMatch(part)) label.Add(part);
                        else
                        {
                            label.Add(part);
                            break;
                        }
                    }
                    else label.Add(part);
                }
            }
            label.Reverse();
            //Text = String.Join("/", label.ToArray());
            ToolTipText = classpath;

            if (oLib != null)
            {
            Text = oLib.sIdName;
            }
        }

        public override void Refresh(bool recursive)
        {
            base.Refresh(recursive);

            base.isInvalid = !Directory.Exists(BackingPath);

            if (!isInvalid)
            {
                ImageIndex = Icons.Classpath.Index;
            }
            else
            {
                ImageIndex = Icons.ClasspathError.Index;
            }

            SelectedImageIndex = ImageIndex;

            NotifyRefresh();
        }
    }


    public class GroupLibNode : GenericNode
    {
        public GroupLibNode(Project _project, string text)
            : base(text)
            // : base(Path.Combine(project.Directory, "__References__"))
         //  : base(Path.Combine(_project.Directory, "__References__"))
        {
            project = _project;
            Text = text;
           // ImageIndex = SelectedImageIndex = Icons.HiddenFolder.Index;
            ImageIndex = SelectedImageIndex = Icons.Classpath.Index;
            isDraggable = false;
            isRenamable = false;
            ToolTipText = text;
        }
        /*
        public override void Refresh(bool recursive)
        { 
        }*/

    }



    public class LibReferencesNode : GenericNode
    {
        public  ProjectManager.Controls.TreeView.ProjectNode projectNode;
        public LibPathNode oCurrlibNode;

        public LibReferencesNode(Project project, string text,  ProjectManager.Controls.TreeView.ProjectNode _projectNode)
            : base(Path.Combine(project.Directory, "__References__"))
        {
            projectNode = _projectNode;
            Text = text;
           // ImageIndex = SelectedImageIndex = Icons.HiddenFolder.Index;
            ImageIndex = SelectedImageIndex = Icons.ClasspathFolder.Index;
            isDraggable = false;
            isRenamable = false;
        }

        //Cw
        public override void Refresh(bool recursive)
        {

            base.Refresh(recursive);
            TraceManager.Add("--CP--Update");
            CwLibGroup _oProjLibGroup = null;
/* //TODO CW
            ////Just  get first item
            foreach (CwLibGroup _oProjLibGroupEach in project.aLib)
            {
                _oProjLibGroup = _oProjLibGroupEach;
                break;
            }

*/
            if (_oProjLibGroup != null)
            {
                fAddLibGroup(projectNode, _oProjLibGroup, true);
                projectNode.Expand();

            }
            else
            {
/*
			   fAddLibGroup(projectNode, _oProjLibGroup, true);
                projectNode.Expand();
                GenericNode oProjLib = new GroupLibNode(project, "Loading...");
                projectNode.Nodes.Insert(0, oProjLib);
				projectNode.Nodes
*/

				 GroupLibNode  SubNode = new GroupLibNode(project, "Loading...");
                 projectNode.Nodes.Add( SubNode);
			
		
            }

            ArrayList projectClasspaths = new ArrayList();
            ArrayList globalClasspaths = new ArrayList();

            GenericNodeList nodesToDie = new GenericNodeList();
            foreach (GenericNode oldRef in Nodes) nodesToDie.Add(oldRef);
            //if (Nodes.Count == 0) recursive = true;

            // explore classpaths
            if (ProjectManager.PluginMain.Settings.ShowProjectClasspaths)
            {
                projectClasspaths.AddRange(project.Classpaths);
                if (project.AdditionalPaths != null) projectClasspaths.AddRange(project.AdditionalPaths);
            }
            projectClasspaths.Sort();

            if (ProjectManager.PluginMain.Settings.ShowGlobalClasspaths)
                globalClasspaths.AddRange(ProjectManager.PluginMain.Settings.GlobalClasspaths);
            globalClasspaths.Sort();


/* //TODO CW
            //////////// Cw //////////////////
            bool _bFirst = true;
            foreach (CwLibGroup _oLibGroup in project.aLib)
            {
                if (!_bFirst) {
                    fAddLibGroup(this, _oLibGroup); 
				 }
                _bFirst = false;
            }
*/

   

            //////////////////////////////////
            /*
            foreach (string globalClasspath in globalClasspaths)
            {
                string absolute = globalClasspath;
                if (!Path.IsPathRooted(absolute))
                    absolute = project.GetAbsolutePath(globalClasspath);
                if (absolute.StartsWith(project.Directory + Path.DirectorySeparatorChar.ToString()))
                    continue;

                cpNode = ReuseNode(absolute, nodesToDie) as ProjectClasspathNode ?? new ClasspathNode(project, absolute, globalClasspath);
                Nodes.Add(cpNode);
                cpNode.Refresh(recursive);
            }*/
            /*
            // add external libraries at the top level also
            if (project is AS3Project)
                foreach (LibraryAsset asset in (project as AS3Project).SwcLibraries)
                {
                    if (!asset.IsSwc) continue;
                    // check if SWC is inside the project or inside a classpath
                    string absolute = asset.Path;
                    if (!Path.IsPathRooted(absolute))
                        absolute = project.GetAbsolutePath(asset.Path);

                    bool showNode = true;
                    if (absolute.StartsWith(project.Directory))
                        showNode = false;
                    foreach (string path in project.AbsoluteClasspaths)
                        if (absolute.StartsWith(path))
                        {
                            showNode = false;
                            break;
                        }
                    foreach (string path in PluginMain.Settings.GlobalClasspaths)
                        if (absolute.StartsWith(path))
                        {
                            showNode = false;
                            break;
                        }

                    if (showNode && !project.ShowHiddenPaths && project.IsPathHidden(absolute))
                        continue;

                    if (showNode && File.Exists(absolute))
                    {
                        SwfFileNode swcNode = ReuseNode(absolute, nodesToDie) as SwfFileNode ?? new SwfFileNode(absolute);
                        Nodes.Add(swcNode);
                        swcNode.Refresh(recursive);
                    }
                }
            */

            foreach (GenericNode node in nodesToDie)
            {
                node.Dispose();
                Nodes.Remove(node);
            }

        }


        private void fAddLibGroup(GenericNode libRootNode, CwLibGroup _oLibGroup, bool _bFirst = false)
        {
            bool bVirtual = false;


            if (_oLibGroup.aLib.Count != 1) //Create a gorup of subgroup
            {
//here
                bVirtual = true;
               GroupLibNode  SubNode = new GroupLibNode(project, _oLibGroup.sName);
                if (_bFirst)
                {
                    libRootNode.Nodes.Insert(0, SubNode);
                }
                else
                {
                    libRootNode.Nodes.Add(SubNode);
                }

				libRootNode = SubNode;
			
					
            }


            foreach (CwLib _oLib in _oLibGroup.aLib)
            {
		
                fAddLibNode(libRootNode, _oLib, bVirtual, _bFirst);
            }
        }


        private void fAddLibNode(GenericNode libRootNode, CwLib _oLib, bool _bVirtual, bool _bFirst = false)
        {
			string _sGroupReadPath = "";
			if( _oLib.oGroup != null) {
				_sGroupReadPath +=  _oLib.oGroup.sReadPath;
			}
			else {
				TraceManager.Add("-- Errror No absolute path on  : " + _oLib.sReadPath);
			}
            string absolute = _sGroupReadPath + _oLib.sReadPath; //TODO multiple group?
			absolute = absolute.Replace('/','\\');

//Console.WriteLine("--absolute:" + absolute);

            if (!Path.IsPathRooted(absolute)) { //Always absolute?
                absolute = project.GetAbsolutePath(_oLib.sReadPath);
			}

			
           /*
            if ((absolute + "\\").StartsWith(project.Directory + "\\"))
                continue;
            if (!project.ShowHiddenPaths && project.IsPathHidden(absolute))
                continue;
            */
            LibPathNode libNode = new LibPathNode(project, absolute, _oLib.sReadPath.Replace('/','\\'), _oLib);
            oCurrlibNode = libNode;

            if (_bFirst)
            {
                libRootNode.Nodes.Insert(0, libNode);
				libRootNode.Expand(); //Expand main nodes TODO better way?
            }
            else
            {
                libRootNode.Nodes.Add(libNode);
            }

		   libNode.Refresh(true);

            if (_bVirtual)
            {
			//   TraceManager.Add("-----Virtual : " + absolute);
                libNode.ImageIndex = libNode.SelectedImageIndex = ProjectManager.Controls.Icons.HiddenFolder.Index;
            }

			     
        }


        private GenericNode ReuseNode(string absolute, GenericNodeList nodesToDie)
        {
            foreach (GenericNode node in nodesToDie)
                if (node.BackingPath == absolute)
                {
                    nodesToDie.Remove(node);
                    Nodes.Remove(node);
                    return node;
                }
            return null;
        }
    }

   
  
}
