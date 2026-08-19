using MessangersUI.DataModel;
using MessangersUI.HttpReuest.PostRequestAvatar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    public partial class Settings : Window
    {
        public long maxsize = 5242880;
        public long onemb = 1048576;

        public AvatarMetaData _avatarmetadata;
        private readonly PostMethodAvatar _postMethodAvatar;
        private readonly string _username;
        private RequestAvatarUsing _avatarUsing;
        private MainWindow _mainWindow;

        public Settings(PostMethodAvatar postMethodAvatar, string username, RequestAvatarUsing avatarUsing, MainWindow mainWindow)
        {
            InitializeComponent();

            _postMethodAvatar = postMethodAvatar;
            _avatarmetadata = new AvatarMetaData();
            _username = username;
            _avatarUsing = avatarUsing;
            _mainWindow = mainWindow;


            UsernameText.Content = _username;
            this.Loaded += async (s, e) =>
            {
                await GiveImageAcatar();
            };
        }

        public async Task GiveEmailAndNumber()
        {
            try
            {
                if (string.IsNullOrEmpty(_username))
                    return;
            }
            catch(Exception ex) 
            {
                
            }
        }

        public async Task GiveImageAcatar()
        {
            try
            {
                if (string.IsNullOrEmpty(_username))
                    return;

                AvatarStructure avatarStructure = await _avatarUsing.Request(_username).ConfigureAwait(false);
                if (avatarStructure.State != null && avatarStructure.Data.Length > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        byte[] bytes = avatarStructure.Data.ToArray();
                        BitmapImage bitmapImage = new BitmapImage();

                        using (MemoryStream stream = new MemoryStream(bytes))
                        {
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = stream;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.EndInit();
                        }
                        ImageAvatar.Source = bitmapImage;
                    });
                }
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Возникло исключение при попытку отобразить аватар {ex.Message}");
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_username))
                return;

            AvatarMetaData avatarMetaData = new AvatarMetaData();
        
            using OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Изображения (*.jpg;*.png;*.bmp)|*.jpg;*.png;*.bmp";
            openFileDialog.Title = "Выберите изображение ";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ReadOnlySpan<char> filepath = openFileDialog.FileName.AsSpan();

                string filepathString = filepath.ToString();

                avatarMetaData.expansion = System.IO.Path.GetExtension(filepathString ?? string.Empty);


                if (avatarMetaData.expansion != ".jpg" && avatarMetaData.expansion != ".png")
                {
                    System.Windows.MessageBox.Show("Неверный формат");
                }

                FileInfo file = new FileInfo(filepathString ?? string.Empty);

                long size = file?.Length ?? 0;

                if (size != 0)
                {
                    long sizemb = size * onemb;

                    avatarMetaData.FileSize = sizemb;
                    avatarMetaData.UserName = _username;
                    avatarMetaData.Filepath = filepathString ?? string.Empty;

                    if (sizemb > maxsize)
                    {
                        await _postMethodAvatar.RequestMethod(avatarMetaData).ConfigureAwait(false);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Слишком большой размер < 5 мб");
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("не удалось получить иозображение");
                }
            }
        }

        private void CloseSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.Show();
            this.Close();
        }
    }
}
