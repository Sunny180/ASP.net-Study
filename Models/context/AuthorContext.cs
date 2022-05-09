using Microsoft.EntityFrameworkCore;

namespace AuthorApi.Models
{
    public class AuthorContext : DbContext
    {
        public AuthorContext(DbContextOptions<AuthorContext> options)
            : base(options)
        {
        }

        public DbSet<GetAuthorOverview> GetAuthorOverview => Set<GetAuthorOverview>();
        public DbSet<GetAuthorId> GetAuthorId => Set<GetAuthorId>();
    }
}