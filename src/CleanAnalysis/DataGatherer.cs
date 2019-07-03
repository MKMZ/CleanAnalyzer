using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using MoreLinq;

namespace CleanAnalysis
{
    internal class DataGatherer
    {
        private IConsoleProgressReporter ProgressReporter { get; }

        public DataGatherer(IConsoleProgressReporter progressReporter = null)
        {
            ProgressReporter = progressReporter;
        }

        public async Task<AnalysisDataContainer> Gather(string[] solutionPaths)
        {
            var solutions = await GetSolutions(solutionPaths);
            var projects = GetProjects(solutions);
            var projectDatas = await GetProjectDatas(projects);
            return new AnalysisDataContainer(solutions, projectDatas);
        }

        private async Task<IImmutableList<ProjectData>> GetProjectDatas(IImmutableList<Project> projects)
        {
            var projectDatas = new List<ProjectData>();
            foreach (var project in projects)
            {
                var compilation = await project.GetCompilationAsync();
                projectDatas.Add(new ProjectData(project, compilation));
            }
            return projectDatas.ToImmutableList();
        }

        private async Task<IImmutableList<Solution>> GetSolutions(string[] solutionPaths)
        {
            using (var workspace = MSBuildWorkspace.Create())
            {
                return (await Task.WhenAll(
                    solutionPaths.Select(path => workspace.OpenSolutionAsync(path, ProgressReporter))
                    )).ToImmutableList();
            }
        }

        private IImmutableList<Project> GetProjects(IImmutableList<Solution> solutions)
            => solutions
                .SelectMany(solution => solution.Projects)
                .Where(p => !p.Name.Contains("Tests"))
                .DistinctBy(p => p.Id)
                .ToImmutableList();
    }
}
