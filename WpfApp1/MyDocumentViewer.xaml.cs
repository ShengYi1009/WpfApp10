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

namespace WpfApp1
{
    /// <summary>
    /// MyDocumentViewer.xaml 的互動邏輯
    /// </summary>
    public partial class MyDocumentViewer : Window
    {
        Color fontColor = Colors.Black;
        public MyDocumentViewer()
        {
            InitializeComponent();
            FontColorPicker.SelectedColor = fontColor;
            foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
            {
                FontFamilyComboBox.Items.Add(fontFamily.Source);
            }
            FontFamilyComboBox.SelectedIndex = 1;

            FontSizeComboBox.ItemsSource = new List<double>()
            {
                8,9,10,11,12,14,16,18,20,22,24,26,28,36,48,72
            };
            FontSizeComboBox.SelectedIndex = 4;
        }
    }

}
