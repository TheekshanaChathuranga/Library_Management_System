namespace UserIdentityService.Api.Constants;

public static class Permissions
{
    public const string ManageCatalog = "catalog.manage";
    public const string ViewCatalog = "catalog.view";
    public const string ManageUsers = "users.manage";
    public const string IssueLoans = "loans.issue";

    public static readonly string[] Default =
    [
        ManageCatalog,
        ViewCatalog,
        ManageUsers,
        IssueLoans
    ];
}
