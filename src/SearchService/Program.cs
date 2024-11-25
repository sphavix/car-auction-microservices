using SearchService.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapControllers();

try
{
    await DatabaseInitializer.Initialize(app);
}
catch(Exception e)
{
    Console.WriteLine(e);
}

app.Run();
