using System.Buffers;
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
            // Trả buffer cũ (dù lớn hơn hay nhỏ hơn)
            if (_buffer != null && _buffer.Length != bufferSize)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
                _buffer = null;
            }

            if(_buffer == null)
                _buffer = ArrayPool<byte>.Shared.Rent(bufferSize); // mượn mới
            this.isReadLenght = isReadLenght;
        }

        public void Clear()
        {
            if (_buffer != null)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
                _buffer = null;
                _bufferSize = 0;
                Offset = 0;
            }
        }

        //public void setReceive(int bufferSize, bool isReadLenght)
        //{
        //    _buffer = new byte[bufferSize];
        //    this.isReadLenght = isReadLenght;
        //}
    }
}
