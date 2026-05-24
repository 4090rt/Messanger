using Messangers.EthernetRequest;
using Messangers.ModelData;
using MessangersUI.DataModel;
using MessangersUI.Delegate;
using MessangersUI.HasihingPass;
using MessangersUI.HttpGetRequest;
using MessangersUI.HttpReuest.PostRequestContact;
using MessangersUI.HttpReuest.PostRequestEthernetStat;
using MessangersUI.HttpReuest.PostRequestHistoryMessage;
using MessangersUI.HttpReuest.PostRequestLoginAndRegister;
using MessangersUI.Notifications;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MessangersUI
{
    /// <summary>
    /// Логика взаимодействия для main.xaml
    /// </summary>
    public partial class main : Window
    {

        private HubConnection? _hubConnection;
        private string _authoken;
        private string _username;
        public string _user;

        public ILogger<PostRequestSaveMessage> _loggersavemessage;
        public PostRequestSaveMessage _saveMessage;


        public ILogger<PostRequestAddFirstUserContact> _loggerPostRequestAddFirstUserContact;
        public ILogger<RegistrPage> _logger;
        public ILogger<GetRequestPing> _pinglogger;
        public ILogger<HttpGetRequestProvider> _loggerprovider;
        public ILogger<PostProviderClient> _postproviderlogger;
        public ILogger<PingRequestServerMessang> _loggerpingMyserver;
        public ILogger<RequesetInfoProviders> _RequesetInfoProviderslogger;
        public ILogger<SearchUserPost> _loggersearchuser;
        public ILogger<PostRequestContacts> _postloggercontacts;
        public ILogger<PostRequestUserContactList> _PostRequestUserContactListlogger;
        public ILogger<HttpPostRequestValidationContact> _loggervalidcontact;
        public ILogger<PostRequestCount> _loggercountcon;
        public ILogger<PostRequestOnlineUsersValidate> _loggervalidateonline;
        public PostRegisterRequest _PostRegisterRequest;
        public ILogger<PostRequestDeleteContact> _loggerdeletecontact;
        public ILogger<PostRequestOnlineUser> _loggeronlineuser;
        public ExceptionDelegate _exceptionDelegate;
        public ILogger<PasswordhASH> _passwordpash;
        public CancellationTokenSource _source;
        public CancellationToken _CancellationToken;
        public MainWindow _MainWindow;
        private readonly ILogger<PostLoginRequest> _loggerlog;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;
        private readonly IServiceCollection _serviceDescriptors;
        private readonly FabricNotification _fabricNotification;
        private readonly PasswordhASH _passwordhASH;
        private readonly PostLoginRequest _postLoginRequest;
        private readonly GetRequestPing _getRequestPing;
        private readonly HttpGetRequestProvider _httpGetRequestProvider;
        public PostProviderClient _PostProviderClient;
        public RequesetInfoProviders _RequestProviderClient;
        public LoginPage _loginpage;
        private readonly IMemoryCache _memoryCache;
        private readonly PingRequestServerMessang _pingRequestServerMessang;
        public SearchUserPost _sarcuuser;
        public PostRequestContacts _postRequestContacts;
        public PostRequestUserContactList _PostRequestUserContactList;
        public PostRequestDeleteContact _deleteContact;
        public HttpPostRequestValidationContact _postRequestValidationContact;
        public PostRequestCount _postRequestCount;
        public PostRequestOnlineUsersValidate _postRequestOnlineUsersValidate;
        public PostRequestOnlineUser _onlineUser;
        public PostRequestAddFirstUserContact _PostRequestAddFirstUserContact;
        public main(HubConnection? hubconnection, string authtoken, string username, string user)
        {
            InitializeComponent();
            _hubConnection = hubconnection;
            _user = user;
            _authoken = authtoken;
            _username = username;
            _source = new CancellationTokenSource();
            _CancellationToken = _source.Token;
            _exceptionDelegate = new ExceptionDelegate();
            _httpExceptionDelegate = new HttpExceptionDelegate();
            _jsonExceptionDelegate = new JsonExceptionDelegate();
            _taskCanccelException = new TaskCanccelException();
            _fabricNotification = new FabricNotification();


            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = loggerFactory.CreateLogger<RegistrPage>();
            _passwordpash = loggerFactory.CreateLogger<PasswordhASH>();
            _loggerlog = loggerFactory.CreateLogger<PostLoginRequest>();
            _pinglogger = loggerFactory.CreateLogger<GetRequestPing>();
            _loggerprovider = loggerFactory.CreateLogger<HttpGetRequestProvider>();
            _postproviderlogger = loggerFactory.CreateLogger<PostProviderClient>();
            _RequesetInfoProviderslogger = loggerFactory.CreateLogger<RequesetInfoProviders>();
            _loggerpingMyserver = loggerFactory.CreateLogger<PingRequestServerMessang>();
            _loggersearchuser = loggerFactory.CreateLogger<SearchUserPost>();
            _postloggercontacts = loggerFactory.CreateLogger<PostRequestContacts>();
            _PostRequestUserContactListlogger = loggerFactory.CreateLogger<PostRequestUserContactList>();
            _loggerdeletecontact = loggerFactory.CreateLogger<PostRequestDeleteContact>();
            _loggervalidcontact = loggerFactory.CreateLogger<HttpPostRequestValidationContact>();
            _loggercountcon = loggerFactory.CreateLogger<PostRequestCount>();
            _loggervalidateonline = loggerFactory.CreateLogger<PostRequestOnlineUsersValidate>();
            _loggeronlineuser = loggerFactory.CreateLogger<PostRequestOnlineUser>();
            _loggersavemessage = loggerFactory.CreateLogger<PostRequestSaveMessage>();
            _loggerPostRequestAddFirstUserContact = loggerFactory.CreateLogger<PostRequestAddFirstUserContact>();


            var services = new ServiceCollection();
            services.AddHttpClient();
            _httpClientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

            _passwordhASH = new PasswordhASH(_passwordpash, _exceptionDelegate);

            _postLoginRequest = new PostLoginRequest(_loggerlog,
                _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);

            _getRequestPing = new GetRequestPing(_pinglogger,
                _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);

            _httpGetRequestProvider = new HttpGetRequestProvider(_loggerprovider,
                _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);

            _PostProviderClient = new PostProviderClient(_postproviderlogger,
                _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _taskCanccelException);
            _RequestProviderClient = new RequesetInfoProviders(_RequesetInfoProviderslogger,
                _httpClientFactory,
                _memoryCache,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);
            _pingRequestServerMessang = new PingRequestServerMessang(_loggerpingMyserver, _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);

            _sarcuuser = new SearchUserPost(_loggersearchuser, _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _taskCanccelException);


            _postRequestContacts = new PostRequestContacts(_postloggercontacts,
                _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);

            _PostRequestUserContactList = new PostRequestUserContactList(_PostRequestUserContactListlogger,
                _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);

            _deleteContact = new PostRequestDeleteContact(_loggerdeletecontact,
                _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);

            _postRequestValidationContact = new HttpPostRequestValidationContact(_loggervalidcontact,
                 _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);

            _postRequestCount = new PostRequestCount(_loggercountcon,
                 _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);

            _postRequestOnlineUsersValidate = new PostRequestOnlineUsersValidate(_loggervalidateonline,
                _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);

            _onlineUser = new PostRequestOnlineUser(_loggeronlineuser,
                _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _taskCanccelException,
                _jsonExceptionDelegate);

            _saveMessage = new PostRequestSaveMessage(_loggersavemessage,
                _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);

            _PostRequestAddFirstUserContact = new PostRequestAddFirstUserContact(_loggerPostRequestAddFirstUserContact,
                _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);

        }

        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;

            // Автоматическая прокрутка к курсору
            textBox?.ScrollToEnd();

            // Обновляем размер скроллвьюера
            if (textBox?.LineCount > 3)
            {
                MessageScrollViewer.UpdateLayout();
            }
        }

        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            string textMessage = MessageTextBox.Text;
            await _hubConnection.InvokeAsync("SendMessage", _user, textMessage);
            var date = DateTime.Now.ToString();
            System.Windows.MessageBox.Show("сОХРАНЯЮ");
            await _saveMessage.PostRequest(_username, _user, textMessage, date, "true");
            MessageTextBox.Clear();
            AddMessage(_username, textMessage, true);
        }
        
        public void AddMessage(string user, string message, bool isCurrentUser)
        {
            Dispatcher.Invoke(() =>
            {
                // Создаем блок сообщения
                Border messageBorder = new Border
                {
                    CornerRadius = new CornerRadius(15),
                    Margin = new Thickness(5, 5, 5, 10),
                    MaxWidth = 350,
                    HorizontalAlignment = isCurrentUser ? System.Windows.HorizontalAlignment.Right : System.Windows.HorizontalAlignment.Left
                    
                };

                // Цвет фона сообщения
                if (isCurrentUser)
                {
                    messageBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(77, 108, 133));
                    messageBorder.Margin = new Thickness(50, 5, 5, 10);
                }
                else
                {
                    messageBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 60));
                    messageBorder.Margin = new Thickness(5, 5, 50, 10);
                }

                // Содержимое сообщения
                StackPanel messagePanel = new StackPanel { Margin = new Thickness(12, 8, 12, 8) };

                TextBlock messageText = new TextBlock
                {
                    Text = message,
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 14,
                    FontFamily = new  System.Windows.Media.FontFamily("Segoe UI"),
                    TextWrapping = TextWrapping.Wrap
                };

                if (!isCurrentUser)
                {
                    TextBlock namesob = new TextBlock
                    {
                        Text = _user,
                        Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 255, 255, 255)),
                        FontSize = 10,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        Margin = new Thickness(0, 5, 0, 0)
                    };

                    messagePanel.Children.Add(namesob);

                }

                TextBlock timeText = new TextBlock
                {
                    Text = DateTime.Now.ToString("HH:mm"),
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 255, 255, 255)),
                    FontSize = 10,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                messagePanel.Children.Add(messageText);
                messagePanel.Children.Add(timeText);
                messageBorder.Child = messagePanel;

                MessagesItemsControl.Items.Add(messageBorder);

                // Прокрутка вниз
                MessagesScrollViewer.ScrollToBottom();
            });
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var MainWindow = new MainWindow(_authoken,
                                    _username,
                                    _getRequestPing,
                                    _httpGetRequestProvider,
                                    _PostProviderClient,
                                    _RequestProviderClient,
                                    _pingRequestServerMessang,
                                    _sarcuuser,
                                    _postRequestContacts,
                                    _PostRequestUserContactList,
                                    _deleteContact,
                                    _postRequestValidationContact,
                                    _postRequestCount,
                                    _onlineUser,
                                    _PostRequestAddFirstUserContact);
            MainWindow.Show();
            this.Close();
        }
    }
}
