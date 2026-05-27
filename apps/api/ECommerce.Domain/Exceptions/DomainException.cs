// File: apps/api/src/Domain/Exceptions/DomainException.cs
namespace ECommerce.Domain.Exceptions;

/// <summary>
/// Base exception for domain-level business rule violations.
/// </summary>
public class DomainException : Exception
{
    public string ErrorCode { get; }

    public DomainException(string message, string errorCode = "DOMAIN_ERROR")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string message, Exception innerException, string errorCode = "DOMAIN_ERROR")
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object identifier)
        : base($"{entityName} with id '{identifier}' was not found.", "NOT_FOUND")
    {
    }
}

public class ValidationException : DomainException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validation failed", "VALIDATION_ERROR")
    {
        Errors = errors;
    }
}
