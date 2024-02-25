namespace Roblox.Dto.Persistence;

public class SetRequest
{
    public string data { get; set; }
}

public class GetKeyScope
{
    public string? scope { get; set; }
    public string target { get; set; }
    public string key { get; set; }
}

public class GetKeysRequest
{
    public IEnumerable<GetKeyScope> qkeys { get; set; }
    // &qkeys[%u].scope=%s&qkeys[%u].target=&qkeys[%u].key=%s
}

public class GetKeyEntry
{
    public string Value { get; set; }
    public string Scope { get; set; }
    public string Key { get; set; }
    public string Target { get; set; }
}