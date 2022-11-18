namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mvc.SourceGen.Generators.Extensions;
using Roslyn.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

public sealed partial class MvcGenerator
{
    internal class Parser
    {
        private readonly MetadataLoadContext _metadataLoadContext;
        private readonly Compilation _compilation;
        private readonly Type _objectType;
        private readonly Type _ienumerableType;
        private readonly Type _icollectionType;
        private readonly Type _fromServicesAttributeType;
        private readonly Type _nonControllerAttributeType;
        private readonly Type _controllerAttributeType;
        private readonly Type _apiControllerAttributeType;
        private readonly Type _nonActionAttributeType;

        public Parser(Compilation compilation)
        {
            _metadataLoadContext = new MetadataLoadContext(compilation);
            _compilation = compilation;

            _objectType = _metadataLoadContext.ResolveType<object>();
            _ienumerableType = _metadataLoadContext.ResolveType<IEnumerable>();
            _icollectionType = _metadataLoadContext.ResolveType<ICollection>();
            _fromServicesAttributeType = _metadataLoadContext.ResolveType("Microsoft.AspNetCore.Mvc.FromServicesAttribute");
            _nonControllerAttributeType = _metadataLoadContext.ResolveType("Microsoft.AspNetCore.Mvc.NonControllerAttribute");
            _controllerAttributeType = _metadataLoadContext.ResolveType("Microsoft.AspNetCore.Mvc.ControllerAttribute");
            _apiControllerAttributeType = _metadataLoadContext.ResolveType("Microsoft.AspNetCore.Mvc.ApiControllerAttribute");
            _apiControllerAttributeType = _metadataLoadContext.ResolveType("Microsoft.AspNetCore.Mvc.ApiControllerAttribute");
            _nonActionAttributeType = _metadataLoadContext.ResolveType("Microsoft.AspNetCore.Mvc.NonActionAttribute");
        }

        internal SourceGenerationSpec Parse(ImmutableArray<ClassDeclarationSyntax> candidateClassDeclarations)
        {
            var controllerTypes = new Dictionary<INamedTypeSymbol, SourceGenerationActionMethodSpec[]?>(SymbolEqualityComparer.Default);
            var modelTypes = new HashSet<SourceGenerationModelSpec>(SourceGenerationModelSpecComparer.Default);

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

                    var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                    List<SourceGenerationActionMethodSpec>? actionMethods = null;

                    foreach (var method in methods)
                    {
                        if (IsAction(method))
                        {
                            actionMethods ??= new List<SourceGenerationActionMethodSpec>();

                            var actionSpec = new SourceGenerationActionMethodSpec() { Method = method.GetMethodSymbol() };

                            // TODO: Verify location
                            if (actionSpec.Method.TryGetAwaiter(compilationSemanticModel, actionSpec.Method.Locations.First().SourceSpan.Start, out var awaiterMethod))
                            {
                                actionSpec.AwaiterType = awaiterMethod!.ReturnType;
                                actionSpec.AsyncResultType = actionSpec.AwaiterType.GetAsyncResult();
                            }

                            actionMethods.Add(actionSpec);

                            var parameters = method.GetParameters();

                            foreach (var parameter in parameters)
                            {
                                if (IsFromServices(parameter))
                                {
                                    continue;
                                }

                                void AddType(Type type)
                                {
                                    var symbol = type.GetTypeSymbol();
                                    var modelSpec = new SourceGenerationModelSpec()
                                    {
                                        Type = symbol,
                                        IsArray = type.IsArray,
                                        IsCollection = _icollectionType.IsAssignableFrom(type),
                                        IsIEnumerable = _ienumerableType.IsAssignableFrom(type)
                                    };

                                    if (modelTypes.Contains(modelSpec))
                                    {
                                        return;
                                    }

                                    if (type.IsArray)
                                    {
                                        AddType(type.GetElementType());
                                        return;
                                    }

                                    if (symbol!.IsReferenceType)
                                    {
                                        symbol = (symbol.WithNullableAnnotation(NullableAnnotation.None) as INamedTypeSymbol)!;
                                    }

                                    if (type.IsGenericType)
                                    {
                                        foreach (var genericType in type.GenericTypeArguments)
                                        {
                                            AddType(genericType);
                                        }
                                    }

                                    _ = modelTypes.Add(modelSpec);

                                    if (type.IsPrimitive || symbol.SpecialType != SpecialType.None || modelSpec.IsIEnumerable || _icollectionType.IsAssignableFrom(type))
                                    {
                                        return;
                                    }

                                    //TODO: Perf issues + Stackoverflow control 
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

                    // Add to the final list
                    controllerTypes[controllerSymbol!] = actionMethods?.ToArray();
                }
            }

            return new SourceGenerationSpec() { ControllerTypes = controllerTypes, ModelTypes = modelTypes.ToArray() };
        }

        private bool IsFromServices(ParameterInfo parameter)
        {
            var attributes = parameter.GetCustomAttributesData();
            return attributes.Any(t => t.AttributeType == _fromServicesAttributeType);
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
            return methodInfo.GetBaseDefinition().DeclaringType != _objectType
                && methodInfo.DeclaringType != _objectType;
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

            return typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) ||
                attributes.Any(t => t.AttributeType == _controllerAttributeType || t.AttributeType == _apiControllerAttributeType);
        }

        private static bool IsIDisposableMethod(MethodInfo methodInfo)
        {
            //TODO: Validate
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