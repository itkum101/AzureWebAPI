namespace AzureWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSubdomainsAndLocalhost",
                    policy =>
                    {
                        policy.WithOrigins("https://usermgmt.subedimukti.com.np", "http://localhost", "https://localhost")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();  // Allow cookies if needed
                    });
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

            // Handle the preflight request with proper headers
            app.Use(async (context, next) =>
            {
                if (context.Request.Method == "OPTIONS")
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Origin"]); // Reflecting the Origin header dynamically
                    context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
                    context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");  // Allow credentials
                    context.Response.StatusCode = 204; // No content
                    return;
                }
                await next();
            });

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}
