using Eventra.Core.BaseClasses;
using Eventra.Example.Events;

namespace Eventra.Example.Entity;

public class Product : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;

    // Constructor private untuk EF Core.
    private Product() { }

    /// <summary>
    /// Factory method untuk membuat Product baru.
    /// Memicu <see cref="ProductCreatedEvent"/>.
    /// </summary>
    public static Product Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        
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