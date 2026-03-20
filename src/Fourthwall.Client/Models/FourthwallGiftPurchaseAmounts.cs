using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Monetary amounts for a gift purchase event.
/// </summary>
public sealed record FourthwallGiftPurchaseAmounts
{
    /// <summary>Gets the subtotal before tax and prepaid shipping.</summary>
    [JsonPropertyName("subtotal")]
    public FourthwallMoney? Subtotal { get; init; }

    /// <summary>Gets the tax amount.</summary>
    [JsonPropertyName("tax")]
    public FourthwallMoney? Tax { get; init; }

    /// <summary>Gets the final total charged.</summary>
    [JsonPropertyName("total")]
    public required FourthwallMoney Total { get; init; }

    /// <summary>Gets the creator profit on this purchase.</summary>
    [JsonPropertyName("profit")]
    public FourthwallMoney? Profit { get; init; }

    /// <summary>Gets the prepaid shipping amount included in this gift purchase.</summary>
    [JsonPropertyName("prepaidShipping")]
    public FourthwallMoney? PrepaidShipping { get; init; }
}
