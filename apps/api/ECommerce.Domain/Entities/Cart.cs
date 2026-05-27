using ECommerce.Domain.Base;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Entities;

public class Cart : AggregateRoot
{
    private readonly List<CartItem> _items = new();
    public Guid UserId { get; private set; }
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
    private Cart() { }
    public Cart(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
    }
    public void AddItem(Guid productId, int quantity, Money unitPrice)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        else
            _items.Add(new CartItem(Id, productId, quantity, unitPrice));
        MarkUpdated();
    }
    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null) { _items.Remove(item); MarkUpdated(); }
    }
    public void Clear() { _items.Clear(); MarkUpdated(); }
}

public class CartItem
{
    public Guid Id { get; private set; }
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    private CartItem() { UnitPrice = Money.Zero(); }
    public CartItem(Guid cartId, Guid productId, int quantity, Money unitPrice)
    {
        Id = Guid.NewGuid();
        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0) throw new ArgumentException("Quantity must be positive");
        Quantity = newQuantity;
    }
}
