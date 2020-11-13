using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using OIDCConsentOrchestrator.EntityFrameworkCore.AutoMapper;

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
                _entityFrameworkMapperAccessor.MapperIgnoreBase.Map(entity, entityInDb);
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
    }

}
