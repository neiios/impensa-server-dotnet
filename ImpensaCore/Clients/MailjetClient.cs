using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;

namespace ImpensaCore.Clients;

public class MailjetClient(string apiKey, string secretKey) : IEmailClient
{
    private readonly Mailjet.Client.MailjetClient _mailjetClient = new(apiKey, secretKey);

    public Task SendTransactionalEmailAsync(TransactionalEmail email)
    {
        return _mailjetClient.SendTransactionalEmailAsync(email);
    }
}