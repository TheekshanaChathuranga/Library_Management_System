using System;

namespace BorrowingReturnsService.Dtos
{
    public class LateFeeDto
    {
        public Guid Id { get; set; }
        public Guid BorrowingId { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
    }
}
