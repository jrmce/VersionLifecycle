namespace VersionLifecycle.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Web.Models;

/// <summary>
/// Controller for user management within tenants.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController(UserManager<IdentityUser> userManager, ITenantContext tenantContext) : ControllerBase
{
    /// <summary>
    /// Gets all users in the current tenant.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTenantUsers()
    {
        var currentTenantId = tenantContext.CurrentTenantId;
        
        if (string.IsNullOrEmpty(currentTenantId))
        {
            return Forbid();
        }

        var allUsers = userManager.Users.ToList();
        var tenantUsers = new List<UserDto>();

        foreach (var user in allUsers)
        {
            var claims = await userManager.GetClaimsAsync(user);
            var userTenantId = claims.FirstOrDefault(c => c.Type == "tenantId")?.Value;

            if (userTenantId == currentTenantId)
            {
                var roles = await userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "Viewer";

                tenantUsers.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    Role = role,
                    TenantId = userTenantId,
                    CreatedAt = DateTime.UtcNow // Identity doesn't track creation date by default
                });
            }
        }

        return Ok(tenantUsers);
    }

    /// <summary>
    /// Updates a user's role within the current tenant.
    /// </summary>
    [HttpPut("{userId}/role")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRole(string userId, [FromBody] UpdateUserRoleDto request)
    {
        var currentTenantId = tenantContext.CurrentTenantId;
        
        if (string.IsNullOrEmpty(currentTenantId))
        {
            return Forbid();
        }

        // Validate role
        var validRoles = new[] { "Viewer", "Manager", "Admin" };
        if (!validRoles.Contains(request.Role))
        {
            return BadRequest(new ErrorResponse 
            { 
                Code = "INVALID_ROLE", 
                Message = "Role must be Viewer, Manager, or Admin", 
                TraceId = HttpContext.TraceIdentifier 
            });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new ErrorResponse 
            { 
                Code = "USER_NOT_FOUND", 
                Message = "User not found", 
                TraceId = HttpContext.TraceIdentifier 
            });
        }

        // Verify user belongs to current tenant
        var claims = await userManager.GetClaimsAsync(user);
        var userTenantId = claims.FirstOrDefault(c => c.Type == "tenantId")?.Value;

        if (userTenantId != currentTenantId)
        {
            return Forbid();
        }

        // Prevent admin from demoting themselves
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == currentUserId)
        {
            return BadRequest(new ErrorResponse 
            { 
                Code = "CANNOT_UPDATE_SELF", 
                Message = "You cannot change your own role", 
                TraceId = HttpContext.TraceIdentifier 
            });
        }

        // Remove existing role and add new one
        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
        }
        await userManager.AddToRoleAsync(user, request.Role);

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Role = request.Role,
            TenantId = userTenantId ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Removes a user from the current tenant.
    /// </summary>
    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var currentTenantId = tenantContext.CurrentTenantId;
        
        if (string.IsNullOrEmpty(currentTenantId))
        {
            return Forbid();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new ErrorResponse 
            { 
                Code = "USER_NOT_FOUND", 
                Message = "User not found", 
                TraceId = HttpContext.TraceIdentifier 
            });
        }

        // Verify user belongs to current tenant
        var claims = await userManager.GetClaimsAsync(user);
        var userTenantId = claims.FirstOrDefault(c => c.Type == "tenantId")?.Value;

        if (userTenantId != currentTenantId)
        {
            return Forbid();
        }

        // Prevent admin from deleting themselves
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == currentUserId)
        {
            return BadRequest(new ErrorResponse 
            { 
                Code = "CANNOT_DELETE_SELF", 
                Message = "You cannot delete your own account", 
                TraceId = HttpContext.TraceIdentifier 
            });
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new ErrorResponse 
            { 
                Code = "DELETE_FAILED", 
                Message = errors, 
                TraceId = HttpContext.TraceIdentifier 
            });
        }

        return NoContent();
    }
}
