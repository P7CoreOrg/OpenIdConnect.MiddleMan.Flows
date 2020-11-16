using AutoMapper;

namespace OIDCConsentOrchestrator.EntityFrameworkCore.AutoMapper
{
    public interface IEntityFrameworkMapperAccessor
    {
        IMapper MapperOneToOne { get; }
        IMapper MapperIgnoreBaseAndForeignTables { get; set; }
    }
}
