using AuctionService.Api.Models.Domain;
using AuctionService.Api.Models.Dtos;
using AutoMapper;

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
        }
    }
}
