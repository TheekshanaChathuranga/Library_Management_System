using BorrowingReturnsService.Dtos.UserIdentity;

namespace BorrowingReturnsService.Services
{
    public interface IUserIdentityClient
    {
        Task<UserSummary?> GetUserAsync(Guid userId);
    }
}
