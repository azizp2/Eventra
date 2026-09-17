namespace Eventra.Abstractions;

/// <summary>
/// Handler untuk domain event tertentu.
/// </summary>
/// <typeparam name="TEvent">Tipe domain event yang di-handle.</typeparam>
/// <remarks>
/// Satu domain event bisa memiliki banyak handler.
/// Semua handler akan dipanggil secara berurutan oleh dispatcher.
/// </remarks>
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    /// <summary>
    /// Menangani domain event.
    /// </summary>
    /// <param name="domainEvent">Domain event yang akan di-handle.</param>
    /// <param name="cancellationToken">Token pembatalan.</param>
    /// <returns>Task yang merepresentasikan operasi asynchronous.</returns>
    Task HandleAsync(
        TEvent domainEvent,
        CancellationToken cancellationToken = default);
}