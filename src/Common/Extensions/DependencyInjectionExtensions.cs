using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddNullDataProtection(this IServiceCollection services)
        {
            services.AddSingleton<IDataProtection, NullDataProtection>();
            return services;
        }
        public static IServiceCollection AddDataProtection(this IServiceCollection services)
        {
            services.AddSingleton<IDataProtection, DataProtection>();
            return services;
        }
    }
}
