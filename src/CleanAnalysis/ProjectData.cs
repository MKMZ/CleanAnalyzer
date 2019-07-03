using Microsoft.CodeAnalysis;

namespace CleanAnalysis
{
    public class ProjectData
    {
        public ProjectData(Project info, Compilation compilation)
        {
            Info = info;
            Compilation = compilation;
        }

        public Project Info { get; }
        public Compilation Compilation { get; }
    }
}
