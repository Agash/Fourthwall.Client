namespace Fourthwall.Client.Options;

/// <summary>
/// Identifies which Fourthwall webhook signature header should be validated.
/// </summary>
public enum FourthwallWebhookSignatureMode
{
    /// <summary>
    /// Validates the standard shop webhook signature header.
    /// </summary>
    ShopWebhook = 0,

    /// <summary>
    /// Validates the platform-app webhook signature header.
    /// </summary>
    PlatformAppWebhook = 1,
}