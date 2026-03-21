using Fourthwall.Client.Generated.Models.Openapi.Model.GiftPurchaseV1;

namespace Fourthwall.Client.Events;

/// <summary>
/// Represents a Fourthwall gift-purchase webhook event.
/// Fired when a gift card is purchased.
/// Data contains a <c>GiftPurchaseV1</c> payload.
/// </summary>
public sealed record FourthwallGiftPurchaseWebhookEvent : FourthwallWebhookEvent
{
    /// <summary>Gets the typed gift purchase payload.</summary>
    public required GiftPurchaseV1 Data { get; init; }
}
