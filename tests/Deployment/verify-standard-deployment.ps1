[CmdletBinding()]
param([string]$Root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
function Require([bool]$Condition,[string]$Message){ if(-not $Condition){ throw "Deployment contract failed: $Message" } }
function Read([string]$Relative){ $p=Join-Path $Root $Relative; Require (Test-Path $p) "missing $Relative"; Get-Content $p -Raw }

$build=Read 'scripts/release/Build-ClarityClinical-TeamCityPackage.sh'
$validate=Read 'scripts/release/Validate-ClarityClinical-ReleasePackage.sh'
$dispatch=Read 'scripts/release/Invoke-ClarityClinical-OctopusDeployment.sh'
$common=Read 'scripts/release/Deploy-ClarityClinicalEnvironment.sh'
$test=Read 'scripts/test-linux/Deploy-ClarityClinicalHenryTest.sh'
$staging=Read 'scripts/staging-host/Deploy-ClarityClinicalStaging.sh'
$provision=Read 'scripts/release/Provision-ClarityClinicalOctopus.ps1'
$octopusContract=Read 'deployment/octopus/clarityclinical.project.json'

foreach($value in @('dotnet publish','npm ci','npm run build','dotnet ef migrations script --idempotent','package/release-manifest.env','GIT_SHA=','ClarityClinical.Api','Web/index.html','migration.sql')){Require ($build.Contains($value)) "TeamCity builder must contain $value"}
foreach($value in @('APPLICATION=ClarityClinical','GIT_SHA=','ClarityClinical.Api','Web/index.html','migration.sql')){Require ($validate.Contains($value)) "package validator must enforce $value"}
Require ($validate.Contains('Immutable package must not contain an environment release tag.')) 'package validator must reject embedded environment release tags'
Require (-not $common.Contains('RELEASE_TAG=')) 'host deployer must not expect RELEASE_TAG inside immutable package bytes'
Require ($dispatch.Contains('TEST)')) 'dispatcher must route TEST'
Require ($dispatch.Contains('STAGING)')) 'dispatcher must route STAGING'
Require ($dispatch.Contains('TEST requires a CI test identity or immutable RC release id')) 'TEST must accept a CI test identity or an RC id'
Require ($dispatch.Contains('STAGING requires an immutable RC')) 'STAGING must require an immutable RC'
Require (-not $dispatch.Contains('PRODUCTION)')) 'production must remain disabled until explicitly implemented'
foreach($value in @('/opt/apps/clarityclinical/test','clarityclinical-test.service','127.0.0.1:5191','/health/ready','/api/system/build','pg_dump','migration.sql')){Require ($test.Contains($value)) "Henry adapter must contain $value"}
foreach($value in @('/opt/clarityclinical/staging','clarityclinical-staging.service','127.0.0.1:5192','clarityclinical.staging.askfordowding.co.uk','/health/ready','/api/system/build','pg_dump','migration.sql')){Require ($staging.Contains($value)) "Stella adapter must contain $value"}
foreach($value in @('clarityclinical','productionEnabled = $false','Safety violation: production target','Invoke-ClarityClinical-OctopusDeployment.sh')){Require ($provision.Contains($value)) "Octopus provisioner must contain $value"}
foreach($value in @('"name": "TEST"','"name": "STAGING"','"host": "Henry TEST"','"host": "Stella"','"targetRole": "clarityclinical"','"packageId": "ClarityClinical"','Clarity Clinical - Test -> Staging')){Require ($octopusContract.Contains($value)) "Octopus contract must contain $value"}
Require (-not $octopusContract.Contains('PRODUCTION')) 'Clarity Octopus contract must not contain PRODUCTION'
Require (-not (Test-Path (Join-Path $Root '.github/workflows/release-rc.yml'))) 'GitHub must not own Clarity RC creation once TeamCity build-once promotion is enabled'
Write-Host 'Clarity Clinical standard deployment contract passed.'
