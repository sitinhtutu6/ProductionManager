using System.Collections.Generic;

namespace ProductionApp.Web.Models
{
    public class ManageUserRolesViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public List<UserRoleSelection> Roles { get; set; } = new List<UserRoleSelection>();
    }

    public class UserRoleSelection
    {
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}