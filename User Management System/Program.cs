using Infrasturcture.DataAccess;
using Microsoft.EntityFrameworkCore;

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
            var cs = builder.Configuration.GetConnectionString("DefaultConnection");
            Console.WriteLine("DB HOST = " + new Npgsql.NpgsqlConnectionStringBuilder(cs).Host);

            builder.Services.AddDbContext<UserDbContext>(options => options.UseNpgsql(cs));

            builder.Services.AddDbContext<UserDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
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
    }
}
