using System;
using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Arvid.Response;

namespace Arvid.Client
{
    public class Client: IDisposable
    {
        private readonly IPEndPoint _controlEndPoint;
        private readonly IPEndPoint _dataEndPoint;
        
        private readonly Socket _control;
        private readonly Socket _data;

        private readonly ArrayPool<ushort> _arrayPool;

        private ushort _nextId = 0;
        
        public bool Connected { get; private set; }
        public bool StillBlitting { get; private set; }

        public Client(string ipAddress)
        {
            var ip = IPAddress.Parse(ipAddress);
            
            _controlEndPoint = new IPEndPoint(ip, 32100);
            _dataEndPoint = new IPEndPoint(ip, 32101);
            
            _control = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _data = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            _arrayPool = ArrayPool<ushort>.Create();
        }

        public void Dispose()
        {
            _control.Dispose();
            _data.Dispose();
        }

        private unsafe int _getRegularResponse()
        {
            Debug.Assert(Connected);
            
            Span<byte> responseData = stackalloc byte[sizeof(RegularResponse)];
            _control.Receive(responseData);
            
            var responseStruct = new RegularResponse();
            fixed (byte* responsePtr = responseData)
            {
                Buffer.MemoryCopy(
                    responsePtr, 
                    &responseStruct, 
                    sizeof(RegularResponse), 
                    sizeof(RegularResponse)
                );
            }

            return responseStruct.responseData;
        }

        private unsafe void _createAndSendControlPayload(CommandType command, params ushort[] arguments)
        {
            Debug.Assert(Connected);

            // the extra is for an id
            var payloadSize = (sizeof(CommandType) >> 1) + arguments.Length + 1;
            var payload = stackalloc ushort[payloadSize];

            payload[0] = (ushort) command;
            payload[1] = _nextId++;

            if (arguments.Length > 0)
            {
                fixed (ushort* argumentsPtr = arguments)
                {
                    var copySize = arguments.Length << 1;
                    Buffer.MemoryCopy(argumentsPtr, &payload[2], copySize, copySize);
                }
            }

            var payloadSpan = new ReadOnlySpan<byte>(&payload, payloadSize << 1);
            _control.Send(payloadSpan);
        }

        public bool Connect()
        {
            if (Connected) return true;

            try
            {
                _control.Connect(_controlEndPoint);
                _data.Connect(_dataEndPoint);
            }
            catch
            {
                if (_control.Connected) _control.Disconnect(true);
                return false;
            }

            _createAndSendControlPayload(CommandType.Init);
            var result = _getRegularResponse();

            return Connected = result >= 0;
        }

        public int Disconnect()
        {
            if (!Connected) return -1;
            
            _createAndSendControlPayload(CommandType.Close);

            var result = _getRegularResponse();
            
            _control.Disconnect(true);
            _data.Disconnect(true);
            Connected = false;

            return result;
        }

        public int BlitBuffer(ReadOnlySpan<ushort> buffer, int width, int height, int stride)
        {
            if (!Connected) return -1;

            var byteSize = stride * height;

            return 0;
        }

        public int GetFrameNumber()
        {
            if (!Connected) return 0;

            _createAndSendControlPayload(CommandType.GetFrameNumber);

            return _getRegularResponse();
        }

        public int WaitForVsync()
        {
            if (!Connected) return 0;

            // TODO: Wait for render to finish
            
            _createAndSendControlPayload(CommandType.Vsync);

            var response = _getRegularResponse();

            Span<byte> responseData = stackalloc byte[4];
            _control.Receive(responseData);

            // TODO: Do something with the response data containing buttons
            
            return response;
        }

        public int SetVideoMode(VideoMode mode, int lines)
        {
            if (!Connected) return -1;

            _createAndSendControlPayload(
                CommandType.SetVideoMode,
                (ushort) mode
            );

            return _getRegularResponse();
        }

        public int GetVideoModeLines(VideoMode mode, float frequency)
        {
            if (!Connected) return -1;

            var normalizedFreq = (ushort)(frequency * 1000);
            
            _createAndSendControlPayload(
                CommandType.GetVideoModeLines,
                (ushort)mode,
                normalizedFreq
            );

            return _getRegularResponse();
        }

        public float GetVideoModeRefreshRate(VideoMode mode, int lines)
        {
            if (!Connected) return 0f;

            _createAndSendControlPayload(
                CommandType.GetVideoModeFrequency,
                (ushort) mode,
                (ushort) lines
            );

            return _getRegularResponse() / 1000.0f;
        }

        public int GetWidth()
        {
            if (!Connected) return 0;
            
            _createAndSendControlPayload(
                CommandType.GetWidth
            );

            return _getRegularResponse();
        }
        
        public int GetHeight()
        {
            if (!Connected) return 0;
            
            _createAndSendControlPayload(
                CommandType.GetHeight
            );

            return _getRegularResponse();
        }

        public int GetVideoModeCount()
        {
            if (!Connected) return 0;
            
            _createAndSendControlPayload(
                CommandType.GetVideoModeCount
            );

            return _getRegularResponse();
        }

        public int EnumVideoModes(ref VideoModeInfo[] videoModeInfos)
        {
            if (!Connected) return -1;

            _createAndSendControlPayload(
                CommandType.EnumVideoModes
            );

            var count = _getRegularResponse();

            Span<byte> responseData = stackalloc byte[120];
            _control.Receive(responseData);

            if (count <= 0 || videoModeInfos.Length < count) return -1;

            unsafe
            {
                fixed (VideoModeInfo* videoModeInfosPtr = videoModeInfos)
                fixed (byte* responseDataPtr = responseData)
                {
                    Buffer.MemoryCopy(
                        responseDataPtr,
                        videoModeInfosPtr,
                        sizeof(VideoModeInfo) * videoModeInfos.Length,
                        sizeof(VideoModeInfo) * count
                    );
                }

                return count;
            }
        }

        public int SetVirtualSync(int vsyncLine)
        {
            if (!Connected) return -1;
            
            _createAndSendControlPayload(
                CommandType.SetVirtualSync,
                (ushort) vsyncLine
            );

            return 0;
        }

        public int SetLinePosMod(int mod)
        {
            if (!Connected) return -1;

            _createAndSendControlPayload(
                CommandType.SetLineMod,
                (ushort) mod
            );
            
            return 0;
        }

        public int GetLinePosMod()
        {
            if (!Connected) return -1;

            _createAndSendControlPayload(
                CommandType.GetLineMod
            );

            return _getRegularResponse();
        }

        public int PowerOffServer()
        {
            if (!Connected) return -1;

            _createAndSendControlPayload(
                CommandType.PowerOff
            );

            return _getRegularResponse();
        }
    }
}