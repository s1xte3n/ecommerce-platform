// File: apps/api/src/Domain/Entities/Product.cs
using ECommerce.Domain.Base;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Product aggregate root.
/// Encapsulates product data, inventory management, and business rules.
/// </summary>
public class Product : AggregateRoot
{
    private readonly List<ProductReview> _reviews = new();

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public Money Price { get; private set; } = Money.Zero();
    public Sku Sku { get; private set; } = Sku.Empty;
    public int StockQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public ProductDimensions? Dimensions { get; private set; }
    public IReadOnlyCollection<ProductReview> Reviews => _reviews.AsReadOnly();

    private Product() { } // EF Core constructor

    public Product(
        string name,
        string description,
        Money price,
        Sku sku,
        int initialStock,
        Guid categoryId)
    {
        Id = Guid.NewGuid();
        SetName(name);
        Description = description?.Trim() ?? throw new DomainException("Description is required");
        Price = price ?? throw new DomainException("Price is required");
        Sku = sku ?? throw new DomainException("SKU is required");

        if (initialStock < 0)
            throw new DomainException("Initial stock cannot be negative");

        StockQuantity = initialStock;
        ReservedQuantity = 0;
        CategoryId = categoryId;
        IsActive = true;

        AddDomainEvent(new ProductCreatedEvent(Id, Name, Sku.Value));
    }

    public int AvailableStock => StockQuantity - ReservedQuantity;

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required");
        if (name.Trim().Length > 200)
            throw new DomainException("Product name cannot exceed 200 characters");

        Name = name.Trim();
        Slug = GenerateSlug(Name);
    }

    public void UpdatePrice(Money newPrice)
    {
        if (newPrice.Amount < 0)
            throw new DomainException("Price cannot be negative");

        var oldPrice = Price;
        Price = newPrice;
        MarkUpdated();

        AddDomainEvent(new ProductPriceUpdatedEvent(Id, oldPrice.Amount, newPrice.Amount));
    }

    /// <summary>
    /// Reserves stock during checkout to prevent overselling.
    /// </summary>
    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Reserve quantity must be positive");

        if (AvailableStock < quantity)
            throw new DomainException(
                $"Insufficient stock. Available: {AvailableStock}, Requested: {quantity}");

        ReservedQuantity += quantity;
        MarkUpdated();

        AddDomainEvent(new StockReservedEvent(Id, quantity, AvailableStock));
    }

    /// <summary>
    /// Confirms reservation and permanently reduces stock.
    /// </summary>
    public void ConfirmReservation(int quantity)
    {
        if (ReservedQuantity < quantity)
            throw new DomainException("Cannot confirm more than reserved quantity");

        ReservedQuantity -= quantity;
        StockQuantity -= quantity;
        MarkUpdated();

        AddDomainEvent(new StockConfirmedEvent(Id, quantity, StockQuantity));

        if (StockQuantity <= 5)
            AddDomainEvent(new LowStockEvent(Id, StockQuantity));
    }

    /// <summary>
    /// Releases reserved stock back to available inventory.
    /// </summary>
    public void ReleaseReservation(int quantity)
    {
        if (ReservedQuantity < quantity)
            throw new DomainException("Cannot release more than reserved quantity");

        ReservedQuantity -= quantity;
        MarkUpdated();
    }

    public void AddReview(Guid userId, int rating, string comment)
    {
        if (rating < 1 || rating > 5)
            throw new DomainException("Rating must be between 1 and 5");

        if (_reviews.Any(r => r.UserId == userId))
            throw new DomainException("User has already reviewed this product");

        var review = new ProductReview(Id, userId, rating, comment?.Trim() ?? string.Empty);
        _reviews.Add(review);
        MarkUpdated();

        AddDomainEvent(new ProductReviewedEvent(Id, review.Id, rating));
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        MarkUpdated();
        AddDomainEvent(new ProductDeactivatedEvent(Id));
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');
    }
}

// Domain Events
public record ProductCreatedEvent(Guid ProductId, string Name, string Sku) : IDomainEvent;
public record ProductPriceUpdatedEvent(Guid ProductId, decimal OldPrice, decimal NewPrice) : IDomainEvent;
public record StockReservedEvent(Guid ProductId, int Quantity, int AvailableAfter) : IDomainEvent;
public record StockConfirmedEvent(Guid ProductId, int Quantity, int RemainingStock) : IDomainEvent;
public record StockReleasedEvent(Guid ProductId, int Quantity, int AvailableAfter) : IDomainEvent;
public record LowStockEvent(Guid ProductId, int CurrentStock) : IDomainEvent;
public record ProductReviewedEvent(Guid ProductId, Guid ReviewId, int Rating) : IDomainEvent;
public record ProductDeactivatedEvent(Guid ProductId) : IDomainEvent;

public class ProductReview
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid UserId { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private ProductReview() { }

    public ProductReview(Guid productId, Guid userId, int rating, string comment)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        UserId = userId;
        Rating = rating;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }
}

