namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System;

public sealed partial class ControllersGenerator
{
    internal class Parser
    {
        private readonly SourceProductionContext _context;
        private readonly Compilation _compilation;
        private readonly HashSet<ITypeSymbol> _cachedTypeSymbols = new(SymbolEqualityComparer.Default);

        public Parser(SourceProductionContext context, Compilation compilation)
        {
            _context = context;
            _compilation = compilation;
        }

        internal SourceGenerationSpec Parse(ImmutableArray<ClassDeclarationSyntax> candidateClassDeclarations)
        {
            List<INamedTypeSymbol> controllerTypes = new List<INamedTypeSymbol>();

            INamedTypeSymbol nonControllerAttributeSymbol = _compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.NonControllerAttribute");
            INamedTypeSymbol controllerAttributeSymbol = _compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.ControllerAttribute");
            INamedTypeSymbol apiControllerAttributeSymbol = _compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.ApiControllerAttribute");

            foreach (IGrouping<SyntaxTree, ClassDeclarationSyntax> group in candidateClassDeclarations.GroupBy(c => c.SyntaxTree))
            {
                SyntaxTree syntaxTree = group.Key;
                SemanticModel compilationSemanticModel = _compilation.GetSemanticModel(syntaxTree);
                CompilationUnitSyntax compilationUnitSyntax = (CompilationUnitSyntax)syntaxTree.GetRoot();

                foreach (ClassDeclarationSyntax classDeclarationSyntax in group)
                {
                    var controllerSymbol = compilationSemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;

                    bool hasControllerAttribute = false;
                    bool hasApiControllerAttribute = false;
                    bool hasNonControllerAttribute = false;

                    // We are not supporting custom attributes

                    foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
                    {
                        AttributeSyntax attributeSyntax = attributeListSyntax.Attributes.First();
                        IMethodSymbol attributeSymbol = compilationSemanticModel.GetSymbolInfo(attributeSyntax).Symbol as IMethodSymbol;
                        if (attributeSymbol == null)
                        {
                            continue;
                        }

                        INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;

                        if (nonControllerAttributeSymbol.Equals(attributeContainingTypeSymbol, SymbolEqualityComparer.Default))
                        {
                            hasNonControllerAttribute = true;
                            break;
                        }
                        else if (controllerAttributeSymbol.Equals(attributeContainingTypeSymbol, SymbolEqualityComparer.Default))
                        {
                            hasControllerAttribute = true;
                            break;
                        }
                        else if (apiControllerAttributeSymbol.Equals(attributeContainingTypeSymbol, SymbolEqualityComparer.Default))
                        {
                            hasApiControllerAttribute = true;
                            break;
                        }
                    }

                    if (hasNonControllerAttribute ||
                        (!hasControllerAttribute && !hasApiControllerAttribute && !controllerSymbol.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    controllerTypes.Add(controllerSymbol);
                }

            }

            return new SourceGenerationSpec() { ControllerTypes = controllerTypes.ToArray() };
        }
    }
}