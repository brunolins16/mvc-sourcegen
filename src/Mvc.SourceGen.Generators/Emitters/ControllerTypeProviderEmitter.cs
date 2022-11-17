namespace Mvc.SourceGen.Generators.Emitters;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class ControllerTypeProviderEmitter : IEmitter
{
    public static readonly IdentifierNameSyntax ControllerTypeProviderType = SyntaxFactory.IdentifierName("Mvc.SourceGen.ISourceGenControllerTypeProvider");
    public static readonly IdentifierNameSyntax ICriticalNotifyCompletionType = SyntaxFactory.IdentifierName("System.Runtime.CompilerServices.ICriticalNotifyCompletion");
    public static readonly IdentifierNameSyntax INotifyCompletionType = SyntaxFactory.IdentifierName("System.Runtime.CompilerServices.INotifyCompletion");

    public static readonly IdentifierNameSyntax ControllerActionInfoType = SyntaxFactory.IdentifierName("Mvc.SourceGen.ControllerActionInfo");
    public static readonly ArrayTypeSyntax ControllerActionInfoArrayType = SyntaxFactory.ArrayType(ControllerActionInfoType)
                                                                                       .AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier()
                                                                                        .AddSizes(SyntaxFactory.OmittedArraySizeExpression()));

    // variables
    public static readonly IdentifierNameSyntax TargetVariable = SyntaxFactory.IdentifierName("target");
    public static readonly IdentifierNameSyntax ParametersVariable = SyntaxFactory.IdentifierName("parameters");
    public static readonly IdentifierNameSyntax TypesVariable = SyntaxFactory.IdentifierName("_types");
    public static readonly IdentifierNameSyntax ControllerTypeVariable = SyntaxFactory.IdentifierName("controllerType");
    public static readonly IdentifierNameSyntax ControllerInfoVariable = SyntaxFactory.IdentifierName("controllerInfo");

    public void Emit(SourceProductionContext context, SourceGenerationSpec spec)
    {// namespace Mvc.SourceGen ?? App namespace
        var declaration = SyntaxFactory.NamespaceDeclaration(WellKnownTypes.SourceGenNamespace)
            .WithUsings(SyntaxFactory.List(new UsingDirectiveSyntax[]
            {
                    // using System.Diagnostics.CodeAnalysis; 
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Diagnostics.CodeAnalysis")),
                    // using System.Reflection;
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Reflection"))
            }))
            .WithLeadingTrivia(SyntaxFactory.Trivia(SyntaxFactory.NullableDirectiveTrivia(SyntaxFactory.Token(SyntaxKind.EnableKeyword), true)))
            // internal partial class MvcSourceGenContext : ISourceGenControllerTypeProvider
            .WithSourceGenControllerTypeProviderContext(spec.ControllerTypes.Keys.ToArray());

        foreach (var controllerType in spec.ControllerTypes)
        {
            var controllerInfoName = SyntaxFactory.IdentifierName($"{controllerType.Key.ToDisplayString().Replace(".", "")}Info");
            var constructorBlock = SyntaxFactory.Block();

            if (controllerType.Value is { } actionMethods)
            {
                var controllerTypeSyntax = SyntaxFactory.IdentifierName(controllerType.Key.ToDisplayString());
                var targetCast = SyntaxFactory.ParenthesizedExpression(SyntaxFactory.CastExpression(controllerTypeSyntax, TargetVariable));
                var controllerTypeOf = SyntaxFactory.TypeOfExpression(controllerTypeSyntax);

                constructorBlock = constructorBlock.AddStatements(

                    SyntaxFactory.ExpressionStatement(
                            // _properties = new ControllerActionInfo[0]
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName("_properties"),
                                SyntaxFactory.ArrayCreationExpression(SyntaxFactory.ArrayType(WellKnownTypes.PropertyInfo))
                                .AddTypeRankSpecifiers(SyntaxFactory.ArrayRankSpecifier().AddSizes(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)))))),

                    SyntaxFactory.ExpressionStatement(
                            // _actions = new ControllerActionInfo[5]
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName("_actions"),
                                SyntaxFactory.ArrayCreationExpression(SyntaxFactory.ArrayType(ControllerActionInfoType))
                                .AddTypeRankSpecifiers(SyntaxFactory.ArrayRankSpecifier().AddSizes(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(actionMethods.Length)))))));

                for (int i = 0; i < actionMethods.Length; i++)
                {
                    var parameters = actionMethods[i].Method.Parameters;
                    ExpressionSyntax parameterTypes = SyntaxFactory.IdentifierName("System.Type.EmptyTypes");
                    List<ArgumentSyntax>? actionInvocationArguments = null;

                    if (parameters.Length > 0)
                    {
                        actionInvocationArguments = new List<ArgumentSyntax>(parameters.Length);
                        var arrayInitializer = SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression);

                        for (var paramIndex = 0; paramIndex < parameters.Length; paramIndex++)
                        {
                            arrayInitializer = arrayInitializer.AddExpressions(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(parameters[paramIndex].Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString())));
                            actionInvocationArguments.Add(SyntaxFactory.Argument(
                                // Nullable warning !
                                SyntaxFactory.CastExpression(
                                    SyntaxFactory.IdentifierName(parameters[paramIndex].Type.ToDisplayString()),
                                    SyntaxFactory.ElementAccessExpression(
                                        ParametersVariable,
                                        SyntaxFactory.BracketedArgumentList()
                                            .AddArguments(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(paramIndex))))
                                ))));
                        }

                        parameterTypes = SyntaxFactory.ImplicitArrayCreationExpression(arrayInitializer);
                    }


                    // typeof(TodoApp.Controllers.TodosController).GetMethod("Get", Type.EmptyTypes)
                    var methodInfoCall = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            controllerTypeOf,
                            SyntaxFactory.IdentifierName("GetMethod")))
                        .AddArgumentListArguments(
                            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(actionMethods[i].Method.Name))),
                            SyntaxFactory.Argument(parameterTypes)
                        );

                    // static (target,parameters) => ((TodoApp.Controllers.TodosController)target).Get()
                    var methodInvoker = SyntaxFactory.ParenthesizedLambdaExpression()
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(TargetVariable.Identifier),
                            SyntaxFactory.Parameter(ParametersVariable.Identifier)
                        )
                        .WithExpressionBody(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, targetCast, SyntaxFactory.IdentifierName(actionMethods[i].Method.Name))
                            )
                            .AddArgumentListArguments(actionInvocationArguments?.ToArray() ?? Array.Empty<ArgumentSyntax>())
                       );

                    // null or new ActionAwaitableInfo ()
                    ExpressionSyntax awaitableInfo = !actionMethods[i].IsAsync ?
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression) :
                        SyntaxFactory.ImplicitObjectCreationExpression()
                            .AddArgumentListArguments(
                                // Type resultType
                                SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(actionMethods[i].AsyncResultType!.ToDisplayString()))),
                                // Func<object, object> getAwaiterMethod
                                SyntaxFactory.Argument(SyntaxFactory.ParenthesizedLambdaExpression()
                                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                                        .AddParameterListParameters(SyntaxFactory.Parameter(TargetVariable.Identifier))
                                        //  (target) => ((Task<Todo[]>)target).GetAwaiter()
                                        .WithExpressionBody(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ParenthesizedExpression(SyntaxFactory.CastExpression(SyntaxFactory.IdentifierName(actionMethods[i].Method.ReturnType.ToDisplayString()), TargetVariable)),
                                                    SyntaxFactory.IdentifierName("GetAwaiter"))))),
                                // Func<object, bool> isCompletedMethod
                                SyntaxFactory.Argument(SyntaxFactory.ParenthesizedLambdaExpression()
                                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                                        .AddParameterListParameters(SyntaxFactory.Parameter(TargetVariable.Identifier))
                                        // (target) => ((TaskAwaiter<Todo[]>)target).IsCompleted,
                                        .WithExpressionBody(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ParenthesizedExpression(SyntaxFactory.CastExpression(SyntaxFactory.IdentifierName(actionMethods[i].AwaiterType!.ToDisplayString()), TargetVariable)),
                                                SyntaxFactory.IdentifierName("IsCompleted")))),
                                // Func<object, object> getResultMethod
                                SyntaxFactory.Argument(SyntaxFactory.ParenthesizedLambdaExpression()
                                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                                        .AddParameterListParameters(SyntaxFactory.Parameter(TargetVariable.Identifier))
                                        // (target) => ((TaskAwaiter<Todo[]>)target).GetResult(),
                                        .WithExpressionBody(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ParenthesizedExpression(SyntaxFactory.CastExpression(SyntaxFactory.IdentifierName(actionMethods[i].AwaiterType!.ToDisplayString()), TargetVariable)),
                                                    SyntaxFactory.IdentifierName("GetResult"))))),
                                // Action<object, Action> onCompletedMethod
                                SyntaxFactory.Argument(SyntaxFactory.ParenthesizedLambdaExpression()
                                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                                        .AddParameterListParameters(SyntaxFactory.Parameter(TargetVariable.Identifier), SyntaxFactory.Parameter(SyntaxFactory.Identifier("action")))
                                        // (target, action) => ((INotifyCompletion)target).OnCompleted(action),
                                        .WithExpressionBody(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ParenthesizedExpression(SyntaxFactory.CastExpression(INotifyCompletionType, TargetVariable)),
                                                    SyntaxFactory.IdentifierName("OnCompleted")))
                                            .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("action"))))),
                                // Action<object, Action>? unsafeOnCompletedMethod
                                SyntaxFactory.Argument(SyntaxFactory.ParenthesizedLambdaExpression()
                                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                                        .AddParameterListParameters(SyntaxFactory.Parameter(TargetVariable.Identifier), SyntaxFactory.Parameter(SyntaxFactory.Identifier("action")))
                                        // (target, action) => ((ICriticalNotifyCompletion)target)?.UnsafeOnCompleted(action)
                                        .WithExpressionBody(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ParenthesizedExpression(SyntaxFactory.CastExpression(ICriticalNotifyCompletionType, TargetVariable)),
                                                    SyntaxFactory.IdentifierName("UnsafeOnCompleted")))
                                            .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("action")))))
                            );

                    // _actions[i] = new ControllerActionInfo()
                    constructorBlock = constructorBlock.AddStatements(SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.ElementAccessExpression(
                                SyntaxFactory.IdentifierName("_actions"),
                                SyntaxFactory.BracketedArgumentList()
                                .AddArguments(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i))))),
                            SyntaxFactory.ImplicitObjectCreationExpression()
                                .AddArgumentListArguments(
                                    //MethodInfo method,                                    
                                    SyntaxFactory.Argument(methodInfoCall),
                                    // Func<object, object?[]?, object> methodInvoker
                                    SyntaxFactory.Argument(methodInvoker),
                                    // ActionModel.ActionAwaitableInfo? methodAwaitableInfo
                                    SyntaxFactory.Argument(awaitableInfo)
                                )
                        )));
                }
            }

            declaration = declaration.AddMembers(
                 //file class TodoControllerModelProvider : ControllerInfo
                 SyntaxFactory.ClassDeclaration(controllerInfoName.Identifier)
                    .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName("ControllerInfo")))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .AddMembers(
                        //private static readonly ControllerActionInfo[] _actions;
                        SyntaxFactory.FieldDeclaration(
                            SyntaxFactory.VariableDeclaration(ControllerActionInfoArrayType)
                            .AddVariables(SyntaxFactory.VariableDeclarator("_actions")))
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword))),

                        //private static readonly PropertyInfo[] _propertyInfo;
                        SyntaxFactory.FieldDeclaration(
                            SyntaxFactory.VariableDeclaration(WellKnownTypes.ArrayOfPropertyInfo)
                            .AddVariables(SyntaxFactory.VariableDeclarator("_properties")))
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)))
                     )
                    // static TodoControllerModelProvider()
                    .WithStaticConstructor(constructorBlock)
                    .AddMembers(

                        // public static TodoControllerModelProvider Instance {  get; } = new();
                        SyntaxFactory.PropertyDeclaration(controllerInfoName, "Instance")
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                            .AddAccessorListAccessors(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))
                            .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ImplicitObjectCreationExpression()))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),

                        //public override ControllerActionInfo[] Actions { get; } = _actions;
                        SyntaxFactory.PropertyDeclaration(ControllerActionInfoArrayType, "Actions")
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
                            //.AddAccessorListAccessors(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))
                            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.IdentifierName("_actions")))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        //.WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName("_actions")))
                        //    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),

                        // public override PropertyInfo[] Properties { get; } = _propertyInfo;
                        SyntaxFactory.PropertyDeclaration(WellKnownTypes.ArrayOfPropertyInfo, "Properties")
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
                            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.IdentifierName("_properties")))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                     )
                );
        }



        context.AddSource(
            "MvcSourceGenContext.ControllerTypeProvider.g.cs",
            declaration
            .NormalizeWhitespace()
            .GetText(encoding: Encoding.UTF8));
    }
}

file static class ControllerTypeProviderExtensions
{
    public static NamespaceDeclarationSyntax WithSourceGenControllerTypeProviderContext(this NamespaceDeclarationSyntax namespaceDeclaration, INamedTypeSymbol[] controllerTypes)
    {
        var getTypeInfoExpressions = new List<ExpressionSyntax>(controllerTypes.Length);

        foreach (var controllerType in controllerTypes)
        {
            // GetTypeInfo<{type.ToDisplayString()}>()
            getTypeInfoExpressions.Add(SyntaxFactory.InvocationExpression(
                SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier("GetTypeInfo"),
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SeparatedList(
                            new TypeSyntax[] { SyntaxFactory.IdentifierName(controllerType.ToDisplayString()) })
                ))));
        }

        var contextDeclaration = SyntaxFactory.ClassDeclaration(WellKnownTypes.MvcSourceGenContext.Identifier)
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                    .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(new BaseTypeSyntax[]
                    {
                            SyntaxFactory.SimpleBaseType(ControllerTypeProviderEmitter.ControllerTypeProviderType),
                    })))
                    // static MvcSourceGenContext()
                    .WithStaticConstructor(SyntaxFactory.Block(
                                SyntaxFactory.ExpressionStatement(
                                    // _types = new TypeInfo[{_spec.ControllerTypes.Length}]
                                    SyntaxFactory.AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        ControllerTypeProviderEmitter.TypesVariable,
                                        SyntaxFactory.ArrayCreationExpression(
                                            SyntaxFactory.ArrayType(WellKnownTypes.TypeInfo)
                                                 .AddRankSpecifiers(
                                                    SyntaxFactory.ArrayRankSpecifier()
                                                        .AddSizes(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(controllerTypes.Length))))
                                            )
                                        .WithInitializer(
                                            SyntaxFactory.InitializerExpression(
                                                SyntaxKind.ArrayInitializerExpression,
                                                SyntaxFactory.SeparatedList(getTypeInfoExpressions)
                                        ))))
                             ))
                    .WithControllerInfoBuilder(controllerTypes)
                    .WithTypeInfoBuilder();

        return namespaceDeclaration.AddMembers(contextDeclaration);
    }

    public static ClassDeclarationSyntax WithControllerInfoBuilder(this ClassDeclarationSyntax classDeclarationSyntax, INamedTypeSymbol[] controllers)
    {
        //TODO: [NotNullWhen(true)]

        var methodBody = SyntaxFactory.Block(
                SyntaxFactory.ExpressionStatement(
                    //  controllerInfo = null;
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        ControllerTypeProviderEmitter.ControllerInfoVariable,
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)))
                );

        foreach (var controllerType in controllers)
        {
            // TODO: HACK 😅 Fix + performance
            var controllerInfoName = $"{controllerType.ToDisplayString().Replace(".", "")}Info";

            methodBody = methodBody.AddStatements(
                // if (typeof(TodoApp.Controllers.TodosController).IsAssignableFrom(controllerType))
                SyntaxFactory.IfStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(controllerType.ToDisplayString())),
                            SyntaxFactory.IdentifierName("IsAssignableFrom")))
                    .AddArgumentListArguments(SyntaxFactory.Argument(ControllerTypeProviderEmitter.ControllerTypeVariable)),

                    SyntaxFactory.Block(

                        //  controllerInfo = {ControllerName}ModelProvider.Instance;
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                ControllerTypeProviderEmitter.ControllerInfoVariable,
                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(controllerInfoName), SyntaxFactory.IdentifierName("Instance")))
                        ),
                        // return true;
                        SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)))
                ));
        }

        // return controllerInfo != null;
        methodBody = methodBody.AddStatements(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.BinaryExpression(
                            SyntaxKind.NotEqualsExpression,
                            ControllerTypeProviderEmitter.ControllerInfoVariable,
                            SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))));

        // public bool TryGetControllerInfo(Type controllerType, [NotNullWhen(true)] out ControllerInfo? controllerInfo)
        return classDeclarationSyntax.AddMembers(SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), "TryGetControllerInfo")
            .WithBody(methodBody)
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .AddParameterListParameters(
                SyntaxFactory.Parameter(ControllerTypeProviderEmitter.ControllerTypeVariable.Identifier).WithType(SyntaxFactory.IdentifierName("System.Type")),
                SyntaxFactory.Parameter(ControllerTypeProviderEmitter.ControllerInfoVariable.Identifier)
                    .WithType(SyntaxFactory.NullableType(SyntaxFactory.IdentifierName("Mvc.SourceGen.ControllerInfo")))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.OutKeyword)))));
    }

    public static ClassDeclarationSyntax WithTypeInfoBuilder(this ClassDeclarationSyntax classDeclarationSyntax)
    {
        // private readonly static TypeInfo[] _types;
        classDeclarationSyntax = classDeclarationSyntax.AddMembers(SyntaxFactory.FieldDeclaration(
            SyntaxFactory.VariableDeclaration(
                SyntaxFactory.ArrayType(WellKnownTypes.TypeInfo)
                    .AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier()
                    .AddSizes(SyntaxFactory.OmittedArraySizeExpression())))
                .AddVariables(SyntaxFactory.VariableDeclarator(ControllerTypeProviderEmitter.TypesVariable.Identifier)
            ))
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))));

        // public IEnumerable<TypeInfo> ControllerTypes => _types;
        classDeclarationSyntax = classDeclarationSyntax.AddMembers(SyntaxFactory.PropertyDeclaration(WellKnownTypes.IEnumerableOfTypeInfo, "ControllerTypes")
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(ControllerTypeProviderEmitter.TypesVariable))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))));

        // internal static TypeInfo GetTypeInfo<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        return classDeclarationSyntax.AddMembers(SyntaxFactory.MethodDeclaration(WellKnownTypes.TypeInfo, "GetTypeInfo")
            .WithTypeParameterList(SyntaxFactory.TypeParameterList().AddParameters(
                    SyntaxFactory.TypeParameter(SyntaxFactory.Identifier("T"))
                        .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new AttributeSyntax[]
                        {
                                                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("DynamicallyAccessedMembers"))
                                                    .AddArgumentListArguments(
                                                        SyntaxFactory.AttributeArgument(
                                                            SyntaxFactory.BinaryExpression(
                                                                SyntaxKind.BitwiseOrExpression,
                                                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, WellKnownTypes.DynamicallyAccessedMemberTypes, SyntaxFactory.IdentifierName("PublicProperties")),
                                                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, WellKnownTypes.DynamicallyAccessedMemberTypes, SyntaxFactory.IdentifierName("PublicConstructors")))))
                        })))
                ))
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
            //  => typeof(T).GetTypeInfo();
            .WithExpressionBody(
                SyntaxFactory.ArrowExpressionClause(
                   SyntaxFactory.InvocationExpression(
                       SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName("T")), SyntaxFactory.IdentifierName("GetTypeInfo")))
                   ))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
    }
}
