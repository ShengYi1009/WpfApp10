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
        Color fontColor = Colors.Black; //預設字體顏色為黑色
        public MyDocumentViewer()
        {
            InitializeComponent();
            FontColorPicker.SelectedColor = fontColor; //設定預設選取顏色
            foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
            {
                FontFamilyComboBox.Items.Add(fontFamily.Source); //將系統字型加入下拉選單
            }
            FontFamilyComboBox.SelectedIndex = 1; //設定預設選取字型
            FontSizeComboBox.ItemsSource = new List<double>() //設定字型大小選單
            {
                8,9,10,11,12,14,16,18,20,22,24,26,28,36,48,72
            };
            FontSizeComboBox.SelectedIndex = 4; //設定預設選取字型大小
        }
    }
}
