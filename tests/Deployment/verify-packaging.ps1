$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$apiDockerfile = Get-Content (Join-Path $root 'deployment\Dockerfile.api') -Raw
$uiDockerfile = Get-Content (Join-Path $root 'deployment\Dockerfile.ui') -Raw
$compose = Get-Content (Join-Path $root 'deployment\compose.public-demo.yml') -Raw
$nginx = Get-Content (Join-Path $root 'deployment\nginx\default.conf') -Raw
$envExample = Get-Content (Join-Path $root 'deployment\.env.example') -Raw
$program = Get-Content (Join-Path $root 'src\ClarityClinical.Api\Program.cs') -Raw

function Require([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
}

Require ($apiDockerfile -match 'mcr.microsoft.com/dotnet/sdk:10.0') 'API image must build with .NET 10.'
Require ($apiDockerfile -match 'migrations bundle') 'API image must contain an EF migration bundle.'
Require ($apiDockerfile -match 'EXPOSE 8080') 'API image must expose port 8080.'
Require ($apiDockerfile -match 'curl --fail') 'API image must use an available HTTP client for its health check.'
Require ($apiDockerfile -match '/health/live') 'API image must define a liveness health check.'
Require ($uiDockerfile -match 'node:22-alpine') 'UI image must build with Node 22.'
Require ($uiDockerfile -match 'nginx:') 'UI image must use nginx for static hosting.'
Require ($nginx -match 'location /api/') 'nginx must proxy API routes.'
Require ($nginx -match 'location /health/') 'nginx must proxy health routes.'
Require ($nginx -match 'try_files \$uri \$uri/ /index.html') 'nginx must support Angular client-side routes.'
Require ($compose -match 'service_completed_successfully') 'API must wait for successful migrations.'
Require ($compose -match 'clarityclinical-postgres:/var/lib/postgresql/data') 'PostgreSQL must use persistent storage.'
Require ($compose -match 'POSTGRES_PASSWORD:\?Set POSTGRES_PASSWORD') 'PostgreSQL password must be supplied externally.'
Require ($compose -match 'Build__Version: \${CLARITY_VERSION') 'Public demo must expose a version setting.'
Require ($compose -match 'Build__Commit: \${CLARITY_COMMIT') 'Public demo must expose a commit setting.'
Require ($compose -match 'Build__Timestamp: \${CLARITY_BUILD_TIMESTAMP') 'Public demo must expose a build timestamp setting.'
Require (-not ($compose -match 'change-me')) 'Compose must not contain a default database password.'
Require ($envExample -match 'POSTGRES_PASSWORD=change-me') 'Environment example must document the password setting.'
Require ($program -match 'IsEnvironment\("PublicDemo"\)') 'PublicDemo must leave HTTPS termination to the edge proxy.'
Require (-not (Test-Path (Join-Path $root '.env'))) 'A real .env file must not be committed at repository root.'
Write-Output 'Deployment packaging contract passed.'
