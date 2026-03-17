$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot

kiota generate `
  --language CSharp `
  --openapi "$Root/specs/fourthwall-platform-open-api.yaml" `
  --output "$Root/src/Fourthwall.Client.Generated/Generated" `
  --namespace-name Fourthwall.Client.Generated `
  --class-name FourthwallApiClient `
  --clean-output