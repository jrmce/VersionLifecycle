# Environment Dashboard

The environment dashboard shows which applications and versions are currently deployed to each environment.

## API

- `GET /api/environments/dashboard`
  - Returns an array of environments with the latest deployment per application in that environment.
  - Shape:
    - `environmentId`, `environmentName`, `order`, `description`
    - `deployments[]` with `deploymentId`, `applicationId`, `applicationName`, `versionId`, `versionNumber`, `status`, `deployedAt`, `completedAt`
- `POST /api/deployments/{id}/promote`
  - Body: `{ targetEnvironmentId, notes? }`
  - Auto-creates a deployment in the next environment (must be immediate next by order) and sets status to `InProgress`.
  - Requires the source deployment status to be `Success`. Promotions from `Pending`, `InProgress`, `Failed`, or `Cancelled` are rejected.

## Frontend

- Route: `/environments/dashboard`
- Container: `EnvironmentDashboardContainerComponent` loads data via `EnvironmentDashboardStore`.
- Presentational: `EnvironmentDashboardComponent` renders environment cards and deployment rows.
- Navigation: exposed as **Env Dashboard** in the top navigation once authenticated.
- Promotion: per-deployment “Promote to {next}” button emits a promotion to the immediate next environment and refreshes the dashboard.
  - Button is disabled if the version already exists in the next environment or if the source deployment is not `Success`.

## Behavior

- Shows latest deployment per application per environment.
- Environments with no deployments are shown with an empty state.
- Includes refresh and error-clear actions.
