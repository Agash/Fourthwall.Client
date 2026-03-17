using System.Text.Json;

namespace Fourthwall.Client.Events;

/// <summary>
/// Represents the normalized base type for Fourthwall webhook events.
/// </summary>
public abstract record FourthwallWebhookEvent
{
    /// <summary>
    /// Gets the unique event identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the originating webhook configuration identifier.
    /// </summary>
    public required string WebhookId { get; init; }

    /// <summary>
    /// Gets the originating shop identifier.
    /// </summary>
    public required string ShopId { get; init; }

    /// <summary>
    /// Gets the webhook event type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets the API version associated with the payload.
    /// </summary>
    public required string ApiVersion { get; init; }

    /// <summary>
    /// Gets the timestamp indicating when the event was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets a value indicating whether the event contains test data.
    /// </summary>
    public required bool TestMode { get; init; }

    /// <summary>
    /// Gets the raw event payload data.
    /// </summary>
    public required JsonElement Data { get; init; }
}