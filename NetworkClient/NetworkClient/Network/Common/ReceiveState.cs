
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkClient.Network.Common
{
    public class ReceiveState
    {
        public byte[] _buffer { get; private set; }
        public bool isReadLenght { get; set; }

        private int _bufferSize;

        public int Length => _bufferSize;
        public int Offset { get; set; }

        public ReceiveState(int bufferSize, bool isReadLenght)
        {
            setReceive(bufferSize, isReadLenght);
        }

        public void setReceive(int bufferSize, bool isReadLenght)
        {
            _bufferSize = bufferSize;
            Offset = 0;
            _buffer = new byte[bufferSize];
            this.isReadLenght = isReadLenght;
        }

        public void Clear()
        {
            _buffer = null;
            _bufferSize = 0;
            Offset = 0;
        }

        //public void setReceive(int bufferSize, bool isReadLenght)
        //{
        //    _buffer = new byte[bufferSize];
        //    this.isReadLenght = isReadLenght;
        //}
    }
}
