using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace rrServiceNetV2.Common
{

    public class CallPackage
    {
        [JsonIgnore]
        public TcpClient Client { get; set; }

        public string Command { get; set; }
        public string Data { get; set; }
        public Guid Guid { get; set; }

        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();

        public CallPackage()
        {
            Guid = System.Guid.NewGuid();
        }
    }
}
