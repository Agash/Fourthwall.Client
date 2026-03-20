using Fourthwall.Client.Models;

namespace Fourthwall.Client.Events;

/// <summary>
/// Represents a Fourthwall order-placed webhook event.
/// Fired when a new order is successfully placed and paid. Data contains an <c>OrderV1</c> payload.
/// </summary>
public sealed record FourthwallOrderPlacedWebhookEvent : FourthwallWebhookEvent
{
    /// <summary>Gets the typed order payload.</summary>
    public required FourthwallOrderData Data { get; init; }
}
