using Microsoft.AspNetCore.SignalR;

namespace ProductionApp.Web.Hubs
{
    public class ProductionHub : Hub
    {
        // Hàm này để JS gọi lên (nếu cần), hiện tại ta chỉ cần Server đẩy xuống Client
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}