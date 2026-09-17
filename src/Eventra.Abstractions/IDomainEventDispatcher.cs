namespace Eventra.Abstractions;

/// <summary>
/// Dispatcher yang bertanggung jawab mengirim domain event
/// ke semua handler-nya.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Mengirim sekumpulan domain event ke handler masing-masing
    /// secara asynchronous.
    /// </summary>
    /// <param name="domainEvents">Koleksi event yang akan di-dispatch.</param>
    /// <param name="cancellationToken">Token pembatalan.</param>
    /// <returns>Task yang merepresentasikan operasi asynchronous.</returns>
    /// <remarks>
    /// Implementasi default <b>tidak</b> akan melempar exception jika
    /// salah satu handler gagal. Error akan di-log dan dispatch akan
    /// lanjut ke event berikutnya.
    /// </remarks>
    Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default);
}