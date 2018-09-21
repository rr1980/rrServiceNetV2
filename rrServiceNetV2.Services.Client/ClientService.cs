using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using rrServiceNetV2.Common;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace rrServiceNetV2.Services.Client
{
    public delegate void ClientHandlePacketData(CallPackage response);


    /// <summary>
    /// Implements a simple TCP client which connects to a specified server and
    /// raises C# events when data is received from the server
    /// </summary>
    public class ClientService
    {
        public event ClientHandlePacketData OnDataReceived;

        private TcpClient tcpClient;
        private NetworkStream clientStream;
        private NetworkBuffer buffer;
        private int writeBufferSize = 1024;
        private int readBufferSize = 1024;
        private int port;
        private string ip;
        private bool started = false;

        private readonly ILogger<ClientService> _logger;

        /// <summary>
        /// Constructs a new client
        /// </summary>
        public ClientService(ILogger<ClientService> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.ip = configuration.GetSection("Server").GetValue<string>("Ip");
            this.port = configuration.GetSection("Server").GetValue<int>("Port");

            buffer = new NetworkBuffer();
            buffer.WriteBuffer = new byte[writeBufferSize];
            buffer.ReadBuffer = new byte[readBufferSize];
            buffer.CurrentWriteByteCount = 0;
        }

        /// <summary>
        /// Initiates a TCP connection to a TCP server with a given address and port
        /// </summary>
        /// <param name="ipAddress">The IP address (IPV4) of the server</param>
        /// <param name="port">The port the server is listening on</param>
        public void ConnectToServer()
        {

            tcpClient = new TcpClient(ip, port);
            clientStream = tcpClient.GetStream();
            _logger.LogDebug("Connected to server, listening for packets");

            Thread t = new Thread(new ThreadStart(ListenForPackets));
            started = true;
            t.Start();
        }

        /// <summary>
        /// This method runs on its own thread, and is responsible for
        /// receiving data from the server and raising an event when data
        /// is received
        /// </summary>
        private void ListenForPackets()
        {
            int bytesRead;

            while (started)
            {
                bytesRead = 0;

                try
                {
                    //Blocks until a message is received from the server
                    bytesRead = clientStream.Read(buffer.ReadBuffer, 0, readBufferSize);
                }
                catch
                {
                    //A socket error has occurred
                    _logger.LogDebug("A socket error has occurred with the client socket " + tcpClient.Client.LocalEndPoint.ToString());
                    break;
                }

                if (bytesRead == 0)
                {
                    //The server has disconnected
                    break;
                }

                if (OnDataReceived != null)
                {
                    //Send off the data for other classes to handle
                    var response = Encoding.ASCII.GetString(buffer.ReadBuffer, 0, bytesRead);

                    CallPackage cp = new CallPackage();
                    cp.Command = "raw";
                    cp.Data = response;

                    try
                    {
                        cp = JsonConvert.DeserializeObject<CallPackage>(response);
                    }
                    catch { }

                    cp.Client = tcpClient;
                    //OnDataReceived(cp);

                    //Send off the data for other classes to handle

                    OnDataReceived(cp);
                }

                Thread.Sleep(15);
            }

            started = false;
            Disconnect();
        }

        public void Call(CallPackage package)
        {
            string json = JsonConvert.SerializeObject(package);

            byte[] _data = Encoding.ASCII.GetBytes(json);
            AddToPacket(_data);
            FlushData();
        }

        public void Send(string data)
        {
            byte[] _data = Encoding.ASCII.GetBytes(data);
            AddToPacket(_data);
            FlushData();
        }

        /// <summary>
        /// Adds data to the packet to be sent out, but does not send it across the network
        /// </summary>
        /// <param name="data">The data to be sent</param>
        public void AddToPacket(byte[] data)
        {
            if (buffer.CurrentWriteByteCount + data.Length > buffer.WriteBuffer.Length)
            {
                FlushData();
            }

            Array.ConstrainedCopy(data, 0, buffer.WriteBuffer, buffer.CurrentWriteByteCount, data.Length);
            buffer.CurrentWriteByteCount += data.Length;
        }

        /// <summary>
        /// Flushes all outgoing data to the server
        /// </summary>
        public void FlushData()
        {
            clientStream.Write(buffer.WriteBuffer, 0, buffer.CurrentWriteByteCount);
            clientStream.Flush();
            buffer.CurrentWriteByteCount = 0;
        }

        /// <summary>
        /// Sends the byte array data immediately to the server
        /// </summary>
        /// <param name="data"></param>
        public void SendImmediate(byte[] data)
        {
            AddToPacket(data);
            FlushData();
        }

        /// <summary>
        /// Tells whether we're connected to the server
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            return started && tcpClient.Connected;
        }

        /// <summary>
        /// Disconnect from the server
        /// </summary>
        public void Disconnect()
        {
            if (tcpClient == null)
            {
                return;
            }

            _logger.LogDebug("Disconnected from server");

            tcpClient.Close();

            started = false;
        }
    }
}
