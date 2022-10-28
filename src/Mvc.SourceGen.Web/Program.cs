var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddSourceGenControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
