namespace Eventra.Core.BaseClasses;

/// <summary>
/// Base class untuk Aggregate Root.
/// </summary>
/// <remarks>
/// Aggregate Root adalah entity utama yang menjadi pintu masuk
/// untuk semua operasi terhadap aggregate-nya. Semua perubahan
/// state harus melalui Aggregate Root.
/// </remarks>
public abstract class AggregateRoot : Entity
{
}