namespace Roblox.Models.Users
{
    public enum AccountStatus
    {
        Ok = 1,
        Suppressed,
        Deleted,
        Poisoned,
        MustValidateEmail,
        Forgotten,
    }

    public enum MembershipType
    {
        None = 0,
        BuildersClub,
        TurboBuildersClub,
        OutrageousBuildersClub
    }

    public enum Gender
    {
        Unknown = 1,
        Male = 2,
        Female = 3,
    }

    public enum GeneralPrivacy
    {
        NoOne = 1,
        Friends,
        Following,
        Followers,
        All = 6,
    }

    public enum TradeQualityFilter
    {
        None = 1,
        Low,
        Medium,
        High,
    }

    public enum InventoryPrivacy
    {
        NoOne = 1,
        Friends,
        FriendsAndFollowing,
        FriendsFollowingAndFollowers,
        AllAuthenticatedUsers,
        AllUsers,
    }

    public enum ThemeTypes
    {
        Light = 1,
        Dark,
    }

    public enum PresenceType
    {
        Offline = 0,
        Online,
        InGame,
        InStudio,
    }

    public enum PasswordResetState
    {
        Created = 1,
        PasswordChanged = 2,
    }
}

