namespace Roblox.Libraries.FastFlag;

public interface IFastFlag
{
    public string key { get; }
    public dynamic value { get; }
    public string type { get; }
    public bool dynamic { get; }
}

public class FastString : IFastFlag
{
    public string key { get; }
    public dynamic value { get; }
    public string type => "string";
    public bool dynamic { get; protected init; } = false;
    
    public FastString(string keyName, string value, bool showValue = true)
    {
        this.key = keyName;
        this.value = showValue ? value : "";
    }
}

public class DFastString : FastString
{
    public DFastString(string keyName, string value, bool showValue = true) : base(keyName, value, showValue)
    {
        dynamic = true;
    }
}

public class FastInt : IFastFlag
{
    public string key { get; }
    public dynamic value { get; }
    public string type => "int";
    public bool dynamic { get; protected init; } = false;

    public FastInt(string keyName, int value, bool showValue = true)
    {
        this.key = keyName;
        this.value = value;
        if (!showValue)
            this.value = 0;
    }
}

public class DFastInt : FastInt
{
    public DFastInt(string keyName, int value, bool showValue = true) : base(keyName, value, showValue)
    {
        dynamic = true;
    }
}

public class FastFlag : IFastFlag
{
    public string key { get; }
    public dynamic value { get; }
    public string type => "bool";
    public bool dynamic { get; protected init; } = false;

    public FastFlag(string keyName, bool value, bool showValue = true)
    {
        this.key = keyName;
        this.value = value;
        if (!showValue)
            this.value = false;
    }
}

public class DFastFlag : FastFlag
{
    public DFastFlag(string keyName, bool value, bool showValue = true) : base(keyName, value, showValue)
    {
        dynamic = true;
    }
}

public class FastLogGroup : IFastFlag
{
    public string key { get; }
    public dynamic value { get; }
    public string type => "flog";
    public bool dynamic { get; protected init; } = false;

    public FastLogGroup(string keyName, int value, bool showValue = true)
    {
        this.key = keyName;
        this.value = value;
        if (!showValue)
            this.value = false;
    }
}


public class FastFlagResult
{
    private IEnumerable<IFastFlag>? flags { get; set; }

    public FastFlagResult AddFlag(IFastFlag flag)
    {
        var newFlags = this.flags?.ToList() ?? new List<IFastFlag>();
        newFlags.Add(flag);
        flags = newFlags;
        return this;
    }
    
    public FastFlagResult AddFlags(IEnumerable<IFastFlag> newFlags)
    {
        var newFlagsList = this.flags?.ToList() ?? new List<IFastFlag>();
        foreach (var item in newFlags)
        {
            newFlagsList.Add(item);   
        }

        flags = newFlagsList;
        return this;
    }

    public Dictionary<string, dynamic> ToDictionary()
    {
        var dict = new Dictionary<string, dynamic>();
        if (flags != null)
        {
            foreach (var flag in flags)
            {
                var k = flag.key;
                if (flag.type == "string")
                {
                    k = "FString" + k;
                }else if (flag.type == "bool")
                {
                    k = "FFlag" + k;
                }else if (flag.type == "int")
                {
                    k = "FInt" + k;
                }else if (flag.type == "flog")
                {
                    k = "FLog" + k;
                }

                if (flag.dynamic)
                    k = "D" + k;
                
                dict.Add(k, flag.value);
            }
        }

        return dict;
    }
}