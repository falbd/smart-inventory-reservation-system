using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace SmartInventory.Infrastructure.Services;

public class RedisLockService
{
    private readonly IDatabase _database;

    public RedisLockService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<bool> AcquireLockAsync(string key, string value, TimeSpan expiry)
    {
        return await _database.StringSetAsync(
            key,
            value,
            expiry,
            When.NotExists);
    }

    public async Task ReleaseLockAsync(string key, string value)
    {
        var currentValue = await _database.StringGetAsync(key);

        if (currentValue == value)
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}
