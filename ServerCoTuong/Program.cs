using ServerCoTuong.loggers;
using ServerCoTuong.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ServerCoTuong
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception ex = e.ExceptionObject as Exception;
                csLog.logErr(ex);
                File.AppendAllText("log.txt",
                    $"Unhandled exception: {ex?.GetType()} - {ex?.Message}\n {ex.StackTrace}\n");
            };
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
