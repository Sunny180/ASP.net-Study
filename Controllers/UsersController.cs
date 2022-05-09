using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using UserApi.Models;
using ArticleApi.Models;
using JwtAuthDemo.Helpers;
using Microsoft.AspNetCore.Authorization;
using Filter;

namespace UserApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UsersController(UserContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        // POST api/users/login
        [AllowAnonymous]
        [HttpPost, Route("login")]
        public async Task<ResponseFormat<string>> Login(Login login)
        {
            ResponseFormat<string> resp = new ResponseFormat<string>();
            string sqlStr1 = @"
            SELECT [user].[Id]
                , [user].[Name]
                , [user].[Role_Id] AS [RoleId] 
            FROM [F62ND_Test].[dbo].[UserChing] AS [user] 
            INNER JOIN [F62ND_Test].[dbo].[RoleChing] AS [role] 
            ON [user].[Role_Id] = [role].[Id] 
            WHERE [user].[Account]= @Account 
            AND [user].[Password]= @Password";
            string sqlStr2 = @"
                IF EXISTS 
                    (SELECT [token].[User_Id] AS [Id] 
                    FROM [F62ND_Test].[dbo].[UserChing] AS [user] 
                    INNER JOIN [F62ND_Test].[dbo].[TokenChing] AS [token] 
                    ON [user].[Id] = [token].[User_Id] 
                    WHERE [user].[Id] = @Id)
                BEGIN
                    UPDATE [F62ND_Test].[dbo].[TokenChing] 
                    SET [Token] = @Token
                        , [UpdateTime] = CURRENT_TIMESTAMP 
                    WHERE [User_Id] = @Id 
                END
                ELSE
                BEGIN
                    INSERT INTO [F62ND_Test].[dbo].[TokenChing] (
                        [User_Id]
                        , [Token]
                        , [UpdateTime])
                    VALUES (@Id
                        , @Token
                        , CURRENT_TIMESTAMP) 
                END";
            SqlParameter[] sqlParameters = new[]
            {
                new SqlParameter("@Account", login.Account ),
                new SqlParameter("@Password", login.Password )
            };
            try
            {
                List<User> rows = await _context.User.FromSqlRaw(sqlStr1, sqlParameters).ToListAsync();
                if (rows.Count != 1)
                {
                    resp.StatusCode = Status.incorrect_account_or_password;
                    resp.Message = nameof(Status.incorrect_account_or_password);
                    resp.Data = "";
                }
                else
                {
                    JwtHelpers jwtHelpers = new JwtHelpers(_configuration);
                    User user = new User();
                    user.Id = rows[0].Id;
                    user.RoleId = rows[0].RoleId;
                    user.Name = rows[0].Name;
                    string jwtToken = jwtHelpers.GenerateToken(user);
                    SqlParameter[] sqlParameters2 = new[]
                    {
                        new SqlParameter("@Id", rows[0].Id),
                        new SqlParameter("@Token", jwtToken)
                    };
                    await _context.Database.ExecuteSqlRawAsync(sqlStr2, sqlParameters2);
                    resp.StatusCode = Status.success;
                    resp.Message = nameof(Status.success);
                    resp.Data = jwtToken;
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


        // POST api/users/logout
        [TypeFilter(typeof(AuthorizationFilter))]
        [HttpPost, Route("logout")]
        public async Task<ResponseFormat<string>> Logout()
        {
            int userId = (int)HttpContext.Items["id"];
            ResponseFormat<string> resp = new ResponseFormat<string>();
            string sqlStr1 = @"
            UPDATE [F62ND_Test].[dbo].[TokenChing] 
            SET [Token] = ''
                , [UpdateTime] = CURRENT_TIMESTAMP
            WHERE [User_Id] = @Id";
            SqlParameter[] sqlParameters = new[]
           {
                new SqlParameter("@Id", userId)
            };
            try
            {
                await _context.Database.ExecuteSqlRawAsync(sqlStr1, sqlParameters);
                resp.StatusCode = Status.success;
                resp.Message = nameof(Status.success);
                resp.Data = "";
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


