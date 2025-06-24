using NetworkClient.Models;
using NetworkClient.Network.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkClient.Interfaces
{
    public interface INetworkConnection : IDisposable
    {
        void DisconnectCalback();
        void MessageCallback(Message message);
        void Connect(string ip, int port);
        void Send(Message message);
        void SendRaw(byte[] data);
        void Disconnect();
        bool IsConnected { get; }
        bool IsWaitConnect { get; }
    }
}
