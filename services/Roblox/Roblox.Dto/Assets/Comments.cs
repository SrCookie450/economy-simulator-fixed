namespace Roblox.Dto.Assets;

public class CommentEntry
{
    public long id { get; set; }
    public DateTime createdAt { get; set; }
    public long userId { get; set; }
    public string username { get; set; }
    public string comment { get; set; }
}