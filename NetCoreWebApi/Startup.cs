using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NetCoreWebApi.DAL;
using NetCoreWebApi.ModelSettings;
using NetCoreWebApi.ModelSettings.Common;
using NetCoreWebApi.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

namespace NetCoreWebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {            
            services.AddControllers()
                    .AddJsonOptions(opt =>
                    {
                        opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;                        
                        opt.JsonSerializerOptions.WriteIndented = true;
                    });
            services.AddOptions();
            services.Configure<NetCoreApiSettings>(Configuration.GetSection("NetCoreWebApiSettings"));            
            services.AddDbContext<NetCoreWebApiDbContext >(options =>
            {                
                NetCoreApiSettings settings = new NetCoreApiSettings();
                Configuration.GetSection("NetCoreWebApiSettings").Bind(settings);
                
                switch (settings.DatabaseType)
                {
                    case DataBaseType.PostgreSql:
                        options.UseNpgsql($"Host={settings.DatabaseServerHost};Port={settings.DatabaseServerPort};Database={settings.DatabaseServerDatabase};Username={settings.DatabaseServerUser};Password={settings.DatabaseServerPassword}")
                               .UseSnakeCaseNamingConvention();
                        break;
                    case DataBaseType.Mysql:
                        options.UseMySQL($"Server={settings.DatabaseServerHost};Port={settings.DatabaseServerPort};Database={settings.DatabaseServerDatabase};User={settings.DatabaseServerUser};Password={settings.DatabaseServerPassword}");
                        break;
                    default:
                        options.UseSqlServer($"Persist Security Info = False;User ID={settings.DatabaseServerUser};Password={settings.DatabaseServerPassword};Initial Catalog={settings.DatabaseServerDatabase};Server={settings.DatabaseServerHost}");
                        break;
                }               
            });
            services.AddScoped<IJwtAuthenticationService, JwtAuthenticationService>()            
                    .AddAuthentication(auth => auth.DefaultChallengeScheme = auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(jwtBearer =>
                    {
                        jwtBearer.RequireHttpsMetadata = false;
                        jwtBearer.SaveToken = true;
                        jwtBearer.TokenValidationParameters = new TokenValidationParameters
                        {
                            IssuerSigningKey = new SymmetricSecurityKey(UTF8Encoding.UTF8.GetBytes(JwtAuthenticationService.SigningKey)),
                            ValidateAudience = false,
                            ValidateIssuerSigningKey = true,
                            ValidateIssuer = false
                        };
                        
                        jwtBearer.Events = new JwtBearerEvents
                        {
                            OnTokenValidated = context => // If the token is validated, then at the end of the response we generate a new one...
                            {
                                string oldToken = context.Request.Headers[HeaderNames.Authorization].First().Replace("Bearer ", string.Empty);
                                context.Response.Headers.Add(JwtAuthenticationService.XAuthHeader, JwtAuthenticationService.RefreshToken(oldToken));

                                return Task.CompletedTask;
                            }
                        };
                    });
            services.AddAuthorization();
                   

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = Assembly.GetExecutingAssembly().GetName().Name,
                    Description = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>().Description,
                    Contact = new OpenApiContact
                    {
                        Name = "Antonio Fdez. Alabarce",
                        Email = "afalabarce@gmail.com",
                        Url = new Uri("https://linkedin.com/in/antonio-f-83415069")
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);                
                c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

                // if necessary, we can add another xml documentation file
                //var xmlModelFile = Path.Combine(AppContext.BaseDirectory, "NetCoreWebApiSettings.xml");
                //c.IncludeXmlComments(xmlModelFile, includeControllerXmlComments: true);
                
                c.EnableAnnotations();
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme()
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Api Key"
                });                
                c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" } }, Array.Empty<string>() } }); });
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, NetCoreWebApiDbContext dbContext)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapSwagger();
                endpoints.MapControllers();
                // By default, we launch swagger's web
                endpoints.MapControllerRoute("default", "swagger");
            });    
            
            dbContext.Database.Migrate();
        }
    }
}

