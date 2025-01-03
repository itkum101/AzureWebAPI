
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
                        policy.WithOrigins("https://usermgmt.subedimukti.com.np", "http://localhost", "https://localhost")  // Specific origins
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();  // Allow cookies if needed
                    });
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.Use(async (context, next) =>
            {
                if (context.Request.Method == "OPTIONS")
                {
                    // Handle the preflight request
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
                    context.Response.StatusCode = 204;
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
