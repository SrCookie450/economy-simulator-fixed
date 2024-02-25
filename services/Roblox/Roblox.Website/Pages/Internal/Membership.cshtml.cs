using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Dto.Users;
using Roblox.Models.Users;

namespace Roblox.Website.Pages.Internal;

public class Membership : RobloxPageModel
{
    public MembershipType existingMembershipType { get; set; }
    public string? successMessage { get; set; }
    public string? errorMessage { get; set; }
    public async Task OnGet()
    {
        if (userSession == null)
            return;
        var mem = await services.users.GetUserMembership(userSession.userId);
        existingMembershipType = mem?.membershipType ?? MembershipType.None;
    }

    [BindProperty]
    public MembershipType membershipType { get; set; }
    public async Task OnPost()
    {
        if (!Enum.IsDefined(membershipType) || userSession is null)
            return;
        await services.users.InsertOrUpdateMembership(userSession.userId, membershipType);
        var metadata = MembershipMetadata.GetMetadata(membershipType);
        successMessage = "Membership successfully changed to " + metadata.displayName + ". You will now receive "  + metadata.dailyRobux + " Robux each day.";
        existingMembershipType = membershipType;
    }
}