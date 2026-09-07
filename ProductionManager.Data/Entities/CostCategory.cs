using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ProductionManager.Data
{
    // CÁI NÀY LÀ MỎ NEO CỦA HỆ THỐNG P&L (Dùng để làm công thức báo cáo)
    public enum PnlSection
    {
        [Display(Name = "Không hạch toán OPEX (Thu tiền/Vật tư)")]
        Exclude = 0,
        [Display(Name = "Nhóm Lương & Bảo hiểm")]
        Salary = 1,
        [Display(Name = "Nhóm Điện nước, Cố định")]
        Utilities = 2,
        [Display(Name = "Nhóm Vận chuyển & Logistics")]
        Logistics = 3,
        [Display(Name = "Nhóm Văn phòng & Hành chính")]
        Office = 4,
        [Display(Name = "Nhóm Marketing & Bán hàng")]
        Marketing = 5,
        [Display(Name = "Nhóm Vật Tư Không Nhập Kho")]
        Material = 6,
        [Display(Name = "Nhóm Chi phí Khác")]
        OtherOpex = 99
    }

    // CÁI NÀY LÀ BẢNG ĐỂ NGƯỜI DÙNG TỰ THÊM/BỚT
    public class CostCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tên loại chi phí")]
        public string Name { get; set; } // VD: "Tiền rác", "Quảng cáo Google"

        [Display(Name = "Thuộc nhóm trên Báo Cáo P&L")]
        public PnlSection Section { get; set; } // Map về mỏ neo ở trên

        public bool IsActive { get; set; } = true;

        public string? Description { get; set; }

        // Mối quan hệ 1-N với Sổ quỹ
        public virtual ICollection<CashEntry>? CashEntries { get; set; }
    }
}