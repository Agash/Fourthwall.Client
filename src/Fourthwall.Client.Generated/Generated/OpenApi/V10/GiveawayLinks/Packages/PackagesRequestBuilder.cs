using System.Collections.Generic;
using Microsoft.Kiota.Abstractions;

namespace Fourthwall.Client.Generated.OpenApi.V10.GiveawayLinks.Packages;

/// <summary>
/// Companion request builder for the generated giveaway-links/packages path.
/// The upstream generated repo currently omits this file even though the parent
/// builder references it.
/// </summary>
[global::System.CodeDom.Compiler.GeneratedCode("StreamWeaver", "1.0.0")]
public sealed class PackagesRequestBuilder : BaseRequestBuilder
{
    public PackagesRequestBuilder(Dictionary<string, object> pathParameters, IRequestAdapter requestAdapter)
        : base(requestAdapter, "{+baseurl}/open-api/v1.0/giveaway-links/packages", pathParameters)
    {
    }

    public PackagesRequestBuilder(string rawUrl, IRequestAdapter requestAdapter)
        : base(requestAdapter, "{+baseurl}/open-api/v1.0/giveaway-links/packages", rawUrl)
    {
    }
}
