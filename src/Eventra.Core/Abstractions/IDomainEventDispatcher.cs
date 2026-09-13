using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Eventra.Core.Abstractions;

public interface IDomainEventDispatcher
{
    /// <summary>
    /// Mengirim sekumpulan domain event ke handler masing-masing secara asynchronous.
    /// </summary>
    /// <param name="domainEvents">Koleksi event yang akan di-dispatch.</param>
    /// <param name="cancellationToken">Token pembatalan.</param>
    Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default);
}