using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

public class UserLevelHandler : AuthorizationHandler<UserLevelRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserLevelRequirement requirement)
    {
        var userLevelClaim = context.User.Claims.FirstOrDefault(c => c.Type == "UserLevel");

        if (userLevelClaim != null && int.TryParse(userLevelClaim.Value, out var userLevel) && userLevel == requirement.RequiredUserLevel)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
