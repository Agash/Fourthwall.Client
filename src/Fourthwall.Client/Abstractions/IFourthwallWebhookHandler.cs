using Agash.Webhook.Abstractions;
using Fourthwall.Client.Events;
using Fourthwall.Client.Options;

namespace Fourthwall.Client.Abstractions;

/// <summary>
/// Defines a transport-neutral handler for processing Fourthwall webhook deliveries.
/// </summary>
public interface IFourthwallWebhookHandler
{
    /// <summary>
    /// Processes an inbound Fourthwall webhook request.
    /// </summary>
    /// <param name="request">The transport-neutral webhook request.</param>
    /// <param name="options">The webhook options used for signature verification.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// A result containing the response to return and the normalized Fourthwall webhook event,
    /// if one was produced.
    /// </returns>
    Task<WebhookHandleResult<FourthwallWebhookEvent>> HandleAsync(
        WebhookRequest request,
        FourthwallWebhookOptions options,
        CancellationToken cancellationToken = default);
}