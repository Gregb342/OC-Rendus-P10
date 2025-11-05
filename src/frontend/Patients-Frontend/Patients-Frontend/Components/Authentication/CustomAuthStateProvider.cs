using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ClaimsPrincipal _anonymous => new(new ClaimsIdentity());

    public CustomAuthStateProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Session.GetString("authToken") is string token && !string.IsNullOrWhiteSpace(token))
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(token))
            {
                var jwt = handler.ReadJwtToken(token);
                if (jwt.ValidTo > DateTime.UtcNow)
                {
                    var identity = new ClaimsIdentity(jwt.Claims, "jwt");
                    return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
                }
            }
        }

        return Task.FromResult(new AuthenticationState(_anonymous));
    }

    public Task MarkUserAsAuthenticated(string token)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        httpContext?.Session.SetString("authToken", token);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        return Task.CompletedTask;
    }

    public Task MarkUserAsLoggedOut()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        httpContext?.Session.Remove("authToken");

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        return Task.CompletedTask;
    }

    public Task<string?> GetTokenAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return Task.FromResult(httpContext?.Session.GetString("authToken"));
    }
}