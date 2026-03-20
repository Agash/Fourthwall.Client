using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Shipping information for an order, including the delivery address.
/// </summary>
public sealed record FourthwallOrderShipping
{
    /// <summary>Gets the shipping destination address.</summary>
    [JsonPropertyName("address")]
    public FourthwallAddress? Address { get; init; }
}
