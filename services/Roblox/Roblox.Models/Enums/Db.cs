namespace Roblox.Models.Db;

public enum SortOrder
{
    Asc = 1,
    Desc,
}

public static class SortOrderExtensions
{
    public static string ToSql(this SortOrder order)
    {
        return order == SortOrder.Asc ? "asc" : "desc";
    }
}