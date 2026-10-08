#!/usr/bin/env bash
set -Eeuo pipefail
archive=''; checksum=''; release_tag=''; commit=''
while [[ $# -gt 0 ]]; do
  case "$1" in
    --archive) archive="$2"; shift 2 ;;
    --checksum) checksum="$2"; shift 2 ;;
    --release-tag) release_tag="$2"; shift 2 ;;
    --commit) commit="${2,,}"; shift 2 ;;
    *) echo "Unknown argument: $1" >&2; exit 2 ;;
  esac
done
[[ -f "$archive" && -f "$checksum" ]] || { echo 'Archive/checksum missing.' >&2; exit 2; }
[[ "$release_tag" =~ ^v[0-9]+\.[0-9]+\.[0-9]+-rc\.[0-9]+$ ]] || { echo 'Invalid RC tag.' >&2; exit 2; }
[[ "$commit" =~ ^[0-9a-f]{40}$ ]] || { echo 'Invalid Git SHA.' >&2; exit 2; }
checksum_dir="$(dirname "$checksum")"; checksum_name="$(basename "$checksum")"
( cd "$checksum_dir" && sha256sum --check "$checksum_name" )
tmp="$(mktemp -d)"; trap 'rm -rf "$tmp"' EXIT
while IFS= read -r entry; do
  n=${entry#./}; [[ -z "$n" || "$n" == '.' ]] && continue
  [[ "$n" != /* && "$n" != '..' && "$n" != ../* && "$n" != *'/../'* ]] || { echo "Unsafe archive path: $entry" >&2; exit 1; }
done < <(tar -tzf "$archive")
tar -xzf "$archive" -C "$tmp"
for f in release-manifest.env Api/ClarityClinical.Api Web/index.html migration.sql scripts/Invoke-ClarityClinical-OctopusDeployment.sh scripts/Deploy-ClarityClinicalHenryTest.sh scripts/Deploy-ClarityClinicalStaging.sh; do
  [[ -f "$tmp/$f" ]] || { echo "Package missing $f" >&2; exit 1; }
done
manifest="$tmp/release-manifest.env"
grep -Fxq 'APPLICATION=ClarityClinical' "$manifest" || { echo 'APPLICATION=ClarityClinical missing.' >&2; exit 1; }
grep -Fxq "RELEASE_TAG=$release_tag" "$manifest" || { echo 'RELEASE_TAG= mismatch.' >&2; exit 1; }
grep -Fxq "GIT_SHA=$commit" "$manifest" || { echo 'GIT_SHA= mismatch.' >&2; exit 1; }
grep -Eq '^VERSION=[0-9]+\.[0-9]+\.[0-9]+$' "$manifest" || { echo 'VERSION invalid.' >&2; exit 1; }
grep -Eq '^BUILD_TIMESTAMP=[0-9]{4}-[0-9]{2}-[0-9]{2}T' "$manifest" || { echo 'BUILD_TIMESTAMP invalid.' >&2; exit 1; }
echo 'CLARITYCLINICAL_RELEASE_VALID'
