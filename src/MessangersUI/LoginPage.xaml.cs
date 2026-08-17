using Messangers.EthernetRequest;
using MessangersUI.DataModel;
using MessangersUI.Delegate;
using MessangersUI.HasihingPass;
using MessangersUI.HttpGetRequest.Ping;
using MessangersUI.HttpReuest.PostRequestAvatar;
using MessangersUI.HttpReuest.PostRequestContact;
using MessangersUI.HttpReuest.PostRequestEthernetStat;
using MessangersUI.HttpReuest.PostRequestHistoryMessage;
using MessangersUI.HttpReuest.PostRequestLoginAndRegister;
using MessangersUI.Notifications;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MessangersUI
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public ILogger<RegistrPage> _logger;
        public ILogger<PostRequestAddFirstUserContact> _loggerPostRequestAddFirstUserContact;
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
        public ILogger<PasswordhASH> _passwordpash;
        public ILogger<PostRequestDeleteChatHistory> _loggerPostRequestDeleteChatHistory;
        public ILogger<PostRequestDeleteContact> _loggerdeletecontact;
        public ILogger<PostRequestOnlineUsers> _loggeronlineuser;
        public ILogger<PostMethodAvatar> _logeraatar;
        public ILogger<RequestAvatarUsing> _loggerusingavatar;
        private readonly ILogger<PostLoginRequest> _loggerlog;

        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceCollection _serviceDescriptors;

        public PostRegisterRequest _PostRegisterRequest;
        public ExceptionDelegate _exceptionDelegate;
        public CancellationTokenSource _source;
        public CancellationToken _CancellationToken;
        public MainWindow _MainWindow;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;
        private readonly FabricNotification _fabricNotification;
        private readonly PasswordhASH _passwordhASH;
        private readonly PostLoginRequest _postLoginRequest;
        private readonly GetRequestPing _getRequestPing;
        private readonly HttpGetRequestProvider _httpGetRequestProvider;
        public PostProviderClient _PostProviderClient;
        public RequesetInfoProviders _RequestProviderClient;
        public LoginPage _loginpage;
        private readonly PingRequestServerMessang _pingRequestServerMessang;
        public SearchUserPost _sarcuuser;
        public PostRequestContacts _postRequestContacts;
        public PostRequestUserContactList _PostRequestUserContactList;
        public PostRequestDeleteContact _deleteContact;
        public HttpPostRequestValidationContact _postRequestValidationContact;
        public PostRequestCount _postRequestCount;
        public PostRequestOnlineUsersValidate _postRequestOnlineUsersValidate;
        public PostRequestOnlineUsers _onlineUser;
        public PostRequestAddFirstUserContact _PostRequestAddFirstUserContact;
        public PostRequestDeleteChatHistory _PostRequestDeleteChatHistory;
        public PostMethodAvatar _postmethodavatar;
        public RequestAvatarUsing _requestAvatarUsing;

        string Login = "";
        string Password = "";
        public  LoginPage()
        {
            InitializeComponent();

            LabelTime.Content = DateTime.Now;

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
            _loggeronlineuser = loggerFactory.CreateLogger<PostRequestOnlineUsers>();
            _loggerPostRequestAddFirstUserContact = loggerFactory.CreateLogger<PostRequestAddFirstUserContact>();
            _loggerPostRequestDeleteChatHistory = loggerFactory.CreateLogger<PostRequestDeleteChatHistory>();
            _logeraatar = loggerFactory.CreateLogger<PostMethodAvatar>();
            _loggerusingavatar = loggerFactory.CreateLogger<RequestAvatarUsing>();
            


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

            _onlineUser = new PostRequestOnlineUsers(_loggeronlineuser,
                _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException
                );

            _PostRequestAddFirstUserContact = new PostRequestAddFirstUserContact(_loggerPostRequestAddFirstUserContact,
                 _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);

            _PostRequestDeleteChatHistory = new PostRequestDeleteChatHistory(_loggerPostRequestDeleteChatHistory,
                 _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);

            _postmethodavatar = new PostMethodAvatar(_logeraatar,
                 _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);

            _requestAvatarUsing = new RequestAvatarUsing(_loggerusingavatar, _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException);


        }
        public async Task RequestLogin()
        {
            try
            {
                Login = TextLogin.Text;
                Password = TextPassword.Password;
                if (Login != null && Password != null)
                {
                    _source = new CancellationTokenSource();
                    _CancellationToken = _source.Token;

                    var requestcontent = new DataLogin()
                    {
                        Login = Login,
                        Password = await _passwordhASH.Hash(Password),
                    };
                    TextLogin.Clear();
                    TextPassword.Clear();
                    if (_source.IsCancellationRequested == true)
                    {
                        throw new OperationCanceledException();
                    }
                    var result = await _postLoginRequest.Request(requestcontent).ConfigureAwait(false);
                    if (result.Succes == true)
                    {
                        bool resultvalidateonline = await _postRequestOnlineUsersValidate.RequestPost(Login);
                        if (resultvalidateonline)
                        {
                            await Dispatcher.InvokeAsync(() =>
                            {
                                _MainWindow = new MainWindow(result.token,
                                    result.username,
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
                                    _PostRequestAddFirstUserContact,
                                    _onlineUser,
                                    _PostRequestDeleteChatHistory,
                                    _postmethodavatar,
                                    _requestAvatarUsing
                                    );
                                _MainWindow.Show();
                                Window windowToClose = (Window)this.Parent;
                                windowToClose?.Close();
                            });
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Этот юзер уже онлайн");
                            return;
                        }
                    }
                    else
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            System.Windows.MessageBox.Show($"Взникла ошибка, {result.errormesseage}, {result.Succes}");
                            return;
                        });
                    }
                }
                else
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        System.Windows.MessageBox.Show("Заполните все поля!");
                        return;
                    });

                }
            }
            catch (OperationCanceledException ex)
            {
                var not = _fabricNotification.Method(NotificationsName.SendCancel);
                not.Notify();
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() => System.Windows.MessageBox.Show($"Ошибка: {ex.Message}"));
            }
            finally
            {
                if (_source != null)
                {
                    _source.Dispose();
                    _source = null;
                }
            }
        }
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
             await RequestLogin();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegistrPage());
        }
    }
}
