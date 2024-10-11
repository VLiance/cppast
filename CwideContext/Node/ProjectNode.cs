using ASCompletion;
using ASCompletion.Context;
using ASCompletion.Model;
using PluginCore.Localization;
using PluginCore.Managers;
using ProjectManager.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CwideContext
{
	public class ProjectNode
	{
	       public Project oProject;
		public ProjectNode node; //NX2 oCurrProject
		 public List<CwLibGroup> aLib = new List<CwLibGroup>(); //NX2 oCurrProject

   public CwideMemberModel[] aImportsIndex;

		  private FixedTreeView outlineTree;
		 private string prevChecksum;
        private TreeNode currentHighlight;
		private ToolStripSpringTextBox findProcTxt;

		 public const int ICON_FILE = 0;
        public const int ICON_FOLDER_CLOSED = 1;
        public const int ICON_FOLDER_OPEN = 2;
        public const int ICON_CHECK_SYNTAX = 3;
        public const int ICON_QUICK_BUILD = 4;
        public const int ICON_PACKAGE = 5;
        public const int ICON_INTERFACE = 6;
        public const int ICON_INTRINSIC_TYPE = 7;
        public const int ICON_TYPE = 8;
        public const int ICON_VAR = 9;
        public const int ICON_PROTECTED_VAR = 10;
        public const int ICON_PRIVATE_VAR = 11;
        public const int ICON_STATIC_VAR = 12;
        public const int ICON_STATIC_PROTECTED_VAR = 13;
        public const int ICON_STATIC_PRIVATE_VAR = 14;
        public const int ICON_CONST = 15;
        public const int ICON_PROTECTED_CONST = 16;
        public const int ICON_PRIVATE_CONST = 17;
        public const int ICON_STATIC_CONST = 18;
        public const int ICON_STATIC_PROTECTED_CONST = 19;
        public const int ICON_STATIC_PRIVATE_CONST = 20;
        public const int ICON_FUNCTION = 21;
        public const int ICON_PROTECTED_FUNCTION = 22;
        public const int ICON_PRIVATE_FUNCTION = 23;
        public const int ICON_STATIC_FUNCTION = 24;
        public const int ICON_STATIC_PROTECTED_FUNCTION = 25;
        public const int ICON_STATIC_PRIVATE_FUNCTION = 26;
        public const int ICON_PROPERTY = 27;
        public const int ICON_PROTECTED_PROPERTY = 28;
        public const int ICON_PRIVATE_PROPERTY = 29;
        public const int ICON_STATIC_PROPERTY = 30;
        public const int ICON_STATIC_PROTECTED_PROPERTY = 31;
        public const int ICON_STATIC_PRIVATE_PROPERTY = 32;
        public const int ICON_TEMPLATE = 33;
        public const int ICON_DECLARATION = 34;


		public Context oContext;
		public ProjectNode(Context _oContext) {
				oContext = _oContext;
		}


//Refer to E:\_Project\_MyPluginFlashDevelop\flashdevelop-development\External\Plugins\ASCompletion\PluginUI.cs

        public void RefreshView(FileModel aFile){

			outlineTree =  PluginMain.oASCompletion.Panel.OutlineTree;
           if(outlineTree == null || !outlineTree.Visible || outlineTree.Width == 0 || outlineTree.Height == 0 ) { //Not vidsible
                return;
            }

            //TraceManager.Add("Outline refresh...");
              outlineTree.BeginStatefulUpdate();
           
 // Thread thread = new Thread(new ThreadStart(() =>  {  



		//OutlineTree 
/*
            if (prevChecksum.StartsWithOrdinal(aFile.FileName))
                aFile.OutlineState = outlineTree.State;
*/
            try
            {
                currentHighlight = null;
                outlineTree.Nodes.Clear();
/*
                // If text == "" then the field has the focus and it's already empty, no need to dispatch unneeded events
                if (findProcTxt.Text != searchInvitation && findProcTxt.Text != string.Empty)
                {
                    findProcTxt.Clear();
                    FindProcTxtLeave(null, null);
                }*/

                TreeNode root = new TreeNode(System.IO.Path.GetFileName(aFile.FileName), ICON_FILE, ICON_FILE);
                outlineTree.Nodes.Add(root);
                if (aFile == FileModel.Ignore)
                    return;

                TreeNodeCollection folders = root.Nodes;
                TreeNodeCollection nodes;
                TreeNode node;
                int img;


                // imports
               // if (oContext.Settings.ShowImports && aFile.Imports.Count > 0){
                if (aFile.Imports.Count > 0){


                    node = new TreeNode("Includes", ICON_FOLDER_OPEN, ICON_FOLDER_OPEN);
                   // node.Text = "Includes";
                    folders.Add(node);
                    nodes = node.Nodes;
                    foreach (MemberModel import in aFile.Imports) {

/*
                        if (import.Type.EndsWithOrdinal(".*"))
                            nodes.Add(new TreeNode(import.Type, ICON_PACKAGE, ICON_PACKAGE));
                        else
                        {*/
                            img = ASCompletion.PluginUI.GetIcon(import.Flags, import.Access); 
                            //((import.Flags & FlagType.Intrinsic) > 0) ? ICON_INTRINSIC_TYPE : ICON_TYPE;
                           // node = new TreeNode(import.Type, img, img);
                            node = new CWideMemberTreeNode((CwideMemberModel)import, img);
                        

                            node.Tag = "import";
                            nodes.Add(node);
                      //  }
                    }
                }

                // class members
                if (aFile.Members.Count > 0)
                {
                    AddMembersSorted(folders, aFile.Members);
                }

                // regions
                if (PluginMain.oASCompletion.PluginSettings.ShowRegions)
                {
                    if (aFile.Regions.Count > 0)
                    {
                        node = new TreeNode(TextHelper.GetString("Info.RegionsNode"), ICON_PACKAGE, ICON_PACKAGE);
                        folders.Add(node);
                        //AddRegions(node.Nodes, aFile.Regions);
 //                       AddRegionsExtended(node.Nodes, aFile);
                    }
                }

                // classes
                if (aFile.Classes.Count > 0)
                {
                    nodes = folders;

                    foreach (ClassModel aClass in aFile.Classes)
                    {
                        img =  ASCompletion.PluginUI.GetIcon(aClass.Flags, aClass.Access);
                      //  node = new TreeNode(aClass.FullName, img, img);
                        node = new TreeNode(aClass.FullName, img, img);
                        
                        node.Tag = "class";
                        nodes.Add(node);
      //                  if (PluginMain.oASCompletion.PluginSettings.ShowExtends) AddExtend(node.Nodes, aClass);
    //                    if (PluginMain.oASCompletion.PluginSettings.ShowImplements) AddImplements(node.Nodes, aClass.Implements);
                        AddMembersSorted(node.Nodes, aClass.Members);
                        node.Expand();
                    }
                }

                root.Expand();
            }
            catch (Exception ex)
            {
  //              ErrorManager.ShowError(/*ex.Message,*/ ex);
            }


      /*
		}));  thread.Start();
 Thread.Sleep(10);
            while(thread.IsAlive) {
                Thread.Sleep(1);
            }*/

       //     finally {
     
			

                // outline state will be restored/saved from the model data
                if (aFile.OutlineState == null)
                    aFile.OutlineState = new TreeState();
                // restore collapsing state
                outlineTree.EndStatefulUpdate(aFile.OutlineState);
                // restore highlighted item
                if (aFile.OutlineState.highlight != null)
                {
                    TreeNode toHighligh = outlineTree.FindClosestPath(outlineTree.State.highlight);
                    if (toHighligh != null) {
    //                    SetHighlight(toHighligh);
                    }else {
   //                     Highlight(ASContext.Context.CurrentClass, ASContext.Context.CurrentMember);
					}
                }




          //  }

        }



		 public class  CwideMemberModel: MemberModel {
			public CwideMemberModel oLinkedMember;
			public uint nIndex;
			public CwideMemberModel():base() {
			}
			 public CwideMemberModel(string name, string type, FlagType flags, Visibility access):base(name, type, flags, access ) {
	
				}


		}

 public class CWideMemberTreeNode : MemberTreeNode
    {
			//public MemberModel oMember;
			public CwideMemberModel oMember;
        public CWideMemberTreeNode(CwideMemberModel member, int imageIndex)
            : base(member, imageIndex)
        {
           oMember = member;
        }
    }


        /// <summary>
        /// Add tree nodes following the user defined members presentation
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="members"></param>
        static public void AddMembers(TreeNodeCollection tree, MemberList members)
        {

            TreeNodeCollection nodes = tree;
            MemberTreeNode node = null;
            int img;
            foreach (MemberModel member in members)
            {
                img =  ASCompletion.PluginUI.GetIcon(member.Flags, member.Access);
                node = new CWideMemberTreeNode((CwideMemberModel)member, img);
                nodes.Add(node);
            }
        }




	   private void AddMembersSorted(TreeNodeCollection tree, MemberList members)
			{/*
				if (settings.SortingMode == OutlineSorting.None)
				{*/
				//ASCompletion.PluginUI.AddMembers(tree, members);
				AddMembers(tree, members);
/*
				}
				else if (settings.SortingMode == OutlineSorting.SortedGroup)
				{
					AddMembersGrouped(tree, members);
				}
				else
				{
					IComparer<MemberModel> comparer = null;
					if (settings.SortingMode == OutlineSorting.Sorted)
						comparer = null;
					else if (settings.SortingMode == OutlineSorting.SortedByKind)
						comparer = new ByKindMemberComparer();
					else if (settings.SortingMode == OutlineSorting.SortedSmart)
						comparer = new SmartMemberComparer();
					else if (settings.SortingMode == OutlineSorting.SortedGroup)
						comparer = new ByKindMemberComparer();

					MemberList copy = new MemberList();
					copy.Add(members);
					copy.Sort(comparer);
					AddMembers(tree, copy);
				}*/
			}



        public System.Drawing.Image GetIcon(int index)
        {

            if (PluginMain.oASCompletion.Panel.TreeIcons.Images.Count > 0)
                return PluginMain.oASCompletion.Panel.TreeIcons.Images[Math.Min(index, PluginMain.oASCompletion.Panel.TreeIcons.Images.Count)];
            else return null;
        }

		
/*
//CW TODO
        public void UpdateProjectNode()
        {

			SelectedNodes = null;
            Nodes.Clear();
            nodeMap.Clear();



            if (projects.Count == 0)
                return;

            foreach (Project project in projects) {
			    activeProject = project;
				Nodes.Add(project.node); //CW//CW
                project.node.Refresh(true);

			//LibReferencesNode PRefs = new LibReferencesNode(project, "Lib",  project.node);
             //  project.node.References = PRefs;

             project.node.References.Expand();
               project.node.Expand();

			}
// Refresh();
        }
*/




/*
        public void RebuildProjectNode(Project project)
        {

            activeProject = project;
        

//OKGOOD
								// create the top-level project node
								ProjectNode projectNode = new ProjectNode(project);
								project.node = projectNode;

								 Nodes.Add(projectNode);
								  projectNode.Refresh(true);

								  LibReferencesNode PRefs = new LibReferencesNode(project, "Lib", projectNode);
								  projectNode.References = PRefs;
								  PRefs.Expand();
								  projectNode.Expand();






        }
*/



	}
}
