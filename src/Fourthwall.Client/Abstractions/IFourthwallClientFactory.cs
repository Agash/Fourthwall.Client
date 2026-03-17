using Fourthwall.Client.Generated;
using Fourthwall.Client.Options;

namespace Fourthwall.Client.Abstractions;

/// <summary>
/// Creates configured <see cref="FourthwallApiClient"/> instances for interacting with the
/// Fourthwall Platform API.
/// </summary>
public interface IFourthwallClientFactory
{
    /// <summary>
    /// Creates a Fourthwall API client using shop-level Basic Authentication.
    /// </summary>
    /// <param name="options">The Basic Authentication options.</param>
    /// <returns>A configured <see cref="FourthwallApiClient"/> instance.</returns>
    FourthwallApiClient CreateWithBasicAuth(FourthwallBasicAuthOptions options);

    /// <summary>
    /// Creates a Fourthwall API client using OAuth bearer-token authentication.
    /// </summary>
    /// <param name="accessToken">The OAuth access token.</param>
    /// <returns>A configured <see cref="FourthwallApiClient"/> instance.</returns>
    FourthwallApiClient CreateWithBearerToken(string accessToken);
}