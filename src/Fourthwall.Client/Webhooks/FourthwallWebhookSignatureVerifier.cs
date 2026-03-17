using Fourthwall.Client.Internal;
using Fourthwall.Client.Options;
using System.Security.Cryptography;
using System.Text;

namespace Fourthwall.Client.Webhooks;

/// <summary>
/// Verifies Fourthwall webhook signatures using HMAC-SHA256 over the raw request body.
/// </summary>
/// <remarks>
/// Fourthwall documents HMAC-based verification using the raw body and distinct headers for
/// standard shop webhooks and platform-app webhooks. :contentReference[oaicite:3]{index=3}
/// </remarks>
public sealed class FourthwallWebhookSignatureVerifier
{
    /// <summary>
    /// Gets the standard shop webhook signature header.
    /// </summary>
    public const string ShopWebhookSignatureHeaderName = "X-Fourthwall-Hmac-SHA256";

    /// <summary>
    /// Gets the platform-app webhook signature header.
    /// </summary>
    public const string PlatformAppWebhookSignatureHeaderName = "X-Fourthwall-Hmac-Apps-SHA256";

    /// <summary>
    /// Verifies the signature for the specified raw request body.
    /// </summary>
    /// <param name="body">The raw request body bytes.</param>
    /// <param name="providedSignature">The signature header value supplied by Fourthwall.</param>
    /// <param name="signingSecret">The configured signing secret.</param>
    /// <returns>
    /// <see langword="true"/> when the provided signature matches the computed base64 HMAC-SHA256 digest;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool Verify(byte[] body, string? providedSignature, string signingSecret)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentException.ThrowIfNullOrEmpty(signingSecret);

        if (string.IsNullOrWhiteSpace(providedSignature))
        {
            return false;
        }

        using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(signingSecret));
        byte[] digest = hmac.ComputeHash(body);
        string expectedSignature = Convert.ToBase64String(digest);

        return ConstantTimeStringComparer.Equals(expectedSignature, providedSignature);
    }

    /// <summary>
    /// Gets the expected header name for the specified signature mode.
    /// </summary>
    /// <param name="mode">The signature mode.</param>
    /// <returns>The expected header name.</returns>
    public static string GetHeaderName(FourthwallWebhookSignatureMode mode)
    {
        return mode switch
        {
            FourthwallWebhookSignatureMode.ShopWebhook => ShopWebhookSignatureHeaderName,
            FourthwallWebhookSignatureMode.PlatformAppWebhook => PlatformAppWebhookSignatureHeaderName,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported signature mode."),
        };
    }
}