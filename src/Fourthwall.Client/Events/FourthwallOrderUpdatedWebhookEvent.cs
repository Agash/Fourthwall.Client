using Fourthwall.Client.Models;

namespace Fourthwall.Client.Events;

/// <summary>
/// Represents a Fourthwall order-updated webhook event.
/// Fired when an order's status, shipping address, or email is updated.
/// Data contains an <c>OrderUpdatedV1</c> payload (wraps the full updated order).
/// </summary>
public sealed record FourthwallOrderUpdatedWebhookEvent : FourthwallWebhookEvent
{
    /// <summary>Gets the typed order-updated payload.</summary>
    public required FourthwallOrderUpdatedData Data { get; init; }
}
