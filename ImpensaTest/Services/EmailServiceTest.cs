using ImpensaCore.Clients;
using ImpensaCore.Models;
using ImpensaCore.Services;
using Mailjet.Client.TransactionalEmails;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ImpensaTest.Services;

public class EmailServiceTests
{
    private readonly Mock<IEmailClient> _emailClientMock;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        Mock<IConfiguration> configurationMock = new();
        _emailClientMock = new Mock<IEmailClient>();
        _emailService = new EmailService(configurationMock.Object, _emailClientMock.Object);
    }

    [Fact]
    public async Task SendWelcomeEmail_ShouldSendCorrectEmail()
    {
        var user = new User { Username = "TestUser", Email = "testuser@example.com", Currency = "USD" };

        await _emailService.SendWelcomeEmail(user);

        _emailClientMock.Verify(client => client.SendTransactionalEmailAsync(
            It.Is<TransactionalEmail>(email =>
                email.Subject == "Impensa - Welcome. Bienvenue. Willkommen. Bienvenido." &&
                email.To[0].Email == user.Email &&
                email.TextPart.Contains("Dear TestUser") &&
                email.TextPart.Contains("Welcome to Impensa, your go-to solution for expense tracking!")
            )), Times.Once);
    }

    [Fact]
    public async Task SendDeletionEmail_ShouldSendCorrectEmail()
    {
        var user = new User { Username = "TestUser", Email = "testuser@example.com", Currency = "USD" };

        await _emailService.SendDeletionEmail(user);

        _emailClientMock.Verify(client => client.SendTransactionalEmailAsync(
            It.Is<TransactionalEmail>(email =>
                email.Subject == "Goodbye from Impensa. We're Here if You Return!" &&
                email.To[0].Email == user.Email &&
                email.TextPart.Contains("We are sorry to see you go.") &&
                email.TextPart.Contains("Your Impensa account and all associated data have been successfully deleted.")
            )), Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetEmail_ShouldSendCorrectEmailWithToken()
    {
        var user = new User { Username = "TestUser", Email = "testuser@example.com", Currency = "USD" };
        const string token = "123456";

        await _emailService.SendPasswordResetEmail(user, token);

        _emailClientMock.Verify(client => client.SendTransactionalEmailAsync(
            It.Is<TransactionalEmail>(email =>
                email.Subject == "Impensa - Password Reset" &&
                email.To[0].Email == user.Email &&
                email.TextPart.Contains("We have received a request to reset your password.") &&
                email.TextPart.Contains("To reset your password, enter this code on a password reset page:") &&
                email.TextPart.Contains(token)
            )), Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetConfirmationEmail_ShouldSendCorrectEmail()
    {
        var user = new User { Username = "TestUser", Email = "testuser@example.com", Currency = "USD" };

        await _emailService.SendPasswordResetConfirmationEmail(user);

        _emailClientMock.Verify(client => client.SendTransactionalEmailAsync(
            It.Is<TransactionalEmail>(email =>
                email.Subject == "Impensa - Password Reset Confirmation" &&
                email.To[0].Email == user.Email &&
                email.TextPart.Contains("Your password has been successfully reset.")
            )), Times.Once);
    }
}