using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Api.Consumers;
using SearchService.Api.RequestHelpers;
using SearchService.Api.Services;
using SearchService.Persistence;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy());

builder.Services.AddAutoMapper(typeof(MappingConfigurations).Assembly);

builder.Services.AddMassTransit(options =>
{
    // where to find the consumers
    options.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    // naming convensions
    options.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    // configure endpoint & context
    options.UsingRabbitMq((context, config) =>
    {
        config.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{

    try
    {
        await DatabaseInitializer.Initialize(app);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
});


app.Run();


static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions.HandleTransientHttpError()
                            .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                            .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
