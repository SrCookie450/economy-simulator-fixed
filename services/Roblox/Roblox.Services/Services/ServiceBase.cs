using System.Text.RegularExpressions;
using Dapper;
using Npgsql;
using Roblox.Cache;
using Roblox.Services.Exceptions;

namespace Roblox.Services
{
    public class ServiceBase : IDisposable
    {
        public NpgsqlConnection? transactionConnection { get; set; }
        public NpgsqlConnection db => transactionConnection ?? Database.connection;

        public DistributedCache redis => Roblox.Services.Cache.distributed;

        private static Regex keywordRegex = new Regex("[a-zA-Z0-9]+");
        protected string FilterKeyword(string dirtyKeyword)
        {
            var newKeyword = keywordRegex.Match(dirtyKeyword);
            return newKeyword.Value;
        }

        /// <summary>
        /// InTransaction updates the DB connection for the current service, and calls the cb once a transaction is created.
        /// </summary>
        /// <param name="cb"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> InTransaction<T>(Func<NpgsqlTransaction,Task<T>> cb)
        {
            if (transactionConnection != null)
            {
                // Nested transaction.
                return await cb(null!);
            }

            // Wrapped in mutex to fix "already executing query" bug
            Database.AcquireConnectionMutex("InTransaction<T>");
            NpgsqlConnection con;
            NpgsqlTransaction trx;
            try
            {
                con = Database.unsafeConnection;
                transactionConnection = con;
                con.Open();
                trx = con.BeginTransaction();
            }
            finally
            {
                Database.ReleaseConnectionMutex();
            }
            
            try
            {
                var result = await cb(trx);
                // If we commit before setting to null, we may break stuff
                await trx.CommitAsync();
                return result;
            }
            catch (System.Exception e)
            {
                Console.WriteLine("[info] Transaction Failure. MSG={0} STACK={1}", e.Message, e.StackTrace);
                await trx.RollbackAsync();
                throw;
            }
            finally
            {
                transactionConnection = null;
                await con.CloseAsync();
            }
        }

        /// <summary>
        /// InLock will get a global lock on the lockName for as long as expiration, and call the callback once the lock is acquired.
        /// </summary>
        /// <param name="lockName">The name of the resource to lock</param>
        /// <param name="expiration">Amount of time to hold the lock for. If you function takes longer to execute than expiration, you risk having multiple requests editing the same lock - you should make this as high as you possibly can.</param>
        /// <param name="cb">The function to call once the lock is acquired</param>
        /// <typeparam name="T">Return type of the callback</typeparam>
        /// <returns>Whatever the callback returns</returns>
        /// <exception cref="LockNotAcquiredException">Lock could not be acquired - likely already in use.</exception>
        public async Task<T> InLock<T>(string lockName, TimeSpan expiration, Func<Task<T>> cb)
        {
            await using (var redLock = await Cache.redLock.CreateLockAsync(lockName, expiration))
            {
                // make sure we got the lock
                if (redLock.IsAcquired)
                {
                    var result = await cb();
                    return result;
                }
            }

            throw new LockNotAcquiredException();
        }

        public async Task<long> InsertAsync<T>(string tableName, T obj)
        {
            var tableData = Database.tableToColumnMap[tableName];
            var idColumn = "id";
            if (!tableData.Contains(idColumn))
            {
                idColumn = tableData.First();
            }
            return await InsertAsync(tableName, idColumn, obj);
        }
        
        public async Task<long> InsertAsync<T>(string tableName, string keyName, T obj)
        {
            // You cannot do a prepared statement with table names or table columns,
            // so we manually make sure the tableName and columns actually exist, to prevent SQL injection
            
            // TODO: failures should be reported somewhere. It's unlikely we'd have failures outside of dev envs unless someone is using this function incorrectly.
            var tableData = Database.tableToColumnMap[tableName];
            if (tableData == null || !Database.tableNames.Contains(tableName)) throw new Exception("Invalid table name: " + tableName);
            if (!tableData.Contains(keyName))
                throw new Exception("Column " + keyName + " does not exist in table " + tableName);
            
            var columnList = new List<string>();
            var props = typeof(T).GetProperties();
            if (obj is Dictionary<string, dynamic?> dict)
            {
                foreach (var kvp in dict)
                {
                    if (!tableData.Contains(kvp.Key))
                        throw new Exception("Column \"" + kvp.Key + "\" of table " + tableName + " does not exist");

                    columnList.Add(kvp.Key);
                }
            }
            else
            {

                foreach (var item in props)
                {
                    if (!tableData.Contains(item.Name))
                        throw new Exception("Column \"" + item.Name + "\" of table " + tableName + " does not exist");

                    columnList.Add(item.Name);
                }
            }

            tableName = "\"" + tableName + "\"";
            var query = $"INSERT INTO {tableName} ({string.Join(",", columnList)}) VALUES ({string.Join(",", columnList.Select(c => ":" + c))}) RETURNING "+keyName;
            var result = await db.ExecuteReaderAsync(query, obj);
            await result.ReadAsync();
            long id;
            try
            {
                id = result.GetInt64(0);
                if (id == 0)
                    throw new Exception("No ID in returned sql");
            }
            catch (InvalidCastException)
            {
                // uses guid
                id = 0;
            }
            
            await result.CloseAsync();
            return id;
            /*
            foreach(IDictionary<string, object> row in result) {
                foreach(var pair in row) {
                    if (pair.Key == keyName)
                    {
                        Console.WriteLine("Check {0}:{1}",pair.Key,pair.Value);
                        var id = (long) pair.Value;
                        if (id == 0)
                            throw new Exception("Returned ID was 0");
                        return id;
                    }
                }
            }

            throw new Exception("DB returned 0 rows");
            */
        }

        public async Task UpdateAsync<TKey, TUpdateObject>(string tableName, TKey foreignKey, TUpdateObject obj)
        {
            await UpdateAsync(tableName, "id", foreignKey, obj);
        }
        
        public async Task UpdateAsync<TKey,TUpdateObject>(string tableName, string foreignKeyName, TKey foreignKey, TUpdateObject obj)
        {
            // You cannot do a prepared statement with table names or table columns,
            // so we manually make sure the tableName and columns actually exist, to prevent SQL injection
            
            // TODO: failures should be reported somewhere. It's unlikely we'd have failures outside of dev envs unless someone is using this function incorrectly.
            var tableData = Database.tableToColumnMap[tableName];
            if (tableData == null || !Database.tableNames.Contains(tableName)) throw new Exception("Invalid table name: " + tableName);
            if (!tableData.Contains(foreignKeyName))
            {
                throw new Exception("Foreign key " + foreignKeyName + " does not exist in table " + tableName);
            }
            
            var columnList = new List<string>();
            if (obj is DynamicParameters par)
            {
                foreach (var col in par.ParameterNames)
                {
                    if (!tableData.Contains(col))
                        throw new Exception("Column \"" + col + "\" of table " + tableName + " does not exist");
                
                    columnList.Add(col);
                }
            }
            else
            {
                var props = typeof(TUpdateObject).GetProperties();
                foreach (var item in props)
                {
                    if (!tableData.Contains(item.Name))
                        throw new Exception("Column \"" + item.Name + "\" of table " + tableName + " does not exist");

                    columnList.Add(item.Name);
                }
            }

            var updateColumns = new List<string>();
            foreach (var item in columnList)
            {
                updateColumns.Add(item + " = " + ":" + item);
            }
            // quote table name
            tableName = "\"" + tableName + "\"";
            var query = $"UPDATE {tableName} SET {string.Join(",", updateColumns)} WHERE {foreignKeyName} = {foreignKey}";
            await db.ExecuteAsync(query, obj);
        }

        public async Task<IEnumerable<TReturnType>> MultiGetAsync<TReturnType, TSearchType>(string tableName, string columnToSearchOn, IEnumerable<string> columns, IEnumerable<TSearchType> items, string sqlOperator = "=")
        {
            sqlOperator = sqlOperator.ToLower();
            var tableData = Database.tableToColumnMap[tableName];
            if (tableData == null || !Database.tableNames.Contains(tableName)) throw new Exception("Invalid table name: " + tableName);
            if (!tableData.Contains(columnToSearchOn))
            {
                throw new Exception("Column " + columnToSearchOn + " does not exist in table " + tableName);
            }

            var columnsList = columns.ToList();
            var itemsList = items.ToList();
            foreach (var item in columnsList)
            {
                if (!columnsList.Contains(item))
                {
                    throw new Exception("Column " + item + " does not exist in table " + tableName);
                }
            }

            if (sqlOperator != "=" && sqlOperator != "ilike" && sqlOperator != "like")
            {
                throw new Exception("Unsupported sql operator: " + sqlOperator);
            }

            var whereClauses = new List<string>();
            var obj = new DynamicParameters();
            for (var i = 0; i < itemsList.Count; i++)
            {
                var paramName = "@param" + i;
                whereClauses.Add(columnToSearchOn + " "+sqlOperator+" " + paramName);
                obj.Add(paramName, itemsList[i]);
            }
            var query = $"SELECT {string.Join(",", columnsList)} FROM \"{tableName}\" WHERE {string.Join(" OR ", whereClauses)}";
            return await db.QueryAsync<TReturnType>(query, obj);
        }

        public void Dispose()
        {
            // Can be implemented by inheritor when they need to dispose stuff
        }
    }
}