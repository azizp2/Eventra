using Eventra.SampleApp.Events;

namespace Eventra.SampleApp.Entity;

public class Product : Core.BaseClasses.Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    // Constructor private untuk EF Core.
    private Product() { }

    /// <summary>
    /// Factory method untuk membuat Product baru.
    /// Memicu <see cref="ProductCreatedEvent"/>.
    /// </summary>
    public static Product Create(string name)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        // Raise event — akan di-dispatch setelah SaveChanges sukses.
        product.RaiseDomainEvent(new ProductCreatedEvent(product.Id, name));

        return product;
    }
}