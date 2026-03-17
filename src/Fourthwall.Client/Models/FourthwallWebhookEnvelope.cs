using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Represents the shared Fourthwall webhook envelope.
/// </summary>
/// <remarks>
/// Fourthwall documents that all webhook events share a common envelope containing metadata
/// such as the event identifier, webhook identifier, shop identifier, event type,
/// API version, creation timestamp, and a <c>data</c> payload object.
/// </remarks>
public sealed record FourthwallWebhookEnvelope
{
    /// <summary>
    /// Gets the value indicating whether the event contains test data.
    /// </summary>
    [JsonPropertyName("testMode")]
    public required bool TestMode { get; init; }

    /// <summary>
    /// Gets the unique event identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Gets the identifier of the webhook configuration that produced the event.
    /// </summary>
    [JsonPropertyName("webhookId")]
    public required string WebhookId { get; init; }

    /// <summary>
    /// Gets the shop identifier associated with the event.
    /// </summary>
    [JsonPropertyName("shopId")]
    public required string ShopId { get; init; }

    /// <summary>
    /// Gets the event type.
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Gets the API version of the payload model.
    /// </summary>
    [JsonPropertyName("apiVersion")]
    public required string ApiVersion { get; init; }

    /// <summary>
    /// Gets the time at which the event was created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the event-specific payload data.
    /// </summary>
    [JsonPropertyName("data")]
    public required JsonElement Data { get; init; }
}