using Microsoft.EntityFrameworkCore;
using Practice.Data;
using Practice.Repositories;
using Practice.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//Database
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Repos
builder.Services.AddScoped<IUserRepository , UserRepository>();

//services
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
