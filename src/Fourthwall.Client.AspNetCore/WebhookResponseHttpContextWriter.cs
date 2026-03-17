using Agash.Webhook.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Fourthwall.Client.AspNetCore;

/// <summary>
/// Writes transport-neutral <see cref="WebhookResponse"/> instances to ASP.NET Core responses.
/// </summary>
public static class WebhookResponseHttpContextWriter
{
    /// <summary>
    /// Writes the specified response to the given HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context whose response should be written.</param>
    /// <param name="response">The transport-neutral response to write.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when writing is complete.</returns>
    public static async Task WriteAsync(
        HttpContext context,
        WebhookResponse response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(response);

        context.Response.StatusCode = response.StatusCode;

        if (!string.IsNullOrWhiteSpace(response.ContentType))
        {
            context.Response.ContentType = response.ContentType;
        }

        foreach ((string key, string[] values) in response.Headers)
        {
            if (string.Equals(key, "Content-Type", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "Content-Length", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            context.Response.Headers[key] = values;
        }

        if (response.Body is { Length: > 0 })
        {
            context.Response.ContentLength = response.Body.Length;
            await context.Response.Body.WriteAsync(response.Body, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            context.Response.ContentLength = 0;
        }
    }
}