using Microsoft.EntityFrameworkCore;

namespace UserApi.Models
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options)
            : base(options)
        {
        }

        public DbSet<User> User => Set<User>();
        public DbSet<UserId> UserId => Set<UserId>();
        
    }
}