using MailKit.Net.Smtp;
using MimeKit;

namespace ARP.Service
{
    public class EmailService
    {
        public async Task EnviarEmail(
            string nomeRemetente,
            string emailRemetente,
            string nomeDestinario,
            string emailDestinario,
            string mensagem)
        {
            // instanciar classe de mensagem 'mimemessage' 
            var message = new MimeMessage();

            //from address
            message.From.Add(new MailboxAddress(nomeRemetente, emailRemetente));

            // subject
            message.Subject = "Mensagem Web API";

            //to address
            message.To.Add(new MailboxAddress(nomeDestinario, emailDestinario));

            //body
            message.Body = new TextPart("html")
            {
                Text = mensagem,
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.kinghost.net", 465, true);

                client.Authenticate("noreply@brgestao.net", "Br@13579");

                client.Send(message);

                client.Disconnect(true);
            }
        }
    }
}
