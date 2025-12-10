# ezMix Desktop

Ứng dụng hỗ trợ xử lý và thao tác với tài liệu Word, được xây dựng trên nền tảng .NET và WPF, sử dụng OpenXML SDK để định dạng và phân tích nội dung.

## ✨ Tính năng chính

### 1. Chuẩn hóa
- Tự động định dạng đoạn văn bản theo chuẩn:
  - Font chữ (Times New Roman, Arial, …).
  - Cỡ chữ (10pt – 20pt).
  - Spacing (0pt trước/sau, line spacing 1.2).
- Đảm bảo tài liệu đồng bộ về hình thức, phù hợp quy định trình bày.

### 2. Phân tích
- Đọc và phân tích cấu trúc tài liệu Word.
- Trích xuất thông tin quan trọng từ đoạn văn, câu hỏi, đáp án.
- Hỗ trợ người dùng kiểm tra nhanh bố cục và nội dung.

### 3. Trộn đề
- Tự động trộn câu hỏi để tạo nhiều phiên bản đề thi khác nhau.
- Giữ nguyên định dạng chuẩn khi trộn.
- Giúp tiết kiệm thời gian soạn đề và đảm bảo tính ngẫu nhiên, công bằng.

## 🛠 Công nghệ
- **.NET 10.0 (Windows target)**  
- **WPF (Windows Presentation Foundation)** cho giao diện.  
- **OpenXML SDK** để thao tác và định dạng tài liệu Word.  
- **PowerShell** script (`Build.ps1`) để build và publish.  
- **MVVM pattern**
