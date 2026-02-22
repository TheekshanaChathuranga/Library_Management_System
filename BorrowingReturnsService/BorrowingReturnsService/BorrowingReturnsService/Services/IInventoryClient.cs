using BorrowingReturnsService.Dtos;
using BorrowingReturnsService.Models;
using System;
using System.Threading.Tasks;

namespace BorrowingReturnsService.Services
{
    public interface IInventoryClient
    {
        Task<InventorySummaryDto> GetInventoryAsync(Guid bookId);
        Task<InventorySummaryDto> BorrowAsync(Guid bookId, BorrowChannel channel, string reference);
        Task<InventorySummaryDto> ReturnAsync(Guid bookId, BorrowChannel channel, string reference);
    }
}
