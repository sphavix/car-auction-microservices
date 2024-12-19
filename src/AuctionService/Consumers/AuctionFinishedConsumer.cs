using AuctionService.Api.Models.Domain;
using AuctionService.Api.Persistence;
using MassTransit;
using SharedContracts.Events.Auctions;

namespace AuctionService.Api.Consumers
{
    public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
    {
        private readonly AuctionDbContext _context;
        public AuctionFinishedConsumer(AuctionDbContext dbcontext)
        {
            _context = dbcontext ?? throw new ArgumentNullException(nameof(dbcontext));
        }


        public async Task Consume(ConsumeContext<AuctionFinished> context)
        {
            Console.WriteLine("---> Consuming auction finished event.");
            var auction = await _context.Auctions.FindAsync(context.Message.AuctionId);

            if(context.Message.ItemSold)
            {
                auction.Winner = context.Message.Winner;
                auction.SoldAmount = context.Message.Amount;
            }

            auction.Status = auction.SoldAmount > auction.ReservePrice
                ? Status.Finished : Status.ReserveNotMet;

            await _context.SaveChangesAsync();
        }

    }
}
