using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Typed payload for the <c>ORDER_UPDATED</c> webhook event (<c>OrderUpdatedV1</c>).
/// Wraps the full updated order alongside an update descriptor that identifies what changed.
/// </summary>
public sealed record FourthwallOrderUpdatedData
{
    /// <summary>Gets the full updated order object.</summary>
    [JsonPropertyName("order")]
    public FourthwallOrderData? Order { get; init; }

    /// <summary>
    /// Gets the descriptor identifying what changed in this update.
    /// The <see cref="FourthwallOrderUpdate.Type"/> discriminator will be one of:
    /// <c>STATUS</c>, <c>SHIPPING.ADDRESS</c>, or <c>EMAIL</c>.
    /// </summary>
    [JsonPropertyName("update")]
    public FourthwallOrderUpdate? Update { get; init; }
}
