using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Quiz_Application.Auth;

public class BasicAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration config)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header."));

        try
        {
            var header = AuthenticationHeaderValue.Parse(Request.Headers.Authorization!);
            if (header.Scheme != "Basic" || string.IsNullOrEmpty(header.Parameter))
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization scheme."));

            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header.Parameter));
            var parts = decoded.Split(':', 2);
            if (parts.Length != 2)
                return Task.FromResult(AuthenticateResult.Fail("Invalid credentials format."));

            var expectedUser = config["Auth:Username"];
            var expectedPass = config["Auth:Password"];

            if (parts[0] != expectedUser || parts[1] != expectedPass)
                return Task.FromResult(AuthenticateResult.Fail("Invalid username or password."));

            var claims = new[] { new Claim(ClaimTypes.Name, parts[0]) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header."));
        }
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.Headers.WWWAuthenticate = "Basic realm=\"QuizApp\"";
        return Task.CompletedTask;
    }
}
