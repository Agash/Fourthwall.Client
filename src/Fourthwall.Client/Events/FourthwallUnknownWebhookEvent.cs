using System.Text.Json;

namespace Fourthwall.Client.Events;

/// <summary>
/// Represents a Fourthwall webhook event whose type is not explicitly recognised by this client.
/// The raw JSON payload is preserved in <see cref="Data"/> for inspection and forward-compatibility.
/// </summary>
public sealed record FourthwallUnknownWebhookEvent : FourthwallWebhookEvent
{
    /// <summary>Gets the raw JSON payload for debugging and forward-compatibility.</summary>
    public required JsonElement Data { get; init; }
}
