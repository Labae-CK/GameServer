﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        // [size(2)][packetId(2)][....]
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;

            while (true)
            {
                // 최소한 헤더는 파싱할 수 있는지 확인
                if (buffer.Count < HeaderSize)
                {
                    break;
                }

                // 패킷이 완전체로 도착했는지 확인.
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                {
                    break;
                }

                // 패킷 조립 가능.
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        private Socket _socket;
        private int _disconnected = 0;

        private object _lock = new object();
        private Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        private List<ArraySegment<byte>> _sendPendingList = new List<ArraySegment<byte>>();
        private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        private RecvBuffer _recvBuffer = new RecvBuffer(100);

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        private void Clear()
        {
            lock(_lock)
            {
                _sendQueue.Clear();
                _sendPendingList.Clear();
            }
        }

        public void Start(Socket socket)
        {
            _socket = socket;
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void Send(ArraySegment<byte> sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_sendPendingList.Count == 0)
                {
                    RegisterSend();
                }
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            {
                return;
            }

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();
        }

        #region Network communications

        private void RegisterSend()
        {
            if (_disconnected == 1)
            {
                return;
            }    

            while (_sendQueue.Count > 0)
            {
                var buff = _sendQueue.Dequeue();
                _sendPendingList.Add(buff);
            }
            _sendArgs.BufferList = _sendPendingList;

            try
            {
                bool pending = _socket.SendAsync(_sendArgs);
                if (pending == false)
                {
                    OnSendCompleted(null, _sendArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Register send failed {e}");
            }
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _sendPendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        //// todo : 이거 필요한지 모르겠음.
                        //Console.WriteLine($"Elapsed sene queue count : {_sendQueue.Count}");
                        if (_sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        private void RegisterRecv()
        {
            if (_disconnected == 1)
            {
                return;
            }

            _recvBuffer.Clean();
            var segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (pending == false)
                {
                    OnRecvCompleted(null, _recvArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Register recv failed : {e}");
            }
        }

        private void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Console.WriteLine("WRITE");
                        Thread.Sleep(100000);
                        Disconnect();
                        return;
                    }

                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        Console.WriteLine("RECV");
                        Thread.Sleep(100000);
                        Disconnect();
                        return;
                    }

                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        Console.WriteLine("READ");
                        Thread.Sleep(100000);
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch (Exception e)
                {
                    Thread.Sleep(10000);
                    Console.WriteLine($"OnRecvCompleted failed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }

        #endregion
    }
}
