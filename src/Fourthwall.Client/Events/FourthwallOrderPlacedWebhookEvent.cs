namespace Fourthwall.Client.Events;

/// <summary>
/// Represents a known Fourthwall order-placed webhook event.
/// </summary>
public sealed record FourthwallOrderPlacedWebhookEvent : FourthwallWebhookEvent
{
    // /// <summary>
    // /// Gets the raw payload data for the order event.
    // /// </summary>
    // public new required JsonElement Data { get; init; }
}