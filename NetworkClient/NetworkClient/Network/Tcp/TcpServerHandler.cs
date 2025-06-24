using NetworkClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace NetworkClient.Network.Tcp
{
    public class TcpServerHandler
    {
        public Socket socketTcp { get; protected set; }
        public int port { get; protected set; }
        public Action<SessionTCP> acceptCallback { get; protected set; }
        public IPEndPoint ipEndPoint { get; protected set; }
        public bool isStart { get; protected set; }

        public TcpServerHandler(int port, Action<SessionTCP> acceptCallback)
        {
            this.port = port;
            this.acceptCallback = acceptCallback;
            ipEndPoint = new IPEndPoint(IPAddress.Loopback, this.port);
        }

        public void startServer()
        {
            socketTcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketTcp.Bind(ipEndPoint);
            socketTcp.Listen(100);
            isStart = true;
            Console.WriteLine($"Server binding port {ipEndPoint.Port}");
        }

        private void StartAccept()
        {
            try
            {
                if (socketTcp != null)
                {
                    socketTcp.BeginAccept(OnClientConnect, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AcceptAsync is error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void OnClientConnect(IAsyncResult asyn)
        {
            Socket sock = null;
            try
            {
                sock = socketTcp.EndAccept(asyn);
                if (sock != null && sock.Connected)
                {
                    SessionTCP s = new SessionTCP(sock);
                    acceptCallback?.Invoke(s);
                }
                else if(sock != null)
                {
                    sock.Close();
                    sock.Dispose();
                }
            }
            catch (Exception rx)
            {
                Console.WriteLine($"AcceptCompleted is error: {rx.Message}\n{rx.StackTrace}");
                if (sock != null)
                {
                    try
                    {
                        sock.Close();
                        sock.Dispose();
                    }
                    catch
                    {
                    }
                }
            }
            finally
            {
                this.StartAccept();
            }
        }
    }
}
