namespace Roblox.Dto.Groups;

public class StatusEntry
{
    public GroupUser poster { get; set; }
    public string body { get; set; }
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
}