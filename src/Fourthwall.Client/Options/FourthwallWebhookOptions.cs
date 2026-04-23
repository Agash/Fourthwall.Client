namespace Fourthwall.Client.Options;

/// <summary>
/// Represents the options used to validate and process a Fourthwall webhook delivery.
/// </summary>
public sealed class FourthwallWebhookOptions
{
    /// <summary>
    /// Gets or sets the HMAC signing secret used to verify the request body.
    /// When <see langword="null"/> or empty, signature verification is skipped entirely
    /// and the raw payload is deserialized without authentication.
    /// When a value is provided, the request must carry a valid HMAC-SHA256 signature
    /// in the header corresponding to <see cref="SignatureMode"/>.
    /// </summary>
    public string? SigningSecret { get; init; }

    /// <summary>
    /// Gets or sets the signature validation mode. Only relevant when
    /// <see cref="SigningSecret"/> is provided.
    /// </summary>
    public FourthwallWebhookSignatureMode SignatureMode { get; init; } =
        FourthwallWebhookSignatureMode.ShopWebhook;
}