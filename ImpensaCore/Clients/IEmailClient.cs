using Mailjet.Client.TransactionalEmails;

namespace ImpensaCore.Clients;

public interface IEmailClient
{
    Task SendTransactionalEmailAsync(TransactionalEmail email);
}