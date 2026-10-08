[CmdletBinding()]
param(
    [string]$Root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Require([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw "Release readiness contract failed: $Message" }
}

$version = (Get-Content (Join-Path $Root 'VERSION') -Raw).Trim()
$package = Get-Content (Join-Path $Root 'src\ClarityClinical.Ui\package.json') -Raw | ConvertFrom-Json
$workflowPath = Join-Path $Root '.github\workflows\ci.yml'
$workflow = if (Test-Path $workflowPath) { Get-Content $workflowPath -Raw } else { '' }
$packaging = Get-Content (Join-Path $Root 'tests\Deployment\verify-packaging.ps1') -Raw

Require ($version -match '^\d+\.\d+\.\d+$') 'VERSION must be semantic version X.Y.Z.'
Require ($package.version -eq $version) 'Angular package version must match VERSION.'
Require (Test-Path $workflowPath) 'GitHub Actions CI workflow is missing.'
Require ($workflow -match 'postgres:18') 'CI must provide PostgreSQL 18 for integration tests.'
Require ($workflow -match 'dotnet test ClarityClinical\.slnx') 'CI must run the complete .NET test suite.'
Require ($workflow -match 'ef migrations has-pending-model-changes') 'CI must reject pending EF model changes.'
Require ($workflow -match 'npm test -- --watch=false --browsers=ChromeHeadless') 'CI must run Angular tests headlessly.'
Require ($workflow -match 'npm run build') 'CI must build the Angular application.'
Require ($workflow -match 'npm audit --omit=dev') 'CI must audit runtime npm dependencies.'
Require ($workflow -match 'verify-packaging\.ps1') 'CI must enforce the public-demo packaging contract.'
Require ($workflow -match 'verify-release-readiness\.ps1') 'CI must enforce this release-readiness contract.'
Require ($workflow -match 'git diff --check') 'CI must reject whitespace errors.'
Require ($packaging -match 'Build__Version') 'Packaging contract must verify build version metadata.'
Require ($packaging -match 'Build__Commit') 'Packaging contract must verify commit metadata.'

Write-Host 'Clarity Clinical release readiness contract passed.'
