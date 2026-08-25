using MessangersUI.GenerateCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.PasswordChange
{
    public interface MailStrategy
    {
        Task TypeMail(string Email);
    }

    public class SendToGoogleMail() : MailStrategy
    {
        public async Task TypeMail(string Email)
        { 
            SMTPGoogle sMTPGoogle = new SMTPGoogle();
            await sMTPGoogle.Send(Generate.GenerateC(), Email);
        }
    }

    public class SendToYandexMail() : MailStrategy
    {
        public async Task TypeMail(string Email)
        {
            SMTPYandex smtpGoogle = new SMTPYandex();
            await smtpGoogle.Send(Generate.GenerateC(),Email);
        }
    }

    public class UseStrategy
    { 
        private MailStrategy _strategy;

        public UseStrategy(MailStrategy strategy)
        {
            _strategy = strategy;
        }

        public void Setstrategy(MailStrategy mailStrategy)
        { 
            _strategy = mailStrategy;
        }

        public async Task SendToMail(string Email)
        {
            await _strategy.TypeMail(Email).ConfigureAwait(false);
        }
    }
}
