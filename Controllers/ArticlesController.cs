using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ArticleApi.Models;
using Microsoft.AspNetCore.Authorization;
using Filter;
using System.Text;

namespace ArticleApi.Controllers
{
    // [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly ArticleContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ArticlesController(ArticleContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: api/Articles
        [HttpGet]
        // [AllowAnonymous]
        public async Task<ResponseFormat<List<GetArticleOverview>>> GetArticles(int userId, string? keyword)
        {
            List<GetArticleOverview> empty = new List<GetArticleOverview>();
            ResponseFormat<List<GetArticleOverview>> resp = new ResponseFormat<List<GetArticleOverview>>();
            StringBuilder sqlStr = new StringBuilder(@"
                            SELECT [article].[Id]
                                ,[article].[Title]
                                ,[article].[Content]
                                ,[article].[User_Id] AS [UserId]
                                ,[user].[Name] 
                            FROM [F62ND_Test].[dbo].[ArticleChing] AS [article] 
                            INNER JOIN [F62ND_Test].[dbo].[UserChing] AS [user] 
                            ON [article].[User_Id] = [user].[Id]");
            if (userId == 0 && keyword == null)
            {
                sqlStr.Append("");
            }
            else if (userId == 0 && keyword != null)
            {
                sqlStr.Append(@$"WHERE [article].[Title] LIKE '%{keyword}%' 
                                OR [user].[Name] LIKE '%{keyword}%'");
            }
            else if (userId != 0 && keyword == null)
            {
                sqlStr.Append($"WHERE [user].[Id] = {userId}");
            }
            else
            {
                sqlStr.Append($@"WHERE [user].[Id] = {userId} 
                                AND [article].[Title] LIKE '%{keyword}%'");
            }
            try
            {
                List<GetArticleOverview> result = await _context.GetArticleOverview.FromSqlRaw(sqlStr.ToString()).ToListAsync();
                resp.StatusCode = Status.success;
                resp.Message = nameof(Status.success);
                resp.Data = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                resp.StatusCode = Status.system_fail;
                resp.Message = nameof(Status.system_fail);
                resp.Data = empty;
            }
            return resp;
        }

        // GET: api/Articles/5
        [HttpGet("{id}")]
        // [AllowAnonymous]
        public async Task<ResponseFormat<GetArticleDetail>> GetArticle(int id)
        {
            GetArticleDetail empty = new GetArticleDetail();
            ResponseFormat<GetArticleDetail> resp = new ResponseFormat<GetArticleDetail>();
            string sqlStr = @$"
            SELECT [User].[Name]
                ,[Article].[Id]
                ,[Article].[Title]
                ,[Article].[Content]
                ,[Article].[User_Id] AS [UserId]
                ,[Article].[CreateTime]
                ,[Article].[UpdateTime]
            FROM [F62ND_Test].[dbo].[ArticleChing] AS [Article]
            INNER JOIN [F62ND_Test].[dbo].[UserChing] AS [User] 
            ON [Article].[User_Id] = [User].[Id]
            WHERE [Article].[Id] = {id}";
            try
            {
                GetArticleDetail? result = await _context.GetArticleDetail.FromSqlRaw(sqlStr).SingleOrDefaultAsync();
                resp.StatusCode = Status.success;
                resp.Message = nameof(Status.success);
                resp.Data = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                resp.StatusCode = Status.system_fail;
                resp.Message = nameof(Status.system_fail);
                resp.Data = empty;
            }
            return resp;
        }

        // PUT: api/Articles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [TypeFilter(typeof(AuthorizationFilter))]
        [HttpPut("{id}")]
        public async Task<ResponseFormat<string>> UpdateArticle(int id, PutArticle putArticle)
        {
            int roleId = (int)HttpContext.Items["roleId"];
            int userId = (int)HttpContext.Items["id"];
            Console.WriteLine(roleId);
            ResponseFormat<string> resp = new ResponseFormat<string>();
            string sqlStr1 = @"
                SELECT [Id] 
                FROM [F62ND_Test].[dbo].[ArticleChing] 
                WHERE [Id] = @Id";
            string sqlStr2 = "";
            if (roleId == 1)
            {
                sqlStr2 = @"
                UPDATE [F62ND_Test].[dbo].[ArticleChing] 
                SET [Title]=@Title
                    ,[Content]=@Content
                    ,[AdminId]=@AdminId 
                WHERE [Id] = @Id";
            }
            else if (roleId == 2)
            {
                sqlStr2 = @"
                UPDATE [F62ND_Test].[dbo].[ArticleChing] 
                SET [Title]=@Title
                    ,[Content]=@Content
                    ,[AdminId]= @AdminId 
                WHERE [Id] = @Id
                AND [User_Id] = @UserId";
            }
            else
            {
                resp.StatusCode = Status.permission_denied;
                resp.Message = nameof(Status.permission_denied);
                resp.Data = "";
            }
            SqlParameter[] sqlParameters = new[]
            {
                new SqlParameter("@Title",putArticle.Title),
                new SqlParameter("@Content",putArticle.Content),
                new SqlParameter("@AdminId",putArticle.AdminId),
                new SqlParameter("@Id", id ),
                new SqlParameter("@UserId", userId )
            };
            try
            {
                List<GetArticleId> rows = await _context.GetArticleId.FromSqlRaw(sqlStr1, sqlParameters).ToListAsync();
                if (rows.Count != 1)
                {
                    resp.StatusCode = Status.data_not_found;
                    resp.Message = nameof(Status.data_not_found);
                    resp.Data = "";
                }
                else
                {
                    await _context.Database.ExecuteSqlRawAsync(sqlStr2, sqlParameters);
                    resp.StatusCode = Status.success;
                    resp.Message = nameof(Status.success);
                    resp.Data = "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                resp.StatusCode = Status.system_fail;
                resp.Message = nameof(Status.system_fail);
                resp.Data = "";
            }
            return resp;
        }


        // POST: api/Articles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [TypeFilter(typeof(AuthorizationFilter))]
        [HttpPost]
        public async Task<ResponseFormat<string>> PostArticle(PostArticle postArticle)
        {
            ResponseFormat<string> resp = new ResponseFormat<string>();
            int roleId = (int)HttpContext.Items["roleId"];
            int userId = (int)HttpContext.Items["id"];
            string sqlStr = @"
                INSERT INTO [F62ND_Test].[dbo].[ArticleChing] (
                    [Title]
                    , [Content]
                    , [User_Id] 
                    , [AdminId]
                    , [UpdateTime]) 
                VALUES (@Title
                    , @Content
                    , @UserId
                    , @AdminId
                    , CURRENT_TIMESTAMP)";
            SqlParameter[] sqlParameters = new[]
            {
                new SqlParameter("@Title", postArticle.Title ),
                new SqlParameter("@Content", postArticle.Content ),
                new SqlParameter("@UserId", postArticle.UserId ),
                new SqlParameter("@AdminId", postArticle.AdminId )
            };
            try
            {
                if (postArticle.UserId != userId)
                {
                    resp.StatusCode = Status.permission_denied;
                    resp.Message = nameof(Status.permission_denied);
                    resp.Data = "";
                }
                else
                {
                    await _context.Database.ExecuteSqlRawAsync(sqlStr, sqlParameters);
                    resp.StatusCode = Status.success;
                    resp.Message = nameof(Status.success);
                    resp.Data = "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                resp.StatusCode = Status.system_fail;
                resp.Message = nameof(Status.system_fail);
                resp.Data = "";
            }
            return resp;
        }

        // DELETE: api/Articles/5
        [TypeFilter(typeof(AuthorizationFilter))]
        [HttpDelete("{id}")]
        public async Task<ResponseFormat<string>> DeleteArticle(int id)
        {

            int roleId = (int)HttpContext.Items["roleId"];
            int userId = (int)HttpContext.Items["id"];
            ResponseFormat<string> resp = new ResponseFormat<string>();
            string sqlStr1 = @"
                SELECT [Id] 
                FROM [F62ND_Test].[dbo].[ArticleChing] 
                WHERE [Id] = @Id";
            string sqlStr2 = "";
            if (roleId == 1)
            {
                sqlStr2 = @"
                DELETE 
                FROM [F62ND_Test].[dbo].[ArticleChing] 
                WHERE [Id] = @Id";
            }
            else if (roleId == 2)
            {
                sqlStr2 = @"
                DELETE 
                FROM [F62ND_Test].[dbo].[ArticleChing] 
                WHERE [Id] = @Id
                AND [User_Id] = @UserId";
            }
            else
            {
                resp.StatusCode = Status.permission_denied;
                resp.Message = nameof(Status.permission_denied);
                resp.Data = "";
            }
            SqlParameter[] sqlParameters = new[]
            {
                    new SqlParameter("@Id", id ),
                    new SqlParameter("@UserId", userId )
                };
            try
            {
                GetArticleId? ArticleId = await _context.GetArticleId.FromSqlRaw(sqlStr1, sqlParameters).SingleOrDefaultAsync();
                if (ArticleId == null)
                {
                    resp.StatusCode = Status.data_not_found;
                    resp.Message = nameof(Status.data_not_found);
                    resp.Data = "";
                }
                else
                {
                    await _context.Database.ExecuteSqlRawAsync(sqlStr2, sqlParameters);
                    resp.StatusCode = Status.success;
                    resp.Message = nameof(Status.success);
                    resp.Data = "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                resp.StatusCode = Status.system_fail;
                resp.Message = nameof(Status.system_fail);
                resp.Data = "";
            }
            return resp;
        }
    }
}
