using Eventra.Abstractions;
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
    /// <remarks>
    /// <para>
    /// Alur method ini:
    /// </para>
    /// <list type="number">
    ///   <item>Kumpulkan entity yang punya domain event <b>sebelum</b> SaveChanges.</item>
    ///   <item>Snapshot event ke list terpisah (karena entity akan di-clear).</item>
    ///   <item>Panggil <see cref="DbContext.SaveChangesAsync(CancellationToken)"/>.</item>
    ///   <item>Clear domain event dari entity <b>setelah</b> SaveChanges sukses.</item>
    ///   <item>Dispatch event ke handler <b>setelah</b> commit sukses.</item>
    /// </list>
    /// <para>
    /// Jika <c>SaveChangesAsync</c> gagal, event <b>tidak</b> akan di-dispatch
    /// dan <b>tidak</b> akan di-clear — sehingga bisa di-retry di masa depan.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public async Task&lt;Guid&gt; Handle(
    ///     CreateProductCommand request,
    ///     CancellationToken cancellationToken)
    /// {
    ///     var product = Product.Create(request.Name);
    ///     _dbContext.Products.Add(product);
    ///
    ///     // Commit + dispatch event otomatis.
    ///     await _dbContext.SaveChangesAndDispatchEventsAsync(
    ///         _dispatcher,
    ///         cancellationToken);
    ///
    ///     return product.Id;
    /// }
    /// </code>
    /// </example>
    public static async Task<int> SaveChangesAndDispatchEventsAsync(
        this DbContext context,
        IDomainEventDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(dispatcher);

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
        //    dan event TIDAK akan di-dispatch.
        var result = await context.SaveChangesAsync(cancellationToken);

        // 4. Clear event dari entity.
        //    Kenapa setelah SaveChanges? Karena kalau SaveChanges gagal,
        //    event harus tetap tersimpan agar bisa di-retry di masa depan.
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