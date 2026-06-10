using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WorldCup.Domain.Abstractions;

namespace WorldCup.Api.Common.Services;

public sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var sub = Principal?.FindFirstValue("sub") ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email => Principal?.FindFirstValue("email") ?? Principal?.FindFirstValue(ClaimTypes.Email);

    public IReadOnlyCollection<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? [];
}
