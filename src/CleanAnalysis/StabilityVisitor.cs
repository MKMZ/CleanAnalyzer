using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace CleanAnalysis
{
    public class StabilityVisitor : SymbolVisitor
    {
        public ISet<INamedTypeSymbol> ExternalTypesUsed => CoreUsageVisitor.ExternalTypesUsed;

        public Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> ExternalTypeReferencingTypes
            => CoreUsageVisitor.ExternalTypeReferencingTypes;

        private UsageVisitor CoreUsageVisitor { get; }

        public StabilityVisitor(IAssemblySymbol assembly, HashSet<string> solutionAssemblyNames)
        {
            CoreUsageVisitor = new UsageVisitor(assembly, solutionAssemblyNames);
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
            CoreUsageVisitor.TypeStack.Push(symbol);
            VisitAll(symbol.GetMembers());
            AnalyzeUsedTypes(symbol.TypeArguments);
            AnalyzeUsedTypes(symbol.Interfaces);
            AnalyzeUsedType(symbol.BaseType);
            CoreUsageVisitor.TypeStack.Pop();
        }

        public override void VisitMethod(IMethodSymbol symbol)
        {
            VisitAll(symbol.Parameters);
            VisitAll(symbol.TypeParameters);
            AnalyzeUsedType(symbol.ReturnType);
        }

        public override void VisitEvent(IEventSymbol symbol)
        {
            AnalyzeUsedType(symbol.Type);
        }

        public override void VisitField(IFieldSymbol symbol)
        {
            AnalyzeUsedType(symbol.Type);
        }

        public override void VisitLocal(ILocalSymbol symbol)
        {
            AnalyzeUsedType(symbol.Type);
        }

        public override void VisitParameter(IParameterSymbol symbol)
        {
            AnalyzeUsedType(symbol.Type);
        }

        public override void VisitProperty(IPropertySymbol symbol)
        {
            AnalyzeUsedType(symbol.Type);
        }

        public override void VisitTypeParameter(ITypeParameterSymbol symbol)
        {
            AnalyzeUsedTypes(symbol.ConstraintTypes);
        }

        private void VisitAll(IEnumerable<ISymbol> symbols)
        {
            foreach (var symbol in symbols)
            {
                symbol.Accept(this);
            }
        }

        private void AnalyzeUsedTypes(IEnumerable<ITypeSymbol> types)
        {
            foreach (var type in types)
            {
                CoreUsageVisitor.Visit(type);
            }
        }

        private void AnalyzeUsedType(ITypeSymbol type)
        {
            CoreUsageVisitor.Visit(type);
        }

        private class UsageVisitor : SymbolVisitor
        {
            public ISet<INamedTypeSymbol> ExternalTypesUsed { get; } = new HashSet<INamedTypeSymbol>();

            public Stack<INamedTypeSymbol> TypeStack { get; } = new Stack<INamedTypeSymbol>();

            /// <summary>
            /// External types are keys, and a set of types that referenced given key is the value.
            /// </summary>
            public Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> ExternalTypeReferencingTypes { get; }
            = new Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>>();

            public IAssemblySymbol Assembly { get; }

            public HashSet<string> SolutionAssemblyNames { get; }

            public UsageVisitor(IAssemblySymbol assembly, HashSet<string> solutionAssemblyNames)
            {
                Assembly = assembly;
                SolutionAssemblyNames = solutionAssemblyNames;
            }

            public override void VisitArrayType(IArrayTypeSymbol symbol)
            {
                Visit(symbol.ElementType);
            }

            public override void VisitNamedType(INamedTypeSymbol symbol)
            {
                foreach (var typeArg in symbol.TypeArguments)
                {
                    Visit(typeArg);
                }
                if (symbol.TypeKind == TypeKind.Error)
                {
                    return;
                }
                if (symbol.SpecialType != SpecialType.None)
                {
                    return;
                }
                if (symbol.IsImplicitlyDeclared)
                {
                    return;
                }
                if (symbol.ContainingAssembly == null)
                {
                    return;
                }
                if (symbol.ContainingAssembly != Assembly)
                {
                    if (!SolutionAssemblyNames.Contains(symbol.ContainingAssembly.MetadataName))
                    {
                        // filter out target types from outside of our solution assemblies
                        return;
                    }
                    ExternalTypesUsed.Add(symbol);
                    if (TypeStack.Count > 0)
                    {
                        var dependentSet = GetDependentsSet();
                        dependentSet.Add(TypeStack.Peek());
                    }
                }
                HashSet<INamedTypeSymbol> GetDependentsSet()
                {
                    if (ExternalTypeReferencingTypes.TryGetValue(symbol, out var existingSet))
                    {
                        return existingSet;
                    }
                    else
                    {
                        var dependentSet = new HashSet<INamedTypeSymbol>();
                        return ExternalTypeReferencingTypes[symbol] = dependentSet;
                    }
                }
            }

            public override void VisitPointerType(IPointerTypeSymbol symbol)
            {
                Visit(symbol.PointedAtType);
            }

            public override void VisitTypeParameter(ITypeParameterSymbol symbol)
            {
                foreach(var constraint in symbol.ConstraintTypes)
                {
                    Visit(constraint);
                }
            }
        }
    }
}
