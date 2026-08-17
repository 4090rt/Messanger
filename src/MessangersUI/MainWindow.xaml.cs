using Messangers.EthernetRequest;
using Messangers.ModelData;
using MessangersUI.DataModel;
using MessangersUI.HttpGetRequest.Ping;
using MessangersUI.HttpReuest.PostRequestAvatar;
using MessangersUI.HttpReuest.PostRequestContact;
using MessangersUI.HttpReuest.PostRequestEthernetStat;
using MessangersUI.HttpReuest.PostRequestHistoryMessage;
using MessangersUI.HttpReuest.PostRequestLoginAndRegister;
using MessangersUI.Notifications;
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
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MessangersUI
{

    public partial class MainWindow : Window
    {
        public PostRequestAddFirstUserContact _PostRequestAddFirstUserContact;
        public CancellationTokenSource _cancellationSource;
        public CancellationToken _token;
        public FabricNotification _fabricNotification;
        public GetRequestPing _getRequestPing;
        public HttpGetRequestProvider _httpGetRequestProvider;
        public PostProviderClient _PostProviderClient;
        public RequesetInfoProviders _RequestProviderClient;
        public PingRequestServerMessang _PingRequestServerMessang;
        public SearchUserPost _searchuser;
        public PostRequestUserContactList _PostRequestUserContactList;
        public PostRequestDeleteContact _deleteContact;
        public HttpPostRequestValidationContact _httpPostRequestValidationContact;
        public PostRequestCount _PostRequestCount;
        public PostRequestContacts _postRequestContacts;
        public PostRequestOnlineUsers _onlineUser;
        public PostRequestDeleteChatHistory _PostRequestDeleteChatHistory;
        public RequestAvatarUsing _avatarUsing;

        public main _main;
        public bool _Openchat = false;
        public string _activeChatWith;
        private readonly string _authToken;
        private readonly string _username;
        public PostMethodAvatar _methodAvatar;

        public MainWindow(string authToken, string username, GetRequestPing getRequestPing, 
            HttpGetRequestProvider httpGetRequestProvider, PostProviderClient postProviderClient,
            RequesetInfoProviders RequestProviderClient, PingRequestServerMessang pingRequestServerMessang,
            SearchUserPost searchUserPost, PostRequestContacts postRequestContacts, PostRequestUserContactList postRequestUserContactList, 
            PostRequestDeleteContact deleteContact, HttpPostRequestValidationContact httpPostRequestValidationContact,
            PostRequestCount postRequestCount, PostRequestAddFirstUserContact PostRequestAddFirstUserContact, PostRequestOnlineUsers postRequestOnlineUsers,
            PostRequestDeleteChatHistory postRequestDeleteChatHistory, PostMethodAvatar methodAvatar, RequestAvatarUsing avatarUsing)
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
            _PingRequestServerMessang = pingRequestServerMessang;
            _searchuser = searchUserPost;
            _postRequestContacts = postRequestContacts;
            _PostRequestUserContactList = postRequestUserContactList;
            _deleteContact = deleteContact;
            _httpPostRequestValidationContact = httpPostRequestValidationContact;
            _PostRequestCount = postRequestCount;
            _PostRequestAddFirstUserContact = PostRequestAddFirstUserContact;
            _onlineUser = postRequestOnlineUsers;
            _PostRequestDeleteChatHistory = postRequestDeleteChatHistory;
            _methodAvatar = methodAvatar;
            _avatarUsing = avatarUsing;

            this.Loaded += async (s, e) =>
            {
                gg();
                await BaseContacts();
                await MainMethod();
                await UIFace();
            };

        }
        private HubConnection? _connection;

        public async Task<List<UserContact>> BaseContacts()
        {
            try
            {
                var result = await _PostRequestUserContactList.RequestPost(_username);
                return result;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message + ex.StackTrace);
                return new List<UserContact>();
            }
        }
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
                _connection.On<string>("UserConnect", (user) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        SetUserState(user, true);
                    });
                });

                _connection.On<string>("UserDisconnect", (user) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        SetUserState(user, false);
                    });
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
        public async Task MainMethod()
        {
            _connection.On<string, string, AttachmentMetadata>("ReceiveMessage", async (fromUser, message, attachmentMetadata) =>
            {
                List<UserContact> list = await BaseContacts().ConfigureAwait(false);

                var result = new HashSet<string>(list?.Select(p => p.Username) ?? new List<string>());

                if (!result.Contains(fromUser))
                {
                    if (System.Windows.MessageBox.Show($"Вам написал {fromUser} \n {message}", "Добавить его в контакты?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        List<UserContact> user = new List<UserContact>
                            {
                                new UserContact
                                {
                                    Name = _username,
                                    Username = fromUser,
                                    photo = "",
                                }
                            };
                        bool saved = await _postRequestContacts.Request(user);
                        if (saved)
                        {
                            System.Windows.MessageBox.Show("Добавлен в контакты");
                            NewUser(fromUser);
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Отклонено");
                    }
                }
                else
                {
                    if (_Openchat == false)
                    {
                        System.Windows.MessageBox.Show($" {_Openchat}");
                        System.Windows.MessageBox.Show($"Вам пришло сообщение от чата c {fromUser} {attachmentMetadata.FileName}");
                        Notifications(fromUser);
                    }
                    else if (_Openchat == true)
                    {
                        if (_activeChatWith == fromUser)
                        {
                            if (_main != null)
                            {
                                System.Windows.MessageBox.Show(
                           $"_Openchat: {_Openchat}\n" +
                           $"_activeChatWith: {_activeChatWith}\n" +
                           $"fromUser: {fromUser}\n" +
                           $"_main != null: {_main != null}"
                       );
                                _main.AddMessage(fromUser, message, false);

                            }
                        }
                    }
                }
            });
        }

        public async void Notifications(string username)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (StackPanel panel in UsersStackPanel.Children)
                {
                    if (panel.Tag.ToString() == username)
                    {
                            var border1 = new Border
                            {
                                Width = 25,
                                Height = 25,
                                CornerRadius = new CornerRadius(20),
                                Background = System.Windows.Media.Brushes.Green,
                                Margin = new Thickness(2),
                            };
                        panel.Children.Add(border1);
                        break;
                    }
                }
            });
        }


        public void SetUserState(string username, bool state)
        {
            foreach (StackPanel panel in UsersStackPanel.Children)
            {
                if (panel.Tag.ToString() == username)
                {
                    var panelfirst = panel.Children[0] as Border;
                    if (panelfirst != null)
                    { 
                        panelfirst.Background = state
                            ? System.Windows.Media.Brushes.Green
                            : System.Windows.Media.Brushes.Red;
                    }
                    break;
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

                        if (_username == userName)
                        {
                            System.Windows.MessageBox.Show("Вы не можете добавить себя в контакты");
                            return;
                        }

                        var resultvalidate = await _httpPostRequestValidationContact.RequestMethod(_username, username);

                        if (resultvalidate == true)
                        {
                            System.Windows.MessageBox.Show("Контакт уже добавлен");
                            return;
                        }
                        else
                        {
                            var resultauserseRCH = await _searchuser.Request(username);
                            if (resultauserseRCH == true)
                            {
                                DateTime date = DateTime.Now;

                                string photo = "";

                                List<UserContact> user = new List<UserContact>
                                {
                                new UserContact
                                {
                                    Name = _username,
                                    Username = username,
                                    photo = photo,
                                }
                                 };
                                bool saved = await _postRequestContacts.Request(user);
                                if (saved)
                                {
                                    string result = await _PostRequestCount.RequestPost(_username);
                                    LabelCount.Content = $"Число ваших контактов: {result}";
                                    NewUser(username);
                                }
                                else
                                {
                                    System.Windows.MessageBox.Show("ОШИБКА!!!");
                                }

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

        public async void NewUser(string user)
        {
            bool result = await _PostRequestAddFirstUserContact.PostRequest(user).ConfigureAwait(false);

            Dispatcher.Invoke(() =>
            {
                StackPanel userPanel = new StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    Margin = new Thickness(10),
                    Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 40)),
                    Tag = user
                };
                System.Windows.Media.Brush circleColor = result
                ? System.Windows.Media.Brushes.Green
                : System.Windows.Media.Brushes.Red;

                Border circle = new Border
                {
                    Width = 40,
                    Height = 40,
                    CornerRadius = new CornerRadius(20),
                    Background = circleColor,
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
                System.Windows.Controls.Button chatbutton = new System.Windows.Controls.Button
                {
                    Content = "Chat",

                    Background = System.Windows.Media.Brushes.Green,
                    Foreground = System.Windows.Media.Brushes.White,
                    Width = 30,
                    Height = 60,
                    Margin = new Thickness(10, 0, 0, 0)
                };
                deleteBtn.Click += async (s, e) =>
                {
                    if (System.Windows.MessageBox.Show($"Удалить контакт {user}?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        if (System.Windows.MessageBox.Show($"Удалить историю сообщений с {user}?",
                        "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            UsersStackPanel.Children.Remove(userPanel);
                            await _deleteContact.Request(_username, user);
                            await _PostRequestDeleteChatHistory.PostDeleteHistory(_username, user);
                            string result = await _PostRequestCount.RequestPost(_username);
                            LabelCount.Content = $"Число ваших контактов: {result}";
                        }
                        else
                        {
                            UsersStackPanel.Children.Remove(userPanel);
                            await _deleteContact.Request(_username, user);
                            string result = await _PostRequestCount.RequestPost(_username);
                            LabelCount.Content = $"Число ваших контактов: {result}";
                        }
                    }
                };


                chatbutton.Click += async (s, e) =>
                {
                    _Openchat = true;
                    var mainWindow = new main(_connection, _authToken, _username, user);
                      _main = mainWindow;
                    _activeChatWith = user;
                    mainWindow.Show();
                    this.Hide();
                };

                userPanel.Children.Add(circle);
                userPanel.Children.Add(name);
                userPanel.Children.Add(deleteBtn);
                userPanel.Children.Add(chatbutton);

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

            string result = await _PostRequestCount.RequestPost(_username);
            LabelCount.Content = $"Число ваших контактов: {result}";

            List<UserContact> list = await BaseContacts();
            List<DataUsersList> listdata = new List<DataUsersList>();

            foreach (var item in list)
            {
                var users = new DataUsersList()
                {
                    User = item.Username
                    
                };
                listdata.Add(users);
            }

            var resultonline = await _onlineUser.RequestPost(listdata).ConfigureAwait(false);

            var onlineusers = new HashSet<string>(resultonline?.Select(u => u.User) ?? new List<string>());

            await Dispatcher.InvokeAsync(() => UsersStackPanel.Children.Clear());

            foreach (UserContact contact in list)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    if (list != null)
                    {
                            StackPanel userPanel = new StackPanel
                            {
                                Orientation = System.Windows.Controls.Orientation.Horizontal,
                                Margin = new Thickness(10),
                                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 40)),
                                Tag = contact.Username
                            };
                            bool isOnline = onlineusers.Contains(contact.Username);
                            System.Windows.Media.Brush circleColor = isOnline
                            ? System.Windows.Media.Brushes.Green
                            : System.Windows.Media.Brushes.Red;

                            //Border circle = new Border
                            //    {
                            //        Width = 40,
                            //        Height = 40,
                            //        CornerRadius = new CornerRadius(20),
                            //        Background = circleColor,
                            //        Margin = new Thickness(5)
                            //    };

                            TextBlock initials = new TextBlock
                            {
                                Text = contact.Name.ToString().ToUpper(),
                                Foreground = System.Windows.Media.Brushes.White,
                                FontSize = 20,
                                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            //circle.Child = initials;

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

                            System.Windows.Controls.Button chatbutton = new System.Windows.Controls.Button
                            {
                                Content = "Chat",

                                Background = System.Windows.Media.Brushes.Green,
                                Foreground = System.Windows.Media.Brushes.White,
                                Width = 30,
                                Height = 60,
                                Margin = new Thickness(10, 0, 0, 0)
                            };

                            deleteBtn.Click += async (s, e) =>
                            {
                                if (System.Windows.MessageBox.Show($"Удалить контакт {contact.Username}?",
                        "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                    UsersStackPanel.Children.Remove(userPanel);
                                await _deleteContact.Request(_username, contact.Username);
                                string result = await _PostRequestCount.RequestPost(_username);
                                LabelCount.Content = $"Число ваших контактов: {result}";
                            };

                            chatbutton.Click += async (s, e) =>
                            {
                                _Openchat = true;
                                var mainWindow = new main(_connection, _authToken, _username, contact.Username);
                                _main = mainWindow;
                                _activeChatWith = contact.Username;
                                mainWindow.Show();
                                this.Hide();
                            };

                            //userPanel.Children.Add(circle);
                            userPanel.Children.Add(name);
                            userPanel.Children.Add(deleteBtn);
                            userPanel.Children.Add(chatbutton);

                            UsersStackPanel.Children.Add(userPanel);
                        }
                    else
                    {
                        UsersStackPanel.Children.Clear();
                        System.Windows.MessageBox.Show("Контактов нет");
                    }
                });
            }
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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Settings settings = new Settings(_methodAvatar, _username, _avatarUsing);
                settings.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }
}