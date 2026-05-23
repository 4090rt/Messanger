using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Логика взаимодействия для main.xaml
    /// </summary>
    public partial class main : Window
    {
        public main()
        {
            InitializeComponent();

        }

        ////private void MessageTextBox_PreviewKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        ////{
        ////    if (e.KeyValue == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
        ////    {
        ////        e.Handled = true;
        ////        SendMessage();
        ////    }
        //}

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
    }
}
