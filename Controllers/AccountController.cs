using AzureWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace AzureWebAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase
    {

        private readonly ApplicationDBContext _dbContext;


        public AccountController(ApplicationDBContext dbContext)
        {

            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {


            if (ModelState.IsValid)
            {
                await _dbContext.Users.AddAsync(user);

                    await  _dbContext.SaveChangesAsync(); 
                    return Ok(user);
                    


               
            }

            // Return validation errors
            return BadRequest(ModelState);

        }

        [HttpGet]

        public async Task<IActionResult> GetAllUser()
        {
            var users = await _dbContext.Users.ToListAsync(); 
            return Ok(users); 
        }


    }
}
