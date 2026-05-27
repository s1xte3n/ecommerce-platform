// File: apps/api/ECommerce.Domain/Entities/Coupon.cs
using ECommerce.Domain.Base;

namespace ECommerce.Domain.Entities;

public class Coupon : AggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DiscountType Type { get; private set; }
    public decimal Value { get; private set; }
    public decimal? MinimumOrderAmount { get; private set; }
    public decimal? MaximumDiscountAmount { get; private set; }
    public int? UsageLimit { get; private set; }
    public int UsageCount { get; private set; }
    public int PerUserLimit { get; private set; } = 1;
    public DateTime StartsAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }

    private Coupon() { }

    public Coupon(string code, DiscountType type, decimal value, DateTime startsAt, DateTime? expiresAt = null)
    {
        Id = Guid.NewGuid();
        Code = code.ToUpperInvariant();
        Type = type;
        Value = value;
        StartsAt = startsAt;
        ExpiresAt = expiresAt;
        IsActive = true;
    }

    public bool IsValid(decimal orderAmount, int userUsageCount)
    {
        if (!IsActive) return false;
        if (DateTime.UtcNow < StartsAt) return false;
        if (ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt) return false;
        if (UsageLimit.HasValue && UsageCount >= UsageLimit) return false;
        if (userUsageCount >= PerUserLimit) return false;
        if (MinimumOrderAmount.HasValue && orderAmount < MinimumOrderAmount) return false;
        return true;
    }

    public void IncrementUsage()
    {
        UsageCount++;
    }
}

public enum DiscountType
{
    Percentage,
    FixedAmount
}
