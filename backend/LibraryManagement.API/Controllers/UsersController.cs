using LibraryManagement.Application.DTOs.Users;
using LibraryManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> List(
        [FromQuery] UserSearchRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await userService.GetAllAsync(request, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, user);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> Update(
        int id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await userService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<ActionResult<UserDto>> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(await userService.DeactivateAsync(id, cancellationToken));
    }
}
