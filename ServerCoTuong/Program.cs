using ServerCoTuong.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCoTuong
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MainServer.INSTANCE.startServer();
            while (true)
            {
                try
                {
                    Thread.Sleep(1000);
                }catch (Exception ex) { }
            }
        }
    }
}
