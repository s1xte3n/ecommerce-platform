using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery : IRequest<ProductDetailDto>
{
    public Guid ProductId { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
}

public class ProductDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public string Sku { get; init; } = string.Empty;
    public int AvailableStock { get; init; }
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public double AverageRating { get; init; }
    public int ReviewCount { get; init; }
    public bool IsActive { get; init; }
}

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetProductByIdQueryHandler> _logger;

    public GetProductByIdQueryHandler(
        IProductRepository productRepository,
        ICacheService cacheService,
        ILogger<GetProductByIdQueryHandler> logger)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ProductDetailDto> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"product:{request.ProductId}";

        var cached = await _cacheService.GetAsync<ProductDetailDto>(cacheKey, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for product {ProductId}", request.ProductId);
            return cached;
        }

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null)
            throw new NotFoundException(nameof(Product), request.ProductId);

        var dto = new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Slug = product.Slug,
            Price = product.Price.Amount,
            Currency = product.Price.Currency,
            Sku = product.Sku.Value,
            AvailableStock = product.AvailableStock,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            AverageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = product.Reviews.Count,
            IsActive = product.IsActive
        };

        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), cancellationToken);

        return dto;
    }
}
