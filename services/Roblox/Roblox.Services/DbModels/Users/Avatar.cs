namespace Roblox.Services.DbModels;

public class DatabaseAvatar
{
    public int head_color_id { get; set; }
    public int torso_color_id { get; set; }
    public int left_leg_color_id { get; set; }
    public int right_leg_color_id { get; set; }
    public int left_arm_color_id { get; set; }
    public int right_arm_color_id { get; set; }
}

public class DatabaseAvatarWithImages : DatabaseAvatar
{
    public string thumbnail_url { get; set; }
    public string headshot_thumbnail_url { get; set; }
}