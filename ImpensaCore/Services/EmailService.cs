using ImpensaCore.Clients;
using ImpensaCore.Models;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;

namespace ImpensaCore.Services;

public class EmailService(IConfiguration configuration, IEmailClient emailClient) : IEmailService
{
    public Task SendWelcomeEmail(User user)
    {
        var deletionEmail =
            $"""
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

        return SendEmail(subject, deletionEmail, user.Email);
    }

    public Task SendDeletionEmail(User user)
    {
        var deletionEmail =
            $"""
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

        return SendEmail(subject, deletionEmail, user.Email);
    }

    public Task SendPasswordResetEmail(User user, string token)
    {
        var resetEmail =
            $"""
             Dear {user.Username},

             We have received a request to reset your password.
             If you did not make this request, you can safely ignore this email.

             To reset your password, enter this code on a password reset page:
             {token}


             Warm regards,
             The Impensa Team
             """;
        const string subject = "Impensa - Password Reset";

        return SendEmail(subject, resetEmail, user.Email);
    }

    public Task SendPasswordResetConfirmationEmail(User user)
    {
        var resetConfirmEmail = $"""
                                 Dear {user.Username},

                                 Your password has been successfully reset.

                                 If you did not make this request, please contact us immediately.

                                 Warm regards,
                                 The Impensa Team
                                 """;
        const string subject = "Impensa - Password Reset Confirmation";

        return SendEmail(subject, resetConfirmEmail, user.Email);
    }

    private async Task SendEmail(string subject, string textPart, string userEmail)
    {
        var email = new TransactionalEmailBuilder()
            .WithFrom(new SendContact("impensa.official@gmail.com"))
            .WithSubject(subject)
            .WithTextPart(textPart)
            .WithTo(new SendContact(userEmail))
            .Build();

        await emailClient.SendTransactionalEmailAsync(email);
    }
}