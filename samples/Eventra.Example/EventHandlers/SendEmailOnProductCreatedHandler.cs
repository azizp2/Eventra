using Eventra.Abstractions;
using Eventra.SampleApp.Events;
using Microsoft.Extensions.Logging;

namespace Eventra.SampleApp.Handlers;

/// <summary>
/// Handler yang mensimulasikan pengiriman email saat produk dibuat.
/// </summary>
public sealed class SendEmailOnProductCreatedHandler(ILogger<SendEmailOnProductCreatedHandler> logger)
    : IDomainEventHandler<ProductCreatedEvent>
{
    public Task HandleAsync(ProductCreatedEvent notification, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "📧 [EMAIL] Produk '{Name}' (Id: {Id}) baru dibuat. Mengirim email ke admin...",
            notification.ProductName,
            notification.ProductId);

        return Task.CompletedTask;
    }
}