using AutoMapper;
using OIDCConsentOrchestrator.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.EntityFrameworkCore.AutoMapper
{
    public static class MapperConfigurationBuilder
    {
        public static IMapper BuidIgnoreBaseMapper => BuidIgnoreBaseMapperConfiguration.CreateMapper();
        public static MapperConfiguration BuidIgnoreBaseMapperConfiguration => new MapperConfiguration(cfg =>
        {

            cfg.CreateMap<ExternalServiceEntity, ExternalServiceEntity>()
                .Ignore(record => record.Id)
                .Ignore(record => record.Created)
                .Ignore(record => record.Updated);
        });
        public static IMapper BuidOneToOneMapper => BuidOneToOneMapperConfiguration.CreateMapper();
        public static MapperConfiguration BuidOneToOneMapperConfiguration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BaseEntity, BaseEntity>();
            cfg.CreateMap<ExternalServiceEntity, ExternalServiceEntity>();
        });
    }
}
