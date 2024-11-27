using AutoMapper;
using MassTransit;
using MongoDB.Entities;
using SearchService.Api.Models.Domain;
using SharedContracts.Events.Auctions;

namespace SearchService.Api.Consumers
{
    public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
    {
        private readonly IMapper _mapper;

        public AuctionCreatedConsumer(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
            Console.WriteLine("--- Consuming auction created:" + context.Message.Id);

            var item = _mapper.Map<Item>(context.Message);

            if(item.Model == "Foo")
            {
                throw new ArgumentException("Cannot sell cars with name  FOO");
            }

            await item.SaveAsync();
        }
    }
}
