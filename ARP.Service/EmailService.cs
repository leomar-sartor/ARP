using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace ARP.Service
{
    /// <summary>
    /// Sends email via SMTP using credentials from environment / configuration.
    /// </summary>
    public class EmailService
    {
        private const string DefaultSmtpHost = "smtp.kinghost.net";
        private const int DefaultSmtpPort = 465;

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Creates an email service that resolves SMTP settings from env and configuration.
        /// </summary>
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Sends an HTML email message through the configured SMTP server.
        /// </summary>
        public async Task EnviarEmail(
            string nomeRemetente,
            string emailRemetente,
            string nomeDestinario,
            string emailDestinario,
            string mensagem,
            CancellationToken cancellationToken = default)
        {
            var user = ResolveSmtpUser(_configuration)
                ?? throw new InvalidOperationException(
                    "SMTP_USER is not set. Configure env var SMTP_USER, User Secrets, or ConnectionStrings:SMTP_USER.");

            var password = ResolveSmtpPassword(_configuration)
                ?? throw new InvalidOperationException(
                    "SMTP_PASSWORD is not set. Configure env var SMTP_PASSWORD, User Secrets, or ConnectionStrings:SMTP_PASSWORD.");

            var host = ResolveSmtpHost(_configuration);
            var port = ResolveSmtpPort(_configuration);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(nomeRemetente, emailRemetente));
            message.Subject = "Mensagem Web API";
            message.To.Add(new MailboxAddress(nomeDestinario, emailDestinario));
            message.Body = new TextPart("html")
            {
                Text = mensagem,
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, true, cancellationToken);
            await client.AuthenticateAsync(user, password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }

        /// <summary>
        /// Resolves SMTP username from environment, then configuration.
        /// </summary>
        private static string? ResolveSmtpUser(IConfiguration configuration)
        {
            var fromEnv = Environment.GetEnvironmentVariable("SMTP_USER");
            if (!string.IsNullOrWhiteSpace(fromEnv))
                return fromEnv;

            return configuration.GetConnectionString("SMTP_USER")
                ?? configuration["SMTP_USER"];
        }

        /// <summary>
        /// Resolves SMTP password from environment, then configuration.
        /// </summary>
        private static string? ResolveSmtpPassword(IConfiguration configuration)
        {
            var fromEnv = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
            if (!string.IsNullOrWhiteSpace(fromEnv))
                return fromEnv;

            return configuration.GetConnectionString("SMTP_PASSWORD")
                ?? configuration["SMTP_PASSWORD"];
        }

        /// <summary>
        /// Resolves SMTP host from environment, then configuration (defaults to KingHost).
        /// </summary>
        private static string ResolveSmtpHost(IConfiguration configuration)
        {
            var fromEnv = Environment.GetEnvironmentVariable("SMTP_HOST");
            if (!string.IsNullOrWhiteSpace(fromEnv))
                return fromEnv;

            return configuration["SMTP_HOST"] ?? DefaultSmtpHost;
        }

        /// <summary>
        /// Resolves SMTP port from environment, then configuration (defaults to 465).
        /// </summary>
        private static int ResolveSmtpPort(IConfiguration configuration)
        {
            var raw =
                Environment.GetEnvironmentVariable("SMTP_PORT")
                ?? configuration["SMTP_PORT"];

            if (!string.IsNullOrWhiteSpace(raw)
                && int.TryParse(raw, out var port)
                && port > 0)
            {
                return port;
            }

            return DefaultSmtpPort;
        }
    }
}
