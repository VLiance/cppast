using ProjectManager.Projects;

namespace CwideContext.Projects
{
    public class GenericMovieOptions : ProjectManager.Projects.MovieOptions
    {

       public string sVersion = "Default";
		  public GenericMovieOptions()
			{

				Language = "cpp";
				MajorVersion = 11;
				Platform = TargetPlatforms[0];
				
			}

		


/*
        public const string DEFAULT = "Default";

        public override string[] TargetPlatforms
        {
            get { return new string[] { DEFAULT }; }
        }

        public override string[] TargetVersions(string platforgm)
        {
            return new string[] { "1.0" };
        }

        public override string DefaultVersion(string platform)
        {
            return "1.0";
        }
*/


       public override string Version 
        { 
            get { return sVersion; }
            set
            {
				if(value == "0.0") {
					sVersion ="Default";
				}else {
					sVersion = value;
				}
			}
        }


        public override OutputType[] OutputTypes
        {

			  //  get { return new OutputType[] {   OutputType.OtherIDE, OutputType.CustomBuild, OutputType.Application, OutputType.Website, OutputType.CustomBuild }; }
				  get { 
						PluginMain.oContext.Settings.GetDefaultSDK(); //ReSet default Sdk if not set

						return new OutputType[] {   OutputType.OtherIDE };
				}

        }

        public override OutputType DefaultOutput(string platform)
        {
            return OutputType.OtherIDE;
        }

        public override bool IsGraphical(string platform)
        {
            return true;
        }

        public override bool DebuggerSupported(string targetBuild)
        {
            return true;
        }
    }
}
