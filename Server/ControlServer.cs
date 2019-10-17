using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using Arvid.Response;
using System.Threading;

namespace Arvid.Server
{
    internal class ControlServer: BaseServer
    {
        private bool _initialized;

        private delegate void Command(ReadOnlySpan<ushort> payload);
        
        private readonly Listener _listener;
        private readonly Dictionary<CommandEnum, Command> _commandMap;
        private readonly Dictionary<CommandEnum, Action> _actionMap;

        public ControlServer(Socket socket, Listener listener) : base(socket)
        {
            _listener = listener;
            _commandMap = new Dictionary<CommandEnum, Command>();
            _actionMap = new Dictionary<CommandEnum, Action>();
        }

        public void Setup()
        {
            var commandEnumValues = Enum.GetValues(typeof(CommandEnum));

            foreach (var commandValue in commandEnumValues)
            {
                var commandEnum = (CommandEnum)commandValue;

                // Ignore these for the map
                switch (commandEnum)
                {
                    case CommandEnum.Init:
                    case CommandEnum.Blit:
                        continue;
                }

                // Create delegate and add to the delegateMap
                var method = GetType().GetMethod(commandValue.ToString(), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (method.GetParameters().Length == 0)
                    _actionMap.Add(commandEnum, Delegate.CreateDelegate(typeof(Action), this, method.Name) as Action);
                else if (method.GetParameters().Length == 1)
                    _commandMap.Add(commandEnum, Delegate.CreateDelegate(typeof(Command), this, method.Name) as Command);
                else
                    throw new Exception("Invalid method definition");
            }
        }

        protected override unsafe void DoWork()
        {
            const int bufferSize = 128;
            var receiveBuffer = stackalloc ushort[bufferSize];
            var receiveSpan = new Span<byte>(receiveBuffer, bufferSize * 2);

            try
            {
                while (true)
                {
                    var receivedWords = Socket.Receive(receiveSpan) / 2;

                    if (receivedWords == 0)
                    {
                        Console.WriteLine("Empty answer received. Shutting down!");
                        _listener.EnqueueMessage(ListenerMessage.Stop);
                        return;
                    }

                    Debug.Assert(receivedWords >= 2);

                    var command = (CommandEnum)receiveBuffer[0];

                    if (command == CommandEnum.Init)
                    {
                        Init();
                        continue;
                    }

                    if (!_initialized) continue;

                    var hasPayload = receivedWords > 2;

                    if (hasPayload)
                        _commandMap[command](new ReadOnlySpan<ushort>(receiveBuffer + 2, receivedWords - 2));
                    else
                        _actionMap[command]();
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
                _listener.EnqueueMessage(ListenerMessage.Stop);
                return;
            }
        }
        
        // Commands

        private unsafe void _sendEmptyResponse()
        {
            var sendBuffer = stackalloc ushort[6];
            Socket.Send(new ReadOnlySpan<byte>(sendBuffer, 12));
        }
        
        private unsafe void _sendStandardResponse(int responseData)
        {
            var response = new StandardResponse {responseData = responseData};
            Socket.Send(new ReadOnlySpan<byte>(response.rawData, sizeof(StandardResponse)));
        }
        
        private unsafe void _sendStandardResponse(float responseData)
        {
            var response = new StandardResponse {floatResponse = responseData};
            Socket.Send(new ReadOnlySpan<byte>(response.rawData, sizeof(StandardResponse)));
        }

        private void Init()
        {
            if (!_initialized)
            {
                _listener.EnqueueMessage(ListenerMessage.Init);
                _initialized = true;
            }
            
            _sendEmptyResponse();
        }

        private void Close()
        {
            _sendEmptyResponse();
            _listener.EnqueueMessage(ListenerMessage.Stop);
        }

        private void PowerOff()
        {
            _sendEmptyResponse();
            _listener.EnqueueMessage(ListenerMessage.ShutDown);
        }

        private void GetFrameNumber()
        {
            _sendStandardResponse((int) PruManager.GetFrameNumber());
        }

        private unsafe void Vsync()
        {
            PruManager.WaitForVsync();
            var response = new VsyncResponse
            {
                frameNumber = PruManager.GetFrameNumber(), buttons = PruManager.GetButtons()
            };
            Socket.Send(new ReadOnlySpan<byte>(response.rawData, sizeof(VsyncResponse)));
        }

        private void SetVideoMode(ReadOnlySpan<ushort> payload)
        {
            var mode = (VideoMode) payload[0];
            var lines = payload[1];
            
            PruManager.SetVideoMode(mode, lines);
            
            _sendEmptyResponse();
        }

        private void GetVideoModeLines(ReadOnlySpan<ushort> payload)
        {
            var modeVal = payload[0];

            //check video mode
            if (modeVal >= (ushort) VideoMode.ModeCount)
            {
                _sendStandardResponse((int) ServerError.ArvidErrorIllegalVideoMode);
                return;
            }

            //search through frame rate table
            var table = VideoModeHelpers.LineRates[modeVal];

            var freq = payload[1] / 1000.0f;

            //limit rates to sane values
            if (freq < table[0].Rate)
            {
                _sendStandardResponse(table[0].Lines);
                return;
            }

            if (freq > table[^1].Rate)
            {
                _sendStandardResponse(table[^1].Lines);
                return;
            }

            var lineRateIndex = Array.BinarySearch(
                table, 
                new VideoModeHelpers.LineRate(0, freq), 
                VideoModeHelpers.FrequencyComparer
            );

            if (lineRateIndex < 0)
                lineRateIndex = ~lineRateIndex;
            
            _sendStandardResponse(table[lineRateIndex].Lines);
        }

        private void GetVideoModeFrequency(ReadOnlySpan<ushort> payload)
        {
            var modeVal = payload[0];

            //check video mode
            if (modeVal >= (ushort) VideoMode.ModeCount)
            {
                _sendStandardResponse((int) ServerError.ArvidErrorIllegalVideoMode);
                return;
            }
            
            //search through frame rate table
            var table = VideoModeHelpers.LineRates[modeVal];
            
            var lines = payload[1];

            //limit lines to sane values
            if (lines > table[0].Lines)
            {
                _sendStandardResponse(table[0].Rate);
                return;
            }

            if (lines < table[^1].Lines)
            {
                _sendStandardResponse(table[^1].Rate);
                return;
            }

            var lineRateIndex = Array.BinarySearch(
                table, 
                new VideoModeHelpers.LineRate(lines, 0), 
                VideoModeHelpers.LinesComparer   
            );

            if (lineRateIndex < 0)
                lineRateIndex = ~lineRateIndex;
            
            _sendStandardResponse(table[lineRateIndex].Rate);
        }

        private void GetWidth()
        {
            _sendStandardResponse(PruManager.Width);
        }

        private void GetHeight()
        {
            _sendStandardResponse(PruManager.Height);
        }

        private unsafe void EnumVideoModes()
        {
            var response = new VideoListResponse {responseData = VideoModeHelpers.VideoModeInfoTable.Length};

            fixed (VideoModeInfo* videoModesPtr = VideoModeHelpers.VideoModeInfoTable)
            {
                Buffer.MemoryCopy(
                    videoModesPtr,
                    response.videoModePayload,
                    sizeof(VideoModeInfo) * VideoModeHelpers.VideoModeInfoTable.Length,
                    sizeof(VideoModeInfo) * VideoModeHelpers.VideoModeInfoTable.Length
                );
            }
            
            Socket.Send(new ReadOnlySpan<byte>(response.rawData, sizeof(StandardResponse)));
        }

        private void GetVideoModeCount()
        {
            _sendStandardResponse(VideoModeHelpers.VideoModeInfoTable.Length);
        }

        private void GetLineMod()
        {
            _sendStandardResponse(PruManager.LinePosMod);
        }

        private void SetLineMod(ReadOnlySpan<ushort> receivedData)
        {
            PruManager.SetLinePosMod(receivedData[0]);
            _sendEmptyResponse();
        }

        private void SetVirtualSync(ReadOnlySpan<ushort> receivedData)
        {
            PruManager.SetVsyncLine(receivedData[0]);
            _sendEmptyResponse();
        }

        private void SetInterlacing(ReadOnlySpan<ushort> receivedData)
        {
            PruManager.SetInterlacing(receivedData[0]);
            _sendEmptyResponse();            
        }
        
        private void UpdateStart(ReadOnlySpan<ushort> receivedData) {}
        private void UpdatePacket(ReadOnlySpan<ushort> receivedData) {}
        private void UpdateEnd(ReadOnlySpan<ushort> receivedData) {}
    }
}