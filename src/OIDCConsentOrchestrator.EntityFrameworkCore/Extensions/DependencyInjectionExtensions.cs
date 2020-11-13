using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OIDCConsentOrchestrator.EntityFrameworkCore.AutoMapper;
using OIDCConsentOrchestrator.EntityFrameworkCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.EntityFrameworkCore.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddCosmosDbContextOptionsProvider(
                  this IServiceCollection services)
        {
            services.AddSingleton<IDbContextOptionsProvider, CosmosDbContextOptionsProvider>();
            return services;
        }
        public static IServiceCollection AddPostgresDbContextOptionsProvider(
              this IServiceCollection services)
        {
            services.AddSingleton<IDbContextOptionsProvider, PostgresDbContextOptionsProvider>();
            return services;
        }
        public static IServiceCollection AddInMemoryDbContextOptionsProvider(
           this IServiceCollection services)
        {
            services.AddSingleton<IDbContextOptionsProvider, InMemoryDbContextOptionsProvider>();
            return services;
        }
        public static IServiceCollection AddDbContextOIDCConsentOrchestrator(this IServiceCollection services)
        {
            services.TryAddScoped<IConfigurationEntityCoreContext, ConfigurationEntityCoreContext>();
            services.TryAddScoped<IOIDCConsentOrchestratorAdmin, OIDCConsentOrchestratorAdmin>();
            
            var mapperOneToOne = MapperConfigurationBuilder.BuidOneToOneMapper;
            var mapperIgnoreBase = MapperConfigurationBuilder.BuidIgnoreBaseMapper;
            services.AddSingleton<IEntityFrameworkMapperAccessor>(new EntityFrameworkMapperAccessor
            {
                MapperOneToOne = mapperOneToOne,
                MapperIgnoreBase = mapperIgnoreBase
            });

            services.AddDbContext<ConfigurationEntityCoreContext>((serviceProvider, optionsBuilder) => {
                var dbContextOptionsProvider = serviceProvider.GetRequiredService<IDbContextOptionsProvider>();
                dbContextOptionsProvider.Configure(optionsBuilder);
            });

            return services;
        }
    }
}
