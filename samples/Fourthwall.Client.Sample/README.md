# Fourthwall.Client.Sample

Interactive sample for:

- `Fourthwall.Client`
- `Fourthwall.Client.DependencyInjection`
- `Fourthwall.Client.AspNetCore`
- `DevTunnels.Client`

This sample:

1. hosts a local ASP.NET Core webhook endpoint,
2. validates Fourthwall webhook signatures,
3. optionally exposes the endpoint publicly with Azure Dev Tunnels,
4. prints normalized webhook events live in the console.

## What it demonstrates

- Fourthwall webhook handling with HMAC-SHA256 verification
- ASP.NET Core integration via `Fourthwall.Client.AspNetCore`
- optional public HTTPS exposure via `DevTunnels.Client`
- interactive setup via `Spectre.Console`

## Run

```bash
dotnet run --project external/Fourthwall.Client/samples/Fourthwall.Client.Sample