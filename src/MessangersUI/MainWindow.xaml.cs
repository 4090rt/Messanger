using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Polly;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MessangersUI
{
    public partial class MainWindow : Window
    {
        public CancellationTokenSource _cancellationSource;
        public CancellationToken _token;
        public MainWindow()
        {
            InitializeComponent();
            _cancellationSource = new CancellationTokenSource();
            _token = _cancellationSource.Token;
            gg();
        }
        private HubConnection? _connection;
        public async void gg()
        {
            MessageBox.Show("Настройка");
            var hubUrl = "https://localhost:7167/chatHub";

            if (_connection == null)
            {
                _connection = new HubConnectionBuilder()
                   .WithUrl(hubUrl)
                   .WithAutomaticReconnect() // автоматическое переподключение
                   .Build();

                var retrypolicy = Policy
                    .Handle<HttpRequestException>()
                    .Or<TimeoutException>()
                    .Or<HubException>()
                    .Or<Exception>()
                    .WaitAndRetryAsync(3, retrycount =>
                    TimeSpan.FromSeconds(Math.Pow(2, retrycount)) +
                    TimeSpan.FromMilliseconds(Random.Shared.Next(0,100))
                    ,onRetry: (outcome, delay, retrycouny, context) =>
                    {
                        Console.WriteLine($"Connection failed, retrying in {delay}... Attempt: {retrycouny}");
                    });

                try
                {
                    if (!_token.IsCancellationRequested)
                    {
                        await retrypolicy.ExecuteAsync(async () =>
                        {
                            if (_connection.State != HubConnectionState.Disconnected || _connection.State != HubConnectionState.Reconnecting)
                            {
                                await _connection.StartAsync(_token);
                                MessageBox.Show("Подключено к чату!");
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка подключения: {ex.Message} {ex.InnerException}");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Вы уже подключены!");
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {

            if (_connection == null)
            {
                MessageBox.Show("_connection == null");
                return;
            }

            MessageBox.Show($"Состояние соединения: {_connection.State}");

            var userName = TextName.Text;
            var message = TextMessage.Text;


            if (string.IsNullOrEmpty(message))
            {
                MessageBox.Show("Сообщение пустое");
                return;          
            }

            if (_connection.State == HubConnectionState.Connected)
            {
                var retrtpolitic = Policy
                    .Handle<Exception>()
                    .Or<HttpRequestException>()
                    .Or<TaskCanceledException>()
                    .WaitAndRetryAsync(3, retrycount =>
                    TimeSpan.FromSeconds(Math.Pow(2, retrycount)) +
                    TimeSpan.FromMilliseconds(Random.Shared.Next()),
                    onRetry:  (outcome, delay, retrycount, context)=>
                    {
                        Console.WriteLine($"Send Message failed, retrying in {delay}... Attempt: {retrycount}");
                    });
                try
                {
                    await Task.Delay(3000);
                    await retrtpolitic.ExecuteAsync(async () =>
                    {
                        if (!_cancellationSource.IsCancellationRequested)
                        {
                            await _connection.InvokeAsync("SendMessage", userName, message);
                            MessageBox.Show("Сообщение отправлено!");
                        }
                        else
                        {
                            MessageBox.Show("Операция отменена");
                            return;
                        }
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show($"ошибка подключения. Состояние: {_connection.State}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _cancellationSource.Cancel();

        }
    }
}