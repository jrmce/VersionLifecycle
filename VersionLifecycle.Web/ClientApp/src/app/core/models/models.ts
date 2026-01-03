// Auth Models
import { UserRole } from '../enums';

export interface LoginDto {
  email: string;
  password: string;
  tenantId: string;
}

export interface RegisterDto {
  email: string;
  password: string;
  confirmPassword: string;
  displayName?: string;
  tenantId: string;
  tenantCode: string;
}

export interface RegisterWithNewTenantDto {
  email: string;
  password: string;
  confirmPassword: string;
  tenantName: string;
  tenantDescription?: string;
  displayName?: string;
}

export interface LoginResponseDto {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
  userId?: string;
  email?: string;
  tenantId?: string;
  role?: UserRole;
  tenantCode?: string;
  tenantName?: string;
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
  id: string;
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
  id: string;
  applicationId: string;
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
  id: string;
  name: string;
  description?: string;
  order: number;
  config?: string;
  createdAt: Date;
}

export interface EnvironmentDeploymentStatus {
  deploymentId: string;
  applicationId: string;
  applicationName: string;
  versionId: string;
  versionNumber: string;
  status: DeploymentStatus;
  deployedAt: Date;
  completedAt?: Date | null;
}

export interface EnvironmentDeploymentOverview {
  environmentId: string;
  environmentName: string;
  order: number;
  description?: string;
  deployments: EnvironmentDeploymentStatus[];
}

export interface PromoteDeploymentRequest {
  targetEnvironmentId: string;
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
  id: string;
  applicationId: string;
  applicationName?: string;
  versionId: string;
  versionNumber?: string;
  environmentId: string;
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
  applicationId: string;
  versionId: string;
  environmentId: string;
}

export interface ConfirmDeploymentDto {
  deploymentId: string;
  confirmationNotes?: string;
}

export interface UpdateDeploymentStatusDto {
  status: DeploymentStatus;
  notes?: string;
  durationMs?: number;
}

export interface DeploymentEventDto {
  id: string;
  deploymentId: string;
  eventType: string;
  message: string;
  createdAt: Date;
}

// Webhook Models
export interface WebhookDto {
  id: string;
  applicationId: string;
  url: string;
  events: string;
  isActive: boolean;
  maxRetries: number;
  createdAt: Date;
}

export interface CreateWebhookDto {
  applicationId?: string;
  url: string;
  secret: string;
  events?: string;
  maxRetries?: number;
}

export interface UpdateWebhookDto {
  url?: string;
  secret?: string;
  events?: string;
  isActive?: boolean;
  maxRetries?: number;
}

export interface WebhookEventDto {
  id: string;
  webhookId: string;
  eventType: string;
  deliveryStatus: string;
  responseStatusCode?: number;
  retryCount: number;
  deliveredAt?: Date;
}

// API Token Models
export interface ApiTokenDto {
  id: string;
  name: string;
  description?: string;
  tokenPrefix: string;
  expiresAt?: Date;
  lastUsedAt?: Date;
  isActive: boolean;
  createdAt: Date;
  createdBy: string;
}

export interface ApiTokenCreatedDto {
  id: string;
  name: string;
  description?: string;
  token: string;  // Full plaintext token (shown only once)
  tokenPrefix: string;
  expiresAt?: Date;
  createdAt: Date;
}

export interface CreateApiTokenDto {
  name: string;
  description?: string;
  expiresAt?: Date;
}

export interface UpdateApiTokenDto {
  name?: string;
  description?: string;
  isActive?: boolean;
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
