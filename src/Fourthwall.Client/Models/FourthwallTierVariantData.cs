using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Represents a specific billing variant of a membership tier
/// (e.g. monthly vs. annual of a given tier).
/// </summary>
public sealed record FourthwallTierVariantData
{
    /// <summary>Gets the variant identifier.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>Gets the parent tier identifier.</summary>
    [JsonPropertyName("tierId")]
    public string? TierId { get; init; }

    /// <summary>Gets the billing interval. Known values: <c>MONTHLY</c>, <c>ANNUAL</c>.</summary>
    [JsonPropertyName("interval")]
    public string? Interval { get; init; }

    /// <summary>Gets the recurring amount charged for this variant.</summary>
    [JsonPropertyName("amount")]
    public FourthwallMoney? Amount { get; init; }
}
