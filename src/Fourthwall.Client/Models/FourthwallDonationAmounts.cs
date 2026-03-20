using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Monetary amounts for a donation event.
/// </summary>
public sealed record FourthwallDonationAmounts
{
    /// <summary>Gets the total charged amount.</summary>
    [JsonPropertyName("total")]
    public required FourthwallMoney Total { get; init; }
}
