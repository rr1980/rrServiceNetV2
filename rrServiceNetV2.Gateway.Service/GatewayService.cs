using Microsoft.Extensions.Logging;
using rrServiceNetV2.Common;
using rrServiceNetV2.Crypter;
using rrServiceNetV2.Gateway.Handler;
using rrServiceNetV2.Gateway.Handler.Commands;
using rrServiceNetV2.Gateway.Server;
using rrServiceNetV2.Mapper;
using System;
using System.Net.Sockets;

namespace rrServiceNetV2.Gateway.Service
{
    public class GatewayService : IRunner
    {
        private readonly ILogger<GatewayService> _logger;
        private readonly ServerService _serverService;
        private readonly GatewayHandler _gatewayHandler;
        private readonly CrypterService _crypterService;
        private readonly MapperService _mapperService;

        public GatewayService(
            ILogger<GatewayService> logger,
            ILogger<Command_Raw> command_Raw_logger,
            ILogger<Command_Register> command_Register_logger,
            ILogger<Command_Close> command_Close_logger,
            ServerService serverService, GatewayHandler gatewayHandler, CrypterService crypterService, MapperService mapperService)
        {
            _logger = logger;
            _serverService = serverService;
            _gatewayHandler = gatewayHandler;
            _crypterService = crypterService;
            _mapperService = mapperService;

            _gatewayHandler.Add(new[] { "raw" }, new Command_Raw(serverService, command_Raw_logger));
            _gatewayHandler.Add(new[] { "register", "call", "response" }, new Command_Register(serverService, command_Register_logger));
            _gatewayHandler.Add(new[] { "close" }, new Command_Close(serverService, command_Close_logger));

            _serverService.OnDataReceived += Server_OnDataReceived;

            _logger.LogTrace("init finished");
            _serverService.Start();
        }

        private void Server_OnDataReceived(byte[] data, int bytesRead, TcpClient client)
        {
            var response_string = _crypterService.Decrypt(data, bytesRead);
            var response = _mapperService.Map<CallPackage>(response_string);

            if (response == null)
            {
                response = new CallPackage();
                response.Command = "raw";
                response.Data = response_string;
            }

            response.Client = client;

            _gatewayHandler.Handle(response);
        }

        public void Execute()
        {
            string s = "";
            do
            {
                if (!string.IsNullOrEmpty(s))
                {
                    _serverService.SendAll(s);
                }

                Console.Write("command:>");
            } while ((s = Console.ReadLine()) != "exit");

            Stop();
        }

        internal void Stop()
        {
            _serverService.Stop();
        }
    }
}
