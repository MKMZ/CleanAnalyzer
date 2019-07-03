using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace CleanAnalysis
{
    public class AnalysisDataContainer
    {
        public AnalysisDataContainer(IImmutableList<Solution> solutions, IImmutableList<ProjectData> projectDatas)
        {
            Solutions = solutions;
            Projects = projectDatas;
        }

        public IImmutableList<Solution> Solutions { get; }
        public IImmutableList<ProjectData> Projects { get; }
    }
}
