using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;
using NetworkClient.Interfaces;
using NetworkClient.Models;

namespace NetworkClient.Network.WebSocket
{
    public class SessionWebSocket : INetworkConnection, IWebSocketConnection
    {
        public Action OnOpen { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Action OnClose { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Action<string> OnMessage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Action<byte[]> OnBinary { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Action<byte[]> OnPing { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Action<byte[]> OnPong { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Action<Exception> OnError { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IWebSocketConnectionInfo ConnectionInfo => throw new NotImplementedException();

        public bool IsAvailable => throw new NotImplementedException();

        public bool IsConnected => throw new NotImplementedException();

        public bool IsWaitConnect => false;

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Close(int code)
        {
            throw new NotImplementedException();
        }

        public void Connect(string ip, int port)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void DisconnectCalback()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void MessageCallback(Message message)
        {
            throw new NotImplementedException();
        }

        public Task Send(string message)
        {
            throw new NotImplementedException();
        }

        public Task Send(byte[] message)
        {
            throw new NotImplementedException();
        }

        public void Send(Message message)
        {
            throw new NotImplementedException();
        }

        public Task SendPing(byte[] message)
        {
            throw new NotImplementedException();
        }

        public Task SendPong(byte[] message)
        {
            throw new NotImplementedException();
        }

        public void SendRaw(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
