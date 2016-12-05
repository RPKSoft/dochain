using System.Collections.Generic;
using System.IO;
using System.Threading;
using Dochain.Web.Interfaces;
using Dochain.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dochain.Web
{
    public class Startup
    {
        private const string DochainAbiKey = "Dochain:abi";
        private const string DochainAddressKey = "Dochain:address";
        private const string SenderAddressKey = "Dochain:SenderAddress";
        private const string PasswordKey = "Dochain:Password";
        private readonly string _dochainAbi = Path.Combine("bin", "Dochain.abi");
        private readonly string _dochainBin = Path.Combine("bin", "Dochain.bin");
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
                if (File.Exists(_dochainAbi))
                {
                    var abi = File.ReadAllText(_dochainAbi);
                    var collection =
                        new List<KeyValuePair<string, string>>(new[]
                            {new KeyValuePair<string, string>(DochainAbiKey, abi)});
                    if (File.Exists(_dochainBin))
                    {
                        var config = builder.Build();
                        var byteCode = "0x" + File.ReadAllText(_dochainBin);
                        var service = new NethereumDochainService(abi, config[SenderAddressKey], config[PasswordKey]);
                        var task = service.Deploy(byteCode);
                        var source = new CancellationTokenSource();
                        var token = source.Token;
                        while (!task.Wait(60000, token))
                        {
                            source.Cancel(true);
                        }
                        collection.Add(new KeyValuePair<string, string>(DochainAddressKey, task.Result));
                    }
                    builder.AddInMemoryCollection(collection);
                }
            }
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc();

            services.AddTransient<IDochainService, NethereumDochainService>(
                provider =>
                    new NethereumDochainService(Configuration[DochainAbiKey], Configuration[DochainAddressKey],
                        Configuration[SenderAddressKey], Configuration[PasswordKey]));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
