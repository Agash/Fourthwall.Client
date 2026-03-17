using Fourthwall.Client.Abstractions;
using Fourthwall.Client.Authentication;
using Fourthwall.Client.Generated;
using Fourthwall.Client.Options;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace Fourthwall.Client;

/// <summary>
/// Creates configured <see cref="FourthwallApiClient"/> instances.
/// </summary>
public sealed class FourthwallClientFactory : IFourthwallClientFactory
{
    private static readonly Uri BaseUri = new("https://api.fourthwall.com");

    /// <inheritdoc />
    public FourthwallApiClient CreateWithBasicAuth(FourthwallBasicAuthOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return CreateClient(new FourthwallBasicAuthenticationProvider(options.Username, options.Password));
    }

    /// <inheritdoc />
    public FourthwallApiClient CreateWithBearerToken(string accessToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(accessToken);

        return CreateClient(new FourthwallBearerAuthenticationProvider(accessToken));
    }

    private static FourthwallApiClient CreateClient(IAuthenticationProvider authenticationProvider)
    {
        HttpClientRequestAdapter adapter = new(authenticationProvider)
        {
            BaseUrl = BaseUri.ToString(),
        };

        return new FourthwallApiClient(adapter);
    }
}