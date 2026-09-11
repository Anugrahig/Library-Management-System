using System.Security.Claims;
using LibraryManagement.Application.DTOs.Reservations;
using LibraryManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationsController(IReservationService reservationService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Student,Faculty")]
    public async Task<ActionResult<ReservationDto>> Create(
        CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var reservation = await reservationService.CreateAsync(GetCurrentUserId(), request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, reservation);
    }

    [HttpPatch("{id:int}/cancel")]
    [Authorize(Roles = "Admin,Librarian,Student,Faculty")]
    public async Task<ActionResult<ReservationDto>> Cancel(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(await reservationService.CancelAsync(
            GetCurrentUserId(),
            new ReservationActionRequest { ReservationId = id },
            cancellationToken));
    }

    [HttpPatch("{id:int}/fulfill")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<ReservationDto>> Fulfill(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(await reservationService.FulfillAsync(
            new ReservationActionRequest { ReservationId = id },
            cancellationToken));
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
