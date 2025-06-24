using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkClient.Network.Common
{
    public class SendState
    {
        public byte[] Data {  get; private set; }
        public int Offset { get; set; }
        public SendState(byte[] data, int offset)
        {
            Data = data;
            Offset = offset;
        }
    }
}
