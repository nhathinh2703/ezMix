using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace Desktop.Helpers
{
    public static class FileHelper
    {
        public static string? BrowseFile(string title = "Chọn file Word", string filter = "Word Documents (*.docx)|*.docx|All Files (*.*)|*.*")
        {
            var dialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                CheckFileExists = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public static string? BrowsePdf()
        {
            return BrowseFile("Chọn tệp PDF", "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*");
        }

        public static string? BrowseImage()
        {
            return BrowseFile("Chọn tệp ảnh", "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All Files (*.*)|*.*");
        }

        public static void OpenFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                MessageHelper.Error("Tệp không tồn tại");
                return;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true // mở bằng ứng dụng mặc định của Windows
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageHelper.Error($"Không thể mở tệp: {ex.Message}");
            }
        }
    }
}
