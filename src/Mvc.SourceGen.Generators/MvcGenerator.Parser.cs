namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System;
using System.Reflection;
using Roslyn.Reflection;

public sealed partial class MvcGenerator
{
    internal class Parser
    {
        private readonly MetadataLoadContext _metadataLoadContext;
        private readonly Compilation _compilation;

        private readonly Type _nonControllerAttributeType;
        private readonly Type _controllerAttributeType;
        private readonly Type _apiControllerAttributeType;
        private readonly Type _nonActionAttributeType;        

        public Parser(Compilation compilation)
        {
            _metadataLoadContext  = new MetadataLoadContext(compilation);
            _compilation = compilation;

            _nonControllerAttributeType = _metadataLoadContext.ResolveType("Microsoft.AspNetCore.Mvc.NonControllerAttribute");
            _controllerAttributeType = _metadataLoadContext.ResolveType("Microsoft.AspNetCore.Mvc.ControllerAttribute");
            _apiControllerAttributeType = _metadataLoadContext.ResolveType("Microsoft.AspNetCore.Mvc.ApiControllerAttribute");
            _apiControllerAttributeType = _metadataLoadContext.ResolveType("Microsoft.AspNetCore.Mvc.ApiControllerAttribute");
            _nonActionAttributeType = _metadataLoadContext.ResolveType("Microsoft.AspNetCore.Mvc.NonActionAttribute");
        }

        internal SourceGenerationSpec Parse(ImmutableArray<ClassDeclarationSyntax> candidateClassDeclarations)
        {
            List<INamedTypeSymbol> controllerTypes = new();
            var modelTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            foreach (IGrouping<SyntaxTree, ClassDeclarationSyntax> group in candidateClassDeclarations.GroupBy(c => c.SyntaxTree))
            {
                SyntaxTree syntaxTree = group.Key;
                SemanticModel compilationSemanticModel = _compilation.GetSemanticModel(syntaxTree);
                CompilationUnitSyntax compilationUnitSyntax = (CompilationUnitSyntax)syntaxTree.GetRoot();

                foreach (ClassDeclarationSyntax classDeclarationSyntax in group)
                {
                    var controllerSymbol = compilationSemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
                    var controllerType = controllerSymbol.AsType(_metadataLoadContext);

                    if (!IsController(controllerType))
                    {
                        continue;
                    }

                    // Add to the final list
                    controllerTypes.Add(controllerSymbol);

                    var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                    foreach (var method in methods)
                    {
                        if (IsAction(method))
                        {
                            var parameters = method.GetParameters();

                            foreach (var parameter in parameters)
                            {                                
                                void AddType(Type type)
                                {
                                    if (type.IsArray)
                                    {
                                        AddType(type.GetElementType());
                                        return;
                                    }

                                    var symbol = type.GetTypeSymbol() as INamedTypeSymbol;
                                    if (symbol.IsReferenceType)
                                    {
                                        symbol = symbol.WithNullableAnnotation(NullableAnnotation.None) as INamedTypeSymbol;
                                    }

                                    _ = modelTypes.Add(symbol);

                                    if (type.IsPrimitive) 
                                    {
                                       return;
                                    }
                                    
                                    if (type.IsGenericType)
                                    {
                                        foreach  (var genericType in type.GenericTypeArguments)
                                        {
                                            AddType(genericType);
                                        }
                                    }

                                    //TODO: Perf issues 
                                    var properties = type.GetProperties();
                                    foreach (var property in properties)
                                    {
                                        AddType(property.PropertyType);
                                    }
                                }

                                AddType(parameter.ParameterType);
                            }

                        }
                    }
                }
            }

            return new SourceGenerationSpec() { ControllerTypes = controllerTypes.ToArray(), ModelTypes = modelTypes.ToArray() };
        }

        private bool IsAction(MethodInfo methodInfo)
        {
            if (!methodInfo.IsPublic)
            {
                return false;
            }

            if (methodInfo.IsStatic)
            {
                return false;
            }

            if (methodInfo.IsAbstract)
            {
                return false;
            }

            if (methodInfo.IsConstructor)
            {
                return false;
            }

            if (methodInfo.IsGenericMethod)
            {
                return false;
            }

            // The SpecialName bit is set to flag members that are treated in a special way by some compilers
            // (such as property accessors and operator overloading methods).
            if (methodInfo.IsSpecialName)
            {
                return false;
            }

            // Dispose method implemented from IDisposable is not valid
            if (IsIDisposableMethod(methodInfo))
            {
                return false;
            }

            var attributes = methodInfo.GetCustomAttributesData();
            if (attributes.Any(t => t.AttributeType == _nonActionAttributeType))
            {
                return false;
            }

            // Overridden methods from Object class, e.g. Equals(Object), GetHashCode(), etc., are not valid.
            if (methodInfo.GetBaseDefinition().DeclaringType == typeof(object))
            {
                return false;
            }            

            return true;
        }

        private bool IsController(Type typeInfo)
        {
            if (!typeInfo.IsClass)
            {
                return false;
            }

            if (typeInfo.IsAbstract)
            {
                return false;
            }

            // We only consider public top-level classes as controllers. IsPublic returns false for nested
            // classes, regardless of visibility modifiers
            if (!typeInfo.IsPublic)
            {
                return false;
            }

            if (typeInfo.ContainsGenericParameters)
            {
                return false;
            }

            var attributes = typeInfo.GetCustomAttributesData();            
            if (attributes.Any(t => t.AttributeType == _nonControllerAttributeType))
            {
                return false;
            }

            if (!typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) &&
                !attributes.Any(t => t.AttributeType == _controllerAttributeType || t.AttributeType == _apiControllerAttributeType))
            {
                return false;
            }

            return true;
        }

        private static bool IsIDisposableMethod(MethodInfo methodInfo)
        {
            // Ideally we do not want Dispose method to be exposed as an action. However there are some scenarios where a user
            // might want to expose a method with name "Dispose" (even though they might not be really disposing resources)
            // Example: A controller deriving from MVC's Controller type might wish to have a method with name Dispose,
            // in which case they can use the "new" keyword to hide the base controller's declaration.

            // Find where the method was originally declared
            var baseMethodInfo = methodInfo.GetBaseDefinition();
            var declaringType = baseMethodInfo.DeclaringType;

            return
                typeof(IDisposable).IsAssignableFrom(declaringType) &&
                 declaringType.GetInterfaceMap(typeof(IDisposable)).TargetMethods[0] == baseMethodInfo;
        }
    }
}