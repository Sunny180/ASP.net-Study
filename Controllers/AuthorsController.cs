using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AuthorApi.Models;
using ArticleApi.Models;
using Filter;

namespace AuthorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly AuthorContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthorsController(AuthorContext context, IConfiguration _configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: api/Authors
        [TypeFilter(typeof(AuthorizationFilter))]
        [HttpGet]
        public async Task<ResponseFormat<List<GetAuthorOverview>>> GetAuthors()
        {
            int roleId = (int)HttpContext.Items["roleId"];
            List<GetAuthorOverview> empty = new List<GetAuthorOverview>();
            ResponseFormat<List<GetAuthorOverview>> resp = new ResponseFormat<List<GetAuthorOverview>>();
            string sqlStr = @"
                        SELECT 
                            [Id]
                            ,[Name] 
                        FROM [F62ND_Test].[dbo].[UserChing]";
            try
            {
                if (roleId != 1)
                {
                    resp.StatusCode = Status.permission_denied;
                    resp.Message = nameof(Status.permission_denied);
                    resp.Data = empty;
                }
                else
                {
                    List<GetAuthorOverview> result = await _context.GetAuthorOverview.FromSqlRaw(sqlStr).ToListAsync();
                    if (result.Count > 0)
                    {
                        resp.StatusCode = Status.success;
                        resp.Message = nameof(Status.success);
                        resp.Data = result;
                    }
                    else
                    {
                        resp.StatusCode = Status.data_not_found;
                        resp.Message = nameof(Status.data_not_found);
                        resp.Data = empty;
                    }
                }
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

        // PUT: api/Authors/5
        [TypeFilter(typeof(AuthorizationFilter))]
        [HttpPut("{id}")]
        public async Task<ResponseFormat<string>> UpdateAuthors(int id, PutAuthor putAuthor)
        {
            int roleId = (int)HttpContext.Items["roleId"];
            ResponseFormat<string> resp = new ResponseFormat<string>();
            string sqlStr1 = @"
                        SELECT [Id] 
                        FROM [F62ND_Test].[dbo].[UserChing] 
                        WHERE [Id] = @AuthorId";
            string sqlStr2 = @"
                        UPDATE [F62ND_Test].[dbo].[UserChing] 
                        SET [Name]= @Name
                            , [AdminId]= @AdminId 
                        WHERE [Id] = @AuthorId";
            SqlParameter[] sqlParameters = new[]
            {
                new SqlParameter("@Name",putAuthor.Name),
                new SqlParameter("@AdminId",putAuthor.AdminId),
                new SqlParameter("@AuthorId", id )
            };
            try
            {
                if (roleId != 1)
                {
                    resp.StatusCode = Status.permission_denied;
                    resp.Message = nameof(Status.permission_denied);
                    resp.Data = "";
                }
                else
                {
                    List<GetAuthorId> rows = await _context.GetAuthorId.FromSqlRaw(sqlStr1, sqlParameters).ToListAsync();
                    if (rows.Count == 1)
                    {
                        await _context.Database.ExecuteSqlRawAsync(sqlStr2, sqlParameters);
                        resp.StatusCode = Status.success;
                        resp.Message = nameof(Status.success);
                        resp.Data = "";
                    }
                    else
                    {
                        resp.StatusCode = Status.data_not_found;
                        resp.Message = nameof(Status.data_not_found);
                        resp.Data = "";
                    }
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
        public async Task<ResponseFormat<string>> DeleteAuthor(int id)
        {
            int roleId = (int)HttpContext.Items["roleId"];
            ResponseFormat<string> resp = new ResponseFormat<string>();
            string sqlStr1 = @"
                        SELECT [Id] 
                        FROM [F62ND_Test].[dbo].[UserChing] 
                        WHERE [Id] = @Id";
            string sqlStr2 = "";
            if (roleId != 1)
            {
                resp.StatusCode = Status.permission_denied;
                resp.Message = nameof(Status.permission_denied);
                resp.Data = "";
            }
            else
            {
                sqlStr2 = @"
                        DELETE [article] 
                        FROM [F62ND_Test].[dbo].[UserChing] AS [user]
                        INNER JOIN [F62ND_Test].[dbo].[ArticleChing] AS [article]
                        ON [user].[Id] = [article].[User_Id] 
                        WHERE [user].[Id] = @Id;
                        DELETE 
                        FROM [F62ND_Test].[dbo].[UserChing] 
                        WHERE [Id] = @Id;
                        DELETE 
                        FROM [F62ND_Test].[dbo].[TokenChing] 
                        WHERE [User_Id] = @Id";
            }
            SqlParameter[] sqlParameters = new[]
            {
                new SqlParameter("@Id", id )
            };
            try
            {
                GetAuthorId? AuthorId = await _context.GetAuthorId.FromSqlRaw(sqlStr1, sqlParameters).SingleOrDefaultAsync();
                if (AuthorId == null)
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
            catch (Exception)
            {
                resp.StatusCode = Status.system_fail;
                resp.Message = nameof(Status.system_fail);
                resp.Data = "";
            }
            return resp;
        }
    }
}