using Fourthwall.Client.Generated.Models.Openapi.Model.OrderV1;

namespace Fourthwall.Client.Events;

/// <summary>
/// Represents a Fourthwall order-placed webhook event.
/// Fired when a new order is successfully placed and paid. Data contains an <c>OrderV1</c> payload.
/// </summary>
public sealed record FourthwallOrderPlacedWebhookEvent : FourthwallWebhookEvent
{
    /// <summary>Gets the typed order payload.</summary>
    public required OrderV1 Data { get; init; }
}
