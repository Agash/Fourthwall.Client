using Fourthwall.Client.Abstractions;
using Fourthwall.Client.Webhooks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fourthwall.Client.DependencyInjection;

/// <summary>
/// Provides dependency injection registration helpers for Fourthwall.Client.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the core Fourthwall client services to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddFourthwallClient(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IFourthwallClientFactory, FourthwallClientFactory>();
        services.TryAddSingleton<FourthwallWebhookSignatureVerifier>();
        services.TryAddSingleton<IFourthwallWebhookHandler, FourthwallWebhookHandler>();

        return services;
    }
}