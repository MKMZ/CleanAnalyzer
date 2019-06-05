using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace CleanAnalysis
{
    public class Visitor : SymbolVisitor
    {
        public IList<INamedTypeSymbol> concretizations = new List<INamedTypeSymbol>();
        public IList<INamedTypeSymbol> abstractions = new List<INamedTypeSymbol>();

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            base.VisitNamedType(symbol);
            switch (symbol.TypeKind)
            {
                case TypeKind.Interface:
                    abstractions.Add(symbol);
                    break;
                case TypeKind.Class:
                    if (symbol.IsAbstract)
                    {
                        abstractions.Add(symbol);
                    }
                    else
                    {
                        concretizations.Add(symbol);
                    }
                    break;
                default:
                    break;
            }

            VisitNestedSymbols(symbol);
        }

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            base.VisitAssembly(symbol);
            symbol.GlobalNamespace.Accept(this);
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            base.VisitNamespace(symbol);
            VisitNestedSymbols(symbol);
        }

        private void VisitNestedSymbols(INamespaceOrTypeSymbol symbol)
        {
            foreach (var nestedSymbol in symbol.GetMembers())
            {
                nestedSymbol.Accept(this);
            }
        }
    }
}
