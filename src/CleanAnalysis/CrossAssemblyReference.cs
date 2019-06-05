using System.Collections.Generic;

namespace CleanAnalysis
{
    public struct CrossAssemblyReference
    {
        public CrossAssemblyReference(string targetAssembly, string targetType, string originAssembly, string originType)
        {
            TargetAssembly = targetAssembly;
            TargetType = targetType;
            OriginAssembly = originAssembly;
            OriginType = originType;
        }

        public string TargetAssembly { get; }
        public string TargetType { get; }
        public string OriginAssembly { get; }
        public string OriginType { get; }

        public override bool Equals(object obj)
            => obj is CrossAssemblyReference reference &&
                   TargetAssembly == reference.TargetAssembly &&
                   TargetType == reference.TargetType &&
                   OriginAssembly == reference.OriginAssembly &&
                   OriginType == reference.OriginType;

        public override int GetHashCode()
        {
            var hashCode = 1500924958;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TargetAssembly);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TargetType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(OriginAssembly);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(OriginType);
            return hashCode;
        }
    }
}
