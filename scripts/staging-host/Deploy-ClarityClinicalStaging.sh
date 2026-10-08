#!/usr/bin/env bash
set -Eeuo pipefail
[[ $# -eq 4 ]] || { echo 'Usage: Deploy-ClarityClinicalStaging.sh <archive> <checksum> <rc-tag> <git-sha>' >&2; exit 2; }
script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)"
common="$script_dir/Deploy-ClarityClinicalEnvironment.sh"
[[ -f "$common" ]] || common="$(cd "$script_dir/.." && pwd -P)/release/Deploy-ClarityClinicalEnvironment.sh"
site=/etc/caddy/conf.d/clarityclinical-staging.caddy
cat > "$site" <<'EOF'
http://clarityclinical.staging.askfordowding.co.uk {
    @api path /api/* /health/*
    handle @api { reverse_proxy 127.0.0.1:5192 }
    handle {
        root * /opt/clarityclinical/staging/current/Web
        try_files {path} /index.html
        file_server
    }
}
EOF
caddy validate --config /etc/caddy/Caddyfile --adapter caddyfile
systemctl reload caddy
exec bash "$common" \
  --environment STAGING \
  --root /opt/clarityclinical/staging \
  --service clarityclinical-staging.service \
  --port 5192 \
  --database clarityclinical_staging \
  --database-role clarityclinical_staging \
  --runtime-environment Staging \
  --public-host clarityclinical.staging.askfordowding.co.uk \
  --archive "$1" --checksum "$2" --release-tag "$3" --commit "$4"
# Verification endpoints: http://127.0.0.1:5192/health/ready and /api/system/build
# Database safety: pg_dump precedes migration.sql on upgrades.
