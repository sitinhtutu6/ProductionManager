using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProductionApp.Web.Services
{
    public class ServerDiscoveryService : BackgroundService
    {
        // Cổng đặc biệt dùng để tìm kiếm (Khác cổng Web 7078)
        private const int DISCOVERY_PORT = 9999;
        private const string REQUEST_MSG = "WHERE_ARE_YOU";
        private const string RESPONSE_MSG = "I_AM_SERVER";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var udpServer = new UdpClient(DISCOVERY_PORT);

            // Cho phép nhận broadcast
            udpServer.EnableBroadcast = true;

            Console.WriteLine($"[UDP] Đang lắng nghe tìm kiếm tại cổng {DISCOVERY_PORT}...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1. Chờ nhận tín hiệu
                    var result = await udpServer.ReceiveAsync(stoppingToken);
                    var message = Encoding.UTF8.GetString(result.Buffer);

                    // 2. Nếu đúng mật khẩu "WHERE_ARE_YOU"
                    if (message == REQUEST_MSG)
                    {
                        // 3. Trả lời lại cho người hỏi
                        var responseData = Encoding.UTF8.GetBytes(RESPONSE_MSG);

                        // Gửi trả lại đúng địa chỉ người hỏi
                        await udpServer.SendAsync(responseData, responseData.Length, result.RemoteEndPoint);

                        Console.WriteLine($"[UDP] Đã chỉ đường cho {result.RemoteEndPoint.Address}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[UDP Error] " + ex.Message);
                }
            }
        }
    }
}