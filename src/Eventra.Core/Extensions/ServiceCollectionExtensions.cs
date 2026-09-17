using System.Reflection;
using Eventra.Abstractions;
using Eventra.Core.Dispatchers;
using Microsoft.Extensions.DependencyInjection;

namespace Eventra.Core.Extensions;

/// <summary>
/// Extension methods untuk registrasi Eventra ke DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Mendaftarkan <see cref="IDomainEventDispatcher"/> dan semua handler
    /// domain event yang ada di assembly yang diberikan.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="assemblies">Assembly yang akan di-scan untuk handler.</param>
    public static IServiceCollection AddEventra(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        // Dispatcher sebagai Scoped karena bergantung pada IServiceProvider.
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Scan semua handler di assembly yang diberikan.
        foreach (var assembly in assemblies)
        {
            RegisterHandlersFromAssembly(services, assembly);
        }

        return services;
    }

    private static void RegisterHandlersFromAssembly(
        IServiceCollection services,
        Assembly assembly)
    {
        var handlerInterfaceType = typeof(IDomainEventHandler<>);

        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == handlerInterfaceType)
                .Select(i => new { Implementation = t, Interface = i }));

        foreach (var handler in handlerTypes)
        {
            services.AddScoped(handler.Interface, handler.Implementation);
        }
    }
}   