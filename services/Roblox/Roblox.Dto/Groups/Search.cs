namespace Roblox.Dto.Groups;

public class SearchGroupEntry
{
    public long id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
    public long memberCount { get; set; }
    public bool publicEntryAllowed { get; set; }
}