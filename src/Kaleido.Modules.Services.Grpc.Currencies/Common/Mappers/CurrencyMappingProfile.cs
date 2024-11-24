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
        // Basic entity mappings
        CreateMap<Currency, CurrencyEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForSourceMember(src => src.Denominations, opt => opt.DoNotValidate());
        CreateMap<CurrencyEntity, CurrencyWithDenominations>()
            .ForMember(dest => dest.Denominations, opt => opt.Ignore()); // Or map from appropriate source
        CreateMap<Denomination, DenominationEntity>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src =>
                Math.Round(decimal.Parse(src.Value.ToString()), 2, MidpointRounding.AwayFromZero)))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CurrencyKey, opt => opt.Ignore());

        // Response mappings
        CreateMap<EntityLifeCycleResult<CurrencyWithDenominations, BaseRevisionEntity>, CurrencyResponse>()
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Entity))
            .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision));
        CreateMap<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>, DenominationResponse>()
            .ForMember(dest => dest.Denomination, opt => opt.MapFrom(src => src.Entity))
            .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision));

        CreateMap<DenominationEntity, Denomination>();
        CreateMap<CurrencyWithDenominations, CurrencyWithDenominationsResponse>();

        // Revision mappings
        CreateMap<BaseRevisionEntity, BaseRevision>();
        CreateMap<DenominationRevisionEntity, BaseRevision>();
        CreateMap<CurrencyRevisionEntity, BaseRevision>();

        // DateTime <-> Timestamp conversions
        CreateMap<Timestamp, DateTime>().ConvertUsing(src => src.ToDateTime());
        CreateMap<DateTime, Timestamp>().ConvertUsing(src => Timestamp.FromDateTime(src.ToUniversalTime()));

        // Mapping for composite types
        CreateMap<EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>, EntityLifeCycleResult<CurrencyWithDenominations, BaseRevisionEntity>>()
            .ForMember(dest => dest.Entity, opt => opt.MapFrom(src => src.Entity))
            .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision));

        CreateMap<IEnumerable<CurrencyResponse>, CurrencyListResponse>()
            .ForMember(dest => dest.Currencies, opt => opt.MapFrom(src => src));

        // Self mappings for entity lifecycle results
        CreateMap<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>, EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>()
            .ForMember(dest => dest.Entity, opt => opt.MapFrom(src => src.Entity))
            .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision));
        CreateMap<DenominationEntity, DenominationEntity>();
        CreateMap<DenominationRevisionEntity, DenominationRevisionEntity>();

        CreateMap<EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>, EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>>()
            .ForMember(dest => dest.Entity, opt => opt.MapFrom(src => src.Entity))
            .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision));
        CreateMap<CurrencyRevisionEntity, CurrencyRevisionEntity>();
        CreateMap<CurrencyEntity, CurrencyEntity>();
    }
}