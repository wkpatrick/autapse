using System;
using System.IO;
using autapse_backend.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace autapse_backend
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        
        public static ConfigOptions Config;

        public static void Main(string[] args)
        {
            Config = Configuration.GetSection("Autapse").Get<ConfigOptions>();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //webBuilder.UseUrls("http://::8675;http://0.0.0.0:8675");
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel();
                });
    }
}