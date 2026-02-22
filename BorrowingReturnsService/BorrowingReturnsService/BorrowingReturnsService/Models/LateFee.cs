using System;

namespace BorrowingReturnsService.Models
{
    public class LateFee
    {
        public Guid Id { get; set; }
        public Guid BorrowingId { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
    }
}
