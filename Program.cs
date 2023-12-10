using Impensa.Repositories;
using Impensa.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
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
        o.Events.OnRedirectToAuthorizationEndpoint = MakeHttps;
        o.Scope.Add("user:email");
    });

Task MakeHttps(RedirectContext<OAuthOptions> arg)
{
    // When behind a load balancer the redirect URL, which is configured as CallbackPath in the appsettings.json
    // is created as HTTP because the HTTPS request is terminated at the NLB and is forwarded in clear text.

    // The policy of most OAuth IDPs is to disallow clear HTTP redirect URLs.

    if (!arg.RedirectUri.Contains("redirect_uri=https", StringComparison.OrdinalIgnoreCase))
    {
        arg.RedirectUri = arg.RedirectUri.Replace("redirect_uri=http","redirect_uri=https", StringComparison.OrdinalIgnoreCase);
    }

    arg.HttpContext.Response.Redirect(arg.RedirectUri);

    return Task.CompletedTask;
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDefaultCategoriesService, DefaultCategoriesService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<UserActivityService>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

if (!DotNetEnv.Env.GetBool("PRODUCTION"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (DotNetEnv.Env.GetBool("PRODUCTION"))
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.MapFallbackToFile("index.html");

    using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
    var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>().Database;
    logger.LogInformation("Migrating database...");
    while (!db.CanConnect())
    {
        logger.LogInformation("Database not ready yet; waiting...");
        Thread.Sleep(1000);
    }

    try
    {
        serviceScope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
        logger.LogInformation("Database migrated successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
