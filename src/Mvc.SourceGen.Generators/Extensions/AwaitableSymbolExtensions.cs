namespace Mvc.SourceGen.Generators.Extensions
{
    using Microsoft.CodeAnalysis;
    using System.Collections.Generic;
    using System.Linq;

    internal static class AwaitableSymbolExtensions
    {
        //copied from https://sourceroslyn.io/#Microsoft.CodeAnalysis.Workspaces/ISymbolExtensions.cs,593

        /// <summary>
        /// If the <paramref name="symbol"/> is a method symbol, returns <see langword="true"/> if the method's return type is "awaitable", but not if it's <see langword="dynamic"/>.
        /// If the <paramref name="symbol"/> is a type symbol, returns <see langword="true"/> if that type is "awaitable".
        /// An "awaitable" is any type that exposes a GetAwaiter method which returns a valid "awaiter". This GetAwaiter method may be an instance method or an extension method.
        /// </summary>
        public static bool TryGetAwaiter(this ISymbol? symbol, SemanticModel semanticModel, int position, out IMethodSymbol? awaiter)
        {
            var methodSymbol = symbol as IMethodSymbol;
            ITypeSymbol? typeSymbol = null;
            awaiter = null;

            if (methodSymbol == null)
            {
                typeSymbol = symbol as ITypeSymbol;
                if (typeSymbol == null)
                {
                    return false;
                }
            }
            else
            {
                if (methodSymbol.ReturnType == null)
                {
                    return false;
                }
            }

            // otherwise: needs valid GetAwaiter
            var potentialGetAwaiters = semanticModel.LookupSymbols(position,
                                                                   container: typeSymbol ?? methodSymbol!.ReturnType,
                                                                   name: WellKnownMemberNames.GetAwaiter,
                                                                   includeReducedExtensionMethods: true);
            awaiter = potentialGetAwaiters.OfType<IMethodSymbol>()
                .Where(x => !x.Parameters.Any())
                .FirstOrDefault(VerifyGetAwaiter);

            return awaiter != null;
        }

        public static bool IsValidGetAwaiter(this IMethodSymbol symbol)
            => symbol.Name == WellKnownMemberNames.GetAwaiter &&
            VerifyGetAwaiter(symbol);

        private static bool VerifyGetAwaiter(IMethodSymbol getAwaiter)
        {
            var returnType = getAwaiter.ReturnType;
            if (returnType == null)
            {
                return false;
            }

            // bool IsCompleted { get }
            if (!returnType.GetMembers().OfType<IPropertySymbol>().Any(p => p.Name == WellKnownMemberNames.IsCompleted && p.Type.SpecialType == SpecialType.System_Boolean && p.GetMethod != null))
            {
                return false;
            }

            var methods = returnType.GetMembers().OfType<IMethodSymbol>();

            // NOTE: (vladres) The current version of C# Spec, §7.7.7.3 'Runtime evaluation of await expressions', requires that
            // NOTE: the interface method INotifyCompletion.OnCompleted or ICriticalNotifyCompletion.UnsafeOnCompleted is invoked
            // NOTE: (rather than any OnCompleted method conforming to a certain pattern).
            // NOTE: Should this code be updated to match the spec?

            // void OnCompleted(Action) 
            // Actions are delegates, so we'll just check for delegates.
            if (!methods.Any(x => x.Name == WellKnownMemberNames.OnCompleted && x.ReturnsVoid && x.Parameters.Length == 1 && x.Parameters.First().Type.TypeKind == TypeKind.Delegate))
            {
                return false;
            }

            // void GetResult() || T GetResult()
            return methods.Any(m => m.Name == WellKnownMemberNames.GetResult && !m.Parameters.Any());
        }

        public static ITypeSymbol? GetAsyncResult(this ITypeSymbol awaiterType)
        {
            var methods = awaiterType.GetMembers().OfType<IMethodSymbol>();

            // void GetResult() || T GetResult()
            var getResult = methods.FirstOrDefault(m => m.Name == WellKnownMemberNames.GetResult && !m.Parameters.Any());
            return getResult?.ReturnType;
        }

        public static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol? type)
        {
            var current = type;
            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }
    }
}
