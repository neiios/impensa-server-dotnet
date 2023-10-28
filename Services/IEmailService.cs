using Impensa.Models;

namespace Impensa.Services;

public interface IEmailService
{
    Task SendWelcomeEmail(User user);
    Task SendDeletionEmail(User user);
}