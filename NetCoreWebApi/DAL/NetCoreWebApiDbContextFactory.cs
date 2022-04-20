using System;
using NetCoreWebApi.ModelSettings;
using NetCoreWebApi.ModelSettings.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace NetCoreWebApi.DAL
{
    public class NetCoreWebApiDbContextFactory: IDesignTimeDbContextFactory<NetCoreWebApiDbContext>
    {
        public NetCoreWebApiDbContextFactory()
        {
        }

        public NetCoreWebApiDbContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<NetCoreWebApiDbContext> optionsBuilder = new DbContextOptionsBuilder<NetCoreWebApiDbContext>();
            NetCoreApiSettings settings = new NetCoreApiSettings();
            string basePath = $"{args[0]}";
            var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)            
            .AddEnvironmentVariables().Build();
            configuration.GetSection("NetCoreWebApiSettings").Bind(settings);

            Console.WriteLine($"Selected Database Engine: {settings.DatabaseType}");
            switch (settings.DatabaseType)
            {
                case DataBaseType.PostgreSql:
                    optionsBuilder.UseNpgsql($"Host={settings.DatabaseServerHost};Port={settings.DatabaseServerPort};Database={settings.DatabaseServerDatabase};Username={settings.DatabaseServerUser};Password={settings.DatabaseServerPassword}")
                                  .UseSnakeCaseNamingConvention();
                    break;
                case DataBaseType.Mysql:
                    optionsBuilder.UseMySQL($"Server={settings.DatabaseServerHost};Port={settings.DatabaseServerPort};Database={settings.DatabaseServerDatabase};User={settings.DatabaseServerUser};Password={settings.DatabaseServerPassword}");
                    break;
                default:
                    optionsBuilder.UseSqlServer($"Persist Security Info = False;User ID={settings.DatabaseServerUser};Password={settings.DatabaseServerPassword};Initial Catalog={settings.DatabaseServerDatabase};Server={settings.DatabaseServerHost}");
                    break;
            }


            NetCoreWebApiDbContext returnValue = new NetCoreWebApiDbContext(optionsBuilder.Options);
            
            return returnValue;
        }
    }
}

