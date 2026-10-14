# ClarityClinical deployment

ClarityClinical uses the shared platform-standard v3 pipeline. The organisation-wide rules live in the deployment-platform repository.

## Flow

1. Work is completed and verified on a feature/fix branch.
2. The verified branch is merged directly into `develop`; pull requests are not part of the managed workflow.
3. TeamCity checks out the exact application SHA and current deployment-platform settings.
4. The pinned Linux build image runs repository governance, build/test/security gates, dependency checks, migration generation where declared, frontend verification and packaging.
5. TeamCity publishes exactly one immutable package `0.0.0-sha.<full-sha>` to Octopus.
6. A fresh annotated four-part tag such as `v1.0.0.17` on that same SHA creates the human-facing Octopus release. No rebuild or repackaging occurs.
7. Octopus deploys the release automatically to Henry TEST.
8. The same release is manually promoted to Stella STAGING after TEST validation.

## Host contract

Canonical paths are `/opt/clarityclinical`, `/etc/clarityclinical` and `/var/lib/clarityclinical`. Immutable releases live beneath `/opt/clarityclinical/releases`; `/opt/clarityclinical/current` selects the active release. The canonical runtime is clarityclinical.service on loopback port 5192.

Before migrations, deployment records the active release and creates/verifies a PostgreSQL backup. Migration scripts are applied only when the central descriptor declares an EF migration stream. Activation uses an atomic `current` symlink switch. Success requires service health, readiness, exact Git SHA, four-part release tag and deployment timestamp verification.

Application rollback restores the previous immutable release pointer and restarts/verifies only this application's services. Database restoration is a separate controlled recovery operation.

GitHub Actions are prohibited for managed CI/CD. GitHub stores source; TeamCity owns CI/build/release orchestration and Octopus owns deployment/promotion.
