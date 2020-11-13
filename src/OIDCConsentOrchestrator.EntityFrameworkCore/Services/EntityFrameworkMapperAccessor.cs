using AutoMapper;
using OIDCConsentOrchestrator.EntityFrameworkCore.AutoMapper;
using System;
using System.Linq;
using System.Text;

namespace OIDCConsentOrchestrator.EntityFrameworkCore.Services
{
    internal class EntityFrameworkMapperAccessor :
                        IEntityFrameworkMapperAccessor
    {
        public IMapper MapperOneToOne { get; set; }
        public IMapper MapperIgnoreBase { get; set; }
    }
}
