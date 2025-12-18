using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper
{
    public static class ServiceLocator
    {
        private static IServiceProvider? _serviceProvider;

        public static IServiceProvider? Provider => _serviceProvider;

        public static void Initialize(IServiceProvider? serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static T GetRequiredService<T>() where T : notnull
        {
            if (_serviceProvider is null)
                throw new Exception("ServiceProvider не инициализирован");

            return _serviceProvider.GetRequiredService<T>();
        }
    }
}
