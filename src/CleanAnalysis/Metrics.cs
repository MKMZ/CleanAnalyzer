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
    }
}
