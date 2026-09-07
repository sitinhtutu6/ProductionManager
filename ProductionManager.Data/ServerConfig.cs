namespace ProductionManager.Data
{
    public class ServerConfig
    {
        // Loại Database: "SQLServer", "SQLite", "MySQL"
        public string DbProvider { get; set; } = "MySQL";

        public string ConnectionString { get; set; }

        public string AdminEmail { get; set; } = "";
        public string SenderEmail { get; set; } = "";
        public string SenderPassword { get; set; } = "";

        public string TelegramToken { get; set; } = "";
        public string TelegramChatId { get; set; } = "";
        public string DiscordWebhook { get; set; } = "";
    }
}