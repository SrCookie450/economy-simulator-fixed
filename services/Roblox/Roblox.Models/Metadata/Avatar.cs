using System.Text.Json;

namespace Roblox.Models.Avatar;

public class ColorMetadataEntry
{
    public int brickColorId { get; set; }
    public string hexColor { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
}

public static class AvatarMetadata
{
    private static List<ColorMetadataEntry>? colors { get; set; }

    public static List<ColorMetadataEntry> GetColors()
    {
        if (colors == null)
        {
            var fi = File.ReadAllText(Roblox.Configuration.JsonDataDirectory + "avatar-colors.json");
            colors = JsonSerializer.Deserialize<List<ColorMetadataEntry>>(fi);
        }

        if (colors == null)
            throw new Exception("Could not deserialize avatar colors configuration");

        return colors;
    }
}