using System.ComponentModel.DataAnnotations;

namespace ProductionManager.Data
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; } = true;
    }
}