using AutoMapper;
using SearchService.Api.Models.Domain;
using SharedContracts.Events.Auctions;

namespace SearchService.Api.RequestHelpers
{
    public class MappingConfigurations : Profile
    {
        public MappingConfigurations()
        {
            CreateMap<AuctionCreated, Item>();
            CreateMap<AuctionUpdated, Item>();
        }
    }
}
