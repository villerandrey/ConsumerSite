using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Be24BLogic;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BE24Services
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var sest = Configuration.GetSection("AppSets")["SessionTout"];
            int setime=30;
            int.TryParse(sest, out setime);
            
            // Add framework services.
            services.AddMemoryCache();   //добавляем кэш
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddDistributedMemoryCache();
            services.AddNodeServices();
            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(setime);
         
            });
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var sect = Configuration.GetSection("AppSets");
             
            SettingsManager.UserName = sect["UserName"];
            SettingsManager.Pass = sect["Pas"];
            SettingsManager.ConnectionString = sect["ConnStr"];
            SettingsManager.CacheTimeSpan = sect["CacheTimeSpan"];
            SettingsManager.LogFileName = sect["LogFileName"];
            SettingsManager.mailsmpt = sect["mailsmpt"];
            SettingsManager.mailport = sect["mailport"];
            SettingsManager.mailadress = sect["mailadress"];
            SettingsManager.mailpass = sect["mailpass"];
            SettingsManager.validurl = sect["validurl"];
            SettingsManager.mailmesstext = sect["mailmesstext"];
            SettingsManager.mailpassmesstext = sect["mailpassmesstext"];
            SettingsManager.robocassaid = sect["robocassaid"];
            SettingsManager.robopass = sect["robopass"];
            SettingsManager.robopass2 = sect["robopass2"];
            SettingsManager.robotest = sect["robotest"];
            SettingsManager.showconloyer = sect["showconloyer"];
            SettingsManager.stimulpath = sect["StimulLicens"];




            SourceSwitch sw = new SourceSwitch("mys", sect["LogLevel"]); 
            CoreLogic core = new CoreLogic();

            var cultureInfo = new CultureInfo(sect["CultureVal"]);
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;


            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddTraceSource(sw, new LogTraceListener(SettingsManager.LogFileName));
            
            CoreLogic.logger = loggerFactory.CreateLogger("AppLogger");
            //lg.LogInformation("Старт приложения", null);
            CoreLogic.logger.LogCritical("Старт приложения, уровень логирования - " + sect["LogLevel"]);

            Stimulsoft.Base.StiLicense.LoadFromFile(SettingsManager.stimulpath);

            CoreLogic.logger.LogCritical("Лицензия отчетов загружена  " );

            app.UseSession();
            //app.Run(async (context) =>
            //{
            //    if (context.Session.Keys.Contains("name"))
            //        await context.Response.WriteAsync($"Hello {context.Session.GetString("name")}!");
            //    else
            //    {
            //        context.Session.SetString("name", "Tom");
            //        await context.Response.WriteAsync("Hello World!");
            //    }
            //});



            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            /*
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "Cookies",
              //  LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Register"),
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            
            });
            */
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}");
                routes.MapRoute(
                    name: "Api",
                    template: "api/{controller}/{action}"
                );
            });


            //app.UseEsiaAuthentication(new EsiaAuthenticationOptions(Esia.GetOptions())
            //{
            //    VerifyTokenSignature = true,    // Будем проверять подпись маркера доступа
            //    Provider = new EsiaAuthenticationProvider
            //    {
            //        OnAuthenticated = context =>    // Сохраним после авторизации маркер доступа в сессии для будущего использования в приложении (HomeController/EsiaPage)
            //        {
            //            HttpContext.Current.Session["esiaToken"] = context.Token;

            //            return Task.FromResult<object>(null);
            //        }
            //    }
            //});

        }
    }
}
