using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ProductionApp.Web.Helpers
{
    public class FileHelper
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileHelper(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        // Hàm Upload file
        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0) return null;

            // 1. Tạo đường dẫn thư mục: wwwroot/uploads/folderName
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folderName);

            // Tạo thư mục nếu chưa có
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            // 2. Tạo tên file độc nhất (Tránh bị trùng tên đè file)
            // Cấu trúc: TênGốc_TimeStamp.Ext
            string uniqueFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";

            // 3. Đường dẫn vật lý đầy đủ
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 4. Lưu file vào ổ cứng
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // 5. Trả về đường dẫn tương đối để lưu vào DB (VD: /uploads/orders/abc.jpg)
            return $"/uploads/{folderName}/{uniqueFileName}";
        }

        // Hàm Xóa file (Dùng khi xóa bản ghi trong DB thì xóa luôn file cho sạch rác)
        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            // Chuyển đường dẫn web (/uploads/...) thành đường dẫn vật lý (C:\Web\wwwroot\uploads\...)
            string absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));

            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }
        }
    }
}