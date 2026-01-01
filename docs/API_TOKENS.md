# API Tokens - System-to-System Authentication

## Overview

API tokens provide a secure way for external systems to authenticate and access your applications on behalf of a tenant without requiring user credentials. This enables automated deployments, monitoring, CI/CD pipelines, and other system-to-system integrations.

## Key Features

### Security
- **Cryptographically Secure Generation**: Tokens are generated using 64 bytes (512 bits) of cryptographically secure random data
- **SHA256 Hashing**: Tokens are hashed with SHA256 before storage - the plaintext token is never stored in the database
- **Token Prefix**: All tokens start with `vl_` for easy identification
- **Admin Privileges**: API tokens automatically have Admin-level access to all tenant resources
- **Tenant Isolation**: Tokens are scoped to a specific tenant and cannot access other tenants' data
- **Expiration Support**: Optional expiration dates can be set to limit token lifetime
- **Soft Delete**: Revoked tokens are soft-deleted, maintaining audit history

### Management
- **Admin-Only Access**: Only users with Admin or SuperAdmin roles can manage API tokens
- **Create Tokens**: Generate new tokens with optional description and expiration
- **Revoke Tokens**: Soft-delete tokens to immediately invalidate them
- **Activate/Deactivate**: Temporarily disable tokens without revoking them
- **Last Used Tracking**: Automatically tracks when each token was last used for authentication

## How to Use API Tokens

### Creating an API Token

1. Log in with an Admin or SuperAdmin account
2. Navigate to **API Tokens** in the navigation menu
3. Click **Create Token**
4. Fill in the form:
   - **Name** (required): A descriptive name (e.g., "CI/CD Pipeline", "Monitoring Service")
   - **Description** (optional): Detailed information about the token's purpose
   - **Expires In**: Choose an expiration period (30, 90, 180, 365 days, or never)
5. Click **Create Token**
6. **Important**: Copy the token immediately - you won't be able to see it again!
   - The full token will be displayed only once
   - Use the "Copy" button to safely copy it to your clipboard

### Using an API Token

Include the token in the `Authorization` header of your HTTP requests:

```bash
Authorization: Bearer vl_<your_token_here>
```

#### Example with cURL

```bash
curl -H "Authorization: Bearer vl_abc123..." \
  https://your-api.com/api/applications
```

#### Example with C# HttpClient

```csharp
using var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", "vl_abc123...");

var response = await client.GetAsync("https://your-api.com/api/applications");
```

#### Example with JavaScript/Node.js

```javascript
const response = await fetch('https://your-api.com/api/applications', {
  headers: {
    'Authorization': 'Bearer vl_abc123...'
  }
});
```

#### Example with Python

```python
import requests

headers = {
    'Authorization': 'Bearer vl_abc123...'
}

response = requests.get('https://your-api.com/api/applications', headers=headers)
```

### Managing Tokens

#### View All Tokens
Navigate to **API Tokens** to see all tokens for your tenant with:
- Token name and description
- Token prefix (first 12 characters)
- Active/Inactive/Expired status
- Expiration date
- Last used timestamp
- Created date

#### Deactivate a Token
Click **Deactivate** to temporarily disable a token without revoking it. You can reactivate it later if needed.

#### Revoke a Token
Click **Revoke** to permanently invalidate a token. This action cannot be undone. The token will be soft-deleted from the database.

## Security Best Practices

1. **Set Expiration Dates**: Always set an expiration date for tokens to limit security risks. 90 days is recommended.

2. **Store Tokens Securely**: 
   - Never commit tokens to source control
   - Use environment variables or secret management services
   - Rotate tokens regularly

3. **Use Descriptive Names**: Give tokens meaningful names so you can easily identify and manage them later.

4. **Monitor Usage**: Check the "Last Used" timestamp to identify unused or suspicious tokens.

5. **Revoke Unused Tokens**: Regularly review and revoke tokens that are no longer needed.

6. **One Token Per System**: Create separate tokens for each external system to enable granular control and revocation.

## API Endpoints

All endpoints require Admin or SuperAdmin role authentication.

### List Tokens
```
GET /api/apitokens
```
Returns all API tokens for the current tenant (without plaintext token values).

### Get Token Details
```
GET /api/apitokens/{id}
```
Returns details for a specific token.

### Create Token
```
POST /api/apitokens
Content-Type: application/json

{
  "name": "CI/CD Pipeline",
  "description": "Token for automated deployments",
  "expiresAt": "2025-04-01T00:00:00Z"  // Optional
}
```
Returns the created token with the plaintext value (shown only once).

### Update Token
```
PUT /api/apitokens/{id}
Content-Type: application/json

{
  "name": "Updated Name",        // Optional
  "description": "New description",  // Optional
  "isActive": false              // Optional
}
```
Updates token metadata (cannot change the token value itself).

### Revoke Token
```
DELETE /api/apitokens/{id}
```
Soft-deletes the token, immediately invalidating it.

## Technical Architecture

### Backend Components

1. **ApiToken Entity** (`Core/Entities/ApiToken.cs`)
   - Inherits from `BaseEntity` for tenant isolation
   - Stores SHA256 hash of the token (never plaintext)
   - Tracks creation, expiration, and last used timestamps

2. **ApiTokenService** (`Infrastructure/Services/ApiTokenService.cs`)
   - Generates cryptographically secure random tokens
   - Hashes tokens with SHA256 before storage
   - Validates tokens and returns tenant/user context

3. **ApiTokenAuthenticationHandler** (`Web/Authentication/ApiTokenAuthenticationHandler.cs`)
   - Custom ASP.NET Core authentication handler
   - Validates Bearer tokens starting with `vl_` prefix
   - Sets tenant context and creates Admin-level claims

4. **ApiTokensController** (`Web/Controllers/ApiTokensController.cs`)
   - RESTful API endpoints for token management
   - Requires Admin role authorization

### Frontend Components

1. **ApiTokensStore** (`features/api-tokens/api-tokens.store.ts`)
   - SignalStore for state management
   - Handles token CRUD operations
   - Manages created token for secure display

2. **List Component** (`features/api-tokens/list/`)
   - Presentational component displaying token list
   - Container component orchestrating data and actions

3. **Create Component** (`features/api-tokens/create/`)
   - Form for creating new tokens
   - Secure token display with copy-to-clipboard functionality

## Troubleshooting

### Token Not Working

1. **Check Token Prefix**: Ensure the token starts with `vl_`
2. **Verify Format**: Use `Authorization: Bearer <token>` header
3. **Check Expiration**: Verify the token hasn't expired
4. **Check Status**: Ensure the token is still active and not revoked
5. **Verify Tenant**: Confirm you're calling APIs for the correct tenant

### Token Creation Fails

1. **Check Permissions**: Only Admin and SuperAdmin roles can create tokens
2. **Verify Name**: Token name is required and must be under 255 characters
3. **Check Expiration**: If set, expiration must be in the future

### Authentication Errors

1. **401 Unauthorized**: Token is invalid, expired, or revoked
2. **403 Forbidden**: Token is valid but lacks permission for the requested resource (unlikely with Admin privileges)

## Future Enhancements

Potential improvements for the API token system:

1. **Rate Limiting**: Per-token rate limits using the metadata field
2. **IP Restrictions**: Limit token usage to specific IP addresses
3. **Scoped Permissions**: Allow tokens with limited permissions (read-only, specific resources)
4. **Webhook Notifications**: Alert on token creation, usage, or revocation
5. **Usage Analytics**: Detailed logs of token usage patterns
6. **Multiple Tokens Per Request**: Support for rotating tokens without downtime

## See Also

- [Architecture Documentation](ARCHITECTURE.md)
- [Development Guide](DEVELOPMENT.md)
- [API Documentation](../swagger/v1/swagger.json)
