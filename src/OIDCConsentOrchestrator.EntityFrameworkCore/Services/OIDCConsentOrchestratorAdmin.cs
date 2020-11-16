using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using OIDCConsentOrchestrator.EntityFrameworkCore.AutoMapper;
using Common;

namespace OIDCConsentOrchestrator.EntityFrameworkCore.Services
{
    internal class OIDCConsentOrchestratorAdmin : IOIDCConsentOrchestratorAdmin
    {
        private string GuidS => Guid.NewGuid().ToString();
        private IConfigurationEntityCoreContext _context;
        private IEntityFrameworkMapperAccessor _entityFrameworkMapperAccessor;
        private ILogger<OIDCConsentOrchestratorAdmin> _logger;

        public OIDCConsentOrchestratorAdmin(
            IConfigurationEntityCoreContext context,
            IEntityFrameworkMapperAccessor entityFrameworkMapperAccessor,
            ILogger<OIDCConsentOrchestratorAdmin> logger) 
        {
            _context = context;
            _entityFrameworkMapperAccessor = entityFrameworkMapperAccessor;
            _logger = logger;
        }

        #region ExternalServices
        public async Task<List<ExternalServiceEntity>> GetAllExternalServiceEntitiesAsync()
        {
            return _context.ExternalServices.ToList();
        }

        public async Task<ExternalServiceEntity> UpsertEntityAsync(ExternalServiceEntity entity)
        {
            
            var utcNow = DateTime.UtcNow;
            var query = from item in _context.ExternalServices
                        where item.Name == entity.Name
                        select item;
            var result = entity;
            var entityInDb = query.FirstOrDefault();
            if(entityInDb != null)
            {
                entityInDb.Updated = utcNow;
                _entityFrameworkMapperAccessor.MapperIgnoreBaseAndForeignTables.Map(entity, entityInDb);
                result = entityInDb;
            }
            else
            {
                entity.Id = GuidS;
                entity.Created = utcNow;
                entity.Updated = utcNow;
                _context.ExternalServices.Add(entity);

            }
            await _context.SaveChangesAsync();
            return result;
        }
        #endregion

        #region DownstreamOIDCConfiguration
        public async Task<RedirectUriEntity> UpsertEntityAsync(
            string downstreamOIDCConfigurationId,
            string oidcClientConfigurationId,
            RedirectUriEntity entity)
        {
            var utcNow = DateTime.UtcNow;
            RedirectUriEntity result = null;
            var oidcClientConfiguration = (from item in _context.OIDCClientConfigurations
                                          where item.DownstreamOIDCConfigurationFK == downstreamOIDCConfigurationId &&
                                          item.Id == oidcClientConfigurationId
                                          select item).FirstOrDefault();
            if(oidcClientConfiguration == null)
            {
                throw new Exception("Item not present");
            }
            var entityByNameInDb = (from item in oidcClientConfiguration.RedirectUris
                              where item.RedirectUri == entity.RedirectUri
                              select item).FirstOrDefault();
            if(entityByNameInDb != null)
            {
                result = entityByNameInDb;
                // already here, so lets delete the entity that is trying to chang into this one.
                var entityInDb = (from item in oidcClientConfiguration.RedirectUris
                                                   where item.Id == entity.Id
                                                   select item).FirstOrDefault();
                if(entityInDb != null)
                {
                    oidcClientConfiguration.RedirectUris.Remove(entityInDb);
                }
            }
            else
            {
                var entityInDb = (from item in oidcClientConfiguration.RedirectUris
                                  where item.Id == entity.Id
                                  select item).FirstOrDefault();
                if (entityInDb != null)
                {
                    result = entityInDb;
                    // update
                    entityInDb.Updated = utcNow;
                    _entityFrameworkMapperAccessor.MapperIgnoreBaseAndForeignTables.Map(entity, entityInDb);
                }
                else
                {
                    // brand new
                    entity.Id = GuidS;
                    entity.Created = utcNow;
                    entity.Updated = utcNow;
                    entity.OIDCClientConfigurationFK = oidcClientConfigurationId;

                    oidcClientConfiguration.RedirectUris.Add(entity);
                    result = entity;
                }
            }
            await _context.SaveChangesAsync();
            return result;
        }

        public async Task DeleteEntityAsync(RedirectUriEntity entity)
        {
            var entityInDb = _context.RedirectUris.Find(entity.Id);
            if (entityInDb != null)
            {
                _context.RedirectUris.Remove(entityInDb);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<OIDCClientConfigurationEntity> UpsertEntityAsync(
             string downstreamOIDCConfigurationId, 
             OIDCClientConfigurationEntity entity)
        {

            var utcNow = DateTime.UtcNow;
            OIDCClientConfigurationEntity result = null;
            var entityInDb = (from item in _context.OIDCClientConfigurations
                                           where item.DownstreamOIDCConfigurationFK == downstreamOIDCConfigurationId &&
                                           item.Id == entity.Id
                                           select item).FirstOrDefault();
            if(entityInDb != null)
            {
                // update
                _entityFrameworkMapperAccessor.MapperIgnoreBaseAndForeignTables.Map(entity, entityInDb);
                result = entity;
            }
            else
            {
                // brand new.
                entity.Id = GuidS;
                entity.Created = utcNow;
                entity.Updated = utcNow;
                entity.DownstreamOIDCConfigurationFK = downstreamOIDCConfigurationId;
                if (entity.RedirectUris != null)
                {
                    foreach (var ru in entity.RedirectUris)
                    {
                        ru.Id = GuidS;
                        ru.Created = utcNow;
                        ru.Updated = utcNow;
                    }
                }
                _context.OIDCClientConfigurations.Add(entity);
                result = entity;
            }

            await _context.SaveChangesAsync();
            return result;
        }

        public async Task DeleteEntityAsync(OIDCClientConfigurationEntity entity)
        {
            var entityInDb = _context.OIDCClientConfigurations.Find(entity.Id);
            if (entityInDb != null)
            {
                _context.OIDCClientConfigurations.Remove(entityInDb);
                await _context.SaveChangesAsync();
            }

        }

        public async Task<DownstreamOIDCConfigurationEntity> UpsertEntityAsync(DownstreamOIDCConfigurationEntity entity)
        {
            var utcNow = DateTime.UtcNow;
            DownstreamOIDCConfigurationEntity result = null;
            var entityInDb = (from item in _context.DownstreamOIDCConfigurations
                              where item.Name == entity.Name
                              select item).FirstOrDefault();
            if (entityInDb != null)
            {
                // update
                entityInDb.Updated = utcNow;
                _entityFrameworkMapperAccessor.MapperIgnoreBaseAndForeignTables.Map(entity, entityInDb);
                result = entity;
            }
            else
            {
                // brand new.
                entity.Id = GuidS;
                entity.Created = utcNow;
                entity.Updated = utcNow;
                if(entity.OIDCClientConfigurations != null)
                {
                    foreach(var item in entity.OIDCClientConfigurations)
                    {
                        item.Id = GuidS;
                        item.Created = utcNow;
                        item.Updated = utcNow;
                        if(item.RedirectUris != null)
                        {
                            foreach(var ru in item.RedirectUris)
                            {
                                ru.Id = GuidS;
                                ru.Created = utcNow;
                                ru.Updated = utcNow;
                            }
                        }
                    }
                }
                _context.DownstreamOIDCConfigurations.Add(entity);
                result = entity;
            }

            await _context.SaveChangesAsync();
            return result;
        }

        public async Task DeleteEntityAsync(DownstreamOIDCConfigurationEntity entity)
        {
            var entityInDb = _context.DownstreamOIDCConfigurations.Find(entity.Id);
            if(entityInDb != null)
            {
                _context.DownstreamOIDCConfigurations.Remove(entityInDb);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<DownstreamOIDCConfigurationEntity>> GetAllDownstreamOIDCConfigurationEntitiesAsync()
        {
            return _context.DownstreamOIDCConfigurations.ToList();
        }

        public async Task<DownstreamOIDCConfigurationEntity> GetDownStreamOIDCConfigurationByNameAsync(string name)
        {
            var config = (from item in _context.DownstreamOIDCConfigurations
                          where item.Name == name
                          select item).FirstOrDefault();
            return config;
        }
        #endregion
    }

}
