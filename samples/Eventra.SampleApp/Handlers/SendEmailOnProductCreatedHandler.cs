using Eventra.Core.Abstractions;
using Eventra.SampleApp.Events;
using Microsoft.Extensions.Logging;

namespace Eventra.SampleApp.Handlers;

/// <summary>
/// Handler yang mensimulasikan pengiriman email saat produk dibuat.
/// </summary>
public sealed class SendEmailOnProductCreatedHandler
    : IDomainEventHandler<ProductCreatedEvent>
{
    private readonly ILogger<SendEmailOnProductCreatedHandler> _logger;

    public SendEmailOnProductCreatedHandler(
        ILogger<SendEmailOnProductCreatedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        ProductCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "📧 [EMAIL] Produk '{Name}' (Id: {Id}) baru dibuat. Mengirim email ke admin...",
            notification.ProductName,
            notification.ProductId);

        return Task.CompletedTask;
    }
}