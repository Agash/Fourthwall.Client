using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Full monetary breakdown for an order event.
/// </summary>
public sealed record FourthwallOrderAmounts
{
    /// <summary>Gets the subtotal before shipping, tax, and discounts.</summary>
    [JsonPropertyName("subtotal")]
    public FourthwallMoney? Subtotal { get; init; }

    /// <summary>Gets the shipping cost.</summary>
    [JsonPropertyName("shipping")]
    public FourthwallMoney? Shipping { get; init; }

    /// <summary>Gets the tax amount.</summary>
    [JsonPropertyName("tax")]
    public FourthwallMoney? Tax { get; init; }

    /// <summary>Gets the donation amount included in the order.</summary>
    [JsonPropertyName("donation")]
    public FourthwallMoney? Donation { get; init; }

    /// <summary>Gets the discount amount applied.</summary>
    [JsonPropertyName("discount")]
    public FourthwallMoney? Discount { get; init; }

    /// <summary>Gets the final total charged to the customer.</summary>
    [JsonPropertyName("total")]
    public FourthwallMoney? Total { get; init; }
}
