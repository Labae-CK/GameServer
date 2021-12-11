using System;

namespace ServerCore
{
    public class RecvBuffer
    {
        private readonly ArraySegment<byte> _buffer;
        private int _readPos;
        private int _writePos;

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize => _writePos - _readPos;
        public int FreeSize => _buffer.Count - _writePos;

        public ArraySegment<byte> ReadSegment =>
            _buffer.Array != null
                ? new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize) : null;

        public ArraySegment<byte> WriteSegment =>
            _buffer.Array != null
                ? new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize) : null;

        public void Clean()
        {
            var dataSize = DataSize;
            if (dataSize == 0)
            {
                // 남은 데이터가 없으면, 복사하지 않고 커서위치만 리셋.
                _readPos = 0;
                _writePos = 0;
            }
            else
            {
                // 남은 데이터가 있으면, 시작위치로 복사.
                if (_buffer.Array != null)
                {
                    Array.Copy(_buffer.Array, 
                        _buffer.Offset + _readPos, _buffer.Array, 
                        _buffer.Offset, dataSize);
                }
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
            {
                return false;
            }

            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
            {
                return false;
            }

            _writePos += numOfBytes;
            return true;
        }
    }
}
