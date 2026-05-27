namespace ECommerce.Domain.ValueObjects;

public class ProductDimensions
{
    public decimal Length { get; private set; }
    public decimal Width { get; private set; }
    public decimal Height { get; private set; }
    public decimal Weight { get; private set; }
    public string Unit { get; private set; } = "cm";

    private ProductDimensions() { }

    public ProductDimensions(decimal length, decimal width, decimal height, decimal weight, string unit = "cm")
    {
        Length = length;
        Width = width;
        Height = height;
        Weight = weight;
        Unit = unit;
    }

    public decimal Volume => Length * Width * Height;
}
