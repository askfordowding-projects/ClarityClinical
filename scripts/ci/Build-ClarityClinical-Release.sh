#!/usr/bin/env bash
set -Eeuo pipefail
umask 027

repo_root=''
commit=''
release_tag=''
output_root=''
while [[ $# -gt 0 ]]; do
  case "$1" in
    --repository-root) repo_root="$2"; shift 2 ;;
    --commit) commit="${2,,}"; shift 2 ;;
    --release-tag) release_tag="$2"; shift 2 ;;
    --output-root) output_root="$2"; shift 2 ;;
    *) echo "Unknown argument: $1" >&2; exit 2 ;;
  esac
done
[[ -d "$repo_root" ]] || { echo 'Valid repository root required.' >&2; exit 2; }
[[ "$commit" =~ ^[0-9a-f]{40}$ ]] || { echo 'Full Git SHA required.' >&2; exit 2; }
[[ "$release_tag" =~ ^v[0-9]+\.[0-9]+\.[0-9]+-rc\.[0-9]+$ ]] || { echo 'Immutable RC tag required.' >&2; exit 2; }
[[ -n "$output_root" ]] || { echo 'Output root required.' >&2; exit 2; }
repo_root="$(cd "$repo_root" && pwd -P)"
[[ "$(git -C "$repo_root" rev-parse HEAD)" == "$commit" ]] || { echo 'Repository HEAD does not match requested commit.' >&2; exit 1; }
rm -rf "$output_root"
mkdir -p "$output_root/package/Api" "$output_root/package/Web" "$output_root/package/scripts"

cd "$repo_root"
dotnet restore ClarityClinical.slnx
dotnet test ClarityClinical.slnx --no-restore -c Release
dotnet publish src/ClarityClinical.Api/ClarityClinical.Api.csproj -c Release -r linux-x64 --self-contained false -o "$output_root/package/Api"
dotnet tool restore
dotnet ef migrations script --idempotent --project src/ClarityClinical.Infrastructure --startup-project src/ClarityClinical.Api --output "$output_root/package/migration.sql"
pushd src/ClarityClinical.Ui >/dev/null
npm ci
npm test -- --watch=false --browsers=ChromeHeadless
npm run build
popd >/dev/null
cp -a src/ClarityClinical.Ui/dist/clarity-clinical.ui/browser/. "$output_root/package/Web/"
cp scripts/release/Validate-ClarityClinical-ReleasePackage.sh "$output_root/package/scripts/"
cp scripts/release/Invoke-ClarityClinical-OctopusDeployment.sh "$output_root/package/scripts/"
cp scripts/test-linux/Deploy-ClarityClinicalHenryTest.sh "$output_root/package/scripts/"
cp scripts/staging-host/Deploy-ClarityClinicalStaging.sh "$output_root/package/scripts/"
version="$(tr -d '\r\n' < VERSION)"
timestamp="$(date -u '+%Y-%m-%dT%H:%M:%SZ')"
cat > "$output_root/package/release-manifest.env" <<EOF
APPLICATION=ClarityClinical
VERSION=$version
RELEASE_TAG=$release_tag
GIT_SHA=$commit
BUILD_TIMESTAMP=$timestamp
EOF
chmod 0750 "$output_root/package/Api/ClarityClinical.Api" "$output_root/package/scripts/"*.sh
tar -C "$output_root/package" -czf "$output_root/clarityclinical-release.tar.gz" .
( cd "$output_root" && sha256sum clarityclinical-release.tar.gz > clarityclinical-release.tar.gz.sha256 )
bash scripts/release/Validate-ClarityClinical-ReleasePackage.sh --archive "$output_root/clarityclinical-release.tar.gz" --checksum "$output_root/clarityclinical-release.tar.gz.sha256" --release-tag "$release_tag" --commit "$commit"
echo "CLARITYCLINICAL_RELEASE=$output_root/clarityclinical-release.tar.gz"
