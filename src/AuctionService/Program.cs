using AuctionService.Api.Configurations;
using AuctionService.Api.Consumers;
using AuctionService.Api.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddDbContext<AuctionDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(typeof(MappingConfigurations).Assembly);

builder.Services.AddMassTransit(options =>
{
    //config the outbox to store messages while the service is down
    options.AddEntityFrameworkOutbox<AuctionDbContext>(x =>
    {
        x.QueryDelay = TimeSpan.FromSeconds(10);

        // use Postgres to store messages
        x.UsePostgres();
        x.UseBusOutbox();

    });
    // configure consumer 
    options.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();

    // name formatter
    options.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));


    options.UsingRabbitMq((context, config) =>
    {
        config.ConfigureEndpoints(context);
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.


app.MapControllers();

try
{
    DatabaseInitializer.Initialize(app);
}
catch(Exception e)
{
    Console.WriteLine(e);
}

app.Run();

