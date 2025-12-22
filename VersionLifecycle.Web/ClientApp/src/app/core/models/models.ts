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
}

export interface LoginResponseDto {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
}

// Tenant Models
export interface TenantDto {
  id: string;
  name: string;
  subdomain: string;
  createdAt: Date;
  modifiedAt: Date;
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
  applicationId: number;
  name: string;
  order: number;
  createdAt: Date;
}

export interface CreateEnvironmentDto {
  name: string;
  order: number;
}

export interface UpdateEnvironmentDto {
  name?: string;
  order?: number;
}

// Deployment Models
export type DeploymentStatus = 'Pending' | 'InProgress' | 'Success' | 'Failed' | 'Cancelled';

export interface DeploymentDto {
  id: number;
  applicationId: number;
  versionId: number;
  environmentId: number;
  status: DeploymentStatus;
  deployedAt: Date | null;
  completedAt: Date | null;
  createdAt: Date;
  modifiedAt: Date;
}

export interface CreatePendingDeploymentDto {
  versionId: number;
  environmentId: number;
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
