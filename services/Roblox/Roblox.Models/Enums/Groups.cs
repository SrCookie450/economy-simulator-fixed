namespace Roblox.Models.Groups;

public enum AuditActionType
{
    DeletePost = 1,
    RemoveMember,
    AcceptJoinRequest,
    DeclineJoinRequest,
    PostStatus,
    ChangeRank,
    BuyAd,
    SendAllyRequest,
    CreateEnemy,
    AcceptAllyRequest,
    DeclineAllyRequest,
    DeleteAlly,
    DeleteEnemy,
    AddGroupPlace,
    RemoveGroupPlace,
    CreateItems,
    ConfigureItems,
    SpendGroupFunds,
    ChangeOwner,
    Delete,
    AdjustCurrencyAmounts,
    Abandon,
    Claim,
    Rename,
    ChangeDescription,
    InviteToClan,
    KickFromClan,
    CancelClanInvite,
    BuyClan,
    CreateGroupAsset,
    UpdateGroupAsset,
    ConfigureGroupAsset,
    RevertGroupAsset,
    CreateGroupDeveloperProduct,
    ConfigureGroupGame,
    Lock,
    Unlock,
    CreateGamePass,
    CreateBadge,
    ConfigureBadge,
    SavePlace,
    PublishPlace,
    UpdateRolesetRank,
    UpdateRolesetData,
}

public enum GroupPermission
{
    DeleteFromWall = 1,
    PostToWall,
    InviteMembers,
    PostToStatus,
    RemoveMembers,
    ViewStatus,
    ViewWall,
    ChangeRank,
    AdvertiseGroup,
    ManageRelationships,
    AddGroupPlaces,
    ViewAuditLogs,
    CreateItems,
    ManageItems,
    SpendGroupFunds,
    ManageClan,
    ManageGroupGames,
    Owner,
}

public enum SocialLinkType
{
    Facebook = 1,
    Twitter,
    YouTube,
    Twitch,
    GooglePlus,
    Discord,
    RobloxGroup
}

public enum RelationshipType
{
    Allies = 1,
    Enemies,
}