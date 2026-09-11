namespace Eventra.Core.Abstractions;

public interface IHasDomainEvents
{
    /// <summary>
    /// Koleksi domain event yang belum di-dispatch.
    /// Read-only dari luar untuk menjaga enkapsulasi.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Membersihkan semua domain event yang tertunda.
    /// Dipanggil setelah event berhasil di-dispatch.
    /// </summary>
    void ClearDomainEvents();
}