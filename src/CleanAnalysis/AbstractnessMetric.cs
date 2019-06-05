namespace CleanAnalysis
{
    public struct AbstractnessMetric
    {
        public AbstractnessMetric(int abstractions, int concretizations)
        {
            Abstractions = abstractions;
            Concretizations = concretizations;
        }

        public int Abstractions { get; }
        public int Concretizations { get; }

        public double Coefficient
        {
            get
            {
                var coeff = (double)Abstractions / (Abstractions + Concretizations);
                return double.IsNaN(coeff) ? 1 : coeff;
            }
        }
    }
}
