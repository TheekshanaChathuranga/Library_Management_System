using BorrowingReturnsService.Models;
using System;

namespace BorrowingReturnsService.Dtos
{
    public class BorrowingDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public DateTime BorrowedAt { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsReturned { get; set; }
        public BorrowChannel Channel { get; set; }
    }
}
