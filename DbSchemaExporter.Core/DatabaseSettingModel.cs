namespace DbSchemaExporter.Core
{
    public class DatabaseSettingModel
    {
        public string DbType { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DatabaseName { get; set; }
    }
}
