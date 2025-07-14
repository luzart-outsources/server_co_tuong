using NetworkClient.Interfaces;
using NetworkClient.Models;
using NetworkClient.Network.Common;
using NetworkClient.Network.Tcp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkClient.Network.WebSocket
{
    public class WebSocketClient : INetworkConnection
    {
        private ClientWebSocket socket;
        private Uri serverUri;
        private CancellationTokenSource cts;
        private CancellationTokenSource ctsReceive;
        private CancellationTokenSource ctsSend;
        private ReceiveState receiveState;

        private Action<Message> messageCallback;
        private Action disconnectCallback;
        private ConcurrentQueue<byte[]> msgSend;

        private byte[] Keys;
        public bool isGetKeyCompress => Keys != null;

        private bool _isConnected;
        public bool IsConnected => !IsWaitConnect && _isConnected && socket != null;

        public bool IsWaitConnect { get; protected set; }
        private bool _isDisconnected;
        private int isSendingInt;
        public Exception getException { get; protected set; }

        public WebSocketClient(Action<Message> messageCallback, Action disconnectCallback)
        {
            this.messageCallback = messageCallback;
            ctsReceive = new CancellationTokenSource();
            ctsSend = new CancellationTokenSource();
            this.disconnectCallback = disconnectCallback;
        }

        public void Connect(string ip, int port)
        {
            if (IsWaitConnect || IsConnected)
                return;

            
            serverUri = new Uri($"ws://{ip}:{port}");
            cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(10)); //timeout 10s
            socket = new ClientWebSocket();
            IsWaitConnect = true;
            _isDisconnected = false;
            _isConnected = false;
            Interlocked.Exchange(ref isSendingInt, 0);
            Task.Run(async () =>
            {
                try
                {
                    await socket.ConnectAsync(serverUri, cts.Token);
                    _isConnected = true;
                    IsWaitConnect = false;
                    msgSend = new ConcurrentQueue<byte[]>();
                    await ReceiveLoop();
                }
                catch (Exception ex)
                {
                    getException = ex;
                    Disconnect();
                }
            });
        }

        private async Task ReceiveLoop()
        {
            try
            {
                int count;
                if (receiveState == null)
                    receiveState = new ReceiveState(3, true);
                while (IsConnected)
                {
                    receiveState.setReceive(3, true);
                    count = await ReceiveExact(receiveState._buffer, 0, receiveState.Length);

                    int size = NetworkUtils.Bytes3ToInt(receiveState._buffer);
                    if (size <= 0 || count == 0)
                        break;

                    receiveState.setReceive(size, true);
                    count = await ReceiveExact(receiveState._buffer, 0, receiveState.Length);
                    if (count == 0)
                        break;

                    byte[] payload;
                    if (isGetKeyCompress)
                        payload = NetworkUtils.deserializer(receiveState._buffer, Keys);
                    else
                    {
                        payload = new byte[receiveState.Length];
                        Buffer.BlockCopy(receiveState._buffer, 0, payload, 0, receiveState.Length);
                    }

                    MessageCallback(new Message(payload));
                }
            }
            catch { }
            finally
            {
                Disconnect();
            }
        }

        private async Task<int> ReceiveExact(byte[] buffer, int offset, int length)
        {
            int cReceive = 0;
            while (cReceive < length)
            {
                var segment = new ArraySegment<byte>(buffer, offset + cReceive, length - cReceive);
                var result = await socket.ReceiveAsync(segment, ctsReceive.Token);

                if (result.MessageType == WebSocketMessageType.Close || result.Count == 0)
                    return 0;

                cReceive += result.Count;
            }
            return cReceive;
        }

        private async Task StartSend()
        {
            if (msgSend == null)
                return;
            try
            {
                while (msgSend.TryDequeue(out var data))
                {
                    var sendSuccess = await SendAsync(data);
                    if (!sendSuccess)
                    {
                        Disconnect();
                        return;
                    }    
                }    
            }
            catch (Exception ex) 
            {
                getException = ex;
                Disconnect();
            }
            finally
            {
                Interlocked.Exchange(ref isSendingInt, 0);
                if (!msgSend.IsEmpty && Interlocked.CompareExchange(ref isSendingInt, 1, 0) != 1)
                    _ = StartSend();
            }
        }

        private async Task<bool> SendAsync(byte[] data)
        {
            if (!IsConnected)
                return false;

            try
            {
                int offset = 0;
                int chunkSize = 8192; // hoặc tùy chọn

                while (offset < data.Length)
                {
                    int toSend = Math.Min(chunkSize, data.Length - offset);
                    bool endOfMessage = (offset + toSend == data.Length);

                    await socket.SendAsync(
                        new ArraySegment<byte>(data, offset, toSend),
                        WebSocketMessageType.Binary,
                        endOfMessage,
                        ctsSend.Token
                    );

                    offset += toSend;
                }

                return true;
            }
            catch (OperationCanceledException e)
            {
                getException = e;
                return false;
            }
            catch (Exception ex)
            {
                getException = ex;
                return false;
            }
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
            if (IsConnected && msgSend != null)
            {
                var bsize = NetworkUtils.IntTo3Bytes(data.Length);
                byte[] payload = new byte[data.Length + 3];
                Buffer.BlockCopy(bsize, 0, payload, 0, bsize.Length);
                Buffer.BlockCopy(data, 0, payload, 3, data.Length);

                msgSend.Enqueue(payload);
                if (Interlocked.CompareExchange(ref isSendingInt, 1, 0) == 0)
                    Task.Run(StartSend);
            }
        }

        public void Disconnect()
        {
            if (_isDisconnected)
                return;
            try
            {
                ctsReceive?.Cancel();
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"[Disconnect] Cancel error: {ex.Message}");
            }

            try
            {
                // Đóng WebSocket nếu còn mở
                if (socket != null && socket.State == WebSocketState.Open)
                {
                    socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None)
                          .GetAwaiter().GetResult(); // sync để đảm bảo xong
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"[Disconnect] CloseAsync error: {ex.Message}");
            }

            try
            {
                socket?.Dispose();
                socket = null;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"[Disconnect] Dispose error: {ex.Message}");
            }


            try
            {
                ctsReceive?.Dispose();
                ctsReceive = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Disconnect] CTS dispose error: {ex.Message}");
            }
            try
            {
                ctsSend?.Dispose();
                ctsSend = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Disconnect] CTS dispose error: {ex.Message}");
            }
            try
            {
                DisconnectCalback();
            }
            catch { }
            Interlocked.Exchange(ref isSendingInt, 0);
            _isDisconnected = true;
            _isConnected = false;
            IsWaitConnect = false;
            Keys = null;
            msgSend = null;

        }

        public void Dispose()
        {
            Disconnect();
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
