using ImpensaCore.Models;

namespace ImpensaCore.Services;

public interface IEmailService
{
    Task SendWelcomeEmail(User user);
    Task SendDeletionEmail(User user);
    Task SendPasswordResetEmail(User user, string token);
    Task SendPasswordResetConfirmationEmail(User user);
}
