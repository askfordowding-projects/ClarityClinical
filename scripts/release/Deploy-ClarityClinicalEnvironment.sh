#!/usr/bin/env bash
set -Eeuo pipefail
umask 027

environment=''; root=''; service=''; port=''; db_name=''; db_role=''; runtime_environment=''; public_host=''
archive=''; checksum=''; release_tag=''; git_sha=''
while [[ $# -gt 0 ]]; do
  case "$1" in
    --environment) environment="$2"; shift 2 ;;
    --root) root="$2"; shift 2 ;;
    --service) service="$2"; shift 2 ;;
    --port) port="$2"; shift 2 ;;
    --database) db_name="$2"; shift 2 ;;
    --database-role) db_role="$2"; shift 2 ;;
    --runtime-environment) runtime_environment="$2"; shift 2 ;;
    --public-host) public_host="$2"; shift 2 ;;
    --archive) archive="$2"; shift 2 ;;
    --checksum) checksum="$2"; shift 2 ;;
    --release-tag) release_tag="$2"; shift 2 ;;
    --commit) git_sha="${2,,}"; shift 2 ;;
    *) echo "Unknown argument: $1" >&2; exit 2 ;;
  esac
done
[[ ${EUID:-$(id -u)} -eq 0 ]] || { echo 'Deployment must run as root via the Octopus Tentacle.' >&2; exit 1; }
[[ "$git_sha" =~ ^[0-9a-f]{40}$ ]] || { echo 'Full Git SHA required.' >&2; exit 2; }
[[ "$release_tag" =~ ^v[0-9]+\.[0-9]+\.[0-9]+-rc\.[0-9]+$ ]] || { echo 'Immutable RC tag required.' >&2; exit 2; }
for cmd in tar sha256sum curl psql pg_dump pg_restore runuser systemctl readlink; do command -v "$cmd" >/dev/null || { echo "Missing command: $cmd" >&2; exit 1; }; done
( cd "$(dirname "$checksum")" && sha256sum --check "$(basename "$checksum")" )

releases="$root/releases"; backups="$root/backups/database"; current="$root/current"
short_sha=${git_sha:0:8}; release_dir="$releases/${release_tag}-${short_sha}"; incoming="$releases/.incoming-${release_tag}-${short_sha}-$$"
previous=''; switched=false
cleanup(){ rm -rf "$incoming" 2>/dev/null || true; }
trap cleanup EXIT
mkdir -p "$releases" "$backups"
[[ ! -e "$release_dir" ]] || { echo "Immutable release already exists: $release_dir" >&2; exit 1; }
mkdir -p "$incoming"; tar -xzf "$archive" -C "$incoming"
manifest="$incoming/release-manifest.env"
grep -Fxq 'APPLICATION=ClarityClinical' "$manifest" || { echo 'Application identity mismatch.' >&2; exit 1; }
grep -Fxq "RELEASE_TAG=$release_tag" "$manifest" || { echo 'Release tag mismatch.' >&2; exit 1; }
grep -Fxq "GIT_SHA=$git_sha" "$manifest" || { echo 'Git SHA mismatch.' >&2; exit 1; }
version="$(sed -n 's/^VERSION=//p' "$manifest" | head -1)"; build_timestamp="$(sed -n 's/^BUILD_TIMESTAMP=//p' "$manifest" | head -1)"

id "$db_role" >/dev/null 2>&1 || useradd --system --home-dir "/var/lib/clarityclinical/${environment,,}" --create-home --shell /usr/sbin/nologin "$db_role"
if ! runuser -u postgres -- psql -Atqc "SELECT 1 FROM pg_roles WHERE rolname='$db_role'" | grep -qx 1; then runuser -u postgres -- createuser --login "$db_role"; fi
if ! runuser -u postgres -- psql -Atqc "SELECT 1 FROM pg_database WHERE datname='$db_name'" | grep -qx 1; then runuser -u postgres -- createdb --owner="$db_role" "$db_name"; fi
install -d -m 0755 /etc/clarityclinical "$root" "$releases"; install -d -m 0700 -o "$db_role" -g "$db_role" "$backups"
env_file="/etc/clarityclinical/${environment,,}.env"
cat > "$env_file" <<EOF
ConnectionStrings__ClarityClinical=Host=/var/run/postgresql;Database=$db_name;Username=$db_role
Build__Version=$version
Build__Commit=$git_sha
Build__Timestamp=$build_timestamp
EOF
chown root:"$db_role" "$env_file"; chmod 0640 "$env_file"

if [[ -L "$current" ]]; then previous="$(readlink -f "$current" || true)"; fi
if [[ -n "$previous" ]]; then backup="$backups/clarityclinical-${release_tag}-${short_sha}-$(date -u '+%Y%m%dT%H%M%SZ').dump"; runuser -u "$db_role" -- pg_dump --format=custom --file="$backup" "$db_name"; [[ -s "$backup" ]]; pg_restore --list "$backup" >/dev/null; fi
runuser -u "$db_role" -- psql --set ON_ERROR_STOP=1 --dbname "$db_name" --file "$incoming/migration.sql"

chown -R root:"$db_role" "$incoming"; find "$incoming" -type d -exec chmod 0750 {} +; find "$incoming" -type f -exec chmod 0640 {} +; chmod 0750 "$incoming/Api/ClarityClinical.Api"; find "$incoming/Web" -type d -exec chmod 0755 {} +; find "$incoming/Web" -type f -exec chmod 0644 {} +
mv "$incoming" "$release_dir"; ln -sfn "$release_dir" "$root/.current-new"; mv -Tf "$root/.current-new" "$current"; switched=true

unit="/etc/systemd/system/$service"
cat > "$unit" <<EOF
[Unit]
Description=Clarity Clinical $environment API
After=network-online.target postgresql.service
Wants=network-online.target
[Service]
Type=simple
User=$db_role
Group=$db_role
WorkingDirectory=$current/Api
ExecStart=$current/Api/ClarityClinical.Api
EnvironmentFile=$env_file
Environment=ASPNETCORE_ENVIRONMENT=$runtime_environment
Environment=DOTNET_ENVIRONMENT=$runtime_environment
Environment=ASPNETCORE_URLS=http://127.0.0.1:$port
Restart=on-failure
RestartSec=5
NoNewPrivileges=true
PrivateTmp=true
ProtectHome=true
ProtectSystem=strict
[Install]
WantedBy=multi-user.target
EOF
systemctl daemon-reload; systemctl enable "$service" >/dev/null; systemctl restart "$service"
rollback(){ if [[ "$switched" == true && -n "$previous" && -d "$previous" ]]; then ln -sfn "$previous" "$root/.current-old"; mv -Tf "$root/.current-old" "$current"; systemctl restart "$service" || true; fi; }
for _ in $(seq 1 30); do if curl --fail --silent --max-time 5 "http://127.0.0.1:$port/health/ready" >/dev/null; then ready=true; break; fi; sleep 2; done
[[ ${ready:-false} == true ]] || { rollback; echo 'Readiness failed.' >&2; exit 1; }
build="$(curl --fail --silent --max-time 5 "http://127.0.0.1:$port/api/system/build")"; grep -Fq "$git_sha" <<<"$build" || { rollback; echo 'Build identity mismatch.' >&2; exit 1; }
[[ "$(readlink -f "$current")" == "$release_dir" ]] || { rollback; echo 'Current release pointer mismatch.' >&2; exit 1; }
if [[ -n "$public_host" ]]; then curl --fail --silent --max-time 5 -H "Host: $public_host" "http://127.0.0.1/health/ready" >/dev/null || { rollback; echo 'Caddy health route failed.' >&2; exit 1; }; fi
echo "Clarity Clinical $environment $release_tag ($git_sha) healthy on 127.0.0.1:$port."
