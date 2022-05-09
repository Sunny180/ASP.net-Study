using Microsoft.EntityFrameworkCore;

namespace ArticleApi.Models
{
    public class ArticleContext : DbContext
    {
        public ArticleContext(DbContextOptions<ArticleContext> options)
            : base(options)
        {
        }

        public DbSet<GetArticleOverview> GetArticleOverview => Set<GetArticleOverview>();
        public DbSet<GetArticleDetail> GetArticleDetail => Set<GetArticleDetail>();
        public DbSet<GetArticleId> GetArticleId => Set<GetArticleId>();
    }
}