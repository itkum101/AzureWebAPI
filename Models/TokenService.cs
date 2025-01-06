using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzureWebAPI.Models
{


    public class TokenService : ControllerBase
    {

        private readonly ApplicationDBContext _dbContext;
        public TokenService(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;

        }

        public async Task SaveRefreshToken(string username, string token)
        {
            var refreshToken = new Refreshtoken
            {
                Username = username,
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            _dbContext.Add(refreshToken);

            await _dbContext.SaveChangesAsync();
        }

        // Retrive Username by Refresh Token 
        public async Task<string> RetriveUserNameByRefreshToken(string refreshToken)
        {
            var tokenRecord = await _dbContext.Refreshtokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.ExpiryDate > DateTime.UtcNow);


            return tokenRecord?.Username;
        }

        // Delete Refresh Tokens 
        public async Task<bool> RevokeRefreshToken(string refreshToken)
        {

            var tokenRecord = await _dbContext.Refreshtokens.FirstOrDefaultAsync(rt => refreshToken == rt.Token);

            if (tokenRecord != null)
            {
                _dbContext.Refreshtokens.Remove(tokenRecord);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;


        }
    }
}
