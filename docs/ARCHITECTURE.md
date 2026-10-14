# ClarityClinical architecture

ClarityClinical is a .NET 10 clinical API with an Angular 22 SPA backed by PostgreSQL. ClarityClinicalDbContext is the persistence boundary; Caddy serves the packaged SPA while the API provides health and build identity endpoints.

The deployed runtime uses one immutable release identity across clarityclinical.service on loopback port 5192. Runtime configuration and secrets are supplied outside the release package. Caddy is the ingress layer, systemd manages native services and persistent state lives beneath `/var/lib/clarityclinical` rather than inside immutable releases.

ClarityClinical is a platform-standard v3 application. Build/deployment behaviour is centrally declared in the deployment-platform descriptor: solution/frontend inputs, publish outputs, PostgreSQL integration-test roles, services, health endpoints, environment URLs and secret bindings. TeamCity builds an exact Git SHA once in the pinned standard build image; Octopus promotes the same immutable package through allowed environments.

Significant design decisions are recorded in `docs/adr/`.
