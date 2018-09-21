using Microsoft.Extensions.Logging;
using rrServiceNetV2.Common;
using rrServiceNetV2.Services.Client;
using System;

namespace rrServiceNetV2.Services.Test.Service
{
    public class TestService : IRunner
    {
        private readonly ILogger<TestService> _logger;
        private readonly ClientService _clientService;

        public TestService(ILogger<TestService> logger, ClientService clientService)
        {
            _logger = logger;
            _clientService = clientService;

            _logger.LogDebug("init finished");

            _clientService.OnDataReceived += Client_OnDataReceived;

            _clientService.ConnectToServer();
        }

        private void Client_OnDataReceived(CallPackage response)
        {
            _logger.LogDebug("+: command: " + response.Command + "\t\t:\t" + response.Data);
        }

        public void Execute()
        {
            CallPackage cp = new CallPackage();
            cp.Command = "register";
            cp.Data = "testerService";

            //cp.Params.Add("commands", new[] { "time", "date" });

            _clientService.Call(cp);

            string s = "";
            do
            {
                if (s == "call")
                {
                    cp = new CallPackage();
                    cp.Command = "call";
                    cp.Data = "time";
                    Console.WriteLine(cp.Guid);
                    _clientService.Call(cp);
                }
                else
                {
                    _clientService.Send(s);
                }
                Console.Write("command:>");
            } while ((s = Console.ReadLine()) != "exit");

            Stop();
        }

        internal void Stop()
        {
            _clientService.Disconnect();
        }
    }
}
