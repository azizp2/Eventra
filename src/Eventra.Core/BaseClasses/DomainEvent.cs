using Eventra.Core.Abstractions;

namespace Eventra.Core.BaseClasses;

public abstract record DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public Guid Id { get; }

    /// <inheritdoc />
    public DateTime OccurredOn { get; }

    /// <summary>
    /// Constructor yang menginisialisasi Id dan OccurredOn.
    /// Menggunakan <see cref="DateTime.UtcNow"/> agar konsisten di semua timezone.
    /// </summary>
    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}