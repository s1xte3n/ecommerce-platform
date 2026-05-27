using ECommerce.Domain.Base;

namespace ECommerce.Domain.Entities;

public class Category : AggregateRoot
{
    private readonly List<Product> _products = new();
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public Guid? ParentCategoryId { get; private set; }
    public Category? ParentCategory { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private Category() { }

    public Category(string name, string description, Guid? parentCategoryId = null)
    {
        Id = Guid.NewGuid();
        SetName(name);
        Description = description;
        ParentCategoryId = parentCategoryId;
        IsActive = true;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required");
        Name = name.Trim();
        Slug = name.ToLowerInvariant().Replace(" ", "-");
    }
}
