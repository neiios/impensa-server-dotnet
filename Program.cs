using Impensa.Repositories;
using Impensa.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention()
);

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie", o =>
    {
        o.Events.OnRedirectToLogin = (ctx) =>
        {
            ctx.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    })
    .AddGitHub("github", o =>
    {
        o.ClientId = configuration["GITHUB_CLIENT_ID"] ?? throw new InvalidOperationException();
        o.ClientSecret = configuration["GITHUB_CLIENT_SECRET"] ?? throw new InvalidOperationException();
        o.SignInScheme = "cookie";
        o.CallbackPath = "/api/v1/auth/github-cb";
        o.Scope.Add("user:email");
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDefaultCategoriesService, DefaultCategoriesService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<UserActivityService>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
