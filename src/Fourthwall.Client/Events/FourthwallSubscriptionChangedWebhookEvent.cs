using Fourthwall.Client.Models;

namespace Fourthwall.Client.Events;

/// <summary>
/// Represents a Fourthwall subscription-changed webhook event.
/// Fired when a membership subscription tier changes.
/// Data contains a <c>MembershipSupporterV1</c> payload.
/// </summary>
public sealed record FourthwallSubscriptionChangedWebhookEvent : FourthwallWebhookEvent
{
    /// <summary>Gets the typed membership supporter payload.</summary>
    public required FourthwallMembershipSupporterData Data { get; init; }
}
