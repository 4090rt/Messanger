using Messangers.EthernetRequest;
using Messangers.ModelData;
using MessangersUI.DataModel;
using MessangersUI.Delegate;
using MessangersUI.HasihingPass;
using MessangersUI.HttpGetRequest.Ping;
using MessangersUI.HttpReuest.PostRequestContact;
using MessangersUI.HttpReuest.PostRequestContact;
using MessangersUI.HttpReuest.PostRequestEthernetStat;
using MessangersUI.HttpReuest.PostRequestHistoryMessage;
using MessangersUI.HttpReuest.PostRequestHistoryMessage.PostFiles;
using MessangersUI.HttpReuest.PostRequestLoginAndRegister;
using MessangersUI.Notifications;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;


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
        public Border _checkFile;
        public string _localpath;
        public static class ChatState
        {
            public static AttachmentMetadata PendingAttachment { get; set; }
        }

        public ILogger<PostRequestSaveMessage> _loggersavemessage;
        public PostRequestSaveMessage _saveMessage;

        public ILogger<PostRequestHistroyDowload> _loggerPostRequestHistroyDowload;
        PostRequestHistroyDowload _PostRequestHistroyDowload;
        public ILogger<PostRequestDeleteConcrectEsaage> _loggerPostRequestDeleteConcrectEsaage;
        public PostRequestDeleteConcrectEsaage _PostRequestDeleteConcrectEsaage;
        public PostRequestHistorySaveFile _PostRequestHistorySaveFile;
        public ILogger<PostRequestHistorySaveFile> _loggerPostRequestHistorySaveFile;
        public PostRequestUpdateID _postRequestUpdateID;
        public ILogger<PostRequestUpdateID> _loggerPostRequestUpdateID;
        public PostHistoryFiles _postHistoryFiles;
        public ILogger<PostHistoryFiles> _loggerPostHistoryFiles;


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
        public ILogger<PostRequestOnlineUsers> _loggeronlineuser;
        public ILogger<PostRequestDeleteChatHistory> _loggerPostRequestDeleteChatHistory;
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
        public PostRequestOnlineUsers _onlineUser;
        public PostRequestAddFirstUserContact _PostRequestAddFirstUserContact;
        public PostRequestDeleteChatHistory _PostRequestDeleteChatHistory;
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
            _loggeronlineuser = loggerFactory.CreateLogger<PostRequestOnlineUsers>();
            _loggersavemessage = loggerFactory.CreateLogger<PostRequestSaveMessage>();
            _loggerPostRequestAddFirstUserContact = loggerFactory.CreateLogger<PostRequestAddFirstUserContact>();
            _loggerPostRequestHistroyDowload = loggerFactory.CreateLogger<PostRequestHistroyDowload>();
            _loggerPostRequestDeleteChatHistory = loggerFactory.CreateLogger<PostRequestDeleteChatHistory>();
            _loggerPostRequestDeleteConcrectEsaage = loggerFactory.CreateLogger<PostRequestDeleteConcrectEsaage>();
            _loggerPostRequestHistorySaveFile = loggerFactory.CreateLogger<PostRequestHistorySaveFile>();
            _loggerPostRequestUpdateID = loggerFactory.CreateLogger<PostRequestUpdateID>();
            _loggerPostHistoryFiles = loggerFactory.CreateLogger<PostHistoryFiles>();



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

            _PostRequestHistroyDowload = new PostRequestHistroyDowload (_loggerPostRequestHistroyDowload,
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

            _PostRequestDeleteConcrectEsaage = new PostRequestDeleteConcrectEsaage(_loggerPostRequestDeleteConcrectEsaage,
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

            _PostRequestHistorySaveFile = new PostRequestHistorySaveFile(_loggerPostRequestHistorySaveFile,
             _httpClientFactory,
            _exceptionDelegate,
            _httpExceptionDelegate,
            _jsonExceptionDelegate,
            _taskCanccelException
            );

            _postRequestUpdateID = new PostRequestUpdateID(_loggerPostRequestUpdateID,
            _httpClientFactory,
            _exceptionDelegate,
            _httpExceptionDelegate,
            _jsonExceptionDelegate,
            _taskCanccelException
            );

            _postHistoryFiles = new PostHistoryFiles(_loggerPostHistoryFiles,
            _httpClientFactory,
            _exceptionDelegate,
            _httpExceptionDelegate,
            _jsonExceptionDelegate,
            _taskCanccelException
            );

            ChatUserName.Text = _user;
            AddHistoryMessage(_username, _user);
            AddHistoryFiles(_username, _user);
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
            var date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            var attachment = ChatState.PendingAttachment;
            if (textMessage == null || textMessage == "")
            {
                if (attachment.FileName != null)
                {
                    textMessage = attachment.FileName;
                }
                else
                {
                    System.Windows.MessageBox.Show("Выберите файл или напишите  сообщение");
                }
            }
            if (textMessage != null && textMessage != "" && attachment != null && attachment.Id != 0)
            {
                textMessage = $"{textMessage}\n File:{attachment.FileName}";
            }
            var result = await _saveMessage.PostRequest(_username, _user, textMessage, date, "true");
            if (attachment == null || attachment.Id == 0)
            {
                await _hubConnection.InvokeAsync("SendMessage", _user, textMessage, new AttachmentMetadata());
               
                await Dispatcher.InvokeAsync(async () =>
                {
                    MessageTextBox.Clear();
                    AddMessage(_username, textMessage, true);
                });
            }
            else if (attachment != null && textMessage == null && textMessage == "" && attachment.Id != 0)
            {
 
                Int64 attachid = attachment.Id;

                bool resultexec = await _postRequestUpdateID.RequestUpdate(result.id, attachid).ConfigureAwait(false);

                await _hubConnection.InvokeAsync("SendMessage", _user, textMessage, attachment);
                
                await Dispatcher.InvokeAsync(async () =>
                {
                    MessageTextBox.Clear();
                    AddMessage(_username, textMessage, true);
                    AddMessageFile(true, attachment);
                    ChatState.PendingAttachment = null;
                    if (_checkFile != null)
                    {
                        MessagesItemsControl.Items.Remove(_checkFile);
                    }
                });
            }
            else
            {
                Int64 attachid = attachment.Id;

                bool resultexec = await _postRequestUpdateID.RequestUpdate(result.id, attachid).ConfigureAwait(false);

                await _hubConnection.InvokeAsync("SendMessage", _user, textMessage, attachment);
                await Dispatcher.InvokeAsync(async () =>
                {
                    MessageTextBox.Clear();
                    AddMessageFile(true, attachment);
                    ChatState.PendingAttachment = null;
                    if (_checkFile != null)
                    {
                        MessagesItemsControl.Items.Remove(_checkFile);
                    }
                });
            }
        }

        public async Task AddHistoryMessage(string username, string user)
        {
            try
            {
                var result = await _PostRequestHistroyDowload.PostRequest(username, user);
                if (result != null)
                {
                    var borders = new List<Border>();
                    foreach (var item in result)
                    {
                        bool isMyMessage = item.LoginUser1 == username;
                        borders.Add(CreateMessageBorder(item, isMyMessage));
                    }
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        foreach (var border in borders)
                        {
                            MessagesItemsControl.Items.Add(border);
                        }

                        // Прокрутка вниз после добавления всех сообщений
                        MessagesScrollViewer?.ScrollToBottom();
                    });
                }
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show("Возникло исключение" + ex.Message + ex.InnerException + ex.StackTrace);            
            }
        }

        public async Task AddHistoryFiles(string username, string user)
        {
            try
            {
                var result = await _postHistoryFiles.Request(username, user); //post запрос на сервер найти все файлы для чата
                if (result != null)
                {
                    var borders = new List<Border>();
                    foreach (var item in result)
                    {
                        bool isMyMessage = item.User == username;
                        borders.Add(AddMessageFile(isMyMessage, item));
                    }
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        foreach (var border in borders)
                        {
                            MessagesItemsControl.Items.Add(border);
                        }

                        // Прокрутка вниз после добавления всех сообщений
                        MessagesScrollViewer?.ScrollToBottom();
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Возникло исключение" + ex.Message + ex.InnerException + ex.StackTrace);
            }
        }


        private Border CreateMessageBorder(MessageData item, bool isMyMessage)
        {
            var border = new Border
            {
                CornerRadius = new CornerRadius(15),
                MaxWidth = 350,
                HorizontalAlignment = isMyMessage
                    ? System.Windows.HorizontalAlignment.Right
                    : System.Windows.HorizontalAlignment.Left,
                Background = isMyMessage
                    ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(77, 108, 133))
                    : new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 60)),
                Margin = isMyMessage
                    ? new Thickness(50, 5, 5, 10)
                    : new Thickness(5, 5, 50, 10),
                Tag = item.Id
            };

            var panel = new StackPanel { Margin = new Thickness(12, 8, 12, 8) };

            if (!isMyMessage)
            {
                panel.Children.Add(new TextBlock
                {
                    Text = item.LoginUser1,
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 255, 255, 255)),
                    FontSize = 10,
                    Margin = new Thickness(0, 0, 0, 4),
                    Tag = item.Id
                }); 
            }

            var contextMenu = new ContextMenu();
            var deleteMenuItem = new MenuItem { Header = "Удалить сообщение" };
            deleteMenuItem.Click += async (s, e) => await DeleteMessage(item.Id, border);

            contextMenu.Items.Add(deleteMenuItem);
            border.ContextMenu = contextMenu;

            panel.Children.Add(new TextBlock
            {
                Text = item.Message,
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            });

            panel.Children.Add(new TextBlock
            {
                Text = DateTime.Parse(item.Date).ToLocalTime().ToString("HH:mm"),
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 255, 255, 255)),
                FontSize = 10,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Margin = new Thickness(0, 4, 0, 0)
            });

            panel.Children.Add(new TextBlock
            {
                Text = $"{item.Id}",
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 255, 255, 255)),
                FontSize = 10,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Margin = new Thickness(0, 4, 0, 0)
            });

            border.Child = panel;
            return border;
        }

        private async Task DeleteMessage(int id, Border border)
        {
            try
            {
                await _PostRequestDeleteConcrectEsaage.RequestDeleteConcret(id);

                await Dispatcher.InvokeAsync(() =>
                {
                    if (MessagesItemsControl.Items.Contains(border))
                    {
                        MessagesItemsControl.Items.Remove(border);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при удалении: {ex.Message}",
      "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public Border AddMessageFile(bool isCurrentUser, AttachmentMetadata attachmentMetadata)
        {
            Border messageBorder = new Border
            {
                CornerRadius = new CornerRadius(15),
                Margin = new Thickness(5, 5, 5, 10),
                MaxWidth = 350,
                HorizontalAlignment = isCurrentUser ? System.Windows.HorizontalAlignment.Right : System.Windows.HorizontalAlignment.Left,
                Background = isCurrentUser ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 122, 255)) : new SolidColorBrush(System.Windows.Media.Color.FromRgb(229, 229, 234)),
                Padding = new Thickness(10, 8, 10, 8)
            };

            Grid contentGrid = new Grid();
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            TextBlock iconText = new TextBlock
            {
                Text = "📁",
                FontSize = 32,
                Margin = new Thickness(0, 0, 12, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            StackPanel infoPanel = new StackPanel();

            TextBlock fileNameText = new TextBlock
            {
                Text = attachmentMetadata.FileName,
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 250
            };

            TextBlock LenghtText = new TextBlock
            {
                Text = $"{attachmentMetadata.FileSize}",
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 250
            };
            System.Windows.MessageBox.Show($"Неgfd");
            if (!isCurrentUser)
            {
                System.Windows.Controls.Button button = new System.Windows.Controls.Button()
                {
                    Width = 40,
                    Height = 40,
                    Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(229, 229, 234)),
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(2),
                    Padding = new Thickness(0)
                };

                TextBlock downloadIcon = new TextBlock
                {
                    Text = "🔽",
                    FontSize = 20,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                button.Content = downloadIcon;
                infoPanel.Children.Add(button);
                button.Click += (s, e) =>
                {
                    //Вызов метода Post метода для скачивания файла
                };

            }
            if (isCurrentUser)
            {
                System.Windows.Controls.Button button = new System.Windows.Controls.Button()
                {
                    Width = 40,
                    Height = 40,
                    Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 122, 255)),
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(2),
                    Padding = new Thickness(0)
                };

                TextBlock downloadIcon = new TextBlock
                {
                    Text = "📁",
                    FontSize = 20,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                button.Content = downloadIcon;
                infoPanel.Children.Add(button);


                button.Click += (s, e) =>
                {
                    try
                    {
                        if (_localpath != null && _localpath != "")
                        {
                            if (File.Exists(_localpath))
                            {
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = _localpath,
                                    UseShellExecute = true,
                                });
                                _localpath = "";
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show($"Путь до файла не найден");
                            _localpath = "";
                        }
                    }
                    catch(Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Не удалось открыть файл: {ex.Message}");
                        _localpath = "";
                    }
                };
            }

            infoPanel.Children.Add(fileNameText);
            infoPanel.Children.Add(LenghtText);

            Grid.SetColumn(iconText, 0);
            Grid.SetColumn(infoPanel, 1);

            contentGrid.Children.Add(iconText);
            contentGrid.Children.Add(infoPanel);

            messageBorder.Child = contentGrid;

            MessagesItemsControl.Items.Add(messageBorder);

            return messageBorder;
        }
        public void AddMessageImage(string user, string message, bool isCurrentUser, AttachmentMetadata attachment)
        {

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
                                    _PostRequestAddFirstUserContact,
                                    _onlineUser,
                                    _PostRequestDeleteChatHistory);
            MainWindow.Show();
            this.Close();
        }

        private async void  SendFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.Title = "Выберите файл для отправки";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            { 
                string filepath = openFileDialog.FileName;
                Dispatcher.Invoke(() =>
                {
                    Border messageBorder = null;

                    messageBorder = new Border
                    {
                        CornerRadius = new CornerRadius(12),
                        Margin = new Thickness(40, 5, 5, 5), 
                        MaxWidth = 350,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80)) // Material Design Green (не сливается)
                    };

                    StackPanel messagePanel = new StackPanel
                    {
                        Margin = new Thickness(8, 6, 12, 6) 
                    };

                    StackPanel contentPanel = new StackPanel
                    {
                        Orientation = System.Windows.Controls.Orientation.Horizontal,
                        Margin = new Thickness(0)
                    };

                    TextBlock fileNameText = new TextBlock
                    {
                        Text = System.IO.Path.GetFileName(filepath), 
                        Foreground = System.Windows.Media.Brushes.White,
                        FontSize = 13,
                        FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                        TextWrapping = TextWrapping.Wrap,
                        MaxWidth = 280,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    System.Windows.Controls.Button deleteBtn = new System.Windows.Controls.Button
                    {
                        Content = "✕", 
                        Background = System.Windows.Media.Brushes.Transparent,
                        Foreground = System.Windows.Media.Brushes.White,
                        Width = 24,
                        Height = 24,
                        Margin = new Thickness(8, 0, 0, 0),
                        Padding = new Thickness(0),
                        BorderThickness = new Thickness(0),
                        Tag = filepath,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 14,
                        FontWeight = FontWeights.Bold
                    };

                    deleteBtn.MouseEnter += (s, e) =>
                    {
                        _checkFile = null;
                        MessagesItemsControl.Items.Remove(messageBorder);
                    };
                    _checkFile = messageBorder;
                    messagePanel.Children.Add(deleteBtn);
                    messagePanel.Children.Add(fileNameText);
                    messageBorder.Child = messagePanel;
                    MessagesItemsControl.Items.Add(messageBorder);

                });
                var result = await _PostRequestHistorySaveFile.ReqoestSAVE(filepath, _user, _username);
                _localpath = filepath;
                System.Windows.MessageBox.Show($"{result.metadata.Id}");
                ChatState.PendingAttachment = result.metadata;

            }
        }
    }
}
