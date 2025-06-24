using NetworkClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.Clients
{
    public class MessageHandler
    {
        public Session session { get; protected set; }
        public MessageHandler(Session session)
        {
            this.session = session;
        }

        public void onMessage(Message msg)
        {
            switch (msg.Command)
            {

            }
        }
    }
}
