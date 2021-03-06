﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.AspNetCore.Health;
using App.Metrics.Filtering;
using App.Metrics.Formatters.Json;
using App.Metrics.Scheduling;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Powerumc.RssFeeds.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var nlogConfigFile = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "nlog.windows.config"
                : "nlog.linux.config";
            
            var logger = NLog.Web.NLogBuilder.ConfigureNLog(nlogConfigFile).GetCurrentClassLogger();

            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                logger.Error(e, "Stopped program becuase of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging(logging => { logging.SetMinimumLevel(LogLevel.Trace); })
                .UseNLog()
                .UseMetricsWebTracking()
                .UseMetricsEndpoints()
                .UseHealth()
                .UseHealthEndpoints();
    }
}
