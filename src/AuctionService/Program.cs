using AuctionService.Api.Configurations;
using AuctionService.Api.Consumers;
using AuctionService.Api.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        config.Host(builder.Configuration["RabbitMQ:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMQ:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMQ:Password", "guest"));
        });
        config.ConfigureEndpoints(context);
    });
});

// authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServiceUrl"]; // get the token from the identity server for auth
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();

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

