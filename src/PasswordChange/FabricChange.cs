using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.PasswordChange
{
    public interface Mail
    {
        Task SendMail(string code,string email);
    }

    public class SendYandex() : Mail
    {
        public async Task SendMail(string code, string email)
        {
            SendToYandexMail sendToYandexMail = new SendToYandexMail();

            UseStrategy useStrategy = new UseStrategy(sendToYandexMail);
            await useStrategy.SendToMail(code, email).ConfigureAwait(false);
        }
    }

    public class SendGoogle() : Mail
    {
        public async Task SendMail(string code, string email)
        {
            SendToGoogleMail sendToGoogleMail = new SendToGoogleMail();

            UseStrategy useStrategy = new UseStrategy(sendToGoogleMail);
            await useStrategy.SendToMail(code, email).ConfigureAwait(false);
        }
    }

    public static class FabricClass
    {
        public static Mail SendVariants(string type)
        {
            return type.ToLower() switch
            {
                "gmail.com" => new SendGoogle(),
                "yandex.ru" => new SendYandex(),
                _ => throw new ArgumentException("Неизвестный тип")
            };
        }
    }
}
