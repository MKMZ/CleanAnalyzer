using System;

namespace CleanAnalysis
{
    public struct Metrics
    {
        public Metrics(StabilityMetric stability, AbstractnessMetric abstractness)
        {
            Stability = stability;
            Abstractness = abstractness;
        }

        public StabilityMetric Stability { get; }

        public AbstractnessMetric Abstractness { get; }

        public double Instability => 1 - Stability.Coefficient;

        public double MainSequenceDistance
            => Math.Abs(Abstractness.Coefficient - Stability.Coefficient);
    }
}
