using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using UserApi.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ArticleApi.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Filter
{
    public class AuthorizationFilter : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public AuthorizationFilter(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            ResponseFormat<string> resp = new ResponseFormat<string>();
            try
            {
                // //判斷是否跳過授權過濾器
                // if (context.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)
                //     || context.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
                // {
                //     return;
                // }
                string? token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (token == null)
                {
                    resp.StatusCode = Status.token_not_found;
                    resp.Message = nameof(Status.token_not_found);
                    resp.Data = "";
                    context.Result = new JsonResult(resp);
                    return;
                }
                else
                {
                    JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SignKey"]);
                    //驗證密鑰的主體對象，包含密鑰的所有相關訊息 (創建、校驗token，返回ClaimsPrincipal)
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidIssuer = _configuration["JwtSettings:Issuer"],
                        ValidAudience = _configuration["JwtSettings:Issuer"],
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        // 過期時間預設為5分鐘。 如果想立即過期
                        // ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);
                    JwtSecurityToken jwtToken = (JwtSecurityToken)validatedToken;
                    string jsonUser = jwtToken.Claims.First(claim => claim.Type == "Users").Value;
                    User? jwtUser = JsonSerializer.Deserialize<User>(jsonUser);
                    if (jwtUser != null)
                    {
                        // User user = new User();
                        // user.Id = jwtUser.Id;
                        // user.RoleId = jwtUser.RoleId;
                        // user.Name = jwtUser.Name;
                        // context.HttpContext.Items.Add("user", user);
                        context.HttpContext.Items.Add("id", jwtUser.Id);
                        context.HttpContext.Items.Add("roleId", jwtUser.RoleId);
                        context.HttpContext.Items.Add("name", jwtUser.Name);
                    }
                    else
                    {
                        resp.StatusCode = Status.jwtuser_not_exist;
                        resp.Message = nameof(Status.jwtuser_not_exist);
                        resp.Data = "";
                        string response = JsonSerializer.Serialize(resp);
                        context.Result = new JsonResult(resp);
                        return;
                    }
                }
            }
            catch (SecurityTokenExpiredException)
            {
                resp.StatusCode = Status.login_timeout;
                resp.Message = nameof(Status.login_timeout);
                resp.Data = "";
                context.Result = new JsonResult(resp);
                return;
            }
            catch (SecurityTokenValidationException)
            {
                resp.StatusCode = Status.invalid_token;
                resp.Message = nameof(Status.invalid_token);
                resp.Data = "";
                context.Result = new JsonResult(resp);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                resp.StatusCode = Status.system_fail;
                resp.Message = nameof(Status.system_fail);
                resp.Data = "";
                context.Result = new JsonResult(resp);
                return;
            }
        }
    }
}
