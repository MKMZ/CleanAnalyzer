using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace CleanAnalysis
{
    public class TypeVisitor : SymbolVisitor
    {
        public IList<INamedTypeSymbol> Concretizations { get; }
            = new List<INamedTypeSymbol>();

        public IList<INamedTypeSymbol> Abstractions { get; }
            = new List<INamedTypeSymbol>();

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            symbol.GlobalNamespace.Accept(this);
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            VisitMembers(symbol);
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            switch (symbol.TypeKind)
            {
                case TypeKind.Interface:
                    Abstractions.Add(symbol);
                    break;
                case TypeKind.Struct:
                case TypeKind.Class:
                    AnalyzeClass(symbol);
                    break;
            }
            VisitMembers(symbol);
        }

        private void AnalyzeClass(INamedTypeSymbol symbol)
        {
            if (symbol.IsAbstract)
            {
                Abstractions.Add(symbol);
            }
            else if (symbol.BaseType?.SpecialType != SpecialType.System_Object
                || symbol.Interfaces.Any())
            {
                Concretizations.Add(symbol);
            }
        }

        private void VisitMembers(INamespaceOrTypeSymbol symbol)
        {
            foreach (var member in symbol.GetMembers())
            {
                member.Accept(this);
            }
        }
    }
}
