using MessangersUI.DataModel;
using MessangersUI.HttpReuest.PostRequestAvatar;
using System;
using System.Collections.Generic;
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
        
        public Settings(PostMethodAvatar postMethodAvatar, string username)
        {
            InitializeComponent();

            _postMethodAvatar = postMethodAvatar;
            _avatarmetadata = new AvatarMetaData();
            _username = username;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_username))
                return;

            AvatarMetaData avatarMetaData = new AvatarMetaData();
        
            using OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Изображения (*.jpg;*.png;*.bmp)|*.jpg;*.png;*.bmp";
            openFileDialog.Title = "Выберите изображение ";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ReadOnlySpan<char> filepath = openFileDialog.FileName.AsSpan();

                string filepathString = filepath.ToString();

                _avatarmetadata.Filepath = filepathString ?? string.Empty;
                avatarMetaData.expansion = System.IO.Path.GetExtension(filepathString ?? string.Empty);

                if (avatarMetaData.expansion != ".jpg" || avatarMetaData.expansion != ".png")
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

                    if (sizemb > maxsize)
                    {
                        this.Loaded += async (s, e) =>
                        {
                            await _postMethodAvatar.RequestMethod(avatarMetaData).ConfigureAwait(false);
                        };
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
    }
}
