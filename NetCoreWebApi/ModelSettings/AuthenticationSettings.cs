using NetCoreWebApi.ModelSettings.Common;

namespace NetCoreWebApi.ModelSettings
{
    public class AuthenticationSettings: DataBaseSettings
    {
        public int TokenLifeTimeInSeconds { get; set; } = 1800; // By default, 30 minutes
        public AuthenticationSettings()
        {

        }
    }
}

