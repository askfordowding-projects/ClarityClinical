#!/usr/bin/env bash
set -Eeuo pipefail
package_root=''; commit=''
while [[ $# -gt 0 ]]; do
  case "$1" in
    --package-root) package_root="$2"; shift 2 ;;
    --commit) commit="${2,,}"; shift 2 ;;
    *) echo "Unknown argument: $1" >&2; exit 2 ;;
  esac
done
[[ -d "$package_root" ]] || { echo 'Package root missing.' >&2; exit 2; }
[[ "$commit" =~ ^[0-9a-f]{40}$ ]] || { echo 'Invalid Git SHA.' >&2; exit 2; }
package_root="$(cd "$package_root" && pwd -P)"
archive="$package_root/clarityclinical-release.tar.gz"
checksum="$package_root/clarityclinical-release.tar.gz.sha256"
[[ -f "$archive" && -f "$checksum" ]] || { echo 'Archive/checksum missing.' >&2; exit 2; }
( cd "$package_root" && sha256sum --check "$(basename "$checksum")" )
tmp="$(mktemp -d)"; trap 'rm -rf "$tmp"' EXIT
while IFS= read -r entry; do
  n=${entry#./}; [[ -z "$n" || "$n" == '.' ]] && continue
  [[ "$n" != /* && "$n" != '..' && "$n" != ../* && "$n" != *'/../'* ]] || { echo "Unsafe archive path: $entry" >&2; exit 1; }
done < <(tar -tzf "$archive")
tar -xzf "$archive" -C "$tmp"
for f in release-manifest.env Api/ClarityClinical.Api Web/index.html migration.sql; do
  [[ -f "$tmp/$f" ]] || { echo "Package missing $f" >&2; exit 1; }
done
for f in Validate-ClarityClinical-ReleasePackage.sh Invoke-ClarityClinical-OctopusDeployment.sh Deploy-ClarityClinicalEnvironment.sh Deploy-ClarityClinicalHenryTest.sh Deploy-ClarityClinicalStaging.sh; do
  [[ -f "$package_root/scripts/$f" ]] || { echo "Package control script missing $f" >&2; exit 1; }
done
manifest="$tmp/release-manifest.env"
grep -Fxq 'APPLICATION=ClarityClinical' "$manifest" || { echo 'APPLICATION=ClarityClinical missing.' >&2; exit 1; }
grep -Fxq "GIT_SHA=$commit" "$manifest" || { echo 'GIT_SHA= mismatch.' >&2; exit 1; }
! grep -q '^RELEASE_TAG=' "$manifest" || { echo 'Immutable package must not contain an environment release tag.' >&2; exit 1; }
grep -Eq '^VERSION=[0-9]+\.[0-9]+\.[0-9]+$' "$manifest" || { echo 'VERSION invalid.' >&2; exit 1; }
grep -Eq '^BUILD_TIMESTAMP=[0-9]{4}-[0-9]{2}-[0-9]{2}T' "$manifest" || { echo 'BUILD_TIMESTAMP invalid.' >&2; exit 1; }
echo 'CLARITYCLINICAL_RELEASE_VALID'
