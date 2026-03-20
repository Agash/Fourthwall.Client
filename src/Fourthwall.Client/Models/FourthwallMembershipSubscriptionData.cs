using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Represents the subscription state within a <see cref="FourthwallMembershipSupporterData"/>.
/// The <c>type</c> discriminator carries one of: ACTIVE, CANCELLED, SUSPENDED, or NONE.
/// </summary>
public sealed record FourthwallMembershipSubscriptionData
{
    /// <summary>
    /// Gets the subscription status discriminator.
    /// Known values: <c>ACTIVE</c>, <c>CANCELLED</c>, <c>SUSPENDED</c>, <c>NONE</c>.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>
    /// Gets the active tier variant associated with this subscription.
    /// Present for ACTIVE, CANCELLED, and SUSPENDED states; absent for NONE.
    /// </summary>
    [JsonPropertyName("variant")]
    public FourthwallTierVariantData? Variant { get; init; }
}
