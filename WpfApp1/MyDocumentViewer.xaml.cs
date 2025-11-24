using Microsoft.Win32;
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

namespace WpfApp1
{
    /// <summary>
    /// MyDocumentViewer.xaml 的互動邏輯
    /// </summary>
    /// 
    public partial class MyDocumentViewer : Window
    {
        Color fontColor = Colors.Black;
        Color backgroundColor = Colors.White;

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
            BackgroundColorPicker.SelectedColor = Colors.White;
        }

        private string ConvertRtfToHtml(RichTextBox richTextBox)
        {
            // 1. 將 RichTextBox 的內容儲存為 XAML 格式 (WPF 的內部格式)
            string xamlText = string.Empty;
            TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);

            // 為了將內容儲存到字串中，我們需要一個 MemoryStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // 將 TextRange 儲存為 XAML 格式
                range.Save(memoryStream, DataFormats.Xaml);

                // 從 MemoryStream 讀取 XAML 字串
                memoryStream.Position = 0;
                using (StreamReader reader = new StreamReader(memoryStream))
                {
                    xamlText = reader.ReadToEnd();
                }
            }

            // 2. 使用內建的 Helper 類別將 XAML 轉換為 HTML
            // 由於我們不能直接從 DataFormats.Xaml 轉到 DataFormats.Html，
            // 我們可以利用 Clipboard 幫忙做間接轉換 (這是一種常見的做法，但有點笨拙)

            // 暫時將 XAML 內容放入剪貼簿
            Clipboard.SetData(DataFormats.Xaml, xamlText);

            // 嘗試從剪貼簿以 HTML 格式取出 (如果剪貼簿有 HTML 格式的話)
            if (Clipboard.ContainsData(DataFormats.Html))
            {
                string html = Clipboard.GetData(DataFormats.Html).ToString();
                // 清除剪貼簿，避免影響其他程式
                Clipboard.Clear();
                return html;
            }
            else
            {
                // 如果無法轉換成 HTML (例如內容太複雜)，我們至少返回一個基本的字串
                Clipboard.Clear();
                return $"<html><body><pre>{range.Text}</pre></body></html>";
            }
        }

        private void FontColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            fontColor = (Color)e.NewValue;
            SolidColorBrush bontBrush = new SolidColorBrush(fontColor);
            MainRichTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, bontBrush);
        }

        private void BackgroundColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            // 1. 取得新的選定顏色
            backgroundColor = (Color)e.NewValue;

            // 2. 建立一個 SolidColorBrush
            SolidColorBrush backgroundBrush = new SolidColorBrush(backgroundColor);

            // 3. 將這個筆刷應用到 RichTextBox 整個文件的背景
            // 這裡我們需要操作 Document 的第一層容器 (FlowDocument)
            // 由於 RichTextBox 預設只有一個 Block (通常是 Paragraph)，我們可以對整個 Document 應用
            MainRichTextBox.Document.Background = backgroundBrush;
        }


        private void FontFamilyComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FontFamilyComboBox.SelectedItem != null)
            {
                MainRichTextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, FontFamilyComboBox.SelectedItem);
            }
        }

        private void FontSizeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FontSizeComboBox.SelectedItem != null)
            {
                MainRichTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, FontSizeComboBox.SelectedItem);
            }
        }

        private void NewCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            MyDocumentViewer myDocumentViewer = new MyDocumentViewer();
            myDocumentViewer.Show();
        }

        private void OpenCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog()
            {
                Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*",
                DefaultExt = ".rtf",
                AddExtension = true
            };

            if (openDialog.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(openDialog.FileName, FileMode.Open);
                TextRange range = new TextRange(MainRichTextBox.Document.ContentStart, MainRichTextBox.Document.ContentEnd);
                range.Load(fileStream, DataFormats.Rtf);
                fileStream.Close();
            }
        }

        private void SaveCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog()
            {
                Filter = "Rich Text Format (*.rtf)|*.rtf|HTML File (*.html)|*.html|All files (*.*)|*.*",
                DefaultExt = ".rtf",
                AddExtension = true
            };
                if (saveDialog.ShowDialog() == true)
                {
                    // 取得使用者選擇的檔案名稱和篩選器索引 (FilterIndex)
                    string fileName = saveDialog.FileName;
                    int filterIndex = saveDialog.FilterIndex; // 1 = RTF, 2 = HTML

                    try
                    {
                        if (filterIndex == 1) // 選擇 RTF 格式
                        {
                            // 使用您原有的 RTF 儲存邏輯
                            using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                            {
                                TextRange range = new TextRange(MainRichTextBox.Document.ContentStart, MainRichTextBox.Document.ContentEnd);
                                range.Save(fileStream, DataFormats.Rtf);
                            }
                        }
                        else if (filterIndex == 2) // 選擇 HTML 格式
                        {
                        // 呼叫轉換方法，將內容轉成 HTML 字串
                        string htmlContent = ConvertRtfToHtml(MainRichTextBox);
                        File.WriteAllText(fileName, htmlContent, Encoding.UTF8);

                        // 將 HTML 內容寫入檔案
                        File.WriteAllText(fileName, htmlContent);
                        }
                        else // 其他檔案類型，可以根據 DefaultExt 設為 RTF
                        {
                            // 這是為了處理「所有檔案 (*.*)」的情況，我們仍然使用 RTF 儲存
                            using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                            {
                                TextRange range = new TextRange(MainRichTextBox.Document.ContentStart, MainRichTextBox.Document.ContentEnd);
                                range.Save(fileStream, DataFormats.Rtf);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"儲存檔案時發生錯誤: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

        private void ClearFileButton_Click(object sender, RoutedEventArgs e)
        {
            MainRichTextBox.Document.Blocks.Clear();
        }

        private void MainRichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var property_bold = MainRichTextBox.Selection.GetPropertyValue(TextElement.FontWeightProperty);
            if (property_bold != null) BoldButton.IsChecked = (property_bold != DependencyProperty.UnsetValue) && (property_bold.Equals(FontWeights.Bold));

            var property_italic = MainRichTextBox.Selection.GetPropertyValue(TextElement.FontStyleProperty);
            if (property_italic != null) ItalicButton.IsChecked = (property_italic != DependencyProperty.UnsetValue) && (property_italic.Equals(FontStyles.Italic));

            var property_underline = MainRichTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            if (property_underline != null) UnderlineButton.IsChecked = (property_underline != DependencyProperty.UnsetValue) && (property_underline.Equals(TextDecorations.Underline));

            var property_fontcolor = MainRichTextBox.Selection.GetPropertyValue(TextElement.ForegroundProperty);
            if (property_fontcolor != null && property_fontcolor is SolidColorBrush brush)
                FontColorPicker.SelectedColor = brush.Color;

            var property_fontfamily = MainRichTextBox.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            if (property_fontfamily != null) FontFamilyComboBox.SelectedItem = property_fontfamily.ToString();

            var property_fontsize = MainRichTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            if (property_fontsize != null) FontSizeComboBox.SelectedItem = property_fontsize;

            var property_background = MainRichTextBox.Document.Background;
            if (property_background != null && property_background is SolidColorBrush bgBrush)
            {
                // 假設你的背景顏色選擇器命名為 BackgroundColorPicker
                BackgroundColorPicker.SelectedColor = bgBrush.Color;
            }
        }
    }

}
