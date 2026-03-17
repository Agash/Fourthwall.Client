namespace Fourthwall.Client.Options;

/// <summary>
/// Represents shop-level Basic Authentication credentials for the Fourthwall Platform API.
/// </summary>
public sealed class FourthwallBasicAuthOptions
{
    /// <summary>
    /// Gets or sets the Basic Auth username.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Gets or sets the Basic Auth password.
    /// </summary>
    public required string Password { get; init; }
}