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


        public async Task<List<User>> GetAllUsers()
        {
            return await _dbcontext.Users.ToListAsync(); 
        }
    }
}
