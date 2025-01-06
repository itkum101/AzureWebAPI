
using AzureWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AzureWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AuthController : ControllerBase
    {


        private readonly ApplicationDBContext _dbContext;

        private readonly IConfiguration _configuration;

        private readonly TokenService _tokenService;

        public AuthController(ApplicationDBContext dbContext, IConfiguration configuration, TokenService tokenService)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            if (ModelState.IsValid)
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.Email == login.Email && u.Password == login.Password);

                if (user == null)
                {
                    return Unauthorized("Invalid user credentials");
                }

                var token = IssueToken(user);

                var refreshToken = GenerateRefreshToken();

                await _tokenService.SaveRefreshToken(user.Username, refreshToken);


                return Ok(new { Token = token, RefreshToken= refreshToken });
            }
            return BadRequest("Invalid Request Body");
        }

        private string IssueToken(User user)
        {
            //Now generating token 

            // new symmetric security key from jwt key
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            // setup sikey gning credentials using above secuirty 
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("Myapp_User_ID", user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id.ToString())
            };

            user.Roles.ForEach(role =>
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            });


            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);


            return new JwtSecurityTokenHandler().WriteToken(token);


        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];

            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                //Fill the buffer
                rng.GetBytes(randomNumber);

                return Convert.ToBase64String(randomNumber);

            }
        }

        // Refresh Token 
        [HttpPost]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest("Refresh token is required");
            }
            try
            {
                var username = await _tokenService.RetriveUserNameByRefreshToken(request.RefreshToken);

                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized("Invalid Request token");
                }
                // Retrive user by username

                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == username);

                if (user == null)
                {
                    return Unauthorized("Invalid User");

                }
                var accessToken = IssueToken(user);

                var newRefreshToken = GenerateRefreshToken();

                await _tokenService.SaveRefreshToken(user.Username, newRefreshToken);

                return Ok(new { AccessToken = accessToken, RefreshToken = newRefreshToken });



            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error {ex.Message} ");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest("Refresh token is required"); 
            }

            try
            {
                var result = await _tokenService.RevokeRefreshToken(request.RefreshToken);
                
                if(!result)
                {
                    return NotFound("Token not found");
                }

                return Ok("Token revoked"); 


            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error");
            }
        }

    }

    
}
