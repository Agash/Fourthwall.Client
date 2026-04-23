# Fourthwall.Client

A .NET client library for the [Fourthwall Platform API](https://fourthwall.com/developers) and webhook processing.

| Package | NuGet |
|---|---|
| `Fourthwall.Client` | [![NuGet](https://img.shields.io/nuget/v/Fourthwall.Client)](https://www.nuget.org/packages/Fourthwall.Client) |
| `Fourthwall.Client.AspNetCore` | [![NuGet](https://img.shields.io/nuget/v/Fourthwall.Client.AspNetCore)](https://www.nuget.org/packages/Fourthwall.Client.AspNetCore) |
| `Fourthwall.Client.DependencyInjection` | [![NuGet](https://img.shields.io/nuget/v/Fourthwall.Client.DependencyInjection)](https://www.nuget.org/packages/Fourthwall.Client.DependencyInjection) |
| `Fourthwall.Client.Generated` | [![NuGet](https://img.shields.io/nuget/v/Fourthwall.Client.Generated)](https://www.nuget.org/packages/Fourthwall.Client.Generated) |

## Features

- **Kiota-generated REST client** for the Fourthwall Platform API (shop data, webhooks, orders, memberships, etc.)
- **Transport-neutral webhook handler** — validate and deserialize incoming Fourthwall webhook deliveries
- **Optional HMAC-SHA256 signature verification** — enforce when you have a signing secret; skip when you don't
- **ASP.NET Core integration** — a single `MapFourthwallWebhook()` call wires up a fully functional webhook endpoint
- **AOT-friendly** — uses `System.Text.Json` source generation

## Quick Start

### 1. Register services

```csharp
builder.Services.AddFourthwallClient();
```

### 2. Map a webhook endpoint

```csharp
app.MapFourthwallWebhook(
    "/webhooks/fourthwall",
    resolveWebhookOptionsAsync: (_, _) => Task.FromResult(new FourthwallWebhookOptions
    {
        // Leave SigningSecret null to skip signature verification.
        // Provide it to enforce HMAC-SHA256 validation.
        SigningSecret = configuration["Fourthwall:SigningSecret"],
        SignatureMode = FourthwallWebhookSignatureMode.ShopWebhook,
    }),
    onEventAsync: async (evt, httpContext, ct) =>
    {
        switch (evt)
        {
            case FourthwallOrderPlacedWebhookEvent order:
                Console.WriteLine($"New order: {order.Id}");
                break;

            case FourthwallDonationWebhookEvent donation:
                Console.WriteLine($"Donation: {donation.Id}");
                break;

            case FourthwallSubscriptionPurchasedWebhookEvent sub:
                Console.WriteLine($"New subscriber: {sub.Id}");
                break;
        }

        await Task.CompletedTask;
    });
```

### 3. Call the REST API

```csharp
// Resolve the factory from DI or create it directly.
IFourthwallClientFactory factory = serviceProvider.GetRequiredService<IFourthwallClientFactory>();

FourthwallApiClient client = factory.CreateWithBasicAuth(new FourthwallBasicAuthOptions
{
    Username = "your-shop-username",
    Password = "your-shop-password",
});

// Register a webhook via the API.
WebhookConfigurationV1? webhook = await client.OpenApi.V10.Webhooks.PostAsync(
    new WebhookConfigurationCreateRequest
    {
        Url = "https://your-public-url/webhooks/fourthwall",
        AllowedTypes =
        [
            WebhookConfigurationCreateRequest_allowedTypes.ORDER_PLACED,
            WebhookConfigurationCreateRequest_allowedTypes.DONATION,
            WebhookConfigurationCreateRequest_allowedTypes.SUBSCRIPTION_PURCHASED,
        ],
    });
```

## Packages

| Package | Purpose |
|---|---|
| `Fourthwall.Client` | Core webhook handler + signature verifier + client factory |
| `Fourthwall.Client.Generated` | Kiota-generated REST client (`FourthwallApiClient`) |
| `Fourthwall.Client.AspNetCore` | `MapFourthwallWebhook()` endpoint extension |
| `Fourthwall.Client.DependencyInjection` | `AddFourthwallClient()` service registration |

## Signature Verification

Fourthwall signs webhook deliveries with HMAC-SHA256. Whether to verify is controlled by `FourthwallWebhookOptions.SigningSecret`:

| `SigningSecret` | Behavior |
|---|---|
| `null` or `""` | Verification is **skipped**. All payloads are accepted and deserialized. |
| Non-empty string | **Enforced**. The request must carry a valid HMAC-SHA256 signature in the header for the selected `SignatureMode`. Requests with a missing or invalid signature are rejected with HTTP 401. |

```csharp
// Skip verification — useful with Dev Tunnels / private endpoints.
new FourthwallWebhookOptions()

// Enforce shop webhook signatures.
new FourthwallWebhookOptions
{
    SigningSecret = "your-hmac-secret",
    SignatureMode = FourthwallWebhookSignatureMode.ShopWebhook,
}

// Enforce platform app webhook signatures.
new FourthwallWebhookOptions
{
    SigningSecret = "your-platform-app-secret",
    SignatureMode = FourthwallWebhookSignatureMode.PlatformAppWebhook,
}
```

### Signature headers

| `SignatureMode` | Header |
|---|---|
| `ShopWebhook` (default) | `X-Fourthwall-Hmac-SHA256` |
| `PlatformAppWebhook` | `X-Fourthwall-Hmac-Apps-SHA256` |

## Webhook Events

| Type | Class |
|---|---|
| `ORDER_PLACED` | `FourthwallOrderPlacedWebhookEvent` |
| `DONATION` | `FourthwallDonationWebhookEvent` |
| `SUBSCRIPTION_PURCHASED` | `FourthwallSubscriptionPurchasedWebhookEvent` |
| `SUBSCRIPTION_EXPIRED` | `FourthwallSubscriptionExpiredWebhookEvent` |
| `SUBSCRIPTION_CHANGED` | `FourthwallSubscriptionChangedWebhookEvent` |
| `GIFT_PURCHASE` | `FourthwallGiftPurchaseWebhookEvent` |
| Any other type | `FourthwallUnknownWebhookEvent` |

All events inherit from `FourthwallWebhookEvent` which carries `Id`, `WebhookId`, `ShopId`, `Type`, `ApiVersion`, `CreatedAt`, and `TestMode`.

## Transport-neutral usage

`FourthwallWebhookHandler` works with any host — ASP.NET Core, Azure Functions, AWS Lambda, console apps, etc.:

```csharp
FourthwallWebhookHandler handler = new(new FourthwallWebhookSignatureVerifier());

WebhookHandleResult<FourthwallWebhookEvent> result = await handler.HandleAsync(
    new WebhookRequest
    {
        Method  = request.Method,
        Path    = request.Path,
        ContentType = request.ContentType,
        Headers = ..., // Dictionary<string, string[]>
        Body    = requestBodyBytes,
    },
    new FourthwallWebhookOptions
    {
        SigningSecret = "optional-secret",
    });

if (result.IsAuthenticated && result.Event is FourthwallOrderPlacedWebhookEvent order)
{
    // handle order
}
```

## Sample

The `samples/Fourthwall.Client.Sample` project is a fully automated interactive console that:

1. Prompts for Fourthwall API credentials (optional — for auto-registration)
2. Prompts for webhook path, port, and signing secret (all optional)
3. Starts an ASP.NET Core webhook receiver
4. Opens an Azure Dev Tunnels public HTTPS endpoint
5. Automatically registers (and deregisters on exit) the webhook via the Fourthwall API
6. Prints incoming events to the console in real time

Run it with:

```
cd samples/Fourthwall.Client.Sample
dotnet run
```

## Requirements

- .NET 10+
- Fourthwall shop credentials (for REST API calls) — optional if you only need webhook receiving

## License

MIT
