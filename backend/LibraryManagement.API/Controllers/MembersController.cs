using System.Security.Claims;
using LibraryManagement.Application.DTOs.Members;
using LibraryManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Route("api/members")]
[Authorize]
public class MembersController(IMemberService memberService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Student,Faculty")]
    public async Task<ActionResult<MemberDto>> Create(
        CreateMemberRequest request,
        CancellationToken cancellationToken)
    {
        var member = await memberService.CreateAsync(GetCurrentUserId(), request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, member);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Student,Faculty")]
    public async Task<ActionResult<MemberDto>> GetMe(CancellationToken cancellationToken)
    {
        return Ok(await memberService.GetByUserIdAsync(GetCurrentUserId(), cancellationToken));
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<IReadOnlyList<MemberDto>>> GetPending(CancellationToken cancellationToken)
    {
        return Ok(await memberService.GetPendingAsync(cancellationToken));
    }

    [HttpPatch("{id:int}/approval")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<MemberDto>> Approve(
        int id,
        ApproveMemberRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await memberService.ApproveAsync(id, request, cancellationToken));
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("The authenticated user ID claim is missing.");
        }

        return userId;
    }
}
