using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EXTool
{
    public class LocalServer
    {
        public ReceivedHandler Handler { get; private set; }
        private byte[] _buffer;
        private Socket _serverSocket;

        public LocalServer()
        {
            Handler = new ReceivedHandler();
        }

        public static LocalServer Create()
        {
            var server = new LocalServer();
            return server;
        }

        public void OpenServer()
        {
            if (_serverSocket != null)
            {
                EXLog.Warning("Local server has been Open!");
                return;
            }
            
            var port = ExMaidSetting.Port;
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var localEndPoint = new IPEndPoint(IPAddress.Loopback, port);

            try
            {
                _serverSocket.Bind(localEndPoint);
                _serverSocket.Listen(10);
                EXLog.Log($"Open the Local Communication Server! Port:{port}");

                _serverSocket.BeginAccept(AcceptCallback, null);
            }
            catch (Exception e)
            {
                EXLog.Error("Error starting server: " + e.Message);
            }
        }

        public void CloseServer()
        {
            if (_serverSocket != null)
            {
                // serverSocket.Shutdown(SocketShutdown.Both);
                _serverSocket.Close();
                EXLog.Log("Close the Local Communication Server.");
            }
            Handler.Clear();
            _serverSocket = null;
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            var clientSocket = _serverSocket.EndAccept(ar);
            _buffer = new byte[1024];
            clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback,
                clientSocket);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var clientSocket = (Socket) ar.AsyncState;
            var bytesRead = clientSocket.EndReceive(ar);

            var data = new byte[bytesRead];
            Array.Copy(_buffer, data, bytesRead);

            var message = Encoding.UTF8.GetString(data);
            EXLog.Log($"Local Server => Received message: {message}");

            // Trigger Received Handler
            TryToTriggerReceivedHandler(message);

            // Close the Client Connection
            // This Local Socket Server is set for short connections.
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();

            _serverSocket.BeginAccept(AcceptCallback, null);
        }

        private void TryToTriggerReceivedHandler(string message)
        {
            // Multiple input parameters are separated by the string "||".
            var words = message.Split("||");
            if (words.Length > 0)
            {
                var command = words[0];
                string[] parameters = null;
                if (words.Length - 1 > 0)
                {
                    parameters = new string[words.Length - 1];
                    for (var i = 0; i < parameters.Length; i++) parameters[i] = words[i + 1];
                }

                Handler.Trigger(command, parameters);
            }
        }

        public bool IsOpening()
        {
            return _serverSocket != null;
        }
    }
}