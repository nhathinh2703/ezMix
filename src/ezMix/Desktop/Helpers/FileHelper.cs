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
                Title = title,          // Tiêu đề hộp thoại
                Filter = filter,        // Bộ lọc loại file (Word, PDF, ảnh,...)
                CheckFileExists = true, // Kiểm tra file có tồn tại không
                //InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) // Thư mục mặc định
            };

            // Nếu người dùng chọn file → trả về đường dẫn, ngược lại trả về null
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
            // ✅ Kiểm tra đường dẫn file có hợp lệ không
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                // Nếu đường dẫn rỗng hoặc file không tồn tại → báo lỗi
                MessageHelper.Error("Tệp không tồn tại");
                return;
            }

            try
            {
                // ⚙️ Tạo đối tượng ProcessStartInfo để mở file
                var psi = new ProcessStartInfo
                {
                    FileName = filePath,   // Đường dẫn file cần mở
                    UseShellExecute = true // Cho phép Windows mở bằng ứng dụng mặc định
                };

                // 🚀 Thực thi mở file
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                // ❌ Nếu có lỗi trong quá trình mở file → hiển thị thông báo lỗi
                MessageHelper.Error($"Không thể mở tệp: {ex.Message}");
            }
        }
    }
}
