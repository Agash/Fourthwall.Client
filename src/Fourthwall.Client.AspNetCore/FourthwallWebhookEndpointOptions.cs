using Agash.Webhook.Abstractions;
using Fourthwall.Client.Events;
using Fourthwall.Client.Options;
using Microsoft.AspNetCore.Http;

namespace Fourthwall.Client.AspNetCore;

/// <summary>
/// Represents configuration callbacks used by the ASP.NET Core Fourthwall webhook endpoint mapper.
/// </summary>
public sealed class FourthwallWebhookEndpointOptions
{
    /// <summary>
    /// Gets or sets the callback used to resolve the effective Fourthwall webhook options
    /// for the current HTTP request.
    /// </summary>
    public required Func<HttpContext, CancellationToken, Task<FourthwallWebhookOptions>> ResolveWebhookOptionsAsync { get; set; }

    /// <summary>
    /// Gets or sets an optional callback invoked after a normalized Fourthwall event has been
    /// produced successfully.
    /// </summary>
    public Func<FourthwallWebhookEvent, HttpContext, CancellationToken, Task>? OnEventAsync { get; set; }

    /// <summary>
    /// Gets or sets an optional callback invoked after the Fourthwall handler completes.
    /// </summary>
    public Func<WebhookHandleResult<FourthwallWebhookEvent>, HttpContext, CancellationToken, Task>? OnResultAsync { get; set; }
}