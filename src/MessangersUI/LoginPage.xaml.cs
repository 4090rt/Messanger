using MessangersUI.DataModel;
using MessangersUI.Delegate;
using MessangersUI.HasihingPass;
using MessangersUI.HttpPostReuest;
using MessangersUI.Notifications;
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
        public PostRegisterRequest _PostRegisterRequest;
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
        public LoginPage _loginpage;

        string Login = "";
        string Password = "";
        List<DataLogin> datalist = new List<DataLogin>();
        public LoginPage()
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

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = loggerFactory.CreateLogger<RegistrPage>();
            _passwordpash = loggerFactory.CreateLogger<PasswordhASH>();
            _loggerlog = loggerFactory.CreateLogger<PostLoginRequest>();

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
        }
        public async Task<List<DataLogin>> RequestLogin()
        {
            try
            {
                Login = TextLogin.Text;
                Password = TextPassword.Text;
                if (Login != null && Password != null)
                {
                    _source = new CancellationTokenSource();
                    _CancellationToken = _source.Token;

                    var requestcontent = new DataLogin()
                    {
                        Login = Login,
                        Password = await _passwordhASH.Hash(Password),
                    };
                    datalist.Add(requestcontent);

                    if (_source.IsCancellationRequested == true)
                    {
                        throw new OperationCanceledException();
                    }
                    await Dispatcher.InvokeAsync(() => System.Windows.MessageBox.Show("Авторизирую..."));
                    await Task.Delay(4000, _CancellationToken);
                    var result = await _postLoginRequest.Request(datalist).ConfigureAwait(false);
                    if (result.Succes == true)
                    {

                        await Dispatcher.InvokeAsync(() => System.Windows.MessageBox.Show("Успешно!"));
                        await Dispatcher.InvokeAsync(() =>
                        {
                            _MainWindow = new MainWindow(result.token, result.username);
                            _MainWindow.Show();
                            Window windowToClose = (Window)this.Parent;
                            windowToClose?.Close();
                        });
                    }
                    else
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            System.Windows.MessageBox.Show($"Взникла ошибка, {result.errormesseage}, {result.Succes}");
                        });
                    }
                }
                else
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        System.Windows.MessageBox.Show("Заполните все поля!");
                    });

                }
                return datalist;
            }
            catch (OperationCanceledException ex)
            {
                var not = _fabricNotification.Method(NotificationsName.SendCancel);
                not.Notify();
                return new List<DataLogin>();
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() => System.Windows.MessageBox.Show($"Ошибка: {ex.Message}"));
                return new List<DataLogin>();
            }
            finally
            {
                _source.Dispose();
                _source = null;
            }
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            await RequestLogin();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() => System.Windows.MessageBox.Show("Операция отменена!"));
            _source.Cancel();
        }
    }
}
