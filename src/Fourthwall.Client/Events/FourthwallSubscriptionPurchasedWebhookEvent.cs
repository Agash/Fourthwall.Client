using Fourthwall.Client.Models;

namespace Fourthwall.Client.Events;

/// <summary>
/// Represents a Fourthwall subscription-purchased webhook event.
/// Fired when a new membership subscription is purchased.
/// Data contains a <c>MembershipSupporterV1</c> payload.
/// </summary>
public sealed record FourthwallSubscriptionPurchasedWebhookEvent : FourthwallWebhookEvent
{
    /// <summary>Gets the typed membership supporter payload.</summary>
    public required FourthwallMembershipSupporterData Data { get; init; }
}
