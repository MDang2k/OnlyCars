using AutoMapper;
using Contracts;

namespace SearchService.RequestHelpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AuctionCreated, Item>()
            .ForMember(d => d.ID, o => o.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.CurrentHighBid, o => o.MapFrom(s => s.CurrentPrice));
    }
}