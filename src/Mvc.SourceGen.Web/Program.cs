using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Mvc.SourceGen.Web;
using Mvc.SourceGen.Web.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddSourceGenControllers();

builder.Services.AddSingleton<IModelMetadataProvider, SourceGenModelMetadataProvider>();

var app = builder.Build();

app.MapControllers();

app.Run();


[JsonSerializable(typeof(Message))]
public partial class SourceGenJsonContext : JsonSerializerContext
{

}