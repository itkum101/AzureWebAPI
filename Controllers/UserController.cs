using AzureWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzureWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : ControllerBase
    {


        private readonly ApplicationDBContext _dbcontext; 

        public UserController(ApplicationDBContext context)
        {
            _dbcontext = context;
        }

        [HttpGet]


        [Authorize(Roles = "admin")]

        [ResponseCache(Duration = 60)]
        // The response cache will vary based on the User-Agent header.
        // [ResponseCache(Duration = 60, VaryByHeader = "User-Agent")]

        // The response will not be cached in any cache.
        //[ResponseCache(NoStore = true)]

        // Caching with Location = None
        // The response will not cached by any cache.
        //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.None)]
        //[HttpGet("nolocation")]

        // The response is cached only on the client.
        //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        //[HttpGet("clientlocation")]

        // Caching with Location = Any
        // The response can be cached by both client and proxy servers.
        //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]

        // GET: api/Products/vary-query?sort=price
        // GET: api/Products/vary-query?sort=name
        // Caches different responses for 120 Seconds based on the value of the "sort" query parameter
        //[ResponseCache(Duration = 120, VaryByQueryKeys = new[] { "sort" })]
        //[HttpGet("vary-query")]
        //public async Task<ActionResult<List<Product>>> GetProductsVaryByQuery([FromQuery] string sort)

        // Different cache entries for different id values.
        //[ResponseCache(Duration = 120, VaryByQueryKeys = new[] { "id" })]
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Product>> GetProduct(int id)
        public async Task<List<User>> GetAllUsers()
        {
            return await _dbcontext.Users.ToListAsync(); 
        }
    }
}
