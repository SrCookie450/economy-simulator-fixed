namespace Roblox.Models.Assets
{
    public enum CreatorType
    {
        User = 1,
        Group,
    }
    
    public enum Type
    {
        Image = 1,

        TeeShirt = 2,
        TShirt = 2,

        Audio = 3,
        Mesh = 4,
        Lua = 5,
        Hat = 8,
        Place = 9,
        Model = 10,
        Shirt = 11,
        Pants = 12,
        Decal = 13,
        Head = 17,
        Face = 18,
        Gear = 19,
        Badge = 21,
        Animation = 24,
        Torso = 27,
        RightArm = 28,
        LeftArm = 29,
        LeftLeg = 30,
        RightLeg = 31,
        Package = 32,
        GamePass = 34,
        Plugin = 38,
        SolidModel = 39,
        MeshPart = 40,
        HairAccessory = 41,
        FaceAccessory = 42,
        NeckAccessory = 43,
        ShoulderAccessory = 44,
        FrontAccessory = 45,
        BackAccessory = 46,
        WaistAccessory = 47,
        ClimbAnimation = 48,
        DeathAnimation = 49,
        FallAnimation = 50,
        IdleAnimation = 51,
        JumpAnimation = 52,
        RunAnimation = 53,
        SwimAnimation = 54,
        WalkAnimation = 55,
        PoseAnimation = 56,
        
        
        
        Special = 500,
    }

    public enum Genre
    {
        All = 0,
        Building = 13,
        Horror = 5,
        TownAndCity = 1,
        Military = 11,
        Comedy = 9,
        Medieval = 2,
        Adventure = 7,
        SciFi = 3,
        Naval = 6,
        FPS = 14,
        RPG = 15,
        Sports = 8,
        Fighting = 4,
        Western = 10,
        Skatepark = 18,
    }

    public enum ModerationStatus
    {
        ReviewApproved = 1,
        AwaitingApproval,
        Declined,
        AwaitingModerationDecision,
    }

    public enum AudioValidation
    {
        Ok = 0,
        TooLong = 1,
        TooShort,
        FileTooLarge,
        EmptyStream,
        UnsupportedFormat,
    }

    public enum UserAdvertisementTargetType
    {
        Asset = 1,
        Group,
    }

    public enum UserAdvertisementType
    {
        Banner728x90 = 1,
        SkyScraper160x600,
        Rectangle300x250,
    }

    public enum AssetVoteType
    {
        Upvote = 1,
        Downvote,
    }

    public static class UserAdvertisementTypeExtensions
    {
        public static int GetWidth(this UserAdvertisementType type)
        {
            switch (type)
            {
                case UserAdvertisementType.Banner728x90:
                    return 728;
                case UserAdvertisementType.Rectangle300x250:
                    return 300;
                case UserAdvertisementType.SkyScraper160x600:
                    return 160;
                default:
                    throw new NotImplementedException("GetWidth() does not support " + type);
            }
        }

        public static int GetHeight(this UserAdvertisementType type)
        {
            switch (type)
            {
                case UserAdvertisementType.Banner728x90:
                    return 90;
                case UserAdvertisementType.Rectangle300x250:
                    return 250;
                case UserAdvertisementType.SkyScraper160x600:
                    return 600;
                default:
                    throw new NotImplementedException("GetHeight() does not support " + type);
            }
        }
    }

    public class LotteryItemEntry
    {
        public string name { get; set; } = string.Empty;
        public long assetId { get; set; }
        public long userAssetId { get; set; }
        public long recentAveragePrice { get; set; }
        public DateTime onlineAt { get; set; }
        public string username { get; set; } = string.Empty;
        public long userId { get; set; }
    }

    public class AssetVotesResponse
    {
        public long upVotes { get; set; }
        public long downVotes { get; set; }
    }
}

