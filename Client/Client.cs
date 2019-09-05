using System;
using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Arvid.Client.Blit;
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

        private ushort _nextId;

        private Blitter _blitter;
        
        public bool Connected { get; private set; }

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
            _blitter?.Dispose();
            _control.Dispose();
            _data.Dispose();
        }

        private unsafe int _getRegularResponse(ushort id)
        {
            var responseStruct = new StandardResponse();

            do
            {
                _control.Receive(new Span<byte>(responseStruct.rawData, sizeof(StandardResponse)));
            } while (id != responseStruct.id);
            
            return responseStruct.responseData;
        }

        private unsafe ushort _createAndSendControlPayload(
            CommandEnum command, 
            ReadOnlySpan<ushort> arguments = new ReadOnlySpan<ushort>()
        ) {
            Debug.Assert(Connected);

            var id = _nextId++;

            // the extra is for an id
            var payloadSize = sizeof(CommandEnum) / 2 + arguments.Length + 1;
            var payload = stackalloc ushort[payloadSize];

            payload[0] = (ushort) command;
            payload[1] = id;

            for (var i = 0; i < arguments.Length; i++)
            {
                payload[i + 2] = arguments[i];
            }

            var payloadSpan = new ReadOnlySpan<byte>(payload, payloadSize * 2);
            _control.Send(payloadSpan);

            return id;
        }

        public bool Connect()
        {
            if (Connected) return true;

            try
            {
                _control.Connect(_controlEndPoint);
                _data.Connect(_dataEndPoint);
                _blitter = new Blitter();
                _blitter.Start();
                _blitter.SegmentReady += BlitterOnSegmentReady;
            }
            catch
            {
                if (_control.Connected) _control.Disconnect(true);
                return false;
            }

            var id = _createAndSendControlPayload(CommandEnum.Init);
            var result = _getRegularResponse(id);

            return Connected = result >= 0;
        }

        public int Disconnect()
        {
            if (!Connected) return -1;
            
            var id = _createAndSendControlPayload(CommandEnum.Close);
            var result = _getRegularResponse(id);
            
            _control.Disconnect(true);
            _data.Disconnect(true);
            Connected = false;
            
            _blitter.Stop();
            _blitter.Dispose();
            _blitter = null;

            return result;
        }
        
        private unsafe void BlitterOnSegmentReady(SegmentWorkOutput output)
        {
            // the extra is for an id
            var payloadSize = (sizeof(CommandEnum) >> 1) + 3 + output.CompressedSize;
            var payload = _arrayPool.Rent(payloadSize);

            payload[0] = (ushort) CommandEnum.Blit;
            payload[1] = output.CompressedSize;
            payload[2] = output.YPos;
            payload[3] = output.Stride;
            
            Array.Copy(
                output.Output, 
                0, 
                payload, 
                4, 
                output.CompressedSize
            );

            fixed (void* payloadPtr = payload)
            {
                var payloadSpan = new ReadOnlySpan<byte>(payloadPtr, payloadSize << 1);
                _data.Send(payloadSpan);
            }
        }

        public int BlitBuffer(ushort[] buffer, int width, int height)
        {
            if (!Connected) return -1;

            _blitter.Blit(buffer, height, width);
            
            return 0;
        }

        public int GetFrameNumber()
        {
            if (!Connected) return 0;

            var id = _createAndSendControlPayload(CommandEnum.GetFrameNumber);
            return _getRegularResponse(id);
        }

        public int WaitForVsync()
        {
            if (!Connected) return 0;

            _blitter.Wait();
            
            var id = _createAndSendControlPayload(CommandEnum.Vsync);
            var response = _getRegularResponse(id);

            Span<byte> responseData = stackalloc byte[4];
            _control.Receive(responseData);

            // TODO: Do something with the response data containing buttons
            
            return response;
        }

        public unsafe int SetVideoMode(VideoMode mode, int lines)
        {
            if (!Connected) return -1;

            var arguments = stackalloc ushort[]
            {
                (ushort) mode,
                (ushort) lines,
            };

            var id = _createAndSendControlPayload(
                CommandEnum.SetVideoMode,
                new Span<ushort>(arguments, 2)
            );

            return _getRegularResponse(id);
        }

        public unsafe int GetVideoModeLines(VideoMode mode, float frequency)
        {
            if (!Connected) return -1;

            var normalizedFreq = (ushort)(frequency * 1000);

            var arguments = stackalloc ushort[]
            {
                (ushort) mode,
                normalizedFreq
            };
            
            var id = _createAndSendControlPayload(
                CommandEnum.GetVideoModeLines,
                new ReadOnlySpan<ushort>(arguments, 2)
            );

            return _getRegularResponse(id);
        }

        public unsafe float GetVideoModeRefreshRate(VideoMode mode, int lines)
        {
            if (!Connected) return 0f;
            
            var arguments = stackalloc ushort[]
            {
                (ushort) mode,
                (ushort) lines
            };

            var id = _createAndSendControlPayload(
                CommandEnum.GetVideoModeFrequency,
                new ReadOnlySpan<ushort>(arguments, 2)
            );

            return _getRegularResponse(id) / 1000.0f;
        }

        public int GetWidth()
        {
            if (!Connected) return 0;
            
            var id = _createAndSendControlPayload(
                CommandEnum.GetWidth
            );

            return _getRegularResponse(id);
        }
        
        public int GetHeight()
        {
            if (!Connected) return 0;
            
            var id = _createAndSendControlPayload(
                CommandEnum.GetHeight
            );

            return _getRegularResponse(id);
        }

        public int GetVideoModeCount()
        {
            if (!Connected) return 0;
            
            var id = _createAndSendControlPayload(
                CommandEnum.GetVideoModeCount
            );

            return _getRegularResponse(id);
        }

        public int EnumVideoModes(ref VideoModeInfo[] videoModeInfos)
        {
            if (!Connected) return -1;

            var id = _createAndSendControlPayload(
                CommandEnum.EnumVideoModes
            );

            var count = _getRegularResponse(id);

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

        public unsafe int SetVirtualSync(int vsyncLine)
        {
            if (!Connected) return -1;

            var arguments = stackalloc ushort[]
            {
                (ushort) vsyncLine,
            };
            
            var id = _createAndSendControlPayload(
                CommandEnum.SetVirtualSync,
                new ReadOnlySpan<ushort>(arguments, 1)
            );

            return _getRegularResponse(id);
        }

        public unsafe int SetLinePosMod(int mod)
        {
            if (!Connected) return -1;

            var arguments = stackalloc ushort[]
            {
                (ushort) mod,
            };
            
            var id = _createAndSendControlPayload(
                CommandEnum.SetLineMod,
                new ReadOnlySpan<ushort>(arguments, 1)
            );
            
            return _getRegularResponse(id);
        }

        public int GetLinePosMod()
        {
            if (!Connected) return -1;

            var id = _createAndSendControlPayload(
                CommandEnum.GetLineMod
            );

            return _getRegularResponse(id);
        }

        public int PowerOffServer()
        {
            if (!Connected) return -1;

            var id = _createAndSendControlPayload(
                CommandEnum.PowerOff
            );
            
            return _getRegularResponse(id);
        }
    }
}