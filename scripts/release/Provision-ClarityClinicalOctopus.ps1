[CmdletBinding()]
param(
    [string]$ContractPath = (Join-Path (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)) 'deployment/octopus/clarityclinical.project.json'),
    [string]$ApiKey = $env:OCTOPUS_API_KEY,
    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$contract = Get-Content -LiteralPath $ContractPath -Raw | ConvertFrom-Json
if ([string]$contract.contract -cne 'askfordowding.octopus.project/v1') { throw 'Unsupported Octopus project contract.' }
$server = ([string]$contract.server).TrimEnd('/')
if ($server -cne 'https://askfordowding.octopus.app') { throw "Unexpected Octopus server '$server'." }
if ([string]$contract.space -cne 'Default') { throw 'Only the Default Octopus Space is authorised.' }
$phaseNames = @($contract.lifecycle.phases | ForEach-Object { [string]$_.name })
if (($phaseNames -join ',') -cne 'TEST,STAGING') { throw 'Clarity lifecycle must contain TEST then STAGING only.' }
if ([string]$contract.lifecycle.phases[0].mode -cne 'automatic') { throw 'TEST must be automatic.' }
if ([string]$contract.lifecycle.phases[1].mode -cne 'manual') { throw 'STAGING must be manual.' }
if ([string]$contract.targetRole -cne 'clarityclinical') { throw 'Clarity target role must be clarityclinical.' }
$deploySpec = $contract.deploymentProcess.deployStep
if ([string]$deploySpec.actionType -cne 'Octopus.Script') { throw 'Clarity deploy action must use Octopus.Script.' }
if ([string]$deploySpec.package.packageId -cne 'ClarityClinical') { throw 'Clarity package id must be ClarityClinical.' }
if ([string]$deploySpec.dispatcher -cne 'scripts/Invoke-ClarityClinical-OctopusDeployment.sh') { throw 'Unexpected Clarity deployment dispatcher.' }

$plan = [ordered]@{
    contract = [string]$contract.contract
    server = "$server/"
    space = [string]$contract.space
    lifecycle = [string]$contract.lifecycle.name
    project = [string]$contract.project.name
    packageId = [string]$contract.project.packageId
    targetRole = [string]$contract.targetRole
    targets = @($contract.environments | ForEach-Object { [ordered]@{ environment=[string]$_.name; host=[string]$_.host } })
    productionEnabled = $false
}
if ($PlanOnly) { $plan | ConvertTo-Json -Depth 8; return }

if ([string]::IsNullOrWhiteSpace($ApiKey)) { throw 'OCTOPUS_API_KEY is required for live provisioning.' }
if ($ApiKey -notmatch '^API-[A-Za-z0-9]+$') { throw 'Octopus API key format is invalid.' }
$headers = @{ 'X-Octopus-ApiKey' = $ApiKey }
function Get-Octopus([string]$Path) { Invoke-RestMethod -Method Get -Uri "$server$Path" -Headers $headers }
function Post-Octopus([string]$Path,[object]$Body) { Invoke-RestMethod -Method Post -Uri "$server$Path" -Headers $headers -ContentType 'application/json' -Body ($Body | ConvertTo-Json -Depth 40) }
function Put-Octopus([string]$Path,[object]$Body) { Invoke-RestMethod -Method Put -Uri "$server$Path" -Headers $headers -ContentType 'application/json' -Body ($Body | ConvertTo-Json -Depth 40) }
function Find-ByName([AllowNull()][object]$Items,[string]$Name) {
    if ($null -eq $Items) { return $null }
    $candidates = if ($Items.PSObject.Properties.Name -contains 'Items') { @($Items.Items) } else { @($Items) }
    foreach ($item in $candidates) { if ($null -ne $item -and [string]$item.Name -ceq $Name) { return $item } }
    return $null
}
function Set-Equals([object[]]$A,[object[]]$B) {
    ((@($A | ForEach-Object { [string]$_ } | Sort-Object)) -join '|') -ceq ((@($B | ForEach-Object { [string]$_ } | Sort-Object)) -join '|')
}

$spaces = @(Get-Octopus '/api/spaces/all')
$space = Find-ByName $spaces ([string]$contract.space)
if ($null -eq $space) { throw "Octopus Space '$($contract.space)' does not exist." }
$spaceId = [string]$space.Id
$existingEnvironments = @(Get-Octopus "/api/$spaceId/environments/all")
$environmentMap = @{}
foreach ($spec in $contract.environments) {
    $environment = Find-ByName $existingEnvironments ([string]$spec.name)
    if ($null -eq $environment) { throw "Required Octopus environment '$($spec.name)' does not exist." }
    $environmentMap[[string]$spec.name] = $environment
}

$lifecycleName = [string]$contract.lifecycle.name
$lifecycles = @(Get-Octopus "/api/$spaceId/lifecycles/all")
$lifecycle = Find-ByName $lifecycles $lifecycleName
$phases = @()
foreach ($spec in $contract.lifecycle.phases) {
    $environmentId = [string]$environmentMap[[string]$spec.environment].Id
    $automatic = @(); $optional = @()
    if ([string]$spec.mode -ceq 'automatic') { $automatic = @($environmentId) } else { $optional = @($environmentId) }
    $phases += [ordered]@{ Name=[string]$spec.name; AutomaticDeploymentTargets=$automatic; OptionalDeploymentTargets=$optional; MinimumEnvironmentsBeforePromotion=0; IsOptionalPhase=(-not [bool]$spec.required) }
}
if ($null -eq $lifecycle) {
    Write-Host "Creating Octopus lifecycle $lifecycleName."
    $lifecycle = Post-Octopus "/api/$spaceId/lifecycles" ([ordered]@{ Name=$lifecycleName; Description=[string]$contract.lifecycle.description; Phases=$phases })
} else {
    if (@($lifecycle.Phases).Count -ne 2) { throw 'Existing Clarity lifecycle phase count drift detected.' }
    for ($i=0; $i -lt 2; $i++) {
        $actual=$lifecycle.Phases[$i]; $expected=$phases[$i]
        if ([string]$actual.Name -cne [string]$expected.Name) { throw "Clarity lifecycle phase $i name drift detected." }
        if (-not (Set-Equals @($actual.AutomaticDeploymentTargets) @($expected.AutomaticDeploymentTargets))) { throw "Clarity lifecycle automatic target drift in '$($actual.Name)'." }
        if (-not (Set-Equals @($actual.OptionalDeploymentTargets) @($expected.OptionalDeploymentTargets))) { throw "Clarity lifecycle manual target drift in '$($actual.Name)'." }
    }
}

$groups = @(Get-Octopus "/api/$spaceId/projectgroups/all")
$group = Find-ByName $groups ([string]$contract.projectGroup.name)
if ($null -eq $group) { throw "Required project group '$($contract.projectGroup.name)' does not exist." }
$projects = @(Get-Octopus "/api/$spaceId/projects/all")
$project = Find-ByName $projects ([string]$contract.project.name)
if ($null -eq $project) {
    Write-Host "Creating Octopus project $($contract.project.name)."
    $project = Post-Octopus "/api/$spaceId/projects" ([ordered]@{ Name=[string]$contract.project.name; Description=[string]$contract.project.description; ProjectGroupId=[string]$group.Id; LifecycleId=[string]$lifecycle.Id })
} else {
    if ([string]$project.ProjectGroupId -cne [string]$group.Id) { throw 'Existing Clarity project group drift detected.' }
    if ([string]$project.LifecycleId -cne [string]$lifecycle.Id) { throw 'Existing Clarity lifecycle drift detected.' }
}

$machines = @(Get-Octopus "/api/$spaceId/machines/all")
foreach ($spec in $contract.environments) {
    $machine = Find-ByName $machines ([string]$spec.host)
    if ($null -eq $machine) { throw "Required deployment target '$($spec.host)' does not exist." }
    $expectedEnvironmentId = [string]$environmentMap[[string]$spec.name].Id
    if (-not (@($machine.EnvironmentIds) -contains $expectedEnvironmentId)) { throw "Target '$($spec.host)' is not scoped to '$($spec.name)'." }
    if (-not (@($machine.Roles) -contains [string]$contract.targetRole)) {
        Write-Host "Adding role '$($contract.targetRole)' to $($spec.host)."
        $machine.Roles = @($machine.Roles) + [string]$contract.targetRole
        [void](Put-Octopus "/api/$spaceId/machines/$([string]$machine.Id)" $machine)
    }
}
$production = Find-ByName $existingEnvironments 'PRODUCTION'
if ($null -ne $production) {
    foreach ($machine in $machines) {
        if ((@($machine.EnvironmentIds) -contains [string]$production.Id) -and (@($machine.Roles) -contains [string]$contract.targetRole)) {
            throw "Safety violation: production target '$($machine.Name)' already has the clarityclinical role."
        }
    }
}

$processPath = if ($project.Links -and $project.Links.DeploymentProcess) { [string]$project.Links.DeploymentProcess } else { "/api/$spaceId/projects/$([string]$project.Id)/deploymentprocesses" }
$process = Get-Octopus $processPath
$scriptBody = @'
set -Eeuo pipefail
PACKAGE_ROOT='#{Octopus.Action.Package[ClarityClinical].ExtractedPath}'
MANIFEST="$PACKAGE_ROOT/package/release-manifest.env"
[[ -f "$MANIFEST" ]] || { echo "Clarity Clinical release manifest is missing: $MANIFEST" >&2; exit 1; }
COMMIT_SHA="$(sed -n 's/^GIT_SHA=//p' "$MANIFEST" | head -n 1 | tr '[:upper:]' '[:lower:]')"
[[ "$COMMIT_SHA" =~ ^[0-9a-f]{40}$ ]] || { echo 'Clarity Clinical manifest does not contain a full Git SHA.' >&2; exit 1; }
DISPATCHER="$PACKAGE_ROOT/scripts/Invoke-ClarityClinical-OctopusDeployment.sh"
[[ -f "$DISPATCHER" ]] || { echo "Clarity Clinical dispatcher is missing: $DISPATCHER" >&2; exit 1; }
ENVIRONMENT='#{Octopus.Environment.Name}'
RELEASE_NUMBER='#{Octopus.Release.Number}'
RELEASE_ID="$RELEASE_NUMBER"
if [[ "$ENVIRONMENT" == 'STAGING' && "$RELEASE_ID" != v* ]]; then RELEASE_ID="v$RELEASE_ID"; fi
exec bash "$DISPATCHER" --environment "$ENVIRONMENT" --package-root "$PACKAGE_ROOT" --commit "$COMMIT_SHA" --release-tag "$RELEASE_ID"
'@

$needsProcess = $true
$steps = @($process.Steps)
if ($steps.Count -eq 1) {
    $step = $steps[0]; $actions = @($step.Actions)
    if ([string]$step.Name -ceq 'Deploy Clarity Clinical' -and [string]$step.Properties.'Octopus.Action.TargetRoles' -ceq 'clarityclinical' -and $actions.Count -eq 1) {
        $action = $actions[0]; $packages = @($action.Packages)
        if ([string]$action.ActionType -ceq 'Octopus.Script' -and $packages.Count -eq 1 -and [string]$packages[0].PackageId -ceq 'ClarityClinical' -and ([string]$action.Properties.'Octopus.Action.Script.ScriptBody').Contains('Invoke-ClarityClinical-OctopusDeployment.sh')) { $needsProcess = $false }
    }
}
if ($needsProcess) {
    Write-Host 'Updating Clarity Clinical deployment process.'
    $process.Steps = @([ordered]@{
        Type='Step'; PackageRequirement='LetOctopusDecide'; Properties=[ordered]@{'Octopus.Action.TargetRoles'='clarityclinical'}; Condition='Success'; StartTrigger='StartAfterPrevious'
        Actions=@([ordered]@{
            Id=[guid]::NewGuid().ToString(); Name='Deploy Clarity Clinical'; Slug='deploy-clarity-clinical'; ActionType='Octopus.Script'; Notes='Deploys the immutable Clarity Clinical package to TEST or STAGING only.'
            IsDisabled=$false; CanBeUsedForProjectVersioning=$true; IsRequired=$true; WorkerPoolId=$null
            Container=[ordered]@{Image=$null;FeedId=$null;GitUrl=$null;Dockerfile=$null}; WorkerPoolVariable=''
            Environments=@(); EnvironmentsVariable=$null; EnvironmentsVariables=@(); ExcludedEnvironments=@(); ExcludedEnvironmentsVariable=$null; ExcludedEnvironmentsVariables=@()
            Channels=@(); ChannelsVariable=$null; TenantTags=@(); TenantTagsVariable=$null; TenantTagsVariables=@()
            Packages=@([ordered]@{Id=[guid]::NewGuid().ToString();Name='ClarityClinical';PackageId='ClarityClinical';FeedId='feeds-builtin';AcquisitionLocation='Server';Properties=[ordered]@{SelectionMode='immediate';Extract='True'}})
            GitDependencies=@(); Condition='Success'
            Properties=[ordered]@{'Octopus.Action.RunOnServer'='False';'Octopus.Action.Script.ScriptSource'='Inline';'Octopus.Action.Script.Syntax'='Bash';'Octopus.Action.Script.ScriptBody'=$scriptBody}
            Links=[ordered]@{}
        })
        Name='Deploy Clarity Clinical'; Id=[guid]::NewGuid().ToString(); Slug='deploy-clarity-clinical'
    })
    $process = Put-Octopus $processPath $process
}

[ordered]@{
    contract='askfordowding.octopus.provision-result/v1'
    spaceId=$spaceId
    lifecycleId=[string]$lifecycle.Id
    projectId=[string]$project.Id
    deploymentProcessId=[string]$process.Id
    deploymentProcessChanged=[bool]$needsProcess
    productionEnabled=$false
} | ConvertTo-Json -Depth 8
