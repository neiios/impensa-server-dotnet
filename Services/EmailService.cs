using Impensa.Models;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;

namespace Impensa.Services;

public class EmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendWelcomeEmail(User user)
    {
        var deletionEmail = $"""
                             Dear {user.Username},

                             Welcome to Impensa, your go-to solution for expense tracking!

                             We are thrilled to have you on board.
                             With Impensa, you can effortlessly track, manage,
                             and analyze your expenses to make smarter financial decisions.

                             To get started:
                             - Log in to your account.
                             - Begin by adding your first expense.

                             Happy expense tracking!

                             Warm regards,
                             The Impensa Team
                             """;
        const string subject = "Impensa - Welcome. Bienvenue. Willkommen. Bienvenido.";

        await SendEmail(subject, deletionEmail, user.Email);
    }

    public async Task SendDeletionEmail(User user)
    {
        var deletionEmail = $"""
                             Dear {user.Username},

                             We are sorry to see you go.

                             Your Impensa account and all associated data have been successfully deleted.
                             We value your privacy and ensure that no traces of your data remain with us.

                             If you change your mind in the future, know that we're always here to help you manage your expenses.
                             Until then, we wish you all the best in your future endeavors.

                             Warm regards,
                             The Impensa Team

                             """;
        const string subject = "Goodbye from Impensa. We're Here if You Return!";

        await SendEmail(subject, deletionEmail, user.Email);
    }

    private async Task SendEmail(string subject, string textPart, string userEmail)
    {
        var apiKey = Environment.GetEnvironmentVariable("MAILJET_API_KEY");
        var secretKey = Environment.GetEnvironmentVariable("MAILJET_SECRET_KEY");

        var client = new MailjetClient(
            apiKey,
            secretKey
        );

        var email = new TransactionalEmailBuilder()
            .WithFrom(new SendContact("impensa.official@gmail.com"))
            .WithSubject(subject)
            .WithTextPart(textPart)
            .WithTo(new SendContact(userEmail))
            .Build();

        await client.SendTransactionalEmailAsync(email);
    }
}
