#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"

kiota generate \
  --language CSharp \
  --openapi "$ROOT/specs/fourthwall-platform-open-api.yaml" \
  --output "$ROOT/src/Fourthwall.Client.Generated/Generated" \
  --namespace-name Fourthwall.Client.Generated \
  --class-name FourthwallApiClient \
  --clean-output