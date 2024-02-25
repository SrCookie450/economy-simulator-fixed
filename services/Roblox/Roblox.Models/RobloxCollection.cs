namespace Roblox.Models;

public class CursorEntry
{
    public int limit { get; set; } = 100;
    public long startId { get; set; }
    public long previousStartId { get; set; }
    /// <summary>
    /// Column to sort on
    /// </summary>
    public string sortColumn { get; set; } = string.Empty;
    /// <summary>
    /// Either "asc" or "desc"
    /// </summary>
    public Sort sort { get; set; }
    public Direction direction { get; set; }
}

public class BadCursorException : System.Exception
{
        
}

public enum Direction
{
    Forwards = 1,
    Backwards = 2,
}

public enum Sort
{
    Asc = 1,
    Desc,
}

public class RobloxCollection<T>
{
    public IEnumerable<T>? data { get; set; }
}

public class RobloxCollectionPaginated<T> : RobloxCollection<T>
{
    public string? nextPageCursor { get; set; }
    public string? previousPageCursor { get; set; }

    public RobloxCollectionPaginated()
    {
        
    }

    public RobloxCollectionPaginated(int limit, int offset, IEnumerable<T> data)
    {
        var dataArray = data.ToArray();
        nextPageCursor = dataArray.Length >= limit ? (dataArray.Length + offset).ToString() : null;
        previousPageCursor = (offset - limit) >= 0 ? (offset - limit).ToString() : null;
        this.data = dataArray;
    }
}