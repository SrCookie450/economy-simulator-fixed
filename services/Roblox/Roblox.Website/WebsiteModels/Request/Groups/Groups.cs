using System.ComponentModel.DataAnnotations;
using Roblox.Models.Assets;

namespace Roblox.Website.WebsiteModels.Groups;

public class SetDescriptionRequest
{
    [MinLength(1), MaxLength(1000)]
    public string description { get; set; }
}

public class SetPrimaryGroupRequest
{
    public long groupId { get; set; }
}

public class GetGroupPoliciesRequest
{
    public IEnumerable<long> groupIds { get; set; }
}

public class CreateGroupRequest
{
    public string name { get; set; }
    public string description { get; set; }
    public bool publicGroup { get; set; }
    public bool buildersClubMembersOnly { get; set; }
    public IFormFile icon { get; set; }
}

// {"PayoutType":"FixedAmount","Recipients":[{"recipientId":12,"recipientType":"User","amount":3}]}
public class PayoutRecipient
{
    public long recipientId { get; set; }
    public CreatorType recipientType { get; set; }
    public long amount { get; set; }
}
public class PayoutRequest
{
    public string PayoutType { get; set; }
    public IEnumerable<PayoutRecipient> Recipients { get; set; }
}