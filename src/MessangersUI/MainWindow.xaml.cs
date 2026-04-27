using MessangersUI.Notifications;
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
        public FabricNotification _fabricNotification;
        
        private readonly string _authToken;
        private readonly string _username;

        public MainWindow(string authToken, string username)
        {
            InitializeComponent();
            _authToken = authToken;
            _username = username;
            _cancellationSource = new CancellationTokenSource();
            _token = _cancellationSource.Token;
            _fabricNotification = new FabricNotification();
            gg();
        }
        private HubConnection? _connection;
        public async void gg()
        {
            // Добавляем токен в query string для WebSocket подключений
            var hubUrl = $"https://localhost:7167/chatHub?access_token={Uri.EscapeDataString(_authToken)}";

            if (_connection == null)
            {
                _connection = new HubConnectionBuilder()
                   .WithUrl(hubUrl)
                   .WithAutomaticReconnect() // автоматическое переподключение
                   .Build();

                _connection.On<string, string>("ReceiveMessage", (fromUser, message) =>
                {
                    textblock.Text = $"{fromUser}\n {message}";
                });

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
                        System.Windows.MessageBox.Show($"Connection failed, retrying in {delay}... Attempt: {retrycouny}");
                    });

                try
                {
                    if (!_token.IsCancellationRequested)
                    {
                        await retrypolicy.ExecuteAsync(async () =>
                        {
                            if (_connection.State != HubConnectionState.Connected &&
                             _connection.State != HubConnectionState.Connecting)
                            {
                                await _connection.StartAsync(_token);
                                var not = _fabricNotification.Method(NotificationsName.Connect);
                                not.Notify();
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка подключения: {ex.Message} {ex.InnerException}");
                    return;
                }
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {   
            if (_connection == null)
            {
                System.Windows.MessageBox.Show("_connection == null");
                return;
            }

            var userName = TextName.Text;
            var message = TextMessage.Text;
            Group.Header = userName;

            if (string.IsNullOrEmpty(message))
            {
                var not = _fabricNotification.Method(NotificationsName.SendCancelNull);
                not.Notify();
                return;
            }

            if (_connection.State == HubConnectionState.Connected)
            {
        
                _cancellationSource = new CancellationTokenSource();
                _token = _cancellationSource.Token;
        

                var retrtpolitic = Policy
                    .Handle<Exception>(ex => ex is not OperationCanceledException)
                    .Or<HttpRequestException>()
                    .WaitAndRetryAsync(3, retrycount =>
                        TimeSpan.FromSeconds(Math.Pow(2, retrycount)) +
                        TimeSpan.FromMilliseconds(Random.Shared.Next()),
                        onRetry: (outcome, delay, retrycount, context) =>
                        {
                            System.Windows.MessageBox.Show($"Send Message failed, retrying in {delay}... Attempt: {retrycount}\nОшибка");
                        });

                try
                {

                    await Task.Delay(3000);

                    await retrtpolitic.ExecuteAsync(async () =>
                    {
                
                        if (_cancellationSource?.IsCancellationRequested == true)
                        {
                            throw new OperationCanceledException();
                        }
                
                        await _connection.InvokeAsync("SendMessage", userName, message);

                        textblock.Text = $"Вы\n {message}";
                        var not = _fabricNotification.Method(NotificationsName.Send);
                        not.Notify();
                    });
          
                }
                catch (OperationCanceledException)
                {
                    var not = _fabricNotification.Method(NotificationsName.SendCancel);
                    not.Notify();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Поймано исключение: {ex.GetType().Name} - {ex.Message}");
                }
                finally
                {
                    _cancellationSource?.Dispose();
                    _cancellationSource = null;
                }
            }
            else
            {
                System.Windows.MessageBox.Show($"ошибка подключения. Состояние: {_connection.State}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _cancellationSource?.Cancel();

        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}