namespace Eventra.Abstractions;

/// <summary>
/// Marker interface untuk semua domain event.
/// </summary>
/// <remarks>
/// Domain event adalah sesuatu yang terjadi di domain yang menarik untuk
/// diketahui oleh bagian lain dari sistem.
/// </remarks>
public interface IDomainEvent
{
    /// <summary>
    /// ID unik untuk event ini.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Waktu (UTC) saat event ini terjadi.
    /// </summary>
    DateTime OccurredOn { get; }
}