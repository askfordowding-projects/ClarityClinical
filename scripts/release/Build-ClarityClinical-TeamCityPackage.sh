#!/usr/bin/env bash
set -Eeuo pipefail
umask 027

output_root=''
commit=''
while [[ $# -gt 0 ]]; do
  case "$1" in
    --output-root) output_root="$2"; shift 2 ;;
    --commit) commit="${2,,}"; shift 2 ;;
    *) echo "Unknown argument: $1" >&2; exit 2 ;;
  esac
done
[[ -n "$output_root" ]] || { echo 'Output root required.' >&2; exit 2; }
[[ "$commit" =~ ^[0-9a-f]{40}$ ]] || { echo 'Full Git SHA required.' >&2; exit 2; }
[[ "$(git rev-parse HEAD | tr '[:upper:]' '[:lower:]')" == "$commit" ]] || { echo 'Checked-out SHA does not match requested commit.' >&2; exit 1; }
command -v dotnet >/dev/null
command -v npm >/dev/null
command -v tar >/dev/null
command -v sha256sum >/dev/null
[[ ! -e "$output_root" ]] || { echo "Refusing to overwrite package output: $output_root" >&2; exit 1; }
mkdir -p "$output_root/package/Api" "$output_root/package/Web" "$output_root/scripts"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
dotnet restore ClarityClinical.slnx
dotnet build ClarityClinical.slnx --configuration Release --no-restore
dotnet test ClarityClinical.slnx --configuration Release --no-build --no-restore
dotnet publish src/ClarityClinical.Api/ClarityClinical.Api.csproj --configuration Release --runtime linux-x64 --self-contained false --output "$output_root/package/Api"
dotnet tool restore
dotnet ef migrations script --idempotent --project src/ClarityClinical.Infrastructure --startup-project src/ClarityClinical.Api --configuration Release --output "$output_root/package/migration.sql"
(
  cd src/ClarityClinical.Ui
  npm ci --no-audit --no-fund
  npm test -- --watch=false --browsers=ChromeHeadless
  npm run build
)
cp -a src/ClarityClinical.Ui/dist/clarity-clinical.ui/browser/. "$output_root/package/Web/"
test -f "$output_root/package/Web/index.html"
version="$(tr -d '\r\n' < VERSION)"
timestamp="$(date -u '+%Y-%m-%dT%H:%M:%SZ')"
cat > "$output_root/package/release-manifest.env" <<EOF
APPLICATION=ClarityClinical
VERSION=$version
GIT_SHA=$commit
BUILD_TIMESTAMP=$timestamp
EOF

cp scripts/release/Validate-ClarityClinical-ReleasePackage.sh "$output_root/scripts/"
cp scripts/release/Invoke-ClarityClinical-OctopusDeployment.sh "$output_root/scripts/"
cp scripts/release/Deploy-ClarityClinicalEnvironment.sh "$output_root/scripts/"
cp scripts/test-linux/Deploy-ClarityClinicalHenryTest.sh "$output_root/scripts/"
cp scripts/staging-host/Deploy-ClarityClinicalStaging.sh "$output_root/scripts/"
chmod 0750 "$output_root/package/Api/ClarityClinical.Api" "$output_root/scripts/"*.sh
archive="$output_root/clarityclinical-release.tar.gz"
checksum="$output_root/clarityclinical-release.tar.gz.sha256"
tar -C "$output_root/package" -czf "$archive" .
( cd "$output_root" && sha256sum "$(basename "$archive")" > "$(basename "$checksum")" )
bash "$output_root/scripts/Validate-ClarityClinical-ReleasePackage.sh" --package-root "$output_root" --commit "$commit"
echo "CLARITYCLINICAL_RELEASE_PACKAGE_ROOT=$output_root"
