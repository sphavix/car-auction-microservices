using MongoDB.Entities;
using MongoDB.Driver;
using SearchService.Api.Models.Domain;
using System.Text.Json;


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

            if(count == 0)
            {
                Console.WriteLine("Database is empty, attempting to populate databse");

                var itemData = await File.ReadAllTextAsync("Persistence/auctions.json");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };


                var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);

                await DB.SaveAsync(items);
            }
        }
    }
}