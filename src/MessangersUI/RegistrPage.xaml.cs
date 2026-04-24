using MessangersUI.DataModel;
using MessangersUI.Delegate;
using MessangersUI.HasihingPass;
using MessangersUI.HttpPostReuest;
using MessangersUI.HTTPSetthings;
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
    /// Логика взаимодействия для RegistrPage.xaml
    /// </summary>
    public partial class RegistrPage : Page
    {
        public ILogger<RegistrPage> _logger;
        public PostRegisterRequest _PostRegisterRequest;
        public ExceptionDelegate _exceptionDelegate;
        public ILogger<PasswordhASH> _passwordpash;
        public CancellationTokenSource _source;
        public CancellationToken _CancellationToken;
        public LoginPage _LoginPage;
        private readonly ILogger<PostRegisterRequest> _loggerpass;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;
        private readonly IServiceCollection _serviceDescriptors;
        private readonly FabricNotification _fabricNotification;
        public RegistrPage _registr;

        string Login = "";
        string Password = "";
        string RepeatPassword = "";

        public RegistrPage()
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
            _loggerpass = loggerFactory.CreateLogger<PostRegisterRequest>();

            var services = new ServiceCollection();
            services.AddHttpClient();
            _httpClientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

            _PostRegisterRequest = new PostRegisterRequest(
                _loggerpass,
                _httpClientFactory,
                _exceptionDelegate,
                _httpExceptionDelegate,
                _jsonExceptionDelegate,
                _taskCanccelException
            );
        }

        public async Task<List<DataRegistr>> RegisterMethod()
        {
            try
            {
                Login = TextLogin.Text;
                Password = TextPassword.Text;
                RepeatPassword = TextPasswordRepeat.Text;
                List<DataRegistr> datalist = new List<DataRegistr>();

                if (Login.Length > 3 && Password.Length > 8 && RepeatPassword.Length > 8)
                {
                    if (Password == RepeatPassword)
                    {
                        _source = new CancellationTokenSource();
                        _CancellationToken = _source.Token;

                        PasswordhASH passwordhASH = new PasswordhASH(_passwordpash, _exceptionDelegate);
                        string CachedPassword = await passwordhASH.Hash(Password);

                        var result = new DataRegistr()
                        {
                            Login = Login,
                            cachePassword = CachedPassword,
                            date = DateTime.Now
                        };


                        if (_source?.IsCancellationRequested == true)
                        {
                            throw new OperationCanceledException();
                        }

                        datalist.Add(result);
                        await Dispatcher.InvokeAsync(() => System.Windows.MessageBox.Show("Регистрирую.."));
                        await Task.Delay(4000, _CancellationToken);
                        var resultat = await _PostRegisterRequest.RequestMETHOD(datalist, _CancellationToken).ConfigureAwait(false);

                        if (resultat.succes == true)
                        {
                            await Dispatcher.InvokeAsync(() => System.Windows.MessageBox.Show("Успешно!"));
                            NavigationService?.Navigate(new LoginPage());
                        }
                        else
                        {
                            await Dispatcher.InvokeAsync(() =>
                            {
                                System.Windows.MessageBox.Show($"Взникла ошибка, {resultat.ErrorMessage}");
                            });
                        }
                    }
                    else
                    {
                        await Dispatcher.InvokeAsync(() => System.Windows.MessageBox.Show("Пароли не совпадают"));
                    }
                    return datalist;
                }
                else
                {
                    await Dispatcher.InvokeAsync(() => System.Windows.MessageBox.Show("Не удлаось отправить данные!"));
                    return new List<DataRegistr>();
                }
            }
            catch (OperationCanceledException ex)
            {
                var not = _fabricNotification.Method(NotificationsName.SendCancel);
                not.Notify();
                return new List<DataRegistr>();
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() => System.Windows.MessageBox.Show($"Ошибка: {ex.Message}"));
                return new List<DataRegistr>();
            }
            finally
            {
                _source.Dispose();
                _source = null;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await RegisterMethod().ConfigureAwait(false);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _source.Cancel();
            System.Windows.MessageBox.Show($"Операция отменена");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // Простой переход на LoginPage
            NavigationService?.Navigate(new LoginPage());
        }
    }
}
