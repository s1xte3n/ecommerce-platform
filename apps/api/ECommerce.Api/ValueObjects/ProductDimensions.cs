// File: apps/api/ECommerce.Domain/ValueObjects/ProductDimensions.cs
namespace ECommerce.Domain.ValueObjects;

/// <summary>
/// Value object representing physical product dimensions.
/// Used for shipping calculations and product display.
/// </summary>
public class ProductDimensions
{
    public decimal Length { get; }
    public decimal Width { get; }
    public decimal Height { get; }
    public decimal Weight { get; }
    public string Unit { get; } // "cm", "in", etc.

    public ProductDimensions(
        decimal length,
        decimal width,
        decimal height,
        decimal weight,
        string unit = "cm")
    {
        if (length <= 0) throw new ArgumentException("Length must be positive", nameof(length));
        if (width <= 0) throw new ArgumentException("Width must be positive", nameof(width));
        if (height <= 0) throw new ArgumentException("Height must be positive", nameof(height));
        if (weight <= 0) throw new ArgumentException("Weight must be positive", nameof(weight));
        if (string.IsNullOrWhiteSpace(unit)) throw new ArgumentException("Unit is required", nameof(unit));

        Length = length;
        Width = width;
        Height = height;
        Weight = weight;
        Unit = unit.ToLowerInvariant();
    }

    public decimal Volume => Length * Width * Height;

    public override string ToString() => $"{Length}x{Width}x{Height} {Unit}, {Weight}{Unit}";
}
