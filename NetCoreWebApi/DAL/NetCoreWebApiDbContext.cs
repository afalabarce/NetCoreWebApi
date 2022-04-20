using NetCoreWebApi.Models;
using NetCoreWebApi.ModelSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace NetCoreWebApi.DAL
{
    /// <summary>
    /// Database access context using EntityFramework Core (CodeFirst)
    /// </summary>
    public class NetCoreWebApiDbContext: DbContext
    {
        /*
            NOTE: To run migrations, you must provide the full path to appsettings.json:
            > dotnet ef migrations add NombreDeLaMigracion -- /Users/afalabarce/Projects/NetCoreWebApiBase/NetCoreWebApi/
            > dotnet ef database update -- /Users/afalabarce/Projects/NetCoreWebApiBase/NetCoreWebApi/           
         */

        private NetCoreApiSettings webApiSettings;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        public NetCoreWebApiDbContext (DbContextOptions<NetCoreWebApiDbContext > options) : base(options)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="settings"></param>
        public NetCoreWebApiDbContext (DbContextOptions<NetCoreWebApiDbContext > options, IOptions<NetCoreApiSettings> settings) : this(options)
        {
            this.webApiSettings = settings.Value;            
        }

        
        public DbSet<WebApiUser> WebApiUsers { get; set; }
    }
}

