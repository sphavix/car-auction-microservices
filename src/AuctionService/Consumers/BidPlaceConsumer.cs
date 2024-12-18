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

        }
    }


}
