using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AzureWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RedisCacheController : ControllerBase
    {

        // To interact with the Redis Cache via IDistributedCache

        private readonly IDistributedCache _distributedCache;

        private readonly IConnectionMultiplexer _redisConnection;

        private readonly IConfiguration _configuration;


        public RedisCacheController(IDistributedCache distributedCache, IConfiguration configuration, IConnectionMultiplexer connectionMultiplixer)
        {
            _distributedCache = distributedCache;
            _redisConnection = connectionMultiplixer;
            _configuration = configuration;
        }

        [HttpGet("all")]

        public async Task<IActionResult> GetAllCachedKeysAndValues()
        {
            try
            {
                var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());

                var keys = server.Keys().ToArray();

                // Get instance from configuration 

                var cacheEntries = new List<KeyValuePair<string, string>>();

                foreach (var key in keys)
                {
                    var value = await _distributedCache.GetStringAsync(key);

                    cacheEntries.Add(new KeyValuePair<string, string>(key, value ?? null));
                }

                return Ok(cacheEntries);


            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error {ex.Message} ");
            }
        }

        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAllCachedKeysAndValues()
        {
            var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());

            var keys = server.Keys().ToArray();


            foreach (var key in keys)
            {


                await _distributedCache.RemoveAsync(key);



            }
            return Ok("All cache entry cleared");

        }

        [HttpDelete("{key}")]

        public async Task<IActionResult> ClearCacheKeyEntryByKey(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);

                // Return a success message indicating the specific cache entry was cleared
                return Ok(new { message = $"Cache entry '{key}' cleared." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to clear cache Entry", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetKeyValue(string key, object value)
        {            // Validate input

            //var value = "21"; 
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return BadRequest("Key or value cannot be null or empty.");
            }

            // Serialize the value to a JSON string
            var serializedValue = JsonSerializer.Serialize(value);

            // Store the value in Redis cache
            await _distributedCache.SetStringAsync(key, serializedValue, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) // Set expiration to 1 hour
            });

            return Ok($"Key '{key}' with value has been set in cache.");
        }

    }
}