namespace BlogAPI;

public static class Configuration
{
    public static string JwtKey = "17f1f8d30b4cf145c52971971e0fb29964e312faa5a9329fb9af966c27fd3dee";
    public static string AzureStorageConnectionString;
    public static string ApiKeyName;
    public static string ApiKey;
    public static SmtpConfiguration Smtp = new();

    public class SmtpConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; } = 25;
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
