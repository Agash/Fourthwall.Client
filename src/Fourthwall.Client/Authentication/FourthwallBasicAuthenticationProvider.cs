using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Text;

namespace Fourthwall.Client.Authentication;

/// <summary>
/// Provides Kiota authentication using Fourthwall shop-level Basic Authentication credentials.
/// </summary>
public sealed class FourthwallBasicAuthenticationProvider : IAuthenticationProvider
{
    private readonly string _authorizationValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="FourthwallBasicAuthenticationProvider"/> class.
    /// </summary>
    /// <param name="username">The Basic Authentication username.</param>
    /// <param name="password">The Basic Authentication password.</param>
    public FourthwallBasicAuthenticationProvider(string username, string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(username);
        ArgumentException.ThrowIfNullOrEmpty(password);

        string raw = $"{username}:{password}";
        string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        _authorizationValue = $"Basic {encoded}";
    }

    /// <inheritdoc />
    public Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Headers.Add("Authorization", _authorizationValue);
        return Task.CompletedTask;
    }
}