using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace CleanAnalysis
{
    public class SolutionAnalyzer
    {
        public async Task AnalyzeSolution(Solution solution)
        {
            var projectFirst = solution.Projects.First();
            var compilation = await projectFirst.GetCompilationAsync();
            var visitor = new TypeVisitor();

            compilation.Assembly.Accept(visitor);
            // TODO: Do analysis on the projects in the loaded solution
        }
    }
}
