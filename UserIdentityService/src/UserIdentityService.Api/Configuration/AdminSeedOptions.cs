namespace UserIdentityService.Api.Configuration;

public class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public string Email { get; set; } = "admin@library.local";
    public string Password { get; set; } = "ChangeMe!123";
    public string DisplayName { get; set; } = "Default Admin";
}
