using ProductionManager.Data;
using System.Collections.Generic;

namespace ProductionApp.Web.Models.ViewModels
{
    public class PartnerGroupVM
    {
        public string CompanyName { get; set; }
        public List<PartnerContact> Contacts { get; set; } = new List<PartnerContact>();
        public int ActiveCount { get; set; } // Đếm số người đang còn làm việc
    }
}