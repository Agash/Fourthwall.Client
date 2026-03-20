using Fourthwall.Client.Models;

namespace Fourthwall.Client.Events;

/// <summary>
/// Represents a Fourthwall donation webhook event.
/// Fired when a new donation is received. Data contains a <c>DonationV1</c> payload.
/// </summary>
public sealed record FourthwallDonationWebhookEvent : FourthwallWebhookEvent
{
    /// <summary>Gets the typed donation payload.</summary>
    public required FourthwallDonationData Data { get; init; }
}
