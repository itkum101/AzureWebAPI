

using AzureWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AzureWebAPI
{
    public class ApplicationDBContext : DbContext
    {
        
 
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

       public DbSet<User> Users { get; set;  }

    }
}
