using Microsoft.AspNetCore.Mvc;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/badges/v1")]
public class BadgesControllerV1
{
    [HttpGet("users/{userId:long}/badges")]
    public dynamic GetBadges()
    {
        return new
        {
            nextPageCursor = (string?) null,
            previousPageCursor = (string?) null,
            data = new List<int>(),
        };
    }
    
    [HttpGet("universes/{universeId:long}/badges")]
    public dynamic GetUniverseBadges()
    {
        return new
        {
            nextPageCursor = (string?) null,
            previousPageCursor = (string?) null,
            data = new List<int>(),
        };
    }
}