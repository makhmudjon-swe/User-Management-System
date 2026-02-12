public interface IEmailSender
{
    Task SendConfirmEmailAsync(
        string toEmail,
        string confirmLink,
        CancellationToken cancellationToken = default);
}
