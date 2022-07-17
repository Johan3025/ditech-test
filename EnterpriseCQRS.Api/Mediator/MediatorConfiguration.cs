using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EnterpriseCQRS.Api.Mediator
{
    public static class MediatorConfiguration
    {
        private static readonly List<string> AssemblyList = new()
        {
            "EnterpriseCQRS.Data",
            "EnterpriseCQRS.Services"
        };

        /// <summary>
        /// Adds the assemblies to mediator.
        /// </summary>
        /// <param name="services">The services.</param>
        public static void AddMediatRConf(this IServiceCollection services)
        {
            foreach (var assembly in AssemblyList.Select(Assembly.Load))
            {
                services.AddMediatR(assembly);
            }
        }
    }
}
