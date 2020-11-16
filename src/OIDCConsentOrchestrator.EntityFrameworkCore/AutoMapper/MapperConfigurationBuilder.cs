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
        public static IMapper BuidIgnoreBaseAndForeignTablesMapper => BuidIgnoreBaseAndForeignTablesMapperConfiguration.CreateMapper();
        public static MapperConfiguration BuidIgnoreBaseAndForeignTablesMapperConfiguration => new MapperConfiguration(cfg =>
        {

            cfg.CreateMap<ExternalServiceEntity, ExternalServiceEntity>()
                .Ignore(record => record.Id)
                .Ignore(record => record.Created)
                .Ignore(record => record.Updated);

            cfg.CreateMap<RedirectUriEntity, RedirectUriEntity>()
                .Ignore(record => record.Id)
                .Ignore(record => record.Created)
                .Ignore(record => record.Updated);

            cfg.CreateMap<OIDCClientConfigurationEntity, OIDCClientConfigurationEntity>()
                .Ignore(record => record.Id)
                .Ignore(record => record.Created)
                .Ignore(record => record.Updated)
                .Ignore(record => record.RedirectUris);

            cfg.CreateMap<DownstreamOIDCConfigurationEntity, DownstreamOIDCConfigurationEntity>()
                .Ignore(record => record.Id)
                .Ignore(record => record.Created)
                .Ignore(record => record.Updated)
                .Ignore(record => record.OIDCClientConfigurations);

        });
        public static IMapper BuidOneToOneMapper => BuidOneToOneMapperConfiguration.CreateMapper();
        public static MapperConfiguration BuidOneToOneMapperConfiguration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BaseEntity, BaseEntity>();
            cfg.CreateMap<ExternalServiceEntity, ExternalServiceEntity>();
            cfg.CreateMap<DownstreamOIDCConfigurationEntity, DownstreamOIDCConfigurationEntity>();
            cfg.CreateMap<OIDCClientConfigurationEntity, OIDCClientConfigurationEntity>();
            cfg.CreateMap<RedirectUriEntity, RedirectUriEntity>();

            
        });
    }
}
