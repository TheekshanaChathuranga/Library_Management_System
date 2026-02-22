namespace BorrowingReturnsService.Dtos.UserIdentity
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public IEnumerable<string> Permissions { get; set; } = new List<string>();
    }
}
