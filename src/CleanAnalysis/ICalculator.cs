using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace CleanAnalysis
{
    interface ICalculator<T>
    {
        IImmutableDictionary<Project, T> Calculate(AnalysisDataContainer analysisDataContainer);
    }
}
