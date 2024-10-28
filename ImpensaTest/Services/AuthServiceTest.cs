using System.Security.Claims;
using ImpensaCore.DTOs.Users;
using ImpensaCore.Models;
using ImpensaCore.Repositories;
using ImpensaCore.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ImpensaTest.Services;

public class AuthServiceTests
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IDefaultCategoriesService> _defaultCategoriesServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly AuthService _authService;
    private readonly DefaultHttpContext _httpContext;
    private readonly Mock<IAuthenticationService> _authenticationServiceMock;

    public AuthServiceTests()
    {
        _dbContext = GetInMemoryDbContext();
        _defaultCategoriesServiceMock = new Mock<IDefaultCategoriesService>();
        _emailServiceMock = new Mock<IEmailService>();
        _authService = new AuthService(_dbContext, _defaultCategoriesServiceMock.Object, _emailServiceMock.Object);

        _httpContext = new DefaultHttpContext();
        _authenticationServiceMock = new Mock<IAuthenticationService>();
        _authenticationServiceMock.Setup(s => s.SignInAsync(
            It.IsAny<HttpContext>(),
            It.IsAny<string>(),
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<AuthenticationProperties>()
        )).Returns(Task.CompletedTask);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_authenticationServiceMock.Object);
        _httpContext.RequestServices = serviceCollection.BuildServiceProvider();
    }

    private static AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        var context = new AppDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(string email, string username)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, username)
        }));
    }

    [Fact]
    public async Task CreateLocalUser_CreatesUserSuccessfully()
    {
        var requestDto = new UserInfoRequestDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Currency = "USD",
            Password = "password123"
        };

        var user = await _authService.CreateLocalUser(_httpContext, requestDto);

        Assert.NotNull(user);
        Assert.Equal(requestDto.Username, user.Username);
        Assert.Equal(requestDto.Email, user.Email);
        Assert.Equal(requestDto.Currency, user.Currency);
        Assert.NotNull(user.Password);

        var savedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(savedUser);

        _defaultCategoriesServiceMock.Verify(s => s.CreateDefaultCategoriesForUser(user.Id), Times.Once);
        _emailServiceMock.Verify(s => s.SendWelcomeEmail(user), Times.Once);
        _authenticationServiceMock.Verify(s => s.SignInAsync(
            It.IsAny<HttpContext>(),
            "cookie",
            It.IsAny<ClaimsPrincipal>(),
            null
        ), Times.Once);
    }

    [Fact]
    public async Task CreateLocalUser_EmailAlreadyExists_ThrowsException()
    {
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "existinguser",
            Email = "test@example.com",
            Currency = "USD",
            Password = "hashedpassword"
        };
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        var requestDto = new UserInfoRequestDto
        {
            Username = "newuser",
            Email = "test@example.com",
            Currency = "USD",
            Password = "password123"
        };

        var exception = await Assert.ThrowsAsync<Exception>(async () =>
        {
            await _authService.CreateLocalUser(_httpContext, requestDto);
        });

        Assert.Equal("Couldn't create a user. The email is already taken.", exception.Message);
    }

    [Fact]
    public async Task CreateGithubUserOrConvertToLocalCookie_GithubUserExists_SignsInLocalUser()
    {
        var userId = Guid.NewGuid();
        var githubId = 1234567890UL;
        var user = new User
        {
            Id = userId,
            Username = "existinguser",
            Email = "existing@example.com",
            Currency = "USD"
        };
        var githubUser = new GithubUser { UserId = userId, GithubId = githubId };

        _dbContext.Users.Add(user);
        _dbContext.GithubUsers.Add(githubUser);
        await _dbContext.SaveChangesAsync();

        var principal = CreateClaimsPrincipal(user.Email, user.Username);
        var returnedUser =
            await _authService.CreateGithubUserOrConvertToLocalCookie(_httpContext, principal, githubId.ToString());

        Assert.Equal(userId, returnedUser.Id);
        _authenticationServiceMock.Verify(s => s.SignInAsync(
            It.IsAny<HttpContext>(),
            "cookie",
            It.IsAny<ClaimsPrincipal>(),
            null
        ), Times.Once);
    }

    [Fact]
    public async Task CreateGithubUserOrConvertToLocalCookie_EmailExists_AssociatesGithubUserAndSignsIn()
    {
        var userId = Guid.NewGuid();
        var githubId = 1234567890UL;
        var email = "existing@example.com";
        var username = "existinguser";
        var user = new User { Id = userId, Username = username, Email = email, Currency = "USD" };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var principal = CreateClaimsPrincipal(email, username);
        var returnedUser =
            await _authService.CreateGithubUserOrConvertToLocalCookie(_httpContext, principal, githubId.ToString());

        Assert.Equal(userId, returnedUser.Id);

        var githubUser = await _dbContext.GithubUsers.FirstOrDefaultAsync(u => u.GithubId == githubId);
        Assert.NotNull(githubUser);
        Assert.Equal(userId, githubUser.UserId);

        _authenticationServiceMock.Verify(s => s.SignInAsync(
            It.IsAny<HttpContext>(),
            "cookie",
            It.IsAny<ClaimsPrincipal>(),
            null
        ), Times.Once);
    }

    [Fact]
    public async Task CreateGithubUserOrConvertToLocalCookie_NewUser_CreatesUserAndSignsIn()
    {
        var githubId = 1234567890UL;
        var email = "newuser@example.com";
        var username = "newuser";
        var principal = CreateClaimsPrincipal(email, username);

        var returnedUser =
            await _authService.CreateGithubUserOrConvertToLocalCookie(_httpContext, principal, githubId.ToString());

        Assert.NotNull(returnedUser);
        Assert.Equal(username, returnedUser.Username);
        Assert.Equal(email, returnedUser.Email);

        var savedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == returnedUser.Id);
        Assert.NotNull(savedUser);

        var githubUser = await _dbContext.GithubUsers.FirstOrDefaultAsync(u => u.GithubId == githubId);
        Assert.NotNull(githubUser);
        Assert.Equal(returnedUser.Id, githubUser.UserId);

        _defaultCategoriesServiceMock.Verify(s => s.CreateDefaultCategoriesForUser(returnedUser.Id), Times.Once);
        _emailServiceMock.Verify(s => s.SendWelcomeEmail(returnedUser), Times.Once);
        _authenticationServiceMock.Verify(s => s.SignInAsync(
            It.IsAny<HttpContext>(),
            "cookie",
            It.IsAny<ClaimsPrincipal>(),
            null
        ), Times.Once);
    }

    [Fact]
    public async Task GeneratePasswordResetToken_SetsTokenAndExpiration()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            Currency = "USD"
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var token = await _authService.GeneratePasswordResetToken(user);

        Assert.NotNull(token);
        Assert.Equal(token, user.PassswordResetToken);
        Assert.True(user.PasswordResetTokenExpiresAt > DateTime.UtcNow);

        var updatedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.Equal(token, updatedUser!.PassswordResetToken);
        Assert.Equal(user.PasswordResetTokenExpiresAt, updatedUser.PasswordResetTokenExpiresAt);
    }

    [Fact]
    public void ValidatePasswordResetToken_ValidToken_ReturnsTrue()
    {
        var token = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            Currency = "USD",
            PassswordResetToken = token,
            PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        var isValid = _authService.ValidatePasswordResetToken(user, token);
        Assert.True(isValid);
    }

    [Fact]
    public void ValidatePasswordResetToken_InvalidToken_ReturnsFalse()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            Currency = "USD",
            PassswordResetToken = "validtoken",
            PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        var isValid = _authService.ValidatePasswordResetToken(user, "invalidtoken");
        Assert.False(isValid);
    }

    [Fact]
    public void ValidatePasswordResetToken_ExpiredToken_ReturnsFalse()
    {
        var token = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            Currency = "USD",
            PassswordResetToken = token,
            PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(-30)
        };

        var isValid = _authService.ValidatePasswordResetToken(user, token);
        Assert.False(isValid);
    }


    [Fact]
    public async Task CreateLocalUser_EmailAlreadyExists_IncorrectExceptionMessage_ShouldFail()
    {
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "existinguser",
            Email = "test@example.com",
            Currency = "USD",
            Password = "hashedpassword"
        };
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        var requestDto = new UserInfoRequestDto
        {
            Username = "newuser",
            Email = "test@example.com",
            Currency = "USD",
            Password = "password123"
        };

        var exception = await Assert.ThrowsAsync<Exception>(async () =>
        {
            await _authService.CreateLocalUser(_httpContext, requestDto);
        });

        Assert.Equal("Email is already in use.",
            exception.Message);
    }
    
    [Fact]
    public async Task CreateGithubUserOrConvertToLocalCookie_DoesNotCreateGithubUser_ShouldFail()
    {
        var githubId = 1234567890UL;
        var email = "newuser@example.com";
        var username = "newuser";
        var principal = CreateClaimsPrincipal(email, username);

        // Mutation: Avoid adding the new GitHub user association in the database
        var returnedUser = await _authService.CreateGithubUserOrConvertToLocalCookie(_httpContext, principal, githubId.ToString());

        var githubUser = await _dbContext.GithubUsers.FirstOrDefaultAsync(u => u.GithubId == githubId);

        Assert.Null(githubUser); // This assertion should fail as the GitHub user should be created
    }

    [Fact]
    public async Task GeneratePasswordResetToken_DoesNotSetExpiration_ShouldFail()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            Currency = "USD"
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var token = await _authService.GeneratePasswordResetToken(user);

        // Mutation: Assert that the expiration time is not set, which should fail
        Assert.Null(user.PasswordResetTokenExpiresAt); // Should fail since expiration should be set
    }

    
}