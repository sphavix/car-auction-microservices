using MassTransit;
using MongoDB.Entities;
using SearchService.Api.Models.Domain;
using SharedContracts.Events.Auctions;

namespace SearchService.Api.Consumers
{
    public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
    {
        public async Task Consume(ConsumeContext<AuctionDeleted> context)
        {
            Console.WriteLine("--- Consuming Auction Deleted: " + context.Message.Id);

            var result = await DB.DeleteAsync<Item>(context.Message.Id);

            if(!result.IsAcknowledged)
            {
                throw new MessageException(typeof(AuctionDeleted), "Error occured while trying to delete auction");
            }
        }
    }
}