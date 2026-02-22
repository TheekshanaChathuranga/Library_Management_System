namespace BorrowingReturnsService.Dtos.UserIdentity
{
    public class UserSummary
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }
}
