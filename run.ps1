param(
    [switch]$InMemory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-Section([string]$text) {
    Write-Host "`n==== $text ====\n" -ForegroundColor Cyan
}

function Test-Command([string]$name) {
    return [bool](Get-Command $name -ErrorAction SilentlyContinue)
}

function Test-Port([int]$Port, [string]$HostName = 'localhost') {
    try {
        $client = New-Object System.Net.Sockets.TcpClient
        $iar = $client.BeginConnect($HostName, $Port, $null, $null)
        $success = $iar.AsyncWaitHandle.WaitOne(1000, $false)
        if ($success) { $client.EndConnect($iar) }
        $client.Close()
        return $success
    } catch { return $false }
}

function Wait-ForPort([int]$Port, [string]$Name, [int]$Retries = 60, [int]$DelaySeconds = 2) {
    for ($i = 1; $i -le $Retries; $i++) {
        if (Test-Port -Port $Port) {
            Write-Host "$Name is ready on port $Port" -ForegroundColor Green
            return $true
        }
        Start-Sleep -Seconds $DelaySeconds
    }
    Write-Warning "$Name did not become ready on port $Port after $Retries attempts"
    return $false
}

function Invoke-WithRetry([scriptblock]$Action, [int]$Retries = 10, [int]$DelaySeconds = 3) {
    for ($i = 1; $i -le $Retries; $i++) {
        try {
            & $Action
            return
        } catch {
            if ($i -eq $Retries) { throw }
            Start-Sleep -Seconds $DelaySeconds
        }
    }
}

$RepoRoot = $PSScriptRoot
Set-Location $RepoRoot

Write-Section "Environment checks"
if (-not (Test-Command dotnet)) { throw "dotnet SDK not found. Please install .NET 9 SDK." }
$hasDocker = Test-Command docker
if ($InMemory) { Write-Host "Forcing in-memory mode via -InMemory" -ForegroundColor Yellow }

$useInMemory = $InMemory

if (-not $useInMemory -and $hasDocker) {
    Write-Section "Starting Docker services (Postgres + RabbitMQ)"

    # Ensure we are NOT in in-memory mode for EF or MassTransit
    $env:UseInMemoryEF = $null
    $env:UseMassTransitInMemory = $null

    # Set RabbitMQ connection env (used by MassTransit config)
    if (-not $env:RabbitMq__HostName) { $env:RabbitMq__HostName = 'localhost' }
    if (-not $env:RabbitMq__UserName) { $env:RabbitMq__UserName = 'guest' }
    if (-not $env:RabbitMq__Password) { $env:RabbitMq__Password = 'guest' }
    if (-not $env:RabbitMq__Port)     { $env:RabbitMq__Port     = '5672' }

    try {
        docker compose up -d | Out-Null
    } catch {
        Write-Warning "docker compose failed. Falling back to in-memory mode."
        $useInMemory = $true
    }

    if (-not $useInMemory) {
        # Wait for Postgres (5432), RabbitMQ (5672) and RabbitMQ UI (15672)
        [void](Wait-ForPort -Port 5432 -Name 'Postgres' -Retries 60 -DelaySeconds 2)
        [void](Wait-ForPort -Port 5672 -Name 'RabbitMQ' -Retries 60 -DelaySeconds 2)
        [void](Wait-ForPort -Port 15672 -Name 'RabbitMQ UI' -Retries 60 -DelaySeconds 2)
    }
}
elseif (-not $hasDocker -and -not $useInMemory) {
    Write-Warning "Docker not found. Falling back to in-memory mode."
    $useInMemory = $true
}

Write-Section "Restoring tools"
dotnet tool restore | Out-Null

if (-not $useInMemory) {
    Write-Section "Applying EF Core migrations"
    $infraProj = Join-Path $RepoRoot 'src/Mottu.Rentals.Infrastructure/Mottu.Rentals.Infrastructure.csproj'
    $apiProj   = Join-Path $RepoRoot 'src/Mottu.Rentals.Api/Mottu.Rentals.Api.csproj'

    # Ensure default connection string via env (matches docker-compose)
    if (-not $env:MOTTU_POSTGRES_CONNECTION) {
        $env:MOTTU_POSTGRES_CONNECTION = 'Host=localhost;Port=5432;Database=mottu_rentals;Username=postgres;Password=postgres'
    }

    Invoke-WithRetry -Retries 10 -DelaySeconds 3 -Action {
        dotnet tool run dotnet-ef database update -p $infraProj -s $apiProj | Out-Null
    }
} else {
    Write-Section "Using in-memory mode (no Docker required)"
    $env:UseInMemoryEF = 'true'
    $env:UseMassTransitInMemory = 'true'
}

Write-Section "Running API"
$env:ASPNETCORE_URLS = 'http://localhost:5000'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project (Join-Path $RepoRoot 'src/Mottu.Rentals.Api')


