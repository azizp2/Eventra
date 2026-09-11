using Eventra.Core.Abstractions;
using Eventra.SampleApp.Events;
using Microsoft.Extensions.Logging;

namespace Eventra.SampleApp.Handlers;

/// <summary>
/// Handler kedua untuk event yang sama — membuktikan bahwa
/// satu event bisa punya banyak handler.
/// </summary>
public sealed class LogOnProductCreatedHandler
    : IDomainEventHandler<ProductCreatedEvent>
{
    private readonly ILogger<LogOnProductCreatedHandler> _logger;

    public LogOnProductCreatedHandler(
        ILogger<LogOnProductCreatedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        ProductCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "📝 [AUDIT LOG] Produk '{Name}' dibuat pada {OccurredOn} UTC.",
            notification.ProductName,
            notification.OccurredOn);

        return Task.CompletedTask;
    }
}