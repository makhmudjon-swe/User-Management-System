using Infrasturcture.DataAccess;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace User_Management_System
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            var fallback = builder.Configuration.GetConnectionString("Default");
            var connectionString = BuildConnectionString(databaseUrl, fallback);

            builder.Services.AddDbContext<UserDbContext>(options =>
                options.UseNpgsql(connectionString)
            );

            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                db.Database.Migrate();
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        private static string BuildConnectionString(string? databaseUrl, string? fallbackConnectionString)
        {
            if (!string.IsNullOrWhiteSpace(databaseUrl))
            {
                var uri = new Uri(databaseUrl);
                var userInfo = uri.UserInfo.Split(':', 2);
                return new NpgsqlConnectionStringBuilder
                {
                    Host = uri.Host,
                    Port = uri.Port,
                    Username = userInfo[0],
                    Password = userInfo.Length > 1 ? userInfo[1] : "",
                    Database = uri.AbsolutePath.Trim('/'),
                    SslMode = SslMode.Require,
                    TrustServerCertificate = true
                }.ToString();
            }
            if (!string.IsNullOrWhiteSpace(fallbackConnectionString))
                return fallbackConnectionString;

            throw new Exception("No database configuration found. Set DATABASE_URL on Render or ConnectionStrings:Default locally.");
        }
    }
}
