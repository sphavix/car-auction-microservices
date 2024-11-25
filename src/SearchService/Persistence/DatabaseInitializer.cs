using MongoDB.Entities;
using MongoDB.Driver;
using SearchService.Api.Models.Domain;
using System.Text.Json;
using SearchService.Api.Services;


namespace SearchService.Persistence
{
    public class DatabaseInitializer
    {
        public static async Task Initialize(WebApplication app)
        {
            await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(
                    app.Configuration.GetConnectionString("MongoDbConnection")));

            await DB.Index<Item>()
                .Key(x => x.Make, KeyType.Text)
                .Key(x => x.Model, KeyType.Text)
                .Key(x => x.Color, KeyType.Text)
                .CreateAsync(); // index data for search optimization.

            var count = await DB.CountAsync<Item>();

            using var scope = app.Services.CreateScope();

            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();

            var items = await httpClient.GetItemsForSearchSvc();

            Console.WriteLine(items.Count + " returned items from the auction service");

            if(items.Count > 0)
            {
                await DB.SaveAsync(items);
            }
        }
    }
}