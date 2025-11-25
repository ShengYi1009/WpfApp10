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
    public partial class MyDocumentViewer : Window
    {
        Color fontColor = Colors.Black;
        Color backgroundColor = Colors.White;

        public MyDocumentViewer()
        {
            InitializeComponent();

            // 初始化設定
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

            // 🔔 初始狀態列訊息
            UpdateStatusBar("應用程式已啟動。");
        }

        /// <summary>
        /// 🔔 更新狀態列主要訊息的簡化方法。
        /// </summary>
        /// <param name="message">要顯示在狀態列上的文字。</param>
        private void UpdateStatusBar(string message)
        {
            // 將訊息設定給 XAML 中名為 ApplicationLabel 的 Label 控制項
            ApplicationLabel.Content = message;
        }

        /// <summary>
        /// 🔔 更新狀態列次要訊息 (例如格式資訊) 的簡化方法。
        /// </summary>
        /// <param name="message">要顯示在狀態列次要區域上的文字。</param>
        private void UpdateFormatStatus(string message)
        {
            // 將訊息設定給 XAML 中名為 TextFormatLabel 的 Label 控制項
            TextFormatLabel.Content = message;
        }

        private string ConvertRtfToHtml(RichTextBox richTextBox)
        {
            // 將 RichTextBox 的內容儲存為 XAML 格式 (WPF 的內部格式)
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

            // 使用內建的 Helper 類別將 XAML 轉換為 HTML

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
            // 🔔 更新狀態列
            UpdateFormatStatus($"字體顏色已變更為: {fontColor.ToString()}");
        }

        private void BackgroundColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            // 取得新的選定顏色
            backgroundColor = (Color)e.NewValue;

            // 建立一個 SolidColorBrush
            SolidColorBrush backgroundBrush = new SolidColorBrush(backgroundColor);

            // 將這個筆刷應用到 RichTextBox 整個文件的背景
            MainRichTextBox.Document.Background = backgroundBrush;
            // 🔔 更新狀態列
            UpdateFormatStatus($"文件背景顏色已變更為: {backgroundColor.ToString()}");
        }


        private void FontFamilyComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FontFamilyComboBox.SelectedItem != null)
            {
                MainRichTextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, FontFamilyComboBox.SelectedItem);
                // 🔔 更新狀態列
                UpdateFormatStatus($"字體已變更為: {FontFamilyComboBox.SelectedItem.ToString()}");
            }
        }

        private void FontSizeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FontSizeComboBox.SelectedItem != null)
            {
                MainRichTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, FontSizeComboBox.SelectedItem);
                // 🔔 更新狀態列
                UpdateFormatStatus($"字體大小已變更為: {FontSizeComboBox.SelectedItem.ToString()} 點");
            }
        }

        private void NewCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            MyDocumentViewer myDocumentViewer = new MyDocumentViewer();
            myDocumentViewer.Show();
            // 🔔 更新狀態列
            UpdateStatusBar("已開啟一個新的文件視窗。");
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
                try
                {
                    using (FileStream fileStream = new FileStream(openDialog.FileName, FileMode.Open))
                    {
                        TextRange range = new TextRange(MainRichTextBox.Document.ContentStart, MainRichTextBox.Document.ContentEnd);
                        range.Load(fileStream, DataFormats.Rtf);
                    }
                    // 🔔 更新狀態列
                    UpdateStatusBar($"文件已成功開啟: {openDialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"開啟檔案時發生錯誤: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    // 🔔 更新狀態列 (錯誤訊息)
                    UpdateStatusBar($"開啟檔案失敗: {ex.Message}");
                }
            }
            else
            {
                // 🔔 更新狀態列 (取消操作)
                UpdateStatusBar("取消開啟檔案操作。");
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
                int filterIndex = saveDialog.FilterIndex; // 1 = RTF, 2 = HTML, 3 = All files

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
                        // 🔔 更新狀態列
                        UpdateStatusBar($"RTF 文件已成功儲存至: {fileName}");
                    }
                    else if (filterIndex == 2) // 選擇 HTML 格式
                    {
                        // 呼叫轉換方法，將內容轉成 HTML 字串
                        string htmlContent = ConvertRtfToHtml(MainRichTextBox);
                        // 使用 UTF8 編碼儲存，以確保中文或特殊字元不會亂碼
                        File.WriteAllText(fileName, htmlContent, Encoding.UTF8);

                        // 🔔 更新狀態列
                        UpdateStatusBar($"HTML 文件已成功儲存至: {fileName}");
                    }
                    else // 其他檔案類型，例如「所有檔案 (*.*)」，我們預設使用 RTF 儲存
                    {
                        using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                        {
                            TextRange range = new TextRange(MainRichTextBox.Document.ContentStart, MainRichTextBox.Document.ContentEnd);
                            range.Save(fileStream, DataFormats.Rtf);
                        }
                        // 🔔 更新狀態列
                        UpdateStatusBar($"文件已儲存 (預設 RTF 格式): {fileName}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"儲存檔案時發生錯誤: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    // 🔔 更新狀態列 (錯誤訊息)
                    UpdateStatusBar($"儲存失敗: {ex.Message}");
                }
            }
            else
            {
                // 🔔 更新狀態列 (取消操作)
                UpdateStatusBar("取消儲存檔案操作。");
            }
        }

        private void ClearFileButton_Click(object sender, RoutedEventArgs e)
        {
            MainRichTextBox.Document.Blocks.Clear();
            // 🔔 更新狀態列
            UpdateStatusBar("文件內容已清除。");
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
                BackgroundColorPicker.SelectedColor = bgBrush.Color;
            }

            // 🔔 當選取範圍改變時，更新 TextFormatLabel 顯示當前的格式狀態
            string currentFont = (property_fontfamily as FontFamily)?.Source ?? "預設字體";
            string currentSize = property_fontsize?.ToString() ?? "預設大小";
            UpdateFormatStatus($"當前格式: {currentFont}, {currentSize} 點");
        }
    }

}