using System.Text.Json;

namespace Fourthwall.Client.Events;

/// <summary>
/// Represents the normalized base type for Fourthwall webhook events.
/// Concrete subtypes add a strongly-typed <c>Data</c> property specific to the event kind.
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
    /// The event's <c>data</c> object exactly as Fourthwall sent it. Unlike the typed model it serializes with
    /// System.Text.Json without reflection, so it is what to store or forward when the event has to outlive the
    /// request (a journal, a queue).
    /// </summary>
    public required JsonElement RawData { get; init; }
}
