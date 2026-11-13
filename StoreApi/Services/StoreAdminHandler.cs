using Microsoft.AspNetCore.Authorization;

namespace StoreApi.Services;

public sealed class StoreAdminRequirement : IAuthorizationRequirement { }

public sealed class StoreAdminHandler : AuthorizationHandler<StoreAdminRequirement>
{
    private readonly StoreProfileService _profiles;
    private readonly ILogger<StoreAdminHandler> _logger;

    public StoreAdminHandler(StoreProfileService profiles, ILogger<StoreAdminHandler> logger)
    {
        _profiles = profiles;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, StoreAdminRequirement requirement)
    {
        if (context.User.FindFirst("role")?.Value != "authenticated" ||
            !Guid.TryParse(context.User.FindFirst("sub")?.Value, out var userId))
            return;

        try
        {
            if (await _profiles.GetOrCreateRoleAsync(userId) == "admin")
                context.Succeed(requirement);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to check BookStore role for user {UserId}", userId);
        }
    }
}
