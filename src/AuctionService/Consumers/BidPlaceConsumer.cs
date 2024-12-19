using AuctionService.Api.Persistence;
using MassTransit;
using SharedContracts.Events.Auctions;

namespace AuctionService.Api.Consumers
{
    public class BidPlaceConsumer : IConsumer<BidPlaced>
    {
        private readonly AuctionDbContext _context;

        public BidPlaceConsumer(AuctionDbContext dbcontext)
        {
            _context = dbcontext ?? throw new ArgumentNullException(nameof(dbcontext));       
        }

        public async Task Consume(ConsumeContext<BidPlaced> context)
        {
            Console.WriteLine("--- Consuming bid placed event message.");

            var auction = await _context.Auctions.FindAsync(context.Message.AuctionId);

            if(auction.CurrentHighBid == null || context.Message.BidStatus.Contains("Accepted") 
                && context.Message.Amount > auction.CurrentHighBid)
            {
                auction.CurrentHighBid = context.Message.Amount;

                await _context.SaveChangesAsync();
            }
        }
    }


}
