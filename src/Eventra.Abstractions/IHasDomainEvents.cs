namespace Eventra.Abstractions;

/// <summary>
/// Kontrak untuk entity yang memiliki domain event pending.
/// </summary>
/// <remarks>
/// Biasanya diimplementasikan oleh base class <c>Entity</c> di
/// <c>Eventra.Core</c>. Tapi bisa juga diimplementasikan manual
/// jika ingin kontrol penuh.
/// </remarks>
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