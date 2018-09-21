using Microsoft.Extensions.DependencyInjection;
using rrServiceNetV2.Crypter;
using rrServiceNetV2.Gateway.Handler;
using rrServiceNetV2.Gateway.Server;
using rrServiceNetV2.Gateway.Service;
using rrServiceNetV2.Mapper;
using rrServiceNetV2.Runner;
using System;
using System.Collections.Generic;

namespace rrServiceNetV2.Gateway.Main
{
    class Program
    {
        public static Dictionary<string, string> config = new Dictionary<string, string>
        {
            {"Server:Port", "11000"},
            {"Logging:LogLevel:Default", "Debug"},
        };

        static void Main(string[] args)
        {
            Console.Title = "Gateway";

            RunContainer.Build<GatewayService>(config, builder =>
            {
                builder.AddSingleton<ServerService>();
                builder.AddSingleton<GatewayHandler>();
                builder.AddSingleton<CrypterService>();
                builder.AddSingleton<MapperService>();

            });

            Console.ReadLine();
        }
    }
}
