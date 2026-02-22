using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserIdentityService.Api.Constants;
using UserIdentityService.Api.Dtos.Roles;
using UserIdentityService.Api.Services.Permissions;

namespace UserIdentityService.Api.Controllers;

[Authorize(Roles = SystemRoles.Admin)]
[ApiController]
[Route("api/[controller]")]
public class RolesController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await roleService.GetRolesAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var role = await roleService.CreateRoleAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetRoles), new { id = role.Id }, role);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{roleId:guid}/permissions")]
    public async Task<IActionResult> UpdatePermissions(Guid roleId, UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var role = await roleService.UpdatePermissionsAsync(roleId, request, cancellationToken);
            return Ok(role);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
