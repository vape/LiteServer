using LiteServer.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LiteServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddOptions();
            services.Configure<DatabaseConfig>((d) =>
            {
                d.ConnectionString = Configuration.GetSection("Database:ConnectionString").Value;
            });
            services.Configure<SocialConfig>((s) =>
            {
                s.Vk = new VkConfig()
                {
                    AppId = Configuration.GetSection("Social:Vk:AppId").Value,
                    SecureKey = Configuration.GetSection("Social:Vk:SecureKey").Value,
                    RedirectUri = Configuration.GetSection("Social:Vk:RedirectUri").Value
                };
            });
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware(typeof(Middleware.ErrorHandlingMiddleware));
            app.UseMvc();
        }
    }
}
