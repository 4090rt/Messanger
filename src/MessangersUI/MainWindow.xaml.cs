using Messangers.EthernetRequest;
using MessangersUI.DataModel;
using MessangersUI.HttpGetRequest;
using MessangersUI.HttpPostReuest;
using MessangersUI.Notifications;
using MessangersUI.Sqlite.CreateTable;
using MessangersUI.Sqlite.DeleteUser;
using MessangersUI.Sqlite.InsertMethods;
using MessangersUI.Sqlite.SelectMethods;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.VisualBasic.ApplicationServices;
using Polly;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MessangersUI
{
    public partial class MainWindow : Window
    {
        public CancellationTokenSource _cancellationSource;
        public CancellationToken _token;
        public FabricNotification _fabricNotification;
        public GetRequestPing _getRequestPing;
        public HttpGetRequestProvider _httpGetRequestProvider;
        public PostProviderClient _PostProviderClient;
        public RequesetInfoProviders _RequestProviderClient;
        public PingRequestServerMessang _PingRequestServerMessang;
        public SearchUserPost _searchuser;
        public Create _create;
        public InsertContacts _insertContacts;
        public SelectContacts _selectcontacts;
        public Delete _delete;
         
        private readonly string _authToken;
        private readonly string _username;

        public MainWindow(string authToken, string username, GetRequestPing getRequestPing, 
            HttpGetRequestProvider httpGetRequestProvider, PostProviderClient postProviderClient, 
            RequesetInfoProviders RequestProviderClient, PingRequestServerMessang pingRequestServerMessang,
            SearchUserPost searchUserPost, Create create, InsertContacts insertContacts, SelectContacts selectContacts,
            Delete delete)
        {
            InitializeComponent();
            _authToken = authToken;
            _username = username;
            _cancellationSource = new CancellationTokenSource();
            _token = _cancellationSource.Token;
            _fabricNotification = new FabricNotification();
            _getRequestPing = getRequestPing;
            _httpGetRequestProvider = httpGetRequestProvider;
            _PostProviderClient = postProviderClient;
            _RequestProviderClient = RequestProviderClient;
            gg();
           _PingRequestServerMessang = pingRequestServerMessang;
            _searchuser = searchUserPost;
            _create = create;
            _insertContacts = insertContacts;
            _selectcontacts = selectContacts;
            _delete = delete;
            UIFace();
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

                    await retrtpolitic.ExecuteAsync(async () =>
                    {
                
                        if (_cancellationSource?.IsCancellationRequested == true)
                        {
                            throw new OperationCanceledException();
                        }
                        string username = TextName.Text;
                        var resultauserseRCH = await _searchuser.Request(username);
                        if (resultauserseRCH == true)
                        {
                            DateTime date = DateTime.Now;
                            string photo = "";

                            bool saved = await _insertContacts.SaveContact(_username,username, date, photo);
                            if (saved)
                            {
                                System.Windows.MessageBox.Show("Сохранено");
                                NewUser(username);
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("ОШИБКА!!!");
                            }
                            
                        }
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

        public async Task<List<UserContact>> BaseContacts()
        {
            try
            {
                List<UserContact> list = await _selectcontacts.CacheRequest(_username);
                return list;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message + ex.StackTrace);
                return new List<UserContact> ();
            }
        }
        public async void NewUser(string user)
        {
            Dispatcher.Invoke(() =>
            {
                StackPanel userPanel = new StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    Margin = new Thickness(10),
                    Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 40)),
                    Tag = user
                };

                Border circle = new Border
                {
                    Width = 40,
                    Height = 40,
                    CornerRadius = new CornerRadius(20),
                    Background = System.Windows.Media.Brushes.Green,
                    Margin = new Thickness(5)
                };

                TextBlock initials = new TextBlock
                {
                    Text = user.ToString().ToUpper(),
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 20,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                circle.Child = initials;

                TextBlock name = new TextBlock
                {
                    Text = user,
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 16,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0)
                };

                System.Windows.Controls.Button deleteBtn = new System.Windows.Controls.Button
                {
                    Content = "X",
                    Background = System.Windows.Media.Brushes.Red,
                    Foreground = System.Windows.Media.Brushes.White,
                    Width = 30,
                    Height = 30,
                    Margin = new Thickness(10, 0, 0, 0),
                    Tag = user
                };

                deleteBtn.Click += async (s, e) =>
                {
                    if (System.Windows.MessageBox.Show($"Удалить контакт {user}?",
            "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        UsersStackPanel.Children.Remove(userPanel);
                        await _delete.DeleteMethod(_username, user);
                    }
                    if (System.Windows.MessageBox.Show($"Удалить контакт {user}?",
"Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    { 
                        
                    }
                };

                userPanel.Children.Add(circle);
                userPanel.Children.Add(name);
                userPanel.Children.Add(deleteBtn);

                UsersStackPanel.Children.Add(userPanel);

                userPanel.BringIntoView();
            });
        }

        public async Task UIFace()
        {    // Проверка что элемент управления существует
            await Dispatcher.InvokeAsync(() =>
            {
                if (UsersStackPanel == null)
                {
                    System.Windows.MessageBox.Show("Ошибка: UsersStackPanel не найден!");
                    return;
                }
            });
            List<UserContact> list = await BaseContacts();
            await Dispatcher.InvokeAsync(() =>
            {
                if (list != null)
                {
                    UsersStackPanel.Children.Clear();
                    foreach (UserContact contact in list)
                    {
                        StackPanel userPanel = new StackPanel
                        {
                            Orientation = System.Windows.Controls.Orientation.Horizontal,
                            Margin = new Thickness(10),
                            Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 40)),
                            Tag = contact.Username
                        };

                        Border circle = new Border
                        {
                            Width = 40,
                            Height = 40,
                            CornerRadius = new CornerRadius(20),
                            Background = System.Windows.Media.Brushes.Green,
                            Margin = new Thickness(5)
                        };

                        TextBlock initials = new TextBlock
                        {
                            Text = contact.Username.ToString().ToUpper(),
                            Foreground = System.Windows.Media.Brushes.White,
                            FontSize = 20,
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        circle.Child = initials;

                        TextBlock name = new TextBlock
                        {
                            Text = contact.Username,
                            Foreground = System.Windows.Media.Brushes.White,
                            FontSize = 16,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(10, 0, 0, 0)
                        };

                        System.Windows.Controls.Button deleteBtn = new System.Windows.Controls.Button
                        {
                            Content = "X",
                            Background = System.Windows.Media.Brushes.Red,
                            Foreground = System.Windows.Media.Brushes.White,
                            Width = 30,
                            Height = 30,
                            Margin = new Thickness(10, 0, 0, 0),
                            Tag = contact.Username
                        };

                        deleteBtn.Click += async (s, e) =>
                        {
                            if (System.Windows.MessageBox.Show($"Удалить контакт {contact.Username}?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                UsersStackPanel.Children.Remove(userPanel);
                            await _delete.DeleteMethod(_username, contact.Username);
                        };

                        userPanel.Children.Add(circle);
                        userPanel.Children.Add(name);
                        userPanel.Children.Add(deleteBtn);

                        UsersStackPanel.Children.Add(userPanel);
                    }
                }
                else
                {
                    UsersStackPanel.Children.Clear();
                    System.Windows.MessageBox.Show("Контактов нет");
                }
            });
        }
        private async void Button_ClickpING(object sender, RoutedEventArgs e)
        {
            try
            {
                string pingToGoogle = "";
                string ProviderInfo = "";
                double pingToMyServer = 0;

                var result = await _getRequestPing.Request();
                var result2 = await _httpGetRequestProvider.Request();
                var result3 = await _PingRequestServerMessang.Request();

                byte[] bytes = await _RequestProviderClient.CacheReqquest();
                await _PostProviderClient.PostRequest(bytes);

                if (result != null && result2 != null && result3 > 0)
                {
                    foreach (var item in result)
                    {
                        pingToGoogle = $"Ping: {item.PingMs}\n Host: {item.Host}";

                        foreach (var item2 in result2)
                        {
                            ProviderInfo = $"City: {item2.City}\n Loc: {item2.Loc}\n TimeZone: {item2.Timezone}";
                        }
                    
                    }

                    pingToMyServer = result3;
                    System.Windows.MessageBox.Show($"Параметры сети: \n" +
                        $"Местоположение сервера:  \n  {ProviderInfo} \n" +
                        $"Пинг до Google: \n {pingToGoogle} \n" +
                        $"Пинг до нашего сервера:  \n Ping: {pingToMyServer}");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("dsdd" + ex.Message);
            }
        }
    }
}