#!/usr/bin/env bash
set -Eeuo pipefail
environment=''; package_root=''; commit=''; release_tag=''
while [[ $# -gt 0 ]]; do
  case "$1" in
    --environment) environment="${2^^}"; shift 2 ;;
    --package-root) package_root="$2"; shift 2 ;;
    --commit) commit="${2,,}"; shift 2 ;;
    --release-tag) release_tag="$2"; shift 2 ;;
    *) echo "Unknown argument: $1" >&2; exit 2 ;;
  esac
done
[[ "$environment" == TEST || "$environment" == STAGING ]] || { echo 'Clarity Clinical production deployment is not enabled.' >&2; exit 2; }
[[ "$commit" =~ ^[0-9a-f]{40}$ ]] || { echo 'Full Git SHA required.' >&2; exit 2; }
[[ "$release_tag" =~ ^v[0-9]+\.[0-9]+\.[0-9]+-rc\.[0-9]+$ ]] || { echo 'Immutable RC tag required.' >&2; exit 2; }
[[ -d "$package_root" ]] || { echo 'Package root missing.' >&2; exit 2; }
package_root="$(cd "$package_root" && pwd -P)"
archive="$package_root/clarityclinical-release.tar.gz"
checksum="$package_root/clarityclinical-release.tar.gz.sha256"
bash "$package_root/scripts/Validate-ClarityClinical-ReleasePackage.sh" --archive "$archive" --checksum "$checksum" --release-tag "$release_tag" --commit "$commit"
case "$environment" in
  TEST) exec bash "$package_root/scripts/Deploy-ClarityClinicalHenryTest.sh" "$archive" "$checksum" "$release_tag" "$commit" ;;
  STAGING) exec bash "$package_root/scripts/Deploy-ClarityClinicalStaging.sh" "$archive" "$checksum" "$release_tag" "$commit" ;;
esac
