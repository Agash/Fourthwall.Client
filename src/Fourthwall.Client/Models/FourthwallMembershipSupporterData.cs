using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Typed payload for <c>SUBSCRIPTION_PURCHASED</c>, <c>SUBSCRIPTION_EXPIRED</c>, and
/// <c>SUBSCRIPTION_CHANGED</c> webhook events (<c>MembershipSupporterV1</c>).
/// </summary>
public sealed record FourthwallMembershipSupporterData
{
    /// <summary>Gets the supporter (member) identifier.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>Gets the supporter's email address.</summary>
    [JsonPropertyName("email")]
    public required string Email { get; init; }

    /// <summary>Gets the supporter's display nickname.</summary>
    [JsonPropertyName("nickname")]
    public string? Nickname { get; init; }

    /// <summary>Gets the UTC timestamp when the membership was first created.</summary>
    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    /// Gets the current subscription state.
    /// The <see cref="FourthwallMembershipSubscriptionData.Type"/> discriminator indicates
    /// whether it is ACTIVE, CANCELLED, SUSPENDED, or NONE.
    /// </summary>
    [JsonPropertyName("subscription")]
    public FourthwallMembershipSubscriptionData? Subscription { get; init; }
}
