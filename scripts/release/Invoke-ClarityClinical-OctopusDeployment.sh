#!/usr/bin/env bash
set -Eeuo pipefail

environment=''; package_root=''; commit=''; release_id=''
while [[ $# -gt 0 ]]; do
  case "$1" in
    --environment) environment="${2^^}"; shift 2 ;;
    --package-root) package_root="$2"; shift 2 ;;
    --commit) commit="${2,,}"; shift 2 ;;
    --release-tag) release_id="$2"; shift 2 ;;
    *) echo "Unknown argument: $1" >&2; exit 2 ;;
  esac
done
[[ "$environment" == TEST || "$environment" == STAGING ]] || { echo 'Clarity Clinical production deployment is not enabled.' >&2; exit 2; }
[[ "$commit" =~ ^[0-9a-f]{40}$ ]] || { echo 'Full Git SHA required.' >&2; exit 2; }
[[ -d "$package_root" ]] || { echo 'Package root missing.' >&2; exit 2; }
package_root="$(cd "$package_root" && pwd -P)"
if [[ "$environment" == TEST ]]; then
  [[ "$release_id" =~ ^test-[0-9]+-[0-9a-f]{8}$ || "$release_id" =~ ^v[0-9]+\.[0-9]+\.[0-9]+-rc\.[0-9]+$ ]] || {
    echo "TEST requires a CI test identity or immutable RC release id: ${release_id:-<missing>}" >&2; exit 2;
  }
else
  [[ "$release_id" =~ ^v[0-9]+\.[0-9]+\.[0-9]+-rc\.[0-9]+$ ]] || {
    echo "STAGING requires an immutable RC --release-tag: ${release_id:-<missing>}" >&2; exit 2;
  }
fi
bash "$package_root/scripts/Validate-ClarityClinical-ReleasePackage.sh" --package-root "$package_root" --commit "$commit"
archive="$package_root/clarityclinical-release.tar.gz"
checksum="$package_root/clarityclinical-release.tar.gz.sha256"
case "$environment" in
  TEST) exec bash "$package_root/scripts/Deploy-ClarityClinicalHenryTest.sh" "$archive" "$checksum" "$release_id" "$commit" ;;
  STAGING) exec bash "$package_root/scripts/Deploy-ClarityClinicalStaging.sh" "$archive" "$checksum" "$release_id" "$commit" ;;
esac
