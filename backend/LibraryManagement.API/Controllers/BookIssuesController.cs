using System.Security.Claims;
using LibraryManagement.Application.DTOs.BookIssues;
using LibraryManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Route("api/issues")]
[Authorize]
public class BookIssuesController(IBookIssueService issueService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<BookIssueDto>> Issue(
        IssueBookRequest request,
        CancellationToken cancellationToken)
    {
        var issue = await issueService.IssueAsync(GetCurrentUserId(), request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, issue);
    }

    [HttpPatch("{id:int}/return")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<BookIssueDto>> Return(
        int id,
        ReturnBookRequest request,
        CancellationToken cancellationToken)
    {
        request.BookIssueId = id;
        return Ok(await issueService.ReturnAsync(GetCurrentUserId(), request, cancellationToken));
    }

    [HttpPatch("{id:int}/renew")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<BookIssueDto>> Renew(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(await issueService.RenewAsync(new RenewBookRequest { BookIssueId = id }, cancellationToken));
    }

    [HttpPatch("{id:int}/pay-fine")]
    [Authorize(Roles = "Admin,Librarian,Student,Faculty")]
    public async Task<ActionResult<BookIssueDto>> PayFine(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(await issueService.PayFineAsync(GetCurrentUserId(), new PayFineRequest { BookIssueId = id }, cancellationToken));
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
