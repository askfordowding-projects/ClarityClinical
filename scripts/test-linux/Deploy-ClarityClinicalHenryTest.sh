#!/usr/bin/env bash
set -Eeuo pipefail
[[ $# -eq 4 ]] || { echo 'Usage: Deploy-ClarityClinicalHenryTest.sh <archive> <checksum> <rc-tag> <git-sha>' >&2; exit 2; }
script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)"
common="$script_dir/Deploy-ClarityClinicalEnvironment.sh"
[[ -f "$common" ]] || common="$(cd "$script_dir/.." && pwd -P)/release/Deploy-ClarityClinicalEnvironment.sh"
exec bash "$common" \
  --environment TEST \
  --root /opt/apps/clarityclinical/test \
  --service clarityclinical-test.service \
  --port 5191 \
  --database clarityclinical_test \
  --database-role clarityclinical_test \
  --runtime-environment Test \
  --archive "$1" --checksum "$2" --release-tag "$3" --commit "$4"
# Verification endpoints: http://127.0.0.1:5191/health/ready and /api/system/build
# Database safety: pg_dump precedes migration.sql on upgrades.
