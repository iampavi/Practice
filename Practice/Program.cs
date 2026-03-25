using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Practice.Data;
using Practice.Repositories;
using Practice.Services;
using Practice.Services.Background;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//Database
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Repos
builder.Services.AddScoped<IUserRepository , UserRepository>();

//services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
builder.Services.AddHostedService<EmailBackgroundService>();

//cache
builder.Services.AddMemoryCache();


builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

//Middleware
app.UseMiddleware<Practice.Middleware.ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
