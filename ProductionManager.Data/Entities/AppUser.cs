using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace ProductionManager.Data
{
    // Kế thừa IdentityUser để có sẵn tính năng đăng nhập, phân quyền chuẩn của Microsoft
    public class AppUser : IdentityUser
    {
        [MaxLength(100)]
        public string FullName { get; set; } = ""; // Tên đầy đủ (VD: Nguyễn Văn A)

        public string Address { get; set; } = ""; // Địa chỉ

        public bool IsActive { get; set; } = true; // Trạng thái hoạt động (True = Cho phép đăng nhập)

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo tài khoản
    }
}