namespace Roblox.Rendering
{
    public class RenderRequest
    {
        public string command { get; set; }
        public IEnumerable<dynamic> args { get; set; }
        public string id { get; set; }
    }

    public class RenderResponse<T>
    {
        public string id { get; set; }
        public T? data { get; set; }
        public int status { get; set; }
    }

    public class AvatarBodyColors
    {
        public int headColorId { get; set; }
        public int torsoColorId { get; set; }
        public int rightArmColorId { get; set; }
        public int leftArmColorId { get; set; }
        public int rightLegColorId { get; set; }
        public int leftLegColorId { get; set; }
    }

    public class AvatarAssetTypeEntry
    {
        public int id { get; set; }
    }
    
    public class AvatarAssetEntry
    {
        public long id { get; set; }
        public AvatarAssetTypeEntry assetType { get; set; }
    }
    
    public class AvatarData
    {
        public long userId { get; set; }
        public AvatarBodyColors bodyColors { get; set; }
        public string playerAvatarType { get; set; }
        public IEnumerable<AvatarAssetEntry> assets { get; set; }
    }
}

