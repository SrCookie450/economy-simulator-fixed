namespace Roblox.Models.Users;

public enum ApplicationRedemptionFailureReason
{
    Ok = 1,
    Expired,
    AlreadyAssociatedWithUser,
    DoesNotExist,
}