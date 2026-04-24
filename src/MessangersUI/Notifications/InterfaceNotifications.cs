using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MessangersUI.Notifications
{
        public class Timer
        {
            public void SetupAutoDispose(NotifyIcon notifyIcon, int delayMs)
            { 
                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Interval = delayMs;
                timer.Elapsed += (e, s) =>
                {
                    notifyIcon.Dispose();
                    timer.Dispose();
                };
                timer.Start();
            }
        }

        public interface Notification
        { 
            public void Notify();
        }

        public class Not1 : Notification
        { 
            public void Notify()
            {
                var notification = new NotifyIcon()
                {
                    Icon = SystemIcons.Application,
                    Text = "Messanger",
                    Visible = true,
                };

                notification.ShowBalloonTip(4000, "Успешно подключено", "Вы успешно подключились!", ToolTipIcon.Info);

                Timer TIMER = new Timer();
                TIMER.SetupAutoDispose(notification, 5000);
            }
        }

    public class Not2 : Notification
    {
        public void Notify()
        {
            var notification = new NotifyIcon()
            {
                Icon = SystemIcons.Application,
                Text = "Messanger",
                Visible = true,
            };

            notification.ShowBalloonTip(4000, "Отправка отменена", "Сообщение не может быть пустым!", ToolTipIcon.Info);

            Timer TIMER = new Timer();
            TIMER.SetupAutoDispose(notification, 5000);
        }
    }

    public class Not3 : Notification
    {
        public void Notify()
        {
            var notification = new NotifyIcon()
            {
                Icon = SystemIcons.Application,
                Text = "Messanger",
                Visible = true,
            };

            notification.ShowBalloonTip(4000, "Отправка", "Сообщение отправлено!", ToolTipIcon.Info);

            Timer TIMER = new Timer();
            TIMER.SetupAutoDispose(notification, 5000);
        }
    }

    public class Not4 : Notification
    {
        public void Notify()
        {
            var notification = new NotifyIcon()
            {
                Icon = SystemIcons.Application,
                Text = "Messanger",
                Visible = true,
            };

            notification.ShowBalloonTip(4000, "Отправка отменена", "Вы отменили отправку", ToolTipIcon.Info);

            Timer TIMER = new Timer();
            TIMER.SetupAutoDispose(notification, 5000);
        }
    }
    public enum NotificationsName
    { 
        Connect,
        SendCancelNull,
        SendCancel,
        Send
    }

    public class FabricNotification
    {
        public Notification Method(NotificationsName notificationsName)
        {
            return notificationsName switch
            {
                NotificationsName.Connect => new Not1(),
                NotificationsName.SendCancelNull => new Not2(),
                NotificationsName.Send => new Not3(),
                NotificationsName.SendCancel => new Not4(),
            };
        }
    }
}
