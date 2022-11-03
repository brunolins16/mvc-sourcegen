using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Mvc.SourceGen.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddSourceGenControllers();

builder.Services.AddSingleton<IModelMetadataProvider, SourceGenModelMetadataProvider>();

var app = builder.Build();

app.MapControllers();

app.Run();