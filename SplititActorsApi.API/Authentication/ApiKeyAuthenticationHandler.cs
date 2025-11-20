using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SplititActorsApi.API.Authentication;

/// <summary>
/// Custom authentication handler to support Splitit's API key format
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "Authorization";
    private const string BearerPrefix = "Bearer ";
    private readonly IConfiguration _configuration;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if Authorization header exists
        if (!Request.Headers.ContainsKey(ApiKeyHeaderName))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header"));
        }

        var authHeader = Request.Headers[ApiKeyHeaderName].ToString();

        // Check if it starts with "Bearer "
        if (!authHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header format"));
        }

        // Extract token
        var token = authHeader.Substring(BearerPrefix.Length).Trim();

        // Validate token format (Splitit's sandbox token format: GUID-number)
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing token"));
        }

        // For Splitit sandbox: accept their token format
        // Token format: {guid}-{number} (e.g., 52cd3e6988814c6ea7b1d52a45be37c3-1)
        if (IsValidSplititToken(token))
        {
            // Create claims for authenticated user
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "SplititUser"),
                new Claim(ClaimTypes.NameIdentifier, token),
                new Claim("TokenType", "ApiKey")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
    }

    /// <summary>
    /// Validates token against allowed API keys from configuration
    /// </summary>
    private bool IsValidSplititToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        // Get allowed API keys from configuration
        var allowedKeys = _configuration.GetSection("Authentication:ApiKeys").Get<string[]>();

        if (allowedKeys == null || allowedKeys.Length == 0)
        {
            Logger.LogWarning("No allowed API keys configured in appsettings.json");
            return false;
        }

        // Check if the provided token matches any allowed key
        return allowedKeys.Contains(token, StringComparer.Ordinal);
    }
}
