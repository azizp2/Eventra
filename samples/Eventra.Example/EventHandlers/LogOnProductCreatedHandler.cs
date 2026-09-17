using Eventra.Abstractions;
using Eventra.SampleApp.Events;
using Microsoft.Extensions.Logging;

namespace Eventra.SampleApp.Handlers;

/// <summary>
/// Handler kedua untuk event yang sama — membuktikan bahwa
/// satu event bisa punya banyak handler.
/// </summary>
public sealed class LogOnProductCreatedHandler(ILogger<LogOnProductCreatedHandler> logger)
    : IDomainEventHandler<ProductCreatedEvent>
{
    public Task HandleAsync(ProductCreatedEvent notification, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "📝 [AUDIT LOG] Produk '{Name}' dibuat pada {OccurredOn} UTC.",
            notification.ProductName,
            notification.OccurredOn);

        return Task.CompletedTask;
    }
}