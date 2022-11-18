using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Mvc.SourceGen.Web.Models;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.AddContext<SourceGenJsonContext>())
    .AddMvcContext();

var app = builder.Build();

app.MapControllers();

app.Run();


//[JsonSerializable(typeof(MessageResponse))]
//[JsonSerializable(typeof(Message))]
//[JsonSerializable(typeof(IEnumerable<Message>))]
//[JsonSerializable(typeof(Message?[]))]
//[JsonSerializable(typeof(MessageRef))]
// Todo Types
[JsonSerializable(typeof(List<Todo>))]
[JsonSerializable(typeof(Todo))]
//[JsonSerializable(typeof(ValidationProblemDetails))]
public partial class SourceGenJsonContext : JsonSerializerContext
{

}