using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eventra.Core.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Eventra.Core.Dispatchers;

/// <summary>
/// Implementasi default dari <see cref="IDomainEventDispatcher"/>.
/// Menggunakan MediatR untuk mem-publish event ke semua handler-nya.
/// </summary>
/// 
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventDispatcher> _logger;
    private IDomainEventDispatcher _domainEventDispatcherImplementation;

    public DomainEventDispatcher(
        IMediator mediator,
        ILogger<DomainEventDispatcher> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <inheritdoc />
    /// 
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

                await _mediator.Publish(domainEvent, cancellationToken);
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
}