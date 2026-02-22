using System;

namespace BorrowingReturnsService.Models
{
    public class Return
    {
        public Guid Id { get; set; }
        public Guid BorrowingId { get; set; }
        public DateTime ReturnedAt { get; set; }
    }
}
