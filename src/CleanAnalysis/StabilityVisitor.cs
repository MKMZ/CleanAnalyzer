using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace CleanAnalysis
{
    public class StabilityVisitor : SymbolVisitor
    {
        public ISet<CrossAssemblyReference> CrossReferences { get; } = new HashSet<CrossAssemblyReference>();

        private IAssemblySymbol Assembly { get; set; }

        private IImmutableList<Solution> Solutions { get; }

        public StabilityVisitor(IAssemblySymbol assembly, IImmutableList<Solution> solutions)
        {
            Assembly = assembly;
            Solutions = solutions;
        }

        public void StartTour()
        {
            Assembly.Accept(this);
        }

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            symbol.GlobalNamespace.Accept(this);
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            VisitAll(symbol.GetMembers());
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            VisitAll(symbol.GetMembers());
            VisitAll(symbol.TypeArguments);
            VisitAll(symbol.Interfaces);
            symbol.BaseType?.Accept(this);
            FindTypeReferences(symbol);
        }

        private void FindTypeReferences(INamedTypeSymbol symbol)
        {
            if (symbol.ContainingAssembly != Assembly)
            {
                return;
            }
            foreach (var solution in Solutions)
            {
                FindTypeReferencesInSingleSolution(symbol, solution);
            }
        }

        private void FindTypeReferencesInSingleSolution(INamedTypeSymbol symbol, Solution solution)
        {
            var references = SymbolFinder.FindReferencesAsync(symbol, solution).Result;
            foreach (var reference in references)
            {
                var referenceType = reference.Definition.ContainingType;
                if (referenceType != null 
                    && referenceType.ContainingAssembly != Assembly)
                {
                    AddCrossAssemblyReference(referenceType, symbol);
                }
            }
        }

        private void AddCrossAssemblyReference(INamedTypeSymbol origin, INamedTypeSymbol target)
        {
            var crossAssemblyRef = new CrossAssemblyReference(
                        origin.ContainingAssembly.Name,
                        origin.Name,
                        target.ContainingAssembly.Name,
                        target.Name);
            CrossReferences.Add(crossAssemblyRef);
        }

        private void VisitAll(IEnumerable<ISymbol> symbols)
        {
            foreach (var symbol in symbols)
            {
                symbol.Accept(this);
            }
        }
    }
}
