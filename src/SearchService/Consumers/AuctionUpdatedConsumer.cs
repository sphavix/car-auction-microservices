using AutoMapper;
using MassTransit;
using MongoDB.Entities;
using SearchService.Api.Models.Domain;
using SharedContracts.Events.Auctions;

namespace SearchService.Api.Consumers
{
    public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
    {
        private readonly IMapper _mapper;

        public AuctionUpdatedConsumer(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task Consume(ConsumeContext<AuctionUpdated> context)
        {
            Console.WriteLine("--- Consuming auction updated: " + context.Message.Id);

            // map the event {auctionUpdated} to domain entity
            var item = _mapper.Map<Item>(context.Message);

            // update properties from the domain entity
            var result = await DB.Update<Item>()
                .Match(x => x.ID == context.Message.Id) // find the item to update from db by using Match
                .ModifyOnly(x => new
                {
                    x.Color,
                    x.Make,
                    x.Model,
                    x.Year,
                    x.Mileage
                }, item).ExecuteAsync();

            if (!result.IsAcknowledged)
            {
                throw new MessageException(typeof(AuctionUpdated), "Error occured while trying to update the database");
            }
        }
    }
}
