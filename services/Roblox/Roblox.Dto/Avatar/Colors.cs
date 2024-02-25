namespace Roblox.Dto.Avatar;

public class ColorEntry
{
    public int headColorId { get; set; }
    public int torsoColorId { get; set; }
    public int leftArmColorId { get; set; }
    public int rightArmColorId { get; set; }
    public int leftLegColorId { get; set; }
    public int rightLegColorId { get; set; }
}

public class AvatarWithColors : ColorEntry
{
    public string? thumbnailUrl { get; set; }
    public string? headshotUrl { get; set; }
}

public class OutfitAvatar : ColorEntry
{
    public long userId { get; set; }
}