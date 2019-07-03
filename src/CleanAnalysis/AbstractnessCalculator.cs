using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace CleanAnalysis
{
    internal class AbstractnessCalculator
        : ICalculator<AbstractnessMetric>
    {
        public IImmutableDictionary<Project, AbstractnessMetric> Calculate(AnalysisDataContainer analysisDataContainer) 
            => analysisDataContainer
                .Projects
                .ToImmutableDictionary(project => project.Info, project => CalculateProjectAbstractness(project.Compilation));

        private AbstractnessMetric CalculateProjectAbstractness(Compilation compilation)
        {
            var visitor = new AbstractnessVisitor();
            visitor.Visit(compilation.Assembly);
            return new AbstractnessMetric(visitor.Abstractions.Count, visitor.Concretizations.Count);
        }
    }
}
