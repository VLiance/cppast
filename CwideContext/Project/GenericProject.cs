using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using ProjectManager.Projects;
using ProjectManager.Controls;
using ProjectManager.Controls.AS3;
using PluginCore;

namespace CwideContext.Projects
{
    public class CwProject : ProjectManager.Projects.Project {
		

        public CwProject(string path) : base(path, new GenericOptions())
        {
//MyGenProject
            movieOptions = new GenericMovieOptions();
         var platform = MovieOptions.PlatformSupport;
    /*
 MovieOptions.TargetBuildTypes = platform.Targets;

// MovieOptions.TargetBuildTypes = new string["aa","bb"];
  string[] weekDays = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
 MovieOptions.TargetBuildTypes = weekDays;
*/
 MovieOptions.TargetBuildTypes =  MovieOptions.TargetPlatforms;

    //        EnableTargetBuildSelector(false);

            

/*
            project.TestMovieBehavior = TestMovieBehavior.Custom;
            project.TestMovieCommand = "bat\\RunApp.bat";
*/
/*
PluginCore.IMainForm
mainForm.ToolStrip
*/


// MovieOptions.TargetPlatforms = weekDays;

	//fCreateComboBox();

     }






        public override string Language { get { return "Cpp"; } }
        public override string LanguageDisplayName { get { return "Cpp"; } }
        public override bool IsCompilable { get { return true; } }
       // public override bool ReadOnly { get { return FileInspector.IsFlexBuilderProject(ProjectPath); } }
        public override bool HasLibraries { get { return OutputType == OutputType.Application || OutputType == OutputType.Library; } }
        public override int MaxTargetsCount { get { return 1; } }
    //    public override string DefaultSearchFilter { get { return "*.as;*.mxml"; } }

 //       public override string Language { get { return "*"; } }
     //   public override string LanguageDisplayName { get { return "*"; } }
     //   public override bool IsCompilable { get { return false; } }


   public override PropertiesDialog CreatePropertiesDialog()
        {
            return new AS3PropertiesDialog();
        }


    public override void ValidateBuild(out string error)
    {
        if (CompileTargets.Count == 0) error = "Description.MissingEntryPoint";
        else error = null;
    }


        public override string DefaultSearchFilter
        {
            get
            {
                if (OutputType == OutputType.Website) return "*.html;*.css;*.js";
                else return "*.*";
            }
        }

        public override string GetInsertFileText(string inFile, string path, string export, string nodeType)
        {
            String inPath = Path.GetDirectoryName(inFile);
            return ProjectPaths.GetRelativePath(inPath, path);
        }

	   public override string GetOtherIDE(bool runOutput, bool releaseMode, out string error)
        {
            error = null;
            return "Cwc";
        }


        #region Load/Save

        public static CwProject Load(string path)
        {
            GenericProjectReader reader = new GenericProjectReader(path);
            try
            {
                return reader.ReadProject();
            }
            catch (XmlException exception)
            {
                string format = string.Format("Error in XML Document line {0}, position {1}.", exception.LineNumber, exception.LinePosition);
                throw new Exception(format, exception);
            }
            finally { reader.Close(); }
        }

        public override void Save()
        {
            SaveAs(ProjectPath);
        }

        public override void SaveAs(string fileName)
        {
            if (!AllowedSaving(fileName)) return;
            try
            {
                GenericProjectWriter writer = new GenericProjectWriter(this, fileName);
                writer.WriteProject();
                writer.Flush();
                writer.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "IO Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    
    }

}

