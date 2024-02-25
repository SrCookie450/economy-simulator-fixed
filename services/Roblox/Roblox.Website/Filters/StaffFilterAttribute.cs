using Microsoft.AspNetCore.Mvc.Filters;
using Roblox.Models.Sessions;
using Roblox.Models.Staff;
using Roblox.Services;
using ServiceProvider = Roblox.Services.ServiceProvider;

namespace Roblox.Website.Filters;

public class StaffFilter : ActionFilterAttribute, IAsyncActionFilter
{
    private static long ownerUserId { get; set; }
    public static void Configure(long newOwnerUserId)
    {
        ownerUserId = newOwnerUserId;
        Console.WriteLine("[info] owner = {0}",ownerUserId);
    }

    public static async Task<IEnumerable<long>> GetStaff()
    {
        using var us = ServiceProvider.GetOrCreate<UsersService>();
        var allStaff = await us.GetAllStaff();
        var list = new List<long>();
        foreach (var staff in allStaff)
        {
            list.Add(staff.userId);
        }
        
        if (!list.Contains(ownerUserId))
            list.Add(ownerUserId);

        return list;
    }

    public static bool IsOwner(long userId)
    {
        return ownerUserId == userId;
    }
    
    public static async Task<bool> IsStaff(long userId)
    {
        return IsOwner(userId) || (await GetPermissions(userId)).Any();
    }

    public static async Task<IEnumerable<Access>> GetPermissions(long userId)
    {
        using var us = ServiceProvider.GetOrCreate<UsersService>();
        return (await us.GetStaffPermissions(userId)).Select(c => c.permission);
    }

    private Access permission { get; set; }
    public StaffFilter(Access requiredPermission)
    {
        permission = requiredPermission;
    }

    private async Task OnFail(HttpContext ctx)
    {
        ctx.Response.StatusCode = 403;
        await ctx.Response.WriteAsJsonAsync(new
        {
            errors = new List<dynamic>()
            {
                new
                {
                    message = "Forbidden",
                    code = 0,
                }
            }
        });
    }
    
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // TODO: better error?
        if (!Enum.IsDefined(permission))
        {
            await OnFail(context.HttpContext);
            return;
        }
        
        var userInfo = (UserSession?) context.HttpContext.Items[".ROBLOSECURITY"];
        if (userInfo == null)
        {
            await OnFail(context.HttpContext);
            return;
        }

        if (ownerUserId == userInfo.userId)
        {
            await next();
            return;
        }

        using var us = ServiceProvider.GetOrCreate<UsersService>();
        var permissions = await us.GetStaffPermissions(userInfo.userId);

        var hasRequiredPermission = permissions.Select(c => c.permission).Contains(permission);
        if (!hasRequiredPermission)
        {
            await OnFail(context.HttpContext);
            return;
        }
        Console.WriteLine("[info] admin authorized user {0} for {1}", userInfo.userId, permission);
        await next();
    }
}