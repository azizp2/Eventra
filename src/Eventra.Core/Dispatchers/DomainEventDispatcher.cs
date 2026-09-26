using System.Collections.Concurrent;
using System.Reflection;
using Eventra.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Eventra.Core.Dispatchers;

/// <summary>
/// Implementasi default dari <see cref="IDomainEventDispatcher"/>.
/// Resolve handler langsung dari DI container dengan delegate caching — tanpa MediatR.
/// </summary>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    // Cache invoker function per tipe event untuk menghindari refleksi berulang pada hot-path
    private static readonly ConcurrentDictionary<Type, Func<IServiceProvider, IDomainEvent, CancellationToken, Task>>
        InvokerCache = new();

    public DomainEventDispatcher(
        IServiceProvider serviceProvider,
        ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (var domainEvent in domainEvents)
        {
            if (domainEvent is null) continue;

            try
            {
                _logger.LogInformation(
                    "Dispatching domain event: {EventName} (Id: {EventId})",
                    domainEvent.GetType().Name,
                    domainEvent.Id);

                var invoker = InvokerCache.GetOrAdd(domainEvent.GetType(), CreateInvoker);
                await invoker(_serviceProvider, domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                // PENTING: Error di handler TIDAK boleh menggagalkan transaksi utama.
                // Kita hanya log error-nya, lalu lanjut ke event berikutnya.
                _logger.LogError(
                    ex,
                    "Error handling domain event: {EventName} (Id: {EventId})",
                    domainEvent.GetType().Name,
                    domainEvent.Id);
            }
        }
    }

    private static Func<IServiceProvider, IDomainEvent, CancellationToken, Task> CreateInvoker(Type eventType)
    {
        var method = typeof(DomainEventDispatcher)
            .GetMethod(nameof(InvokeHandlersAsync), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(eventType);

        return (Func<IServiceProvider, IDomainEvent, CancellationToken, Task>)
            Delegate.CreateDelegate(typeof(Func<IServiceProvider, IDomainEvent, CancellationToken, Task>), method);
    }

    private static async Task InvokeHandlersAsync<TEvent>(
        IServiceProvider serviceProvider,
        IDomainEvent domainEvent,
        CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        var handlers = serviceProvider.GetServices<IDomainEventHandler<TEvent>>();
        var typedEvent = (TEvent)domainEvent;

        foreach (var handler in handlers)
        {
            if (handler is null) continue;
            await handler.HandleAsync(typedEvent, cancellationToken);
        }
    }
}