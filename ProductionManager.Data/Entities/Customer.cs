using System.ComponentModel.DataAnnotations;

namespace ProductionManager.Data
{
    public enum PartnerType
    {
        Customer = 1, // Khách hàng
        Supplier = 2, // Nhà cung cấp
        Both = 3      // Cả hai
    }

    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string CustomerCode { get; set; } // Mã KH (VD: KH001)

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; }

        public PartnerType Type { get; set; } = PartnerType.Customer;

        [StringLength(50)]
        public string? TaxCode { get; set; }

        [StringLength(50)]
        public string? BankAccount { get; set; }

        [StringLength(100)]
        public string? BankName { get; set; }

        public string? ContactPerson { get; set; } // Dấu ? nghĩa là được phép null
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }

        // Danh sách đơn hàng của khách này
        public ICollection<Order>? Orders { get; set; }
    }
}