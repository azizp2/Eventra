using Eventra.Core.Abstractions;

namespace Eventra.Core.BaseClasses;

public abstract class Entity : IHasDomainEvents
{
    // List private: hanya bisa dimodifikasi dari dalam class ini.
    private readonly List<IDomainEvent> _domainEvents = new();

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