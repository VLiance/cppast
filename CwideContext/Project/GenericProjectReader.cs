namespace CwideContext.Projects
{
    public class GenericProjectReader : ProjectManager.Projects.ProjectReader
    {
        CwProject project;

        public GenericProjectReader(string filename)
            : base(filename, new CwProject(filename))
        {
            this.project = base.Project as CwProject;
        }

        public new CwProject ReadProject()
        {
            return base.ReadProject() as CwProject;
        }
    }
}
