using Eventra.Core.Abstractions;

namespace Eventra.Core.BaseClasses;

/// <summary>
/// Base class untuk semua entity dengan identitas bertipe <typeparamref name="TId"/>.
/// </summary>
/// <typeparam name="TId">Tipe identitas (Guid, int, string, dll).</typeparam>
public abstract class Entity<TId> : IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Identitas unik entity.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <inheritdoc />
    public IReadOnlyCollection<IDomainEvent> DomainEvents
        => _domainEvents.AsReadOnly();

    /// <summary>
    /// Menambahkan domain event ke antrian.
    /// Bersifat <c>protected</c> agar hanya bisa dipanggil dari dalam entity.
    /// </summary>
    /// <param name="domainEvent">Event yang akan ditambahkan.</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <inheritdoc />
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

/// <summary>
/// Base class untuk entity dengan identitas <see cref="Guid"/>.
/// Alias untuk backward compatibility.
/// </summary>
public abstract class Entity : Entity<Guid>
{
}