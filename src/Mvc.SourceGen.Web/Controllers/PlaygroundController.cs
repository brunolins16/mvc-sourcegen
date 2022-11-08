namespace Mvc.SourceGen.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using Mvc.SourceGen.Web.Models;

[ApiController]
[Route("[controller]")]
public class PlaygroundController : ControllerBase
{
    [HttpGet(Name = "SayHello")]
    public string SayHello([FromQuery] MessageRef message)
    {
        return message.ToString();
    }

    /* System.NotSupportedException: 'Microsoft.AspNetCore.Mvc.ModelBinding.Validation.DefaultCollectionValidationStrategy.GetEnumerator[Mvc.SourceGen.Web.Controllers.Message](System.Object)' is missing native code. MethodInfo.MakeGenericMethod() is not compatible with AOT compilation. Inspect and fix AOT related warnings that were generated when the app was published. For more information see https://aka.ms/nativeaot-compatibility
     at System.Reflection.Runtime.MethodInfos.RuntimeNamedMethodInfo`1.GetUncachedMethodInvoker(RuntimeTypeInfo[], MemberInfo) + 0x2f
     at System.Reflection.Runtime.MethodInfos.RuntimeNamedMethodInfo`1.MakeGenericMethod(Type[]) + 0x18e
     at Microsoft.AspNetCore.Mvc.ModelBinding.Validation.DefaultCollectionValidationStrategy.<>c.<GetEnumeratorForElementType>b__5_0(Type type) + 0x42
     at System.Collections.Concurrent.ConcurrentDictionary`2.GetOrAdd(TKey, Func`2) + 0x85
     at Microsoft.AspNetCore.Mvc.ModelBinding.Validation.DefaultCollectionValidationStrategy.GetEnumeratorForElementType(ModelMetadata, Object) + 0x5d
     at Microsoft.AspNetCore.Mvc.ModelBinding.Validation.DefaultCollectionValidationStrategy.GetChildren(ModelMetadata, String, Object) + 0x19
     at Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidationVisitor.VisitChildren(IValidationStrategy) + 0x63
     at Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidationVisitor.VisitComplexType(IValidationStrategy) + 0x3e
     at Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidationVisitor.VisitImplementation(ModelMetadata&, String&, Object) + 0x273
     at Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidationVisitor.Visit(ModelMetadata, String, Object) + 0x5d
     at Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidationVisitor.Validate(ModelMetadata, String, Object, Boolean, Object) + 0xcd
     at Microsoft.AspNetCore.Mvc.ModelBinding.ObjectModelValidator.Validate(ActionContext, ValidationStateDictionary, String, Object, ModelMetadata, Object) + 0x67
     at Microsoft.AspNetCore.Mvc.ModelBinding.ParameterBinder.EnforceBindRequiredAndValidate(ObjectModelValidator, ActionContext, ParameterDescriptor, ModelMetadata, ModelBindingContext, ModelBindingResult, Object) + 0x12f*/
    [HttpPost(Name = "PostSayHello")]
    public string PostSayHello([FromBody] Message?[] message)
    {
        return message.ToString();
    }

    /* System.NotSupportedException: 'Microsoft.AspNetCore.Mvc.ModelBinding.Validation.DefaultCollectionValidationStrategy.GetEnumerator[Mvc.SourceGen.Web.Controllers.Message](System.Object)' is missing native code. MethodInfo.MakeGenericMethod() is not compatible with AOT compilation. Inspect and fix AOT related warnings that were generated when the app was published. For more information see https://aka.ms/nativeaot-compatibility
         at System.Reflection.Runtime.MethodInfos.RuntimeNamedMethodInfo`1.GetUncachedMethodInvoker(RuntimeTypeInfo[], MemberInfo) + 0x2f
         at System.Reflection.Runtime.MethodInfos.RuntimeNamedMethodInfo`1.MakeGenericMethod(Type[]) + 0x18e
         at Microsoft.AspNetCore.Mvc.ModelBinding.Validation.DefaultCollectionValidationStrategy.<>c.<GetEnumeratorForElementType>b__5_0(Type type) + 0x42
         at System.Collections.Concurrent.ConcurrentDictionary`2.GetOrAdd(TKey, Func`2) + 0x85
         at Microsoft.AspNetCore.Mvc.ModelBinding.Validation.DefaultCollectionValidationStrategy.GetEnumeratorForElementType(ModelMetadata, Object) + 0x5d
         at Microsoft.AspNetCore.Mvc.ModelBinding.Validation.DefaultCollectionValidationStrategy.GetChildren(ModelMetadata, String, Object) + 0x19
         at Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidationVisitor.VisitChildren(IValidationStrategy) + 0x63
         at Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidationVisitor.VisitComplexType(IValidationStrategy) + 0x3e
         at Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidationVisitor.VisitImplementation(ModelMetadata&, String&, Object) + 0x273
         at Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidationVisitor.Visit(ModelMetadata, String, Object) + 0x5d
         at Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidationVisitor.Validate(ModelMetadata, String, Object, Boolean, Object) + 0xcd
         at Microsoft.AspNetCore.Mvc.ModelBinding.ObjectModelValidator.Validate(ActionContext, ValidationStateDictionary, String, Object, ModelMetadata, Object) + 0x67
         at Microsoft.AspNetCore.Mvc.ModelBinding.ParameterBinder.EnforceBindRequiredAndValidate(ObjectModelValidator, ActionContext, ParameterDescriptor, ModelMetadata, ModelBindingContext, ModelBindingResult, Object) + 0x12f*/
    [HttpPost("ienumerable", Name = "PostSayHelloArray")]
    public string PostSayHello2([FromBody] IEnumerable<Message> message)
    {
        return message.ToString();
    }

    [HttpPost("simple", Name = "PostSayHelloSimple")]
    public string PostSayHello2([FromBody] Message message)
    {
        return message.ToString();
    }

    [HttpPost("simple-ref", Name = "PostSayHelloSimpleRef")]
    public string PostSayHello2([FromBody] MessageRef message)
    {
        return message.ToString();
    }

    [HttpGet("record", Name = "SayHelloRecord")]
    public string SayHelloJson([FromQuery] MessageV2 message)
    {
        return message.ToString();
    }

    [HttpGet("json", Name = "SayHelloJson")]
    public IActionResult SayHelloJson([FromQuery] MessageRef message)
    {
        return Ok(new MessageResponse() { Text = message.ToString() });
    }

    /*
     *  System.TypeInitializationException: A type initializer threw an exception. To determine which type, inspect the InnerException's StackTrace property.
       ---> System.ArgumentException: Instance property 'Result' is not defined for type 'Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingContext' (Parameter 'propertyName')
         at System.Linq.Expressions.Expression.Property(Expression, String) + 0xd4
         at Microsoft.AspNetCore.Mvc.ModelBinding.Binders.TryParseModelBinder..cctor() + 0x16a
         at System.Runtime.CompilerServices.ClassConstructorRunner.EnsureClassConstructorRun(StaticClassConstructionContext*) + 0xc4*/
    [HttpGet("{id}", Name = "ById")]
    public IActionResult GetById(int id)
    {
        return Ok(new MessageResponse() { });
    }

}

public record MessageV2(string Text, string? Name)
{

    public override string ToString()
    {
        //
        return $"{Name ?? "unknown"} says: {Text}";
    }
}

public class MessageRef
{
    public string Text { get; set; }
    public string? Name { get; set; }

    public override string ToString()
    {
        return $"{Name ?? "unknown"} says: {Text}";
    }
}


public struct Message
{
    public string Text { get; set; }
    public string? Name { get; set; }

    public override string ToString()
    {
        return $"{Name ?? "unknown"} says: {Text}";
    }
}

public class MessageResponse
{
    public string Text { get; init; }
}