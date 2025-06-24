using NetworkClient.Network.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkClient.Models
{
    public class Message : IDisposable
    {
        public int Command {  get; protected set; }
        public myWriter Writer { get; protected set; }
        public myReader Reader { get; protected set; }
        public Message(byte[] data)
        {
            Reader = new myReader(NetworkUtils.ByteToSByte(data));
            Command = Reader.readByte();
        }

        /// <summary>
        /// Command value within byte limit
        /// </summary>
        /// <param name="command"></param>
        public Message(int command)
        {
            Writer = new myWriter((byte)command);
        }

        public byte[] getData()
        {
            return NetworkUtils.SByteToByte(Writer.getData());
        }

        public void Dispose()
        {
            if(Writer!=null)
                Writer.Clear();
            if(Reader!=null)
                Reader.Clear();
            Writer = null;
            Reader = null;
        }
    }
}
