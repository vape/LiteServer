using LiteServer.Config;
using LiteServer.IO.DAL.Context;
using LiteServer.IO.DAL.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LiteServer
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureRepositories(this IServiceCollection services, DatabaseConfig config)
        {
            services.AddScoped<IUserRepository>((s) => new UserRepository(new BaseContext(config.ConnectionString)));
            services.AddScoped<ITokenRepository>((s) => new TokenRepository(new BaseContext(config.ConnectionString)));
            services.AddScoped<IGroupRepository>((s) => new GroupRepository(new BaseContext(config.ConnectionString)));
        }
    }

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
            services.Configure<DatabaseConfig>((d) => Configuration.GetSection("Database").Bind(d));
            services.Configure<SocialConfig>((s) => Configuration.GetSection("Social").Bind(s));
            services.Configure<PlatformConfig>((p) => Configuration.GetSection("PlatformConfig").Bind(p));

            var databaseConfig = Configuration.GetSection("Database").Get<DatabaseConfig>();
            services.ConfigureRepositories(databaseConfig);
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
