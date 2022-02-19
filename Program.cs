using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OASystemSynergy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
           .ConfigureLogging((context, loggingBuilder) =>
           {
               loggingBuilder.AddFilter("System", LogLevel.Warning);
               loggingBuilder.AddFilter("Microsoft", LogLevel.Warning);
               // Ê¹ÓÃlog4net
               loggingBuilder.AddLog4Net("Log4Net/log4net.config");
           })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
