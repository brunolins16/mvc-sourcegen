using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddSourceGenControllers();

var app = builder.Build();

app.MapControllers();

app.Run();


record Person(string Name);
record Person2(string Name);
record Person3(string Name);


[JsonSerializable(typeof(Person))]
[JsonSerializable(typeof(Person2))]
[JsonSerializable(typeof(Person3))]
partial class SampleContext : JsonSerializerContext
{

}