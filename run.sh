#!/usr/bin/env bash

set -euo pipefail

# Usage: ./run.sh [--in-memory]

IN_MEMORY=false
for arg in "$@"; do
  case "$arg" in
    --in-memory|-m)
      IN_MEMORY=true
      shift
      ;;
    *)
      ;;
  esac
done

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$REPO_ROOT"

section() {
  echo -e "\n==== $1 ====\n"
}

have_cmd() {
  command -v "$1" >/dev/null 2>&1
}

test_port() {
  local host="${2:-localhost}"
  local port="$1"
  # Bash TCP check
  (echo > /dev/tcp/"$host"/"$port") >/dev/null 2>&1
}

wait_for_port() {
  local port="$1"
  local name="$2"
  local retries="${3:-60}"
  local delay="${4:-2}"
  for ((i=1;i<=retries;i++)); do
    if test_port "$port"; then
      echo "$name is ready on port $port"
      return 0
    fi
    sleep "$delay"
  done
  echo "WARNING: $name did not become ready on port $port after $retries attempts" >&2
  return 1
}

invoke_with_retry() {
  local cmd="$1"
  local retries="${2:-10}"
  local delay="${3:-3}"
  local n=1
  while true; do
    if bash -lc "$cmd"; then
      return 0
    fi
    if [[ $n -ge $retries ]]; then
      echo "ERROR: Command failed after $retries attempts: $cmd" >&2
      return 1
    fi
    sleep "$delay"
    n=$((n+1))
  done
}

section "Environment checks"
if ! have_cmd dotnet; then
  echo "dotnet SDK not found. Please install .NET 9 SDK." >&2
  exit 1
fi
HAS_DOCKER=false
if have_cmd docker; then HAS_DOCKER=true; fi
if [[ "$IN_MEMORY" == "true" ]]; then echo "Forcing in-memory mode via --in-memory"; fi

USE_IN_MEMORY="$IN_MEMORY"

if [[ "$USE_IN_MEMORY" != "true" && "$HAS_DOCKER" == "true" ]]; then
  section "Starting Docker services (Postgres + RabbitMQ)"

  # Ensure we are NOT in in-memory mode for EF or MassTransit
  unset UseInMemoryEF || true
  unset UseMassTransitInMemory || true

  # Set RabbitMQ connection env (used by MassTransit config)
  export RabbitMq__HostName="${RabbitMq__HostName:-localhost}"
  export RabbitMq__UserName="${RabbitMq__UserName:-guest}"
  export RabbitMq__Password="${RabbitMq__Password:-guest}"
  export RabbitMq__Port="${RabbitMq__Port:-5672}"

  if ! docker compose up -d; then
    echo "docker compose failed. Falling back to in-memory mode."
    USE_IN_MEMORY=true
  fi

  if [[ "$USE_IN_MEMORY" != "true" ]]; then
    # Wait for Postgres (5432), RabbitMQ (5672) and RabbitMQ UI (15672)
    wait_for_port 5432 "Postgres" 60 2 || true
    wait_for_port 5672 "RabbitMQ" 60 2 || true
    wait_for_port 15672 "RabbitMQ UI" 60 2 || true
  fi
elif [[ "$USE_IN_MEMORY" != "true" && "$HAS_DOCKER" != "true" ]]; then
  echo "Docker not found. Falling back to in-memory mode."
  USE_IN_MEMORY=true
fi

section "Restoring tools"
dotnet tool restore >/dev/null

if [[ "$USE_IN_MEMORY" != "true" ]]; then
  section "Applying EF Core migrations"
  INFRA_PROJ="$REPO_ROOT/src/Mottu.Rentals.Infrastructure/Mottu.Rentals.Infrastructure.csproj"
  API_PROJ="$REPO_ROOT/src/Mottu.Rentals.Api/Mottu.Rentals.Api.csproj"

  # Ensure default connection string via env (matches docker-compose)
  export MOTTU_POSTGRES_CONNECTION="${MOTTU_POSTGRES_CONNECTION:-Host=localhost;Port=5432;Database=mottu_rentals;Username=postgres;Password=postgres}"

  invoke_with_retry "dotnet tool run dotnet-ef database update -p '$INFRA_PROJ' -s '$API_PROJ' >/dev/null" 10 3
else
  section "Using in-memory mode (no Docker required)"
  export UseInMemoryEF=true
  export UseMassTransitInMemory=true
fi

section "Running API"
export ASPNETCORE_URLS="http://localhost:5000"
export ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project "$REPO_ROOT/src/Mottu.Rentals.Api"


