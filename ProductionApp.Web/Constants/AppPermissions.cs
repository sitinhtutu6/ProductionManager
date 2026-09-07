using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ProductionApp.Web.Constants
{
    public static class AppPermissions
    {
        public const string ClaimType = "Permission";

        // =========================================================================
        // KHU VỰC 1: KHAI BÁO MODULE & QUYỀN (Thêm/Sửa module tại đây)
        // =========================================================================

        public static class Products
        {
            public const string View = "Permissions.Products.View";
            public const string Create = "Permissions.Products.Create";
            public const string Edit = "Permissions.Products.Edit";
            public const string Delete = "Permissions.Products.Delete";
        }

        public static class Orders
        {
            public const string View = "Permissions.Orders.View";
            public const string Create = "Permissions.Orders.Create";
            public const string Edit = "Permissions.Orders.Edit";
            public const string Delete = "Permissions.Orders.Delete";
            public const string Approve = "Permissions.Orders.Approve"; // Quyền riêng
        }

        public static class Documents
        {
            public const string View = "Permissions.Documents.View";
            public const string Upload = "Permissions.Documents.Create"; // Mapping Upload = Create
            public const string Delete = "Permissions.Documents.Delete";
        }

        // --- Các module khác (Copy paste và đổi tên) ---
        public static class Warehouse
        {
            public const string View = "Permissions.Warehouse.View";
            public const string Create = "Permissions.Warehouse.Create";
            public const string Edit = "Permissions.Warehouse.Edit";
            public const string Delete = "Permissions.Warehouse.Delete";
        }

        public static class Customers
        {
            public const string View = "Permissions.Customers.View";
            public const string Create = "Permissions.Customers.Create";
            public const string Edit = "Permissions.Customers.Edit";
            public const string Delete = "Permissions.Customers.Delete";
        }

        public static class Users { public const string View = "Permissions.Users.View"; public const string Create = "Permissions.Users.Create"; public const string Edit = "Permissions.Users.Edit"; public const string Delete = "Permissions.Users.Delete"; }
        public static class Roles { public const string View = "Permissions.Roles.View"; public const string Create = "Permissions.Roles.Create"; public const string Edit = "Permissions.Roles.Edit"; public const string Delete = "Permissions.Roles.Delete"; }
        public static class Debts { public const string View = "Permissions.Debts.View"; public const string Create = "Permissions.Debts.Create"; public const string Edit = "Permissions.Debts.Edit"; public const string Delete = "Permissions.Debts.Delete"; }
        public static class Plans { public const string View = "Permissions.Plans.View"; public const string Create = "Permissions.Plans.Create"; public const string Edit = "Permissions.Plans.Edit"; public const string Delete = "Permissions.Plans.Delete"; }
        public static class Norms { public const string View = "Permissions.Norms.View"; public const string Create = "Permissions.Norms.Create"; public const string Edit = "Permissions.Norms.Edit"; public const string Delete = "Permissions.Norms.Delete"; }
        public static class Shipments { public const string View = "Permissions.Shipments.View"; public const string Create = "Permissions.Shipments.Create"; public const string Edit = "Permissions.Shipments.Edit"; public const string Delete = "Permissions.Shipments.Delete"; }


        // =========================================================================
        // LOGIC TỰ ĐỘNG 
        // =========================================================================
        public static List<string> GetAllPolicyNames()
        {
            var allPermissions = new List<string>();

            // Lấy tất cả các class con (Products, Orders, Documents...)
            var nestedTypes = typeof(AppPermissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

            foreach (var type in nestedTypes)
            {
                // Lấy tất cả biến const string trong class đó
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                 .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string));

                foreach (var field in fields)
                {
                    var value = field.GetValue(null) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        allPermissions.Add(value);
                    }
                }
            }
            return allPermissions;
        }
    }
}