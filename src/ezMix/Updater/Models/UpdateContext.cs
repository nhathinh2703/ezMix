namespace Updater.Models
{
    public class UpdateContext
    {
        public string Url { get; set; } = default!;         // Link file .zip cần tải về
        public string FileName { get; set; } = default!;    // Tên file trong zip cần ghi đè
        public string AppExe { get; set; } = default!;      // Đường dẫn process cần chạy lại
    }

}
