using Microsoft.AspNetCore.Authorization;

public class UserLevelRequirement : IAuthorizationRequirement
{
    public int RequiredUserLevel { get; }

    public UserLevelRequirement(int requiredUserLevel)
    {
        RequiredUserLevel = requiredUserLevel;
    }
}
