using Agash.Webhook.Abstractions;
using Fourthwall.Client.Abstractions;
using Fourthwall.Client.Events;
using Fourthwall.Client.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Fourthwall.Client.AspNetCore;

/// <summary>
/// Provides endpoint mapping extensions for exposing <see cref="IFourthwallWebhookHandler"/>
/// through ASP.NET Core minimal APIs.
/// </summary>
public static class FourthwallEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps a Fourthwall webhook endpoint using the supplied endpoint options.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder to extend.</param>
    /// <param name="pattern">The route pattern to map.</param>
    /// <param name="configure">The callback used to configure endpoint options.</param>
    /// <returns>An endpoint convention builder for further configuration.</returns>
    public static IEndpointConventionBuilder MapFourthwallWebhook(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Action<FourthwallWebhookEndpointOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentException.ThrowIfNullOrEmpty(pattern);
        ArgumentNullException.ThrowIfNull(configure);

        FourthwallWebhookEndpointOptions options = new()
        {
            ResolveWebhookOptionsAsync = static (_, _) => Task.FromResult(new FourthwallWebhookOptions()),
        };

        configure(options);

        return endpoints.MapPost(pattern, async (HttpContext context) =>
        {
            IFourthwallWebhookHandler handler = context.RequestServices.GetRequiredService<IFourthwallWebhookHandler>();

            FourthwallWebhookOptions webhookOptions =
                await options.ResolveWebhookOptionsAsync(context, context.RequestAborted).ConfigureAwait(false);

            WebhookRequest request =
                await HttpContextWebhookRequestMapper.FromHttpContextAsync(context, context.RequestAborted)
                    .ConfigureAwait(false);

            WebhookHandleResult<FourthwallWebhookEvent> result =
                await handler.HandleAsync(request, webhookOptions, context.RequestAborted)
                    .ConfigureAwait(false);

            if (result.Event is FourthwallWebhookEvent evt && options.OnEventAsync is not null)
            {
                await options.OnEventAsync(evt, context, context.RequestAborted).ConfigureAwait(false);
            }

            if (options.OnResultAsync is not null)
            {
                await options.OnResultAsync(result, context, context.RequestAborted).ConfigureAwait(false);
            }

            await WebhookResponseHttpContextWriter.WriteAsync(context, result.Response, context.RequestAborted)
                .ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Maps a Fourthwall webhook endpoint using a direct webhook options resolver delegate.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder to extend.</param>
    /// <param name="pattern">The route pattern to map.</param>
    /// <param name="resolveWebhookOptionsAsync">The callback used to resolve webhook options.</param>
    /// <param name="onEventAsync">An optional event callback.</param>
    /// <param name="onResultAsync">An optional result callback.</param>
    /// <returns>An endpoint convention builder for further configuration.</returns>
    public static IEndpointConventionBuilder MapFourthwallWebhook(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<HttpContext, CancellationToken, Task<FourthwallWebhookOptions>> resolveWebhookOptionsAsync,
        Func<FourthwallWebhookEvent, HttpContext, CancellationToken, Task>? onEventAsync = null,
        Func<WebhookHandleResult<FourthwallWebhookEvent>, HttpContext, CancellationToken, Task>? onResultAsync = null)
    {
        ArgumentNullException.ThrowIfNull(resolveWebhookOptionsAsync);

        return endpoints.MapFourthwallWebhook(
            pattern,
            options =>
            {
                options.ResolveWebhookOptionsAsync = resolveWebhookOptionsAsync;
                options.OnEventAsync = onEventAsync;
                options.OnResultAsync = onResultAsync;
            });
    }
}