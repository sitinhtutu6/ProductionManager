using System;
using System.ComponentModel.DataAnnotations;

namespace ProductionManager.Data
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; } // Tên file gốc (VD: HopDong_A.pdf)

        public string SystemFileName { get; set; } // Tên file trên ổ cứng (VD: guid_HopDong_A.pdf)

        public string FilePath { get; set; } // Đường dẫn (VD: /uploads/documents/...)

        public string Group { get; set; } = "Khác"; // Tự động nhận diện: Hợp đồng, Bản vẽ, Hóa đơn...

        public long FileSize { get; set; } // Kích thước (KB)

        public string UploadedBy { get; set; } // Người upload

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}