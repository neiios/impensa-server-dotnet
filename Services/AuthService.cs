using System.Security.Claims;
using Impensa.DTOs.Users;
using Impensa.Models;
using Impensa.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Impensa.Services;

public class AuthService(
    AppDbContext dbctx,
    IDefaultCategoriesService defaultCategoriesService,
    IEmailService emailService)
{
    private static Task GenerateLocalCookie(HttpContext ctx, Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var claimsIdentity = new ClaimsIdentity(claims, "cookie");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        return ctx.SignInAsync("cookie", claimsPrincipal);
    }

    public async Task<User> CreateLocalUser(HttpContext ctx, UserInfoRequestDto requestDto)
    {
        var user = new User
        {
            Username = requestDto.Username,
            Email = requestDto.Email,
            Currency = requestDto.Currency,
            Password = BCrypt.Net.BCrypt.HashPassword(requestDto.Password)
        };

        try
        {
            dbctx.Users.Add(user);
            await dbctx.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex.Message);
            throw new Exception("Couldn't create a user. The email is already taken.");
        }

        await defaultCategoriesService.CreateDefaultCategoriesForUser(user.Id);
        await emailService.SendWelcomeEmail(user);
        await GenerateLocalCookie(ctx, user.Id);

        return user;
    }

    public async Task<User> CreateGithubUserOrConvertToLocalCookie(HttpContext ctx, ClaimsPrincipal principal,
        string cookieId)
    {
        var githubId = ulong.Parse(cookieId);
        var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var username = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        var githubUser = await dbctx.GithubUsers.FirstOrDefaultAsync(u => u.GithubId == githubId);
        if (githubUser != null)
        {
            await GenerateLocalCookie(ctx, githubUser.UserId);
            var oldUser = await dbctx.Users.FirstOrDefaultAsync(u => u.Id == githubUser.UserId);
            return oldUser!;
        }

        // if github user doesn't exist, check if email is registered
        var user = await dbctx.Users.FirstOrDefaultAsync(u => u.Email == email);
        // if email is registered, associate existing account with github
        if (user != null)
        {
            var newGithubUser = new GithubUser
            {
                UserId = user.Id,
                GithubId = githubId
            };
            dbctx.GithubUsers.Add(newGithubUser);
            await dbctx.SaveChangesAsync();

            await GenerateLocalCookie(ctx, newGithubUser.UserId);
            return user;
        }
        // else create new account with github
        else
        {
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = username!,
                Email = email!,
                Currency = "USD",
            };
            var newGithubUser = new GithubUser
            {
                UserId = newUser.Id,
                GithubId = githubId
            };
            dbctx.Users.Add(newUser);
            dbctx.GithubUsers.Add(newGithubUser);
            await dbctx.SaveChangesAsync();

            await defaultCategoriesService.CreateDefaultCategoriesForUser(newUser.Id);
            await emailService.SendWelcomeEmail(newUser);
            await GenerateLocalCookie(ctx, newUser.Id);

            return newUser;
        }
    }
}
