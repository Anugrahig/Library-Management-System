using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Domain.Entities;

public class Reservation
{
    public int ReservationId { get; set; }

    public int BookId { get; set; }

    public int MemberId { get; set; }

    public DateTime ReservationDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public DateTime? FulfilledDate { get; set; }

    public DateTime? CancelledDate { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Book Book { get; set; } = null!;

    public Member Member { get; set; } = null!;
}
