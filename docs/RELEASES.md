# ClarityClinical release governance

ClarityClinical uses four-part release versions: `MAJOR.MINOR.PATCH.REVISION`, tagged as a fresh annotated `vMAJOR.MINOR.PATCH.REVISION` tag. Tags are immutable and are never moved or reused.

The package is built once for the exact Git SHA before the human-facing version is chosen. TeamCity publishes `0.0.0-sha.<full-sha>`. The four-part tag resolves that existing package and creates one Octopus release without compiling or repackaging.

The same Octopus release is promoted through every enabled environment. A code or package-byte change burns the old version and requires a new four-part tag.

Runtime release identity must expose application identity, four-part tag/version, full Git SHA, build timestamp, deployment timestamp, checksum and manifest identity. The v3 deployer supplies `AFD_RELEASE_TAG`, `AFD_GIT_SHA` and `AFD_RELEASED_AT_UTC` to the runtime.
