namespace LibraryManagement.Application.DTOs.Reservations;

public class CreateReservationRequest
{
    public int BookId { get; set; }
}

public class ReservationActionRequest
{
    public int ReservationId { get; set; }
}

public class ReservationDto
{
    public int ReservationId { get; set; }

    public int BookId { get; set; }

    public int MemberId { get; set; }

    public DateTime ReservationDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public DateTime? FulfilledDate { get; set; }

    public DateTime? CancelledDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
