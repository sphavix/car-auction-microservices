using AuctionService.Api.Models.Domain;
using AuctionService.Api.Models.Dtos;
using AutoMapper;
using SharedContracts.Events.Auctions;

namespace AuctionService.Api.Configurations
{
    public class MappingConfigurations : Profile
    {
        public MappingConfigurations()
        {
            CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);

            CreateMap<Item, AuctionDto>();

            CreateMap<CreateAuctionDto, Auction>()
                .ForMember(d => d.Item, x => x.MapFrom(src => src));

            CreateMap<CreateAuctionDto, Item>();

            // event created mapping
            CreateMap<AuctionDto, AuctionCreated>();
            CreateMap<Auction, AuctionUpdated>().IncludeMembers(x => x.Item);
            CreateMap<Item, AuctionUpdated>();
        }
    }
}
