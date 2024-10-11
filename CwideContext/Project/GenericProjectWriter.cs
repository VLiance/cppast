namespace CwideContext.Projects
{
    public class GenericProjectWriter :  ProjectManager.Projects.ProjectWriter
    {
        CwProject project;

        public GenericProjectWriter(CwProject project, string filename)
            : base(project, filename)
        {
            this.project = base.Project as CwProject;
        }
    }
}
