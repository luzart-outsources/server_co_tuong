using NetworkClient.Interfaces;
using NetworkClient.Models;
using NetworkClient.Network.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace NetworkClient.Network.Tcp
{
    public class TcpClientHandler : INetworkConnection
    {
        private Socket socket;

        public TcpClientHandler(Action<Message> messageCallback, Action disconnectCallback)
        {
            this.messageCallback = messageCallback;
            this.disconnectCallback = disconnectCallback;
        }

        private bool _isConnected;

        public bool IsConnected => !IsWaitConnect && _isConnected && socket != null;

        public bool IsWaitConnect { get; protected set; }
        protected bool _disconnected;

        private ConcurrentQueue<byte[]> msgSend;

        private byte[] Keys;
        public bool isGetKeyCompress => Keys != null;
        private ReceiveState receiveState;
        private Action<Message> messageCallback;
        private Action disconnectCallback;

        private int isSendingInt;

        public void Connect(string ip, int port)
        {
            if (IsWaitConnect || IsConnected)
                return;
            try
            {
                IsWaitConnect = true;
                _disconnected = false;
                _isConnected = false;
                if (socket == null)
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), ConnectCallback, null);
            }
            catch (Exception ex)
            {
                Disconnect();
            }
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                socket.EndConnect(ar);
                Console.WriteLine("Connected to server!");

                msgSend = new ConcurrentQueue<byte[]>();
                _isConnected = true;
                IsWaitConnect = false;

                StartRecive();
            }
            catch (Exception ex)
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Bắt đầu nhận dữ liệu từ server
        /// </summary>
        private void StartRecive(ReceiveState state = null)
        {
            if (IsConnected)
            {
                if (state == null)
                    receiveState = state = new ReceiveState(3, true);
                else
                    state.setReceive(3, true);
                try
                {
                    socket.BeginReceive(state._buffer, 0, state._buffer.Length, SocketFlags.None, ReceiveCallback, state);
                }
                catch (Exception ex)
                {
                    Disconnect();
                }
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var state = (ReceiveState)ar.AsyncState;
                bool islength = state.isReadLenght;
                var bytesRead = socket.EndReceive(ar);
                if (bytesRead <= 0)
                {
                    Disconnect();
                    return;
                }

                state.Offset += bytesRead;
                if (state.Offset < state.Length)
                {
                    // Chưa đủ → nhận tiếp
                    socket.BeginReceive(state._buffer, state.Offset, state.Length - state.Offset, SocketFlags.None, ReceiveCallback, state);
                    return;
                }

                if (islength)
                {
                    int size = NetworkUtils.Bytes3ToInt(state._buffer);
                    if (size <= 0)
                    {
                        Disconnect();
                        return;
                    }

                    state.setReceive(size, false);
                    socket.BeginReceive(state._buffer, 0, state._buffer.Length, SocketFlags.None, ReceiveCallback, state);
                }
                else
                {
                    var data = state._buffer;
                    if (isGetKeyCompress)
                        data = NetworkUtils.deserializer(data, Keys);
                    MessageCallback(new Message(data));

                    StartRecive(state);
                }
            }
            catch (Exception ex)
            {
                Disconnect();
            }
        }
        
        public void Disconnect()
        {
            if (_disconnected)
                return;
            IsWaitConnect = false;
            _isConnected = false;
            _disconnected = true;
            Keys = null;
            Interlocked.Exchange(ref isSendingInt, 0);

            try
            {
                if(socket != null)
                {
                    if (socket.Connected)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }

                    socket.Close();
                    socket.Dispose();
                    socket = null;
                }
            }
            catch { }
            try
            {
                receiveState?.Clear();  // 👍 Giải phóng buffer
                receiveState = null;
            }
            catch { }
            try
            {
                DisconnectCalback();
            }
            catch { }
            msgSend = null;
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void Send(Message message)
        {
            if (!IsConnected)
                return;
            var data = message.getData();
            if (isGetKeyCompress)
                data = NetworkUtils.serializer(data, Keys);
            SendRaw(data);
        }

        public void SendRaw(byte[] data)
        {
            if(IsConnected)
            {
                var bsize = NetworkUtils.IntTo3Bytes(data.Length);
                byte[] payload = new byte[data.Length+3];
                Buffer.BlockCopy(bsize, 0, payload, 0, bsize.Length);
                Buffer.BlockCopy(data, 0, payload, 3, data.Length);

                msgSend.Enqueue(payload);
                StartSend();
            }    
        }

        private void StartSend()
        {
            if (msgSend == null || Interlocked.CompareExchange(ref isSendingInt, 1, 0) != 0)
                return;
            try
            {
                if(msgSend.TryDequeue(out var data))
                    socket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallback, new SendState(data, 0));
                else
                    Interlocked.Exchange(ref isSendingInt, 0);
            }
            catch
            {
                Disconnect() ;
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var state = (SendState)ar.AsyncState;
                int sent = socket.EndSend(ar);

                state.Offset += sent;

                if (state.Offset < state.Data.Length)
                {
                    socket.BeginSend(state.Data, state.Offset, state.Data.Length - state.Offset, SocketFlags.None, SendCallback, state);
                }
                else
                {
                    Interlocked.Exchange(ref isSendingInt, 0);
                    StartSend();
                }
            }
            catch
            {
                Disconnect();
            }
        }

        public void MessageCallback(Message message)
        {
            messageCallback?.Invoke(message);
        }

        public void DisconnectCalback()
        {
            disconnectCallback?.Invoke();
        }
    }
}
