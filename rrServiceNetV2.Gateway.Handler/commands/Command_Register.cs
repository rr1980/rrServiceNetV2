

using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using rrServiceNetV2.Common;
using rrServiceNetV2.Gateway.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
namespace rrServiceNetV2.Gateway.Handler.Commands
{
    public class Command_Register : CommandBase<Command_Register>
    {
        readonly Dictionary<string[], TcpClient> commands = new Dictionary<string[], TcpClient>();
        readonly Dictionary<Guid, TcpClient> awaitResponses = new Dictionary<Guid, TcpClient>();

        public Command_Register(ServerService serverService, ILogger<Command_Register> logger) : base(serverService, logger)
        {
        }

        internal override void Execute(CallPackage cp)
        {
            _logger.LogTrace(cp.Guid.ToString());

            switch (cp.Command)
            {
                case "register":
                    Register(cp);
                    break;
                case "call":
                    Call(cp);
                    break;
                case "response":
                    Response(cp);
                    break;
            }


            //string[] commands = (string[])cp.Params.FirstOrDefault(x => x.Key == "commands").Value;

        }

        private void Register(CallPackage cp)
        {
            var d = cp.Params.FirstOrDefault(x => x.Key == "commands").Value;
            if (d != null)
            {
                JArray jsonResponse = JArray.Parse(d.ToString());
                commands.Add(jsonResponse.ToObject<string[]>(), cp.Client);
            }

            ServerService.Send(cp.Client, "registred by server");
        }

        private void Call(CallPackage cp)
        {
            var _client = commands.FirstOrDefault(x => x.Key.Any(y => y == cp.Data)).Value;
            if (_client != null)
            {
                awaitResponses.Add(cp.Guid, cp.Client);
                ServerService.Call(cp, _client);
                _logger.LogDebug("call " + cp.Data);
            }
            else
            {
                cp.Data = "no service for " + cp.Data;
                ServerService.Call(cp, cp.Client);
                _logger.LogDebug("no service for " + cp.Data);
            }
        }

        private void Response(CallPackage cp)
        {
            var _client = awaitResponses.FirstOrDefault(x => x.Key == cp.Guid).Value;

            ServerService.Call(cp, _client);

            _logger.LogDebug("response " + cp.Data);
        }
    }
}
