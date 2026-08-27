using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.PasswordChange
{
    public class SMTPGoogle
    {
        public async Task Send(string code, string mail)
        {
            try
            {
                string smtpHost = "smtp.gmail.com";
                int smtpport = 587;

                string username = "artem2007yannurow@gmail.com";
                string password = "bedi clrl njit wlmh";

                var message = new MimeMessage();

                message.From.Add(new MailboxAddress("Messangers", username));

                message.To.Add(MailboxAddress.Parse(mail));
                message.Subject = "Код потверждения";

                message.Body = new TextPart("plain")
                {
                    Text = code
                };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    try
                    {
                        await client.ConnectAsync(smtpHost, smtpport, SecureSocketOptions.StartTls).ConfigureAwait(false);

                        await client.AuthenticateAsync(username, password).ConfigureAwait(false);

                        await client.SendAsync(message).ConfigureAwait(false);

                        await client.DisconnectAsync(true).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("не удалось отправить на почту" + ex.Message);
                    }
                }
            }
            catch(Exception ex) 
            {
                MessageBox.Show("не удалось отправить на почту" + ex.Message);
            }
        }
    }
}
