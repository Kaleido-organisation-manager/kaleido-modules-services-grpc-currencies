using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Common.Mappers;

public class CurrencyMappingProfile : Profile
{
    public CurrencyMappingProfile()
    {
        CreateMap<EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>, CurrencyResponse>()
            .ForMember(c => c.Currency, opt => opt.MapFrom(src => src.Entity))
            .ForMember(c => c.Revision, opt => opt.MapFrom(src => src.Revision))
            .ForMember(c => c.Key, opt => opt.MapFrom(src => src.Key));

        CreateMap<CurrencyEntity, Currency>().ReverseMap();
        CreateMap<BaseRevisionEntity, CurrencyRevision>();

        CreateMap<IEnumerable<EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>>, CurrencyListResponse>()
            .ForMember(c => c.Currencies, opt => opt.MapFrom(c => c));

        // DateTime <-> Timestamp conversions
        CreateMap<Timestamp, DateTime>().ConvertUsing(src => src.ToDateTime());
        CreateMap<DateTime, Timestamp>().ConvertUsing(src => Timestamp.FromDateTime(src.ToUniversalTime()));

    }
}