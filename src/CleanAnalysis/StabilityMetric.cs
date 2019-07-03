namespace CleanAnalysis
{
    public struct StabilityMetric
    {
        public StabilityMetric(int dependencies, int dependents)
        {
            Dependencies = dependencies;
            Dependents = dependents;
        }

        public int Dependencies { get; }
        public int Dependents { get; }

        public double Coefficient
        {
            get
            {
                var coeff = (double)Dependents / (Dependents + Dependencies);
                return double.IsNaN(coeff) ? 1 : coeff;
            }
        }

        internal StabilityMetric IncreaseDependencies()
            => new StabilityMetric(Dependencies + 1, Dependents);

        internal StabilityMetric IncreaseDependents()
            => new StabilityMetric(Dependencies, Dependents + 1);
    }
}
