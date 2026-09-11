using Eventra.Core.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Eventra.EntityFrameworkCore.Extensions;


/// <summary>
/// Extension methods untuk integrasi domain event dengan EF Core.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Menyimpan perubahan ke database, lalu men-dispatch semua domain event
    /// yang tertunda setelah commit sukses.
    /// </summary>
    /// <param name="context">DbContext.</param>
    /// <param name="dispatcher">Domain event dispatcher.</param>
    /// <param name="cancellationToken">Token pembatalan.</param>
    /// <returns>Jumlah state entry yang tersimpan.</returns>
    public static async Task<int> SaveChangesAndDispatchEventsAsync(
        this DbContext context,
        IDomainEventDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        // 1. Ambil semua entity yang punya domain event PENDING.
        //    Kita lakukan ini SEBELUM SaveChanges agar tidak terpengaruh
        //    oleh state changes yang mungkin ditambahkan EF Core.
        var entitiesWithEvents = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // 2. Kumpulkan semua event ke list terpisah.
        //    Kita snapshot event-nya sekarang, karena nanti entity akan di-clear.
        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // 3. Simpan perubahan ke database. Jika gagal, exception akan dilempar
        //    dan event TIDAK akan di-dispatch (sesuai requirement F-09 & F-10).
        var result = await context.SaveChangesAsync(cancellationToken);

        // 4. Clear event dari entity.
        //    Kenapa setelah SaveChanges? Karena kalau SaveChanges gagal,
        //    event harus tetap tersimpan agar bisa di-retry di masa depan
        //    (walaupun saat ini belum ada retry mechanism).
        //    Setelah sukses, event sudah tidak diperlukan lagi.
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        // 5. Dispatch event SETELAH commit sukses.
        //    Ini memastikan handler hanya jalan kalau data benar-benar tersimpan.
        if (domainEvents.Count > 0)
        {
            await dispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return result;
    }
}
