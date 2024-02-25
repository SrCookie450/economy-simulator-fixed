using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Roblox.Website.WebsiteModels.Groups;

public class CreateWallPostRequest
{
    [MaxLength(500), MinLength(1)]
    public string body { get; set; }
}