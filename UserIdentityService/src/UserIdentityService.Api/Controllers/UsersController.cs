using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserIdentityService.Api.Constants;
using UserIdentityService.Api.Dtos.Users;
using UserIdentityService.Api.Services.Auth;

namespace UserIdentityService.Api.Controllers;

[Authorize(Roles = SystemRoles.Admin)]
[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await userService.GetUsersAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetUser(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userService.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPut("{userId:guid}/roles")]
    public async Task<IActionResult> AssignRoles(Guid userId, AssignRolesRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await userService.AssignRolesAsync(userId, request, cancellationToken);
            return Ok(result);
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
