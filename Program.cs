using Impensa.Repositories;
using Impensa.Services;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING"))
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
        o.ClientId = Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID")!;
        o.ClientSecret = Environment.GetEnvironmentVariable("GITHUB_CLIENT_SECRET")!;
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
