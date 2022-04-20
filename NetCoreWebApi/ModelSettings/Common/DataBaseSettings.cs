namespace NetCoreWebApi.ModelSettings.Common
{
    public class DataBaseSettings
    {
        public DataBaseType DatabaseType { get; set; }
        public string DatabaseServerHost { get; set; }
        public int DatabaseServerPort { get; set; }
        public string DatabaseServerDatabase { get; set; }
        public string DatabaseServerUser { get; set; }
        public string DatabaseServerPassword { get; set; }

        public DataBaseSettings()
        {
        }
    }
}

