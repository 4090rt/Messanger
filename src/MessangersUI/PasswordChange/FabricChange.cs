using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.PasswordChange
{
    public interface Mail
    {
        Task SendMail(string email);
    }

    public class SendYandex() : Mail
    {
        public async Task SendMail(string email)
        {
            SendToYandexMail sendToYandexMail = new SendToYandexMail();
            await sendToYandexMail.TypeMail(email).ConfigureAwait(false);
        }
    }

    public class SendGoogle() : Mail
    {
        public async Task SendMail(string email)
        {
            SendToGoogleMail sendToGoogleMail = new SendToGoogleMail();
            await sendToGoogleMail.TypeMail(email).ConfigureAwait(false);
        }
    }

    public static class FabricClass
    {
        public static Mail SendVariants(string type)
        {
            return type.ToLower() switch
            {
                "gmail.com" => new SendGoogle(),
                "yandex.com" => new SendYandex(),
                _ => throw new ArgumentException("Неизвестный тип")
            };
        }
    }
}
