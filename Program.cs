using AzureWebAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;

namespace AzureWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<ApplicationDBContext>(options =>
            {
                options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 3
            ));

            });
            
            builder.Services.AddStackExchangeRedisCache(options =>

            {
                options.Configuration = builder.Configuration.GetValue<String>("RedisURL");
            });

            // Register the Redis connection multiplexer as a singleton service
            // This allows the application to interact directly with Redis for advanced scenarios
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
                // Establish a connection to the Redis server using the configuration from appsettings.json
                ConnectionMultiplexer.Connect(builder.Configuration.GetValue<String>("RedisURL")));



            builder.Services.AddScoped<TokenService>();
            // Register Response Caching services
            builder.Services.AddResponseCaching();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    policy =>
                    {
                        policy.AllowAnyOrigin()  // Allow all origins
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });
            // Configure JWT authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                        RoleClaimType = ClaimTypes.Role,
                    };
                });


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Apply the new CORS policy
            app.UseCors("AllowAllOrigins");

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}