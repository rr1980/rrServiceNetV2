using Microsoft.Extensions.DependencyInjection;
using rrServiceNetV2.Runner;
using rrServiceNetV2.Services.Client;
using rrServiceNetV2.Services.Test.Service;
using System;
using System.Collections.Generic;

namespace rrServiceNetV2.Services.Test.Main
{
    class Program
    {
        public static Dictionary<string, string> config = new Dictionary<string, string>
        {
            {"Server:Ip", "127.0.0.1"},
            {"Server:Port", "11000"},
            {"Logging:LogLevel:Default", "Debug"},
        };

        static void Main(string[] args)
        {
            Console.Title = "Test";

            RunContainer.Build<TestService>(config, builder =>
            {
                builder.AddSingleton<ClientService>();
            });

            Console.ReadLine();
        }
    }
}
