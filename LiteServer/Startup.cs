using LiteServer.Config;
using LiteServer.Controllers.Chats;
using LiteServer.Filters;
using LiteServer.IO.DAL.Context;
using LiteServer.IO.DAL.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiteServer
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureRepositories(this IServiceCollection services, DatabaseConfig config)
        {
            services.AddScoped<IUserRepository>((s) => new UserRepository(new BaseContext(config.ConnectionString)));
            services.AddScoped<ITokenRepository>((s) => new TokenRepository(new BaseContext(config.ConnectionString)));
            services.AddScoped<IGroupRepository>((s) => new GroupRepository(new BaseContext(config.ConnectionString)));
            services.AddScoped<IMessageRepository>((s) => new MessageRepository(new BaseContext(config.ConnectionString)));
            services.AddScoped<IRequestRepository>((s) => new RequestRepository(new BaseContext(config.ConnectionString)));
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
            services
                .AddMvc((config) => { config.Filters.Add(new GlobalModelValidationFilter()); })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services
                .Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

            services.Configure<DatabaseConfig>((d) => Configuration.GetSection("Database").Bind(d));
            services.Configure<SocialConfig>((s) => Configuration.GetSection("Social").Bind(s));
            services.Configure<PlatformConfig>((p) => Configuration.GetSection("PlatformConfig").Bind(p));
            
            var databaseConfig = Configuration.GetSection("Database").Get<DatabaseConfig>();
            services.ConfigureRepositories(databaseConfig);
            
            var logger = new LoggerFactory().AddDebug().AddConsole().CreateLogger(typeof(ChatManager).ToString());
            services.AddScoped<IChatManager>((s) => new ChatManager(logger));
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets();
            app.UseMiddleware(typeof(Middleware.ErrorHandlingMiddleware));
            app.UseMvc();
        }
    }
}
