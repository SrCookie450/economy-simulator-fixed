using Dapper;
using Roblox.Dto.Assets;

namespace Roblox.Services;

public enum KeyType
{
    Standard = 1,
    Sorted,
}

public class DataStoreService : ServiceBase, IService
{
    private KeyType ParseType(string? type)
    {
        if (type == "sorted")
            return KeyType.Sorted;
        
        if (string.IsNullOrWhiteSpace(type) || type == "standard")
            return KeyType.Standard;
        throw new ArgumentException("Invalid " + nameof(type));
    }

    private async Task<IEnumerable<DataStoreEntry>> GetAllEntries(long placeId, string key, string scope, string name)
    {
        return await db.QueryAsync<DataStoreEntry>(
            "SELECT id, value from asset_datastore WHERE asset_id = :place_id AND key = :key AND scope = :scope AND name = :name ORDER BY id DESC",
            new
            {
                place_id = placeId,
                key,
                scope,
                name,
            });
    }

    private async Task PurgeExpiredEntries(DataStoreEntry[] all)
    {
        if (all.Length >= 5)
        {
            foreach (var item in all.Skip(5))
            {
                await db.ExecuteAsync(
                    "DELETE FROM asset_datastore WHERE id = :id",
                    new
                    {
                        id = item.id,
                    });
            }
        }
    }

    public async Task Set(long placeId, string key, string type, string scope, string target, int valueLength, string value)
    {
        if (valueLength != value.Length)
            throw new Exception("ValueLength != value.length");
        if (valueLength > 1024 * 1024 * 1)
            throw new Exception("Value length limit exceeded, max 1MB");
        
        // target is the data store target (e.g. would be "DS" in game:GetService("DataStoreService"):GetDataStore("DS")
        // key is the DS key
        // scope is either global or a custom scope - essentially a key prefix
        
        var t = ParseType(type);
        if (t != KeyType.Standard)
            return; // ignore for now
        
        var uni = placeId == 0 ? 0 : await ServiceProvider.GetOrCreate<GamesService>().GetUniverseId(placeId);

        var entries = (await GetAllEntries(placeId, key, scope, target)).ToArray();
        await PurgeExpiredEntries(entries);
        if (entries.Length > 0 && entries[0].value == value)
            return; // No need to set
        
        await db.ExecuteAsync(
            "INSERT INTO asset_datastore (asset_id, universe_id, scope, key, name, value) VALUES (:place_id, :universe_id, :scope, :key, :name, :value)",
            new
            {
                place_id = placeId,
                universe_id = uni,
                scope = scope,
                key = key,
                name = target,
                value = value,
            });
    }

    public async Task<string?> Get(long placeId, string type, string scope, string key, string target)
    {
        var t = ParseType(type);
        if (t != KeyType.Standard)
        {
            // Ignored
            return null;
        }

        // Type can be "standard" or "sorted"
        // long placeId, string type, string scope   
        var ent = await GetAllEntries(placeId, key, scope, target);
        return ent.FirstOrDefault()?.value;
    }
    
    public bool IsThreadSafe()
    {
        return false;
    }

    public bool IsReusable()
    {
        return false;
    }
}