using ECommerce.Domain.Base;
using ECommerce.Domain.Enums;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Entities;

public class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = new();
    public Guid UserId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public Money Subtotal { get; private set; } = Money.Zero();
    public Money Tax { get; private set; } = Money.Zero();
    public Money Shipping { get; private set; } = Money.Zero();
    public Money Discount { get; private set; } = Money.Zero();
    public Money Total { get; private set; } = Money.Zero();
    public string? CouponCode { get; private set; }
    public string ShippingAddress { get; private set; } = string.Empty;
    public string BillingAddress { get; private set; } = string.Empty;
    public Guid? PaymentId { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public string? Notes { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    private Order() { }
    public Order(Guid userId, string shippingAddress, string billingAddress)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..8];
        Status = OrderStatus.Pending;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        PaymentStatus = PaymentStatus.Pending;
    }
    public void AddItem(Product product, int quantity, Money unitPrice)
    {
        var item = new OrderItem(Id, product.Id, product.Name, product.Sku.Value, quantity, unitPrice);
        _items.Add(item);
        RecalculateTotals();
    }
    private void RecalculateTotals()
    {
        Subtotal = _items.Aggregate(Money.Zero(), (sum, item) => sum.Add(item.TotalPrice));
        Total = Subtotal.Add(Tax).Add(Shipping).Subtract(Discount);
    }
}

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string ProductSku { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = Money.Zero();
    public Money TotalPrice => UnitPrice.Multiply(Quantity);
    private OrderItem() { }
    public OrderItem(Guid orderId, Guid productId, string productName, string productSku, int quantity, Money unitPrice)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        ProductSku = productSku;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
