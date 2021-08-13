﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerCare.RecipientList.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CustomerCare.RecipientList
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") != "Development")
            {
                Thread.Sleep(15000);
            }
            
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddEnvironmentVariables();
                    
                    var env = context.HostingEnvironment;

                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", 
                            optional: true, reloadOnChange: true);

                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<Messaging>(context.Configuration.GetSection("Messaging"));
                    services.Configure<Database>(context.Configuration.GetSection("Database"));
                    
                    services.AddHostedService<RecipientListHostedService>();
                });

            await hostBuilder.RunConsoleAsync();
        }
    }
}