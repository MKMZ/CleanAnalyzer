using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace CleanAnalysis
{
    public class SolutionAnalyzer
    {
        public async Task<Metrics> AnalyzeSolution(Solution solution)
        {
            foreach (var project in solution.Projects)
            {
                await AnalyzeProject(project);
            }
            return default;
        }

        public async Task<Metrics> AnalyzeProject(Project project)
        {
            var compilation = await project.GetCompilationAsync();
            var abstractness = CalculateAbstractness(compilation);
            return new Metrics(default, abstractness);
        }

        private AbstractnessMetric CalculateAbstractness(Compilation compilation)
        {
            var visitor = new AbstractnessVisitor();
            compilation.Assembly.Accept(visitor);
            return new AbstractnessMetric(
                visitor.Abstractions.Count,
                visitor.Concretizations.Count);
        }

        private StabilityMetric CalculateStability(Compilation compilation)
        {

            return new StabilityMetric();
        }
    }
}
