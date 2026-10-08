# Clarity Clinical

Clarity Clinical is a research and demonstration prototype exploring how clinical decision support can improve consultation workflows while preserving clinician judgement, patient involvement and meaningful human oversight.

> **Demonstration system. Synthetic data only. Not clinically validated. Not a medical device. Not for diagnosis or treatment.**

## Current prototype

The current vertical slice implements the Miguel Santos synthetic consultation used by the project concept. It includes:

- a clinician-facing workspace based on the Figure 1 consultation design
- synthetic patient context, allergies, conditions and medications
- isolated public demo sessions
- deterministic, versioned illustrative clinical rules
- competing DVT, cellulitis and musculoskeletal assessment candidates
- supporting and missing evidence
- reversible evidence exclusion with an explanation of score changes
- clinician Accept, Modify, Reject and Defer responses
- an append-only application audit trail
- public demo role selection with server-side role enforcement
- PostgreSQL persistence
- Spanish â†” English browser speech translation with persisted transcript segments
- clinician-controlled transcript correction, speaker re-attribution, reasoning exclusion and redaction
- deterministic transcript-to-clinical-fact projection with source-event provenance and reversible score effects
- typed transcript fallback when live speech is unavailable
- liveness and readiness health checks

The percentages displayed by the prototype are **illustrative priority scores**. They are not calibrated probabilities of disease and must not be interpreted as validated clinical risk estimates.

## Architecture

Clarity Clinical is a modular monolith.

```text
Angular 22
    |
ASP.NET Core 10 API
    |
Application / Domain
    |
EF Core 10 + Npgsql
    |
PostgreSQL
```

The backend separates Domain, Application, Infrastructure and API concerns. Clinical scoring remains outside the Angular application and is executed by a deterministic application-layer provider. External speech, translation, identity and EPR/EHR integrations are intended to sit behind replaceable interfaces in later iterations.

## Technology

- Angular 22.2.1
- TypeScript 6.0
- .NET 10 / ASP.NET Core 10
- EF Core 10
- PostgreSQL
- Npgsql
- xUnit
- Karma / Jasmine

## Local development

### Prerequisites

- .NET 10 SDK
- Node.js 22+
- PostgreSQL

The default development database is:

```text
Host=127.0.0.1;Port=5432;Username=postgres;Database=clarityclinical_dev
```

Override `ConnectionStrings__ClarityClinical` in your environment when a different local connection is required. Do not commit credentials or production connection strings.

### Backend

Create the development database, then from the repository root run:

```powershell
dotnet restore
dotnet ef database update --project src/ClarityClinical.Infrastructure --startup-project src/ClarityClinical.Api
dotnet run --project src/ClarityClinical.Api
```

Health endpoints:

```text
/health/live
/health/ready
```

`/health/live` reports whether the API process is running. `/health/ready` also verifies PostgreSQL connectivity.

### Speech translation

Live speech is optional. The browser obtains a short-lived authorization token from the API; the Speech subscription key remains server-side and must never be committed to source control.

Configure the backend with environment variables or user secrets:

```text
Speech__SubscriptionKey=<secret>
Speech__Region=<azure-region>
```

Without these settings the API returns `503 Service Unavailable` for speech authorization and the consultation remains usable through typed transcript input.

Patient speech is configured as Spanish to English. Clinician speech is configured as English to Spanish. Raw microphone audio is not persisted by Clarity Clinical.

Transcript text is stored separately from structured clinical facts. A deterministic projector recognises the limited synthetic Miguel scenario vocabulary and creates provenance-linked facts only for Patient and Clinician speech. Carer, family, interpreter, other and unknown speaker roles do not automatically alter clinical reasoning. Correcting, re-attributing, excluding or redacting a transcript re-projects only that segment and preserves withdrawn facts as history outside active reasoning.
### Frontend

```powershell
cd src/ClarityClinical.Ui
npm install
npm start
```

## Tests

Backend:

```powershell
dotnet test ClarityClinical.slnx
```

Frontend:

```powershell
cd src/ClarityClinical.Ui
npm test -- --watch=false --browsers=ChromeHeadless
npm run build
npm audit --omit=dev
```

The API acceptance suite includes the complete Miguel consultation path, including deterministic score changes, evidence exclusion, clinician response and audit verification.

## Safety boundary

Clarity Clinical must not be used with real patient data. The prototype does not autonomously diagnose, prescribe or make final clinical decisions. Machine- or rule-generated output is presented as provisional decision support and remains subject to clinician review.

The local synthetic patient store is prototype infrastructure. A real deployment would obtain patient context through approved integrations with existing clinical systems rather than replace the host EPR/EHR.

- Visitor-specific sandbox copies let public administrators edit synthetic patient/scenario data without mutating canonical templates; inactive sandboxes expire after two hours.
