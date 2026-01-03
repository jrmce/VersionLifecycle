# Tenant Registration Workflows

This document describes the two user registration workflows available in the Version Lifecycle Management application.

## Overview

The application supports multi-tenancy, where each tenant (organization/team) has isolated data. New users can either:
1. **Create a new tenant** and become its administrator
2. **Join an existing tenant** using an invite code

## Workflow 1: Create New Tenant (New Organization)

This workflow is for users who want to start using the application for their organization.

### Endpoint
```
POST /api/auth/register-with-tenant
```

### Request Body
```json
{
  "email": "admin@example.com",
  "password": "SecurePass123",
  "confirmPassword": "SecurePass123",
  "tenantName": "My Organization",
  "tenantDescription": "Optional description",
  "displayName": "John Doe"
}
```

### Response
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "abc123...",
  "expiresIn": 3600,
  "userId": "user-uuid",
  "email": "admin@example.com",
  "tenantId": "tenant-uuid",
  "role": "Admin",
  "tokenType": "Bearer",
  "tenantCode": "ABC123XY",
  "tenantName": "My Organization"
}
```

### Process
1. User provides email, password, and tenant information
2. System creates a new tenant with:
   - Auto-generated unique ID (UUID)
   - Auto-generated 8-character invite code (e.g., "ABC123XY")
   - Subscription plan set to "Free"
   - Active status
3. System creates the user account
4. User is automatically assigned **Admin** role for their tenant
5. Response includes:
   - JWT authentication token
   - Tenant code (to share with team members)
   - Tenant name

### User Roles
The first user (tenant creator) receives the **Admin** role, which grants:
- Full access to tenant settings
- Ability to manage applications, versions, and deployments
- Access to tenant statistics
- Invite code sharing capability

## Workflow 2: Join Existing Tenant

This workflow is for users joining an existing organization.

### Endpoint
```
POST /api/auth/register
```

### Request Body
```json
{
  "email": "user@example.com",
  "password": "SecurePass123",
  "confirmPassword": "SecurePass123",
  "tenantId": "tenant-uuid",
  "tenantCode": "ABC123XY",
  "displayName": "Jane Smith"
}
```

### Response
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "abc123...",
  "expiresIn": 3600,
  "userId": "user-uuid",
  "email": "user@example.com",
  "tenantId": "tenant-uuid",
  "role": "Viewer",
  "tokenType": "Bearer"
}
```

### Process
1. User obtains tenant ID and invite code from their team administrator
2. User provides email, password, tenant ID, and tenant code
3. System validates:
   - Tenant exists and is active
   - Tenant code matches
4. System creates the user account
5. User is automatically assigned **Viewer** role
6. Response includes JWT authentication token

### User Roles
New users joining an existing tenant receive the **Viewer** role, which grants:
- Read-only access to applications, versions, and deployments
- Cannot create or modify resources
- Can view deployment history and status

**Note:** Tenant admins can later upgrade user roles if needed (future feature).

## Getting Tenant List

For registration UI, you can retrieve available tenants:

### Endpoint
```
GET /api/tenants?activeOnly=true
```

### Response
```json
[
  {
    "id": "tenant-uuid",
    "name": "My Organization",
    "description": "Optional description"
  }
]
```

**Note:** This endpoint returns only basic tenant information (no codes). Users must obtain the invite code from their administrator.

## Frontend Implementation Guidelines

### Registration Form Design

1. **Initial Choice**: Present two options
   - "Create new organization" → Workflow 1
   - "Join existing organization" → Workflow 2

2. **Create New Organization Form**
   - Email (required)
   - Password (required, min 8 chars, uppercase + digit)
   - Confirm Password (required)
   - Organization Name (required, max 255 chars)
   - Organization Description (optional, max 2000 chars)
   - Display Name (optional)

3. **Join Existing Organization Form**
   - Email (required)
   - Password (required, min 8 chars, uppercase + digit)
   - Confirm Password (required)
   - Tenant dropdown (from GET /api/tenants)
   - Tenant Code (required, 8 characters)
   - Display Name (optional)

4. **Success Handling**
   - For new tenant creation: Display tenant code prominently with copy button
   - For both: Store JWT token and redirect to dashboard
   - For new tenant: Show welcome wizard or onboarding flow

## Security Considerations

1. **Tenant Codes**: 
   - 8 characters, alphanumeric (excluding confusing chars: 0, O, I, 1)
   - Unique across all tenants
   - Not exposed in public tenant listing
   - Required for registration validation

2. **Password Requirements**:
   - Minimum 8 characters
   - At least one uppercase letter
   - At least one digit
   - Passwords must match confirmation

3. **Validation**:
   - Email format validation
   - Tenant existence and active status check
   - Tenant code case-insensitive matching
   - Duplicate email prevention (ASP.NET Identity)

## API Response Codes

| Code | Scenario |
|------|----------|
| 201 Created | Registration successful |
| 400 Bad Request | Validation failed, invalid tenant, wrong code |
| 401 Unauthorized | Authentication failed (login only) |

## Error Response Format
```json
{
  "code": "ERROR_CODE",
  "message": "Human-readable error message",
  "traceId": "request-trace-id"
}
```

Common error codes:
- `INVALID_REQUEST`: Validation failed
- `TENANT_NOT_FOUND`: Tenant doesn't exist or inactive
- `INVALID_TENANT_CODE`: Wrong invite code
- `REGISTRATION_FAILED`: User creation failed (e.g., duplicate email)

## Future Enhancements

- Email verification for new accounts
- Password reset flow
- User management UI for admins
- Role upgrade capability for admins
- Team member invitation via email
- Tenant deactivation and suspension
- Usage limits per subscription plan
