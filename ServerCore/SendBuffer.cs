using System;
using System.Threading;

namespace ServerCore
{
    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> currentBuffer = new ThreadLocal<SendBuffer>(() => null);

        public static int ChunkSize { get; set; } = 65535 * 100;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            currentBuffer.Value ??= new SendBuffer(ChunkSize);

            if (currentBuffer.Value.FreeSize < reserveSize)
            {
                currentBuffer.Value = new SendBuffer(ChunkSize);
            }

            return currentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return currentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer
    {
        private readonly byte[] _buffer;
        private int _usedSize;

        public int FreeSize => _buffer.Length - _usedSize;

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }

        public ArraySegment<byte> Open(int reserveSize)
        {
            return reserveSize > FreeSize ? null
                : new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize)
        {
            var segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }
    }
}
