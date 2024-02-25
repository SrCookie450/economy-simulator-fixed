using Microsoft.AspNetCore.Mvc;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/followings/v1")]
public class FollowingsControllerV1 : ControllerBase
{
    [HttpGet("users/{userId}/universes/{universeId}/status")]
    public dynamic GetFollowingsStatus()
    {
        return new
        {
            UniverseId = 1,
            UserId = 1,
            CanFollow = true,
            IsFollowing = false,
            FollowingCountByType = 1,
            FollowingLimitByType = 200,
        };
    }
}