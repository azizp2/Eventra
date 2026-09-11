using System.Reflection;
using Eventra.Core.Abstractions;
using Eventra.Core.Dispatchers;
using Microsoft.Extensions.DependencyInjection;

namespace Eventra.Core.Extensions;


/// <summary>
/// Extension methods untuk registrasi domain event framework ke DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Mendaftarkan <see cref="IDomainEventDispatcher"/> dan semua handler
    /// domain event yang ada di assembly yang diberikan.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="assemblies">Assembly yang akan di-scan untuk handler.</param>
    public static IServiceCollection AddDomainEvents(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        // Dispatcher sebagai Scoped karena bergantung pada IMediator (yang juga Scoped).
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // MediatR akan otomatis scan semua INotificationHandler<> di assembly.
        // Karena IDomainEventHandler<T> mewarisi INotificationHandler<T>,
        // semua handler kita akan ter-register.
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies);
        });

        return services;
    }
}