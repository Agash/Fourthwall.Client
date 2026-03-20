using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Describes what changed in an <c>ORDER_UPDATED</c> webhook event (<c>OrderUpdatedV1.Update</c>).
/// The <c>type</c> discriminator identifies the nature of the update.
/// </summary>
/// <remarks>
/// Known <c>type</c> values:
/// <list type="bullet">
///   <item><description><c>STATUS</c> — the order status changed.</description></item>
///   <item><description><c>SHIPPING.ADDRESS</c> — the shipping address was corrected.</description></item>
///   <item><description><c>EMAIL</c> — the customer email was updated.</description></item>
/// </list>
/// </remarks>
public sealed record FourthwallOrderUpdate
{
    /// <summary>
    /// Gets the update type discriminator.
    /// Known values: <c>STATUS</c>, <c>SHIPPING.ADDRESS</c>, <c>EMAIL</c>.
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }
}
