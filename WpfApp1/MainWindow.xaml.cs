using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            MyDocumentViewer myDocument = new MyDocumentViewer(); //建立新視窗物件
            myDocument.Show(); //用Show方式開啟新視窗
            //myDocument.ShowDialog(); <---要把子視窗關掉控制權才會回到主視窗
        }
    }
}