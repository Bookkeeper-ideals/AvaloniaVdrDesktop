namespace VdrDesktop.Models
{
    public class SyncSettings
    {
        public string[] Folders { get; set; } = new string[0];
        public string AuthToken { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}
