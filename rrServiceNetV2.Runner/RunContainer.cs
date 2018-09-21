using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using rrServiceNetV2.Common;
using System;
using System.Collections.Generic;

namespace rrServiceNetV2.Runner
{
    public static class RunContainer
    {
        public static IServiceProvider Build<T>(Dictionary<string, string> congig, Action<IServiceCollection> p) where T : class, IRunner
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(congig);

            var configuration = configurationBuilder.Build();

            IServiceCollection ServiceCollection = new ServiceCollection();

            ServiceCollection.AddSingleton<IRunner, T>();

            ServiceCollection.AddSingleton<IConfiguration>(configuration);

            ServiceCollection.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);

                builder.AddConfiguration(configuration.GetSection("Logging"))
                    .AddConsole()
                    .AddDebug();
            });


            p(ServiceCollection);

            var sp = ServiceCollection.BuildServiceProvider();

            var _runner = sp.GetRequiredService<IRunner>();

            _runner.Execute();

            return sp;
        }
    }
}
