namespace Fourthwall.Client.Options;

/// <summary>
/// Represents the options required to validate and process a Fourthwall webhook delivery.
/// </summary>
public sealed class FourthwallWebhookOptions
{
    /// <summary>
    /// Gets or sets the HMAC signing secret used to verify the request body.
    /// </summary>
    public required string SigningSecret { get; init; }

    /// <summary>
    /// Gets or sets the signature validation mode.
    /// </summary>
    public FourthwallWebhookSignatureMode SignatureMode { get; init; } =
        FourthwallWebhookSignatureMode.ShopWebhook;
}