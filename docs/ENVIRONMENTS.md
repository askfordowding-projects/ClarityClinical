# ClarityClinical environments

| Environment | Host | Primary application address | Promotion |
| --- | --- | --- | --- |
| TEST | Henry | `(configured through environment data)` | Automatic after Octopus release creation |
| STAGING | Stella | `(configured through environment data)` | Manual after TEST validation |

PRODUCTION is not currently enabled for this application in the central platform policy. Enabling it requires an explicit policy change and a validated Jimmy target.

The package bytes, filesystem layout, service names, runtime architecture and deployment process are common across environments. Differences are limited to URLs, secrets, environment variables, database connections and scale.

Henry is the TEST host. Stella is STAGING and Jimmy is PRODUCTION. Stella and Jimmy are not build machines; TeamCity owns CI/build orchestration and Octopus owns deployment/promotion.
