using System.Collections.Generic;

namespace ProductionApp.Web.Models
{
    // Dùng để hiển thị danh sách User + Role
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; } // Dùng làm tên đăng nhập
        public string FullName { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}