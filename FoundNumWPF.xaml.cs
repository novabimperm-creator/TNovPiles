using System.Windows;

namespace TNovPiles
{
    /// <summary>
    /// Логика взаимодействия для FoundNumWPF.xaml
    /// </summary>
    public partial class FoundNumWPF : Window
    {
        public FoundNumWPF(FoundNumViewModel viewModel)
        {
            InitializeComponent();
            textBox1.Focus();
            DataContext = viewModel;
            
            this.SizeToContent = SizeToContent.Height;
            
        }
        /*
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        */


        private void escButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close(); // закрытие окна
        }

        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            string commandText = @"https://portal.talan.group/knowledge/proektirovanie/svai_xmqe/";
            var proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = commandText;
            proc.StartInfo.UseShellExecute = true;
            proc.Start();
        }
    }
}
