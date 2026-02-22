using System;

namespace BorrowingReturnsService.Dtos
{
    public class ReturnDto
    {
        public Guid Id { get; set; }
        public Guid BorrowingId { get; set; }
        public DateTime ReturnedAt { get; set; }
    }
}
