
using Polly;
using Polly.Extensions.Http;
using SearchService.Api.Services;
using SearchService.Persistence;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy());

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
