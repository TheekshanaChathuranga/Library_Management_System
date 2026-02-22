using BorrowingReturnsService.Models;
using System;

namespace BorrowingReturnsService.Dtos
{
    public class CreateBorrowingDto
    {
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public BorrowChannel Channel { get; set; } = BorrowChannel.Physical;
    }
}
