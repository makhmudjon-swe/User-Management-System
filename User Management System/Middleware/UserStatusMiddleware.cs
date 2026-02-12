using System.Security.Claims;
using Domain.Enums;
using Infrasturcture.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace User_Management_System.Middleware;

public class UserStatusMiddleware
{
    private readonly RequestDelegate _next;

    public UserStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext ctx, UserDbContext db)
    {
        var path = (ctx.Request.Path.Value ?? "").ToLowerInvariant();
        var isFree =
            path.StartsWith("/api/auth/register") ||
            path.StartsWith("/api/auth/login") ||
            path.StartsWith("/api/auth/confirm-email") ||
            path.StartsWith("/swagger");

        if (!isFree && ctx.User.Identity?.IsAuthenticated == true)
        {
            var sub =
                ctx.User.FindFirst("sub")?.Value
                ?? ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // <-- MUHIM

            if (!Guid.TryParse(sub, out var userId))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await ctx.Response.WriteAsync("Invalid token");
                return;
            }

            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            if (user is null)
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await ctx.Response.WriteAsync("User not found");
                return;
            }

            if (user.Status == UserStatus.Blocked)
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                await ctx.Response.WriteAsync("User blocked");
                return;
            }
        }

        await _next(ctx);
    }
}
