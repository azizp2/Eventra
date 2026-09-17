using Eventra.Abstractions;

namespace Eventra.Core.BaseClasses;

/// <summary>
/// Base record untuk semua domain event.
/// Menyediakan metadata (<see cref="Id"/> dan <see cref="OccurredOn"/>) secara otomatis.
/// </summary>
/// <remarks>
/// Gunakan <c>record</c> untuk event konkret agar immutable dan value-based equality:
/// <code>
/// public sealed record ProductCreatedEvent(
///     Guid ProductId,
///     string ProductName
/// ) : DomainEvent;
/// </code>
/// </remarks>
public abstract record DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public Guid Id { get; }

    /// <inheritdoc />
    public DateTime OccurredOn { get; }

    /// <summary>
    /// Constructor yang menginisialisasi <see cref="Id"/> dan <see cref="OccurredOn"/>.
    /// </summary>
    /// <param name="timeProvider">
    /// Time provider opsional. Default: <see cref="TimeProvider.System"/>.
    /// Berguna untuk unit test yang butuh waktu deterministic.
    /// </param>
    protected DomainEvent(TimeProvider? timeProvider = null)
    {
        var now = (timeProvider ?? TimeProvider.System).GetUtcNow();
        Id = Guid.NewGuid();
        OccurredOn = now.UtcDateTime;
    }
}