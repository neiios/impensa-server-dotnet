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
        _emailClientMock = new Mock<IEmailClient>();
        _emailService = new EmailService(_emailClientMock.Object);
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
    
    
    [Fact]
    public async Task SendWelcomeEmail_MissingUsernameInTextPart_ShouldFail()
    {
        var user = new User { Username = "TestUser", Email = "testuser@example.com", Currency = "USD" };

        await _emailService.SendWelcomeEmail(user);

        // Mutation: Verify that the TextPart does not contain the username
        _emailClientMock.Verify(client => client.SendTransactionalEmailAsync(
            It.Is<TransactionalEmail>(email =>
                email.Subject == "Impensa - Welcome. Bienvenue. Willkommen. Bienvenido." &&
                email.To[0].Email == user.Email &&
                !email.TextPart.Contains("Dear TestUser") && // This should fail, as the username is expected
                email.TextPart.Contains("Welcome to Impensa, your go-to solution for expense tracking!")
            )), Times.Once);
    }

    [Fact]
    public async Task SendDeletionEmail_WrongSubject_ShouldFail()
    {
        var user = new User { Username = "TestUser", Email = "testuser@example.com", Currency = "USD" };

        await _emailService.SendDeletionEmail(user);

        // Mutation: Verify that the email is sent with an incorrect subject
        _emailClientMock.Verify(client => client.SendTransactionalEmailAsync(
            It.Is<TransactionalEmail>(email =>
                email.Subject == "Wrong Subject" && // Incorrect subject to trigger a failure
                email.To[0].Email == user.Email &&
                email.TextPart.Contains("We are sorry to see you go.") &&
                email.TextPart.Contains("Your Impensa account and all associated data have been successfully deleted.")
            )), Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetEmail_MissingToken_ShouldFail()
    {
        var user = new User { Username = "TestUser", Email = "testuser@example.com", Currency = "USD" };
        const string token = "123456";

        await _emailService.SendPasswordResetEmail(user, token);

        // Mutation: Verify that the email does not contain the password reset token
        _emailClientMock.Verify(client => client.SendTransactionalEmailAsync(
            It.Is<TransactionalEmail>(email =>
                    email.Subject == "Impensa - Password Reset" &&
                    email.To[0].Email == user.Email &&
                    email.TextPart.Contains("We have received a request to reset your password.") &&
                    !email.TextPart.Contains(token) // This should fail, as the token is expected in the email
            )), Times.Once);
    }

}
