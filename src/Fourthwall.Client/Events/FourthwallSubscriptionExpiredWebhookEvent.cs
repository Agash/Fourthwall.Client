using Fourthwall.Client.Models;

namespace Fourthwall.Client.Events;

/// <summary>
/// Represents a Fourthwall subscription-expired webhook event.
/// Fired when a membership subscription expires.
/// Data contains a <c>MembershipSupporterV1</c> payload.
/// </summary>
public sealed record FourthwallSubscriptionExpiredWebhookEvent : FourthwallWebhookEvent
{
    /// <summary>Gets the typed membership supporter payload.</summary>
    public required FourthwallMembershipSupporterData Data { get; init; }
}
