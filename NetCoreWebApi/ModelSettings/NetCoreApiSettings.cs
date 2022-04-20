using NetCoreWebApi.ModelSettings.Common;

namespace NetCoreWebApi.ModelSettings
{
    public class NetCoreApiSettings : DataBaseSettings
    {
        public string PublicKeyRSA { get; set; }
        public string PrivateKeyRSA { get; set; }
        public AuthenticationSettings Authentication { get; set; }        

        public NetCoreApiSettings()
        {
            this.Authentication = new AuthenticationSettings();            
        }
    }
}

