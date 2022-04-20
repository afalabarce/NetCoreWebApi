using NetCoreWebApi.DAL;
using NetCoreWebApi.ModelSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace NetCoreWebApi.Controllers.Common
{
    public class ConnectedController: ControllerBase
    {
        public readonly NetCoreApiSettings webApiSettings;
        public readonly IOptions<NetCoreApiSettings> iWebApiSettings;
        public readonly NetCoreWebApiDbContext dbContext;        
        public ConnectedController(IOptions<NetCoreApiSettings> settings, NetCoreWebApiDbContext contextDb)
        {
            this.dbContext = contextDb;
            this.webApiSettings = settings.Value;
            this.iWebApiSettings = settings;            
        }        
    }
}

