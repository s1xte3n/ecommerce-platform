// File: apps/api/ECommerce.Domain/Entities/User.cs
using ECommerce.Domain.Base;

namespace ECommerce.Domain.Entities;

public class User : AggregateRoot
{
    public string Email { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public List<string> Roles { get; private set; } = new();

    private User() { }

    public User(string email, string username, string firstName, string lastName)
    {
        Id = Guid.NewGuid();
        SetEmail(email);
        SetUsername(username);
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
        EmailConfirmed = false;
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("Valid email is required", nameof(email));
        Email = email.ToLowerInvariant().Trim();
    }

    public void SetUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            throw new ArgumentException("Username must be at least 3 characters", nameof(username));
        Username = username.Trim();
    }

    public void SetPassword(string passwordHash)
    {
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
    }

    public void SetRefreshToken(string token, DateTime expiresAt)
    {
        RefreshToken = token;
        RefreshTokenExpiresAt = expiresAt;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}
