using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.PasswordChange
{
    public class SMTPYandex
    {
        public async Task Send(string code, string Email)
        {
            try
            {
                string smtpHost = "smtp.yandex.ru";
                int port = 465;

                string username = "yannuroff.a@yandex.ru";
                string password = "";

                var message = new MimeMessage();

                message.From.Add(new MailboxAddress("Messangers", username));

                message.To.Add(MailboxAddress.Parse(Email));
                message.Subject = "код потверждения";

                message.Body = new TextPart
                {
                    Text = code
                };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    try
                    {
                        await client.ConnectAsync(smtpHost, port).ConfigureAwait(false);

                        await client.AuthenticateAsync(username, password).ConfigureAwait(false);

                        await client.SendAsync(message).ConfigureAwait(false);

                        await client.DisconnectAsync(false).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("не удалось отправить на почту" + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("не удалось отправить на почту" + ex.Message);
            }
        }
    }
}
