# Fourthwall.Client

Modern .NET packages for working with the Fourthwall Platform API and signed webhooks.

## Packages

- `Fourthwall.Client`
- `Fourthwall.Client.AspNetCore`
- `Fourthwall.Client.DependencyInjection`
- `Fourthwall.Client.Generated`

## What this repo provides

- a typed Kiota-based API client for the Fourthwall platform
- a transport-neutral webhook handler surface
- ASP.NET Core endpoint mapping helpers
- dependency-injection helpers for application composition
- an interactive sample that can expose a local webhook endpoint through Azure Dev Tunnels

## Install

```bash
dotnet add package Fourthwall.Client
dotnet add package Fourthwall.Client.AspNetCore
```

Add the DI package when you want the registration helpers:

```bash
dotnet add package Fourthwall.Client.DependencyInjection
```

## Quick start

```csharp
using Fourthwall.Client;
using Fourthwall.Client.Webhooks;

FourthwallWebhookHandler handler = new();

WebhookHandleResult result = await handler.HandleAsync(request, cancellationToken);
if (result.IsAccepted)
{
    Console.WriteLine("Fourthwall webhook accepted.");
}
```

## ASP.NET Core webhook example

```csharp
using Fourthwall.Client.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapFourthwallWebhook(
    "/webhooks/fourthwall",
    static (_, _) => Task.FromResult(new FourthwallWebhookOptions()),
    static (evt, _, _) =>
    {
        Console.WriteLine(evt.EventType);
        return Task.CompletedTask;
    });

await app.RunAsync();
```

## Sample

The interactive sample hosts a local ASP.NET Core webhook endpoint, validates webhook signatures, and can expose that endpoint publicly through Azure Dev Tunnels:

```bash
dotnet run --project samples/Fourthwall.Client.Sample
```

See [samples/Fourthwall.Client.Sample/README.md](/C:/repos/StreamWeaver/external/Fourthwall.Client/samples/Fourthwall.Client.Sample/README.md) for the walkthrough.

## Development

```bash
dotnet test Fourthwall.Client.slnx
```
