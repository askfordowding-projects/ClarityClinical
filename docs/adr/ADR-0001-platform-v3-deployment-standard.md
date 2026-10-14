# ADR-0001: Adopt the shared v3 deployment contract for ClarityClinical

## Status

Accepted on 14 October 2026.

## Context

ClarityClinical previously carried environment-specific deployment behaviour and/or GitHub Actions workflows. The platform constitution requires one build, package, deployment, host, health and rollback contract, with environments differing only through configuration such as URLs, secrets and database connections.

## Decision

ClarityClinical uses the central deployment-platform v3 descriptor and the same service/port identities in every managed environment:

- `clarityclinical.service` on loopback port `5192`

Canonical host roots are `/opt/clarityclinical`, `/etc/clarityclinical` and `/var/lib/clarityclinical`. TeamCity owns CI/build/test/security/release orchestration. Octopus owns deployment and promotion. GitHub Actions and pull requests are not part of the managed workflow. Releases use immutable annotated four-part tags `vMAJOR.MINOR.PATCH.REVISION`, and the exact SHA-addressed package is promoted without rebuilding.

## Consequences

Environment-specific deploy workflows and service-name drift are retired. TEST on Henry proves the same runtime contract later used by Stella and, where enabled, Jimmy. Application rollback and database rollback remain separate operations.

## Security considerations

Secrets stay outside Git, build images and immutable packages. Sensitive values are supplied through Octopus/approved protected configuration and materialised with restrictive permissions. Build and dependency security gates run in TeamCity.
