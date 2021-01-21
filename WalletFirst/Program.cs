using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalletFirst.Controllers;

namespace WalletFirst
{
    public class Program
    {
        public static void Main (string[] args)
        {
            System.Diagnostics.Debug.WriteLine("test");
            CreateHostBuilder(args).Build().Run();

        }

        public static IHostBuilder CreateHostBuilder (string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })/*.ConfigureServices(services =>
                    services.AddHostedService<BackgroundPrinter>())*/;
    }
}
