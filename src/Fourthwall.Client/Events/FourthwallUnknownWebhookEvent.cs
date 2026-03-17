namespace Fourthwall.Client.Events;

/// <summary>
/// Represents a Fourthwall webhook event whose type is not explicitly recognized by this client.
/// </summary>
public sealed record FourthwallUnknownWebhookEvent : FourthwallWebhookEvent;