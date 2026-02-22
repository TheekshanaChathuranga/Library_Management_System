using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace CatalogService.Authentication;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.Fail("Missing Authorization Header");
        }

        try
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }

            var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var parts = credentials.Split(':', 2);

            if (parts.Length != 2)
            {
                return AuthenticateResult.Fail("Invalid Authorization Header Format");
            }

            var username = parts[0];
            var password = parts[1];

            // Validate credentials
            if (!ValidateCredentials(username, password))
            {
                return AuthenticateResult.Fail("Invalid Username or Password");
            }

            // Create claims and principal
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Name, username),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during authentication");
            return AuthenticateResult.Fail("Error during authentication");
        }
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers["WWW-Authenticate"] = "Basic realm=\"CatalogService\"";
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    private bool ValidateCredentials(string username, string password)
    {
        // Get credentials from configuration
        var validUsername = _configuration["BasicAuth:Username"];
        var validPassword = _configuration["BasicAuth:Password"];

        // In production, you should use a more secure method to store and validate credentials
        // such as hashing passwords and storing them in a database
        return username == validUsername && password == validPassword;
    }
}
