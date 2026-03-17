using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Fourthwall.Client.Authentication;

/// <summary>
/// Provides Kiota authentication using a Fourthwall OAuth bearer token.
/// </summary>
public sealed class FourthwallBearerAuthenticationProvider : IAuthenticationProvider
{
    private readonly string _authorizationValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="FourthwallBearerAuthenticationProvider"/> class.
    /// </summary>
    /// <param name="accessToken">The OAuth access token.</param>
    public FourthwallBearerAuthenticationProvider(string accessToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(accessToken);
        _authorizationValue = $"Bearer {accessToken}";
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