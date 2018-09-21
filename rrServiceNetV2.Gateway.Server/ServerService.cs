using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using rrServiceNetV2.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace rrServiceNetV2.Gateway.Server
{
    //public delegate void ServerHandlePacketData(CallPackage req);
    public delegate void ServerHandlePacketData(byte[] data, int bytesRead, TcpClient client);

    public class ServerService
    {
        public event ServerHandlePacketData OnDataReceived;

        private TcpListener listener;
        private ConcurrentDictionary<TcpClient, NetworkBuffer> clientBuffers;
        private List<TcpClient> clients;
        private int sendBufferSize = 1024;
        private int readBufferSize = 1024;
        private int port;
        private bool started = false;

        private readonly ILogger<ServerService> _logger;

        public ServerService(ILogger<ServerService> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.port = configuration.GetSection("Server").GetValue<int>("Port");
            clientBuffers = new ConcurrentDictionary<TcpClient, NetworkBuffer>();
            clients = new List<TcpClient>();

            _logger.LogTrace("init finished");
        }

        public void Start()
        {
            listener = new TcpListener(IPAddress.Any, port);
            _logger.LogDebug("Started server on " + listener.LocalEndpoint);

            Thread thread = new Thread(new ThreadStart(ListenForClients));
            thread.Start();
            started = true;
        }

        public void Stop()
        {
            if (!listener.Pending())
            {
                listener.Stop();
                started = false;
            }
        }

        private void ListenForClients()
        {
            listener.Start();

            try
            {
                while (started)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Thread clientThread = new Thread(new ParameterizedThreadStart(WorkWithClient));

                    Console.WriteLine();
                    _logger.LogDebug("New client connected");

                    NetworkBuffer newBuff = new NetworkBuffer();
                    newBuff.WriteBuffer = new byte[sendBufferSize];
                    newBuff.ReadBuffer = new byte[readBufferSize];
                    newBuff.CurrentWriteByteCount = 0;
                    clientBuffers.GetOrAdd(client, newBuff);
                    clients.Add(client);

                    clientThread.Start(client);
                    Thread.Sleep(15);
                }
            }
            catch
            {

            }
        }

        private void WorkWithClient(object client)
        {
            TcpClient tcpClient = client as TcpClient;
            if (tcpClient == null)
            {
                Console.WriteLine();
                _logger.LogDebug("TCP client is null, stopping processing for this client");
                DisconnectClient(tcpClient);
                return;
            }

            NetworkStream clientStream = tcpClient.GetStream();
            int bytesRead;

            while (started)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(clientBuffers[tcpClient].ReadBuffer, 0, readBufferSize);
                }
                catch
                {
                    //a socket error has occurred
                    Console.WriteLine();
                    _logger.LogDebug("A socket error has occurred with client: " + tcpClient.Client.LocalEndPoint.ToString());
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                if (OnDataReceived != null)
                {
                    //Send off the data for other classes to handle
                    //var response = Encoding.ASCII.GetString(clientBuffers[tcpClient].ReadBuffer, 0, bytesRead);

                    //CallPackage cp = new CallPackage();
                    //cp.Command = "raw";
                    //cp.Data = response;

                    //try
                    //{
                    //    cp = JsonConvert.DeserializeObject<CallPackage>(response);
                    //}
                    //catch { }

                    //cp.Client = tcpClient;
                    //OnDataReceived(cp);
                    Console.WriteLine();
                    OnDataReceived(clientBuffers[tcpClient].ReadBuffer, bytesRead, tcpClient);
                }

                Thread.Sleep(15);
            }

            DisconnectClient(tcpClient);
        }

        public void DisconnectClient(TcpClient client)
        {
            if (client == null)
            {
                return;
            }

            Console.WriteLine();

            _logger.LogDebug("Disconnected client: " + client.Client.LocalEndPoint.ToString());

            client.Close();

            clients.Remove(client);
            NetworkBuffer buffer;
            clientBuffers.TryRemove(client, out buffer);
        }

        public void AddToPacket(byte[] data, TcpClient client)
        {
            if (clientBuffers[client].CurrentWriteByteCount + data.Length > clientBuffers[client].WriteBuffer.Length)
            {
                FlushData(client);
            }

            Array.ConstrainedCopy(data, 0, clientBuffers[client].WriteBuffer, clientBuffers[client].CurrentWriteByteCount, data.Length);

            clientBuffers[client].CurrentWriteByteCount += data.Length;
        }

        public void AddToPacketToAll(byte[] data)
        {
            lock (clients)
            {
                foreach (TcpClient client in clients)
                {
                    if (clientBuffers[client].CurrentWriteByteCount + data.Length > clientBuffers[client].WriteBuffer.Length)
                    {
                        FlushData(client);
                    }

                    Array.ConstrainedCopy(data, 0, clientBuffers[client].WriteBuffer, clientBuffers[client].CurrentWriteByteCount, data.Length);

                    clientBuffers[client].CurrentWriteByteCount += data.Length;
                }
            }
        }

        private void FlushData(TcpClient client)
        {
            client.GetStream().Write(clientBuffers[client].WriteBuffer, 0, clientBuffers[client].CurrentWriteByteCount);
            client.GetStream().Flush();
            clientBuffers[client].CurrentWriteByteCount = 0;
        }

        private void FlushDataToAll()
        {
            lock (clients)
            {
                foreach (TcpClient client in clients)
                {
                    client.GetStream().Write(clientBuffers[client].WriteBuffer, 0, clientBuffers[client].CurrentWriteByteCount);
                    client.GetStream().Flush();
                    clientBuffers[client].CurrentWriteByteCount = 0;
                }
            }
        }

        public void Call(CallPackage package, TcpClient client)
        {
            string json = JsonConvert.SerializeObject(package);

            byte[] _data = Encoding.ASCII.GetBytes(json);
            SendImmediate(_data, client);
        }

        public void Send(TcpClient client, string data)
        {
            byte[] _data = Encoding.ASCII.GetBytes(data);
            AddToPacket(_data, client);
            FlushData(client);
        }

        public void SendAll(string data)
        {
            byte[] _data = Encoding.ASCII.GetBytes(data);
            SendImmediateToAll(_data);
        }

        public void SendImmediate(byte[] data, TcpClient client)
        {
            AddToPacket(data, client);
            FlushData(client);
        }

        public void SendImmediateToAll(byte[] data)
        {
            lock (clients)
            {
                foreach (TcpClient client in clients)
                {
                    AddToPacket(data, client);
                    FlushData(client);
                }
            }
        }
    }
}
