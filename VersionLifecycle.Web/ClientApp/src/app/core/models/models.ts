// Auth Models
export interface LoginDto {
  email: string;
  password: string;
  tenantId: string;
}

export interface RegisterDto {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  tenantId: string;
  tenantCode: string;
}

export interface LoginResponseDto {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
  userId?: string;
  email?: string;
  tenantId?: string;
  role?: string;
}

// Tenant Models
export interface TenantDto {
  id: string;
  name: string;
  subdomain: string;
  code: string;
  subscriptionPlan: string;
  createdAt: Date;
  modifiedAt: Date;
}

export interface TenantLookupDto {
  id: string;
  name: string;
  description?: string;
}

export interface CreateTenantDto {
  name: string;
  description?: string;
  subscriptionPlan?: string;
}

export interface TenantStatsDto {
  tenantId: string;
  userCount: number;
  applicationCount: number;
  versionCount: number;
  deploymentCount: number;
}

// Application Models
export interface ApplicationDto {
  id: number;
  tenantId: string;
  name: string;
  description: string;
  repositoryUrl: string;
  createdAt: Date;
  modifiedAt: Date;
}

export interface CreateApplicationDto {
  name: string;
  description: string;
  repositoryUrl: string;
}

export interface UpdateApplicationDto {
  name?: string;
  description?: string;
  repositoryUrl?: string;
}

// Version Models
export type VersionStatus = 'Draft' | 'Released' | 'Deprecated' | 'Archived';

export interface VersionDto {
  id: number;
  applicationId: number;
  versionNumber: string;
  status: VersionStatus;
  releaseNotes: string;
  createdAt: Date;
  modifiedAt: Date;
}

export interface CreateVersionDto {
  versionNumber: string;
  releaseNotes: string;
}

export interface UpdateVersionDto {
  status?: VersionStatus;
  releaseNotes?: string;
}

// Environment Models
export interface EnvironmentDto {
  id: number;
  name: string;
  description?: string;
  order: number;
  config?: string;
  createdAt: Date;
}

export interface EnvironmentDeploymentStatus {
  deploymentId: number;
  applicationId: number;
  applicationName: string;
  versionId: number;
  versionNumber: string;
  status: DeploymentStatus;
  deployedAt: Date;
  completedAt?: Date | null;
}

export interface EnvironmentDeploymentOverview {
  environmentId: number;
  environmentName: string;
  order: number;
  description?: string;
  deployments: EnvironmentDeploymentStatus[];
}

export interface PromoteDeploymentRequest {
  targetEnvironmentId: number;
  notes?: string;
}

export interface CreateEnvironmentDto {
  name: string;
  description?: string;
  order: number;
  config?: string;
}

export interface UpdateEnvironmentDto {
  name?: string;
  description?: string;
  order?: number;
  config?: string;
}

// Deployment Models
export type DeploymentStatus = 'Pending' | 'InProgress' | 'Success' | 'Failed' | 'Cancelled';

export interface DeploymentDto {
  id: number;
  applicationId: number;
  applicationName?: string;
  versionId: number;
  versionNumber?: string;
  environmentId: number;
  environmentName?: string;
  status: DeploymentStatus;
  deployedAt: Date | null;
  completedAt: Date | null;
  durationMs?: number | null;
  notes?: string | null;
  createdAt: Date;
  modifiedAt: Date;
}

export interface CreatePendingDeploymentDto {
  applicationId: number;
  versionId: number;
  environmentId: number;
}

export interface ConfirmDeploymentDto {
  deploymentId: number;
  confirmationNotes?: string;
}

export interface UpdateDeploymentStatusDto {
  status: DeploymentStatus;
  notes?: string;
  durationMs?: number;
}

export interface DeploymentEventDto {
  id: number;
  deploymentId: number;
  eventType: string;
  message: string;
  createdAt: Date;
}

// Webhook Models
export interface WebhookDto {
  id: number;
  applicationId: number;
  url: string;
  isActive: boolean;
  createdAt: Date;
  modifiedAt: Date;
}

export interface CreateWebhookDto {
  url: string;
  secret: string;
}

export interface WebhookEventDto {
  id: number;
  webhookId: number;
  event: string;
  payload: string;
  statusCode: number;
  createdAt: Date;
}

// Pagination
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  skip: number;
  take: number;
}

// Error Response
export interface ErrorResponse {
  message: string;
  details?: string;
  code?: string;
}
