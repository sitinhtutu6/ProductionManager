using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    public class ProductDocument
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } // Tên tài liệu (VD: Bản vẽ v1.2, Hình ảnh thực tế)

        public string? Note { get; set; } // Ghi chú thay đổi

        public string? FilePath { get; set; } // Đường dẫn lưu: sanpham/2026/filename.pdf
        public string? FileExtension { get; set; } // Đuôi file (.pdf, .dwg, .jpg)

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true; // Trạng thái: True = Đang dùng, False = Đã xóa (Soft Delete)

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}