using Microsoft.CodeAnalysis;

namespace CleanAnalysis
{
    public class ProjectMetrics
    {
        public ProjectMetrics(Project project, Metrics metrics)
        {
            Project = project;
            Metrics = metrics;
        }

        public Project Project { get; set; }
        public Metrics Metrics { get; set; }
    }
}
