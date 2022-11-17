using Mvc.SourceGen.Web.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.AddContext<SourceGenJsonContext>())
    .AddSourceGeneratorProviders();

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