using System.Reflection;
using Eventra.Abstractions;
using Eventra.Core.Dispatchers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Eventra.Core.Extensions;

/// <summary>
/// Extension methods untuk registrasi Eventra ke DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Mendaftarkan <see cref="IDomainEventDispatcher"/> dan semua handler
    /// domain event yang ada di assembly yang diberikan dengan masa hidup Scoped.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="assemblies">Assembly yang akan di-scan untuk handler.</param>
    public static IServiceCollection AddEventra(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        return services.AddEventra(ServiceLifetime.Scoped, assemblies);
    }

    /// <summary>
    /// Mendaftarkan <see cref="IDomainEventDispatcher"/> dan semua handler
    /// domain event yang ada di assembly yang diberikan dengan <see cref="ServiceLifetime"/> kustom.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="lifetime">Masa hidup handler di DI (Scoped, Transient, atau Singleton).</param>
    /// <param name="assemblies">Assembly yang akan di-scan untuk handler.</param>
    public static IServiceCollection AddEventra(
        this IServiceCollection services,
        ServiceLifetime lifetime,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        // Dispatcher sebagai Scoped karena bergantung pada IServiceProvider.
        services.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        var distinctAssemblies = assemblies.Distinct();
        foreach (var assembly in distinctAssemblies)
        {
            if (assembly is null) continue;
            RegisterHandlersFromAssembly(services, assembly, lifetime);
        }

        return services;
    }

    private static void RegisterHandlersFromAssembly(
        IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime)
    {
        var handlerInterfaceType = typeof(IDomainEventHandler<>);
        var types = GetTypesSafely(assembly);

        var handlerTypes = types
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == handlerInterfaceType)
                .Select(i => new { Implementation = t, Interface = i }));

        foreach (var handler in handlerTypes)
        {
            services.TryAddEnumerable(new ServiceDescriptor(handler.Interface, handler.Implementation, lifetime));
        }
    }

    private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }
}