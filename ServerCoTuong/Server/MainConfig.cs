using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.Server
{
    internal static class MainConfig
    {
        public static string BaseUrl = "https://codetool247.com/";
        public static string UrlRegister = BaseUrl + "register";
        public static bool isDebug = true;
        internal static int MaxFriend = 50;
        internal static long timeWaitInvite = 30_000;
        internal static int MaxInviteRoom = 50;
    }
}
