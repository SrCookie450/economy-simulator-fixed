using Dapper;

namespace Roblox;

public static class SqlBuilderExtensions
{
    // AddClause("where", sql, parameters, " OR ", "WHERE ", "\n", true);
    public static SqlBuilder OrWhereMulti<T>(this SqlBuilder builder, string sql, IEnumerable<T> sqlParameters)
    {
        var idx = 0;
        foreach (var id in sqlParameters)
        {
            var sqlStatement = sql;
            var placeHolderIdx = sqlStatement.IndexOf("$1", StringComparison.Ordinal);
            if (placeHolderIdx == -1)
            {
                throw new ArgumentException("Sql is expected to have the magic character $1");
            }

            idx++;
            var fullArgumentName = ":param" + idx;
            var dynParams = new DynamicParameters();
            dynParams.Add(fullArgumentName, id);
            var fullSql = sqlStatement.Replace("$1", fullArgumentName);
            builder.OrWhere(fullSql, dynParams);
        }

        return builder;
    }
}