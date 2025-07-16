using System.ComponentModel;
using System.Windows;

namespace DetailViewer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // Отменяем закрытие окна
            e.Cancel = true;
            // Скрываем окно вместо закрытия
            Hide();
        }
    }
}