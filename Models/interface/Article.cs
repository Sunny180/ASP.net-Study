namespace ArticleApi.Models
{
    public class GetArticleOverview
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int UserId { get; set; }
        public string? Name { get; set; }
    }

    public class GetArticleDetail
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int UserId { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }

    public class GetArticleId
    {
        public int Id { get; set; }
    }

    public class PutArticle
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int AdminId { get; set; }

    }

    public class PostArticle
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int UserId { get; set; }
        public int AdminId { get; set; }
        public DateTime UpdateTime { get; set; }
    }

}


