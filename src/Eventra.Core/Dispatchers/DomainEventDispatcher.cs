using Eventra.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Eventra.Core.Dispatchers;

/// <summary>
/// Implementasi default dari <see cref="IDomainEventDispatcher"/>.
/// Resolve handler langsung dari DI container — tanpa MediatR.
/// </summary>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

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
        foreach (var domainEvent in domainEvents)
        {
            try
            {
                _logger.LogInformation(
                    "Dispatching domain event: {EventName} (Id: {EventId})",
                    domainEvent.GetType().Name,
                    domainEvent.Id);

                await DispatchSingleEventAsync(domainEvent, cancellationToken);
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

    private async Task DispatchSingleEventAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        // Cari tipe handler: IDomainEventHandler<TEvent>
        var handlerType = typeof(IDomainEventHandler<>)
            .MakeGenericType(domainEvent.GetType());

        // Resolve SEMUA handler yang implement IDomainEventHandler<TEvent>
        var handlers = _serviceProvider.GetServices(handlerType);

        // Ambil method HandleAsync
        var method = handlerType.GetMethod(
            nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;

        // Panggil HandleAsync pada masing-masing handler
        foreach (var handler in handlers)
        {
            if (handler is null) continue;

            var task = (Task)method.Invoke(
                handler,
                new object[] { domainEvent, cancellationToken })!;

            await task;
        }
    }
}