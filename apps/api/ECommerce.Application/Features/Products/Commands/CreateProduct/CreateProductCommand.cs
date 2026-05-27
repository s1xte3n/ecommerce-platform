using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand : IRequest<CreateProductResponse>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public string Sku { get; init; } = string.Empty;
    public int InitialStock { get; init; }
    public Guid CategoryId { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
}

public record CreateProductResponse
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty();

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .Length(3);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.InitialStock)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CategoryId)
            .NotEmpty();
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CreateProductResponse> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product: {ProductName}", request.Name);

        var existingProduct = await _productRepository.GetBySkuAsync(
            new Sku(request.Sku), cancellationToken);

        if (existingProduct != null)
            throw new DomainException($"Product with SKU '{request.Sku}' already exists", "DUPLICATE_SKU");

        var product = new Product(
            request.Name,
            request.Description,
            new Money(request.Price, request.Currency),
            new Sku(request.Sku),
            request.InitialStock,
            request.CategoryId);

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product created: {ProductId}", product.Id);

        return new CreateProductResponse
        {
            ProductId = product.Id,
            Name = product.Name,
            CreatedAt = product.CreatedAt
        };
    }
}
