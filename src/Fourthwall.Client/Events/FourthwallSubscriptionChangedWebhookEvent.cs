using Fourthwall.Client.Generated.Models.Openapi.Model.MembershipSupporterV1;

namespace Fourthwall.Client.Events;

/// <summary>
/// Represents a Fourthwall subscription-changed webhook event.
/// Fired when a membership subscription tier changes.
/// Data contains a <c>MembershipSupporterV1</c> payload.
/// </summary>
public sealed record FourthwallSubscriptionChangedWebhookEvent : FourthwallWebhookEvent
{
    /// <summary>Gets the typed membership supporter payload.</summary>
    public required MembershipSupporterV1 Data { get; init; }
}
