using Microsoft.AspNetCore.Mvc;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/contacts/v1")]
public class ContactsControllerV1
{
    [HttpGet("contacts/metadata")]
    public dynamic GetMetadata()
    {
        return new
        {
            multiGetContactsMaxSize = 200,
        };
    }

    [HttpGet("user/get-tags")]
    public dynamic GetTags()
    {
        return new List<string>();
    }
}