using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Dto.Avatar;
using Roblox.Exceptions.Services.Users;
using Roblox.Models.Avatar;
using Roblox.Website.Controllers;
using ControllerBase = Roblox.Website.Controllers.ControllerBase;

namespace Roblox.Website.Pages.Auth;

public class AccountDeletion : RobloxPageModel
{
    static AccountDeletion()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                var currentTime = DateTime.UtcNow;
                var reset = currentTime.Add(TimeSpan.FromDays(1));
                var trueResetTime = new DateTime(reset.Year, reset.Month, reset.Day, 0, 0, 0);
                if (trueResetTime < currentTime)
                {
                    trueResetTime = trueResetTime.Add(TimeSpan.FromDays(1));
                }
                var delay = trueResetTime.Subtract(currentTime);
                Console.WriteLine("[info] will clear AccountDeletion request dictionary in {0}",delay);
                await Task.Delay(delay);
                attempts.Clear();
            }
        });
    }
    private static Dictionary<string, int> attempts = new();
    public string? successMessage { get; set; }
    public string? failureMessage { get; set; }
    public void OnGet()
    {
        
    }
    
    [BindProperty]
    public string? username { get; set; }
    [BindProperty]
    public string? password { get; set; }

    public async Task<IActionResult> OnPost()
    {
        var services = new ControllerServices();
        var rlKey = ControllerBase.GetIP(ControllerBase.GetRequesterIpRaw(HttpContext)) + "_" + DateTime.UtcNow.ToString("d");
        var tries = attempts.ContainsKey(rlKey) ? attempts[rlKey] : 0;
        if (tries >= 10)
        {
            failureMessage = "You have been making too many attempts. Try again tomorrow.";
            return new PageResult();
        }

        if (!attempts.ContainsKey(rlKey))
            attempts[rlKey] = 0;
        attempts[rlKey]++;
        if (username == null)
        {
            password = null;
            failureMessage = "Invalid username provided.";
            return new PageResult();
        }
        
        var user = (await services.users.MultiGetUsersByUsername(new[] {username})).ToArray();
        if (user.Length == 0)
        {
            username = null;
            password = null;
            failureMessage = "The username provided is invalid or does not exist. Your account may already be deleted.";
            return new PageResult();
        }

        var loginOk = await services.users.VerifyPassword(user[0].id, password);
        if (!loginOk)
        {
            username = null;
            password = null;
            failureMessage = "The username and password combination provided is invalid. Please try again.";
            return new PageResult();
        }

        try
        {
            await services.users.DeleteUser(user[0].id, false);
        }
        catch (AccountLastOnlineTooRecentlyException)
        {
            failureMessage = "Your account was last online too recently. Please try again later.";
            return new PageResult();
        }

        // reset av
        services.avatar.RedrawAvatar(user[0].id, new List<long>(), new ColorEntry()
        {
            headColorId = 194,
            torsoColorId = 23,
            rightArmColorId = 194,
            leftArmColorId = 194,
            rightLegColorId = 102,
            leftLegColorId = 102,
        }, AvatarType.R6);
        successMessage = "Your account has been successfully deleted.";
        return new PageResult();
    }
}