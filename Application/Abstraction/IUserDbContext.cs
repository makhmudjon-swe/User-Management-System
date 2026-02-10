using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstraction
{
    public interface IUserDbContext
    {
        public DbSet<User> Users { get; set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
