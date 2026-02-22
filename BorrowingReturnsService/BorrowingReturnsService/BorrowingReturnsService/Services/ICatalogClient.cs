using BorrowingReturnsService.Dtos;
using System;
using System.Threading.Tasks;

namespace BorrowingReturnsService.Services
{
    public interface ICatalogClient
    {
        Task<CatalogBookDto> GetBookByIdAsync(Guid bookId);
        Task<bool> UpdateBookAvailabilityAsync(Guid bookId, bool isAvailable);
    }
}
