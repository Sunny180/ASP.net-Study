using Microsoft.EntityFrameworkCore;
using JwtAuthDemo.Helpers;
using ArticleApi.Models;
using AuthorApi.Models;
using UserApi.Models;

var builder = WebApplication.CreateBuilder(args);
string cors = builder.Configuration.GetValue<string>("Cors", "AllowedOriginsList");
// Add services to the container.

//return Data 維持大小寫
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
        {
            builder.WithOrigins(cors);
        });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ArticleContext>(opt =>
            opt.UseSqlServer(builder.Configuration.GetConnectionString("DBconn")));
builder.Services.AddDbContext<AuthorContext>(opt =>
            opt.UseSqlServer(builder.Configuration.GetConnectionString("DBconn")));
builder.Services.AddDbContext<UserContext>(opt =>
            opt.UseSqlServer(builder.Configuration.GetConnectionString("DBconn")));

builder.Services.AddSingleton<JwtHelpers>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddAuthentication();

//filter全域
// builder.Services.AddMvc(option =>{
//     option.Filters.Add(typeof(AuthorizationFilter));
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
