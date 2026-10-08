[CmdletBinding()]
param([string]$Root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
function Require([bool]$Condition,[string]$Message){ if(-not $Condition){ throw "Deployment contract failed: $Message" } }
function Read([string]$Relative){ $p=Join-Path $Root $Relative; Require (Test-Path $p) "missing $Relative"; Get-Content $p -Raw }

$build=Read 'scripts/ci/Build-ClarityClinical-Release.sh'
$validate=Read 'scripts/release/Validate-ClarityClinical-ReleasePackage.sh'
$dispatch=Read 'scripts/release/Invoke-ClarityClinical-OctopusDeployment.sh'
$test=Read 'scripts/test-linux/Deploy-ClarityClinicalHenryTest.sh'
$staging=Read 'scripts/staging-host/Deploy-ClarityClinicalStaging.sh'
$workflow=Read '.github/workflows/release-rc.yml'

foreach($value in @('dotnet publish','npm ci','npm run build','dotnet ef migrations script --idempotent','release-manifest.env','clarityclinical-release.tar.gz','sha256sum')){Require ($build.Contains($value)) "release builder must contain $value"}
foreach($value in @('APPLICATION=ClarityClinical','RELEASE_TAG=','GIT_SHA=','ClarityClinical.Api','Web/index.html','migration.sql')){Require ($validate.Contains($value)) "package validator must enforce $value"}
Require ($dispatch.Contains('TEST)')) 'dispatcher must route TEST'
Require ($dispatch.Contains('STAGING)')) 'dispatcher must route STAGING'
Require (-not $dispatch.Contains('PRODUCTION)')) 'production must remain disabled until explicitly implemented'
foreach($value in @('/opt/apps/clarityclinical/test','clarityclinical-test.service','127.0.0.1:5191','/health/ready','/api/system/build','pg_dump','migration.sql')){Require ($test.Contains($value)) "Henry adapter must contain $value"}
foreach($value in @('/opt/clarityclinical/staging','clarityclinical-staging.service','127.0.0.1:5192','clarityclinical.staging.askfordowding.co.uk','/health/ready','/api/system/build','pg_dump','migration.sql')){Require ($staging.Contains($value)) "Stella adapter must contain $value"}
Require ($workflow.Contains('workflow_dispatch')) 'RC release workflow must be explicit/manual'
Require ($workflow.Contains('Build-ClarityClinical-Release.sh')) 'RC release workflow must call canonical builder'
Require ($workflow.Contains('gh release create')) 'RC release workflow must publish immutable GitHub release assets'
Require (-not $workflow.Contains('pull_request')) 'RC release workflow must not be PR-triggered'
Write-Host 'Clarity Clinical standard deployment contract passed.'
