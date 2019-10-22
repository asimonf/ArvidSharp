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

        private delegate void Command(ushort id, ReadOnlySpan<ushort> payload);
        
        private readonly Listener _listener;
        private readonly Dictionary<CommandEnum, Command> _commandMap;

        public ControlServer(Socket socket, Listener listener) : base(socket)
        {
            _listener = listener;
            _commandMap = new Dictionary<CommandEnum, Command>();
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

                _commandMap.Add(commandEnum, Delegate.CreateDelegate(typeof(Command), this, method.Name) as Command);
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
                    // Clear out the buffer
                    for (var i = 0; i < bufferSize; i++)
                    {
                        receiveBuffer[i] = 0;
                    }
                    
                    var receivedWords = Socket.Receive(receiveSpan) >> 1;

                    if (receivedWords == 0)
                    {
                        Console.WriteLine("Empty answer received. Shutting down!");
                        _listener.EnqueueMessage(ListenerMessage.Stop);
                        return;
                    }

                    Debug.Assert(receivedWords >= 2);

                    var command = (CommandEnum)receiveBuffer[0];
                    var id = receiveBuffer[1];
                    var hasPayload = receivedWords > 2;

                    if (command == CommandEnum.Init)
                    {
                        Init(id, ReadOnlySpan<ushort>.Empty);
                        continue;
                    }
                    
                    Debug.Assert(command == CommandEnum.Vsync);
                    Debug.Assert(hasPayload == false);

                    if (!_initialized) continue;
                    
                    var payload = hasPayload
                        ? new ReadOnlySpan<ushort>(receiveBuffer + 2, receivedWords - 2)
                        : ReadOnlySpan<ushort>.Empty
                    ;

                    _commandMap[command](id, payload);
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
                _listener.EnqueueMessage(ListenerMessage.Stop);
            }
        }
        
        // Commands

        private unsafe void _sendStandardResponse(ushort id, int responseData = 0)
        {
            var response = new StandardResponse
            {
                id = id,
                responseData = responseData
            };
            Socket.Send(new ReadOnlySpan<byte>(response.rawData, sizeof(StandardResponse)));
        }
        
        private unsafe void _sendStandardResponse(ushort id, float responseData)
        {
            var response = new StandardResponse
            {
                id = id,
                floatResponse = responseData
            };
            Socket.Send(new ReadOnlySpan<byte>(response.rawData, sizeof(StandardResponse)));
        }

        private void Init(ushort id, ReadOnlySpan<ushort> payload)
        {
            if (!_initialized)
            {
                _listener.EnqueueMessage(ListenerMessage.Init);
                _initialized = true;
            }

            _sendStandardResponse(id);
        }

        private void Close(ushort id, ReadOnlySpan<ushort> payload)
        {
            _sendStandardResponse(id);
            _listener.EnqueueMessage(ListenerMessage.Stop);
        }

        private void PowerOff(ushort id, ReadOnlySpan<ushort> payload)
        {
            _sendStandardResponse(id);
            _listener.EnqueueMessage(ListenerMessage.ShutDown);
        }

        private void GetFrameNumber(ushort id, ReadOnlySpan<ushort> payload)
        {
            _sendStandardResponse(id, (int) PruManager.GetFrameNumber());
        }

        private unsafe void Vsync(ushort id, ReadOnlySpan<ushort> payload)
        {
            PruManager.WaitForVsync();
            var response = new VsyncResponse
            {
                id = id,
                frameNumber = PruManager.GetFrameNumber(), buttons = PruManager.GetButtons()
            };
            Socket.Send(new ReadOnlySpan<byte>(response.rawData, sizeof(VsyncResponse)));
        }

        private void SetVideoMode(ushort id, ReadOnlySpan<ushort> payload)
        {
            var mode = (VideoMode) payload[0];
            var lines = payload[1];
            
            PruManager.SetVideoMode(mode, lines);

            _sendStandardResponse(id);
        }

        private void GetVideoModeLines(ushort id, ReadOnlySpan<ushort> payload)
        {
            var modeVal = payload[0];

            //check video mode
            if (modeVal >= (ushort) VideoMode.ModeCount)
            {
                _sendStandardResponse(id, (int) ServerError.ArvidErrorIllegalVideoMode);
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
            
            _sendStandardResponse(id, table[lineRateIndex].Lines);
        }

        private void GetVideoModeFrequency(ushort id, ReadOnlySpan<ushort> payload)
        {
            var modeVal = payload[0];

            //check video mode
            if (modeVal >= (ushort) VideoMode.ModeCount)
            {
                _sendStandardResponse(id, (int) ServerError.ArvidErrorIllegalVideoMode);
                return;
            }
            
            //search through frame rate table
            var table = VideoModeHelpers.LineRates[modeVal];
            
            var lines = payload[1];

            //limit lines to sane values
            if (lines > table[0].Lines)
            {
                _sendStandardResponse(id, table[0].Rate);
                return;
            }

            if (lines < table[^1].Lines)
            {
                _sendStandardResponse(id, table[^1].Rate);
                return;
            }

            var lineRateIndex = Array.BinarySearch(
                table, 
                new VideoModeHelpers.LineRate(lines, 0), 
                VideoModeHelpers.LinesComparer   
            );

            if (lineRateIndex < 0)
                lineRateIndex = ~lineRateIndex;
            
            _sendStandardResponse(id, table[lineRateIndex].Rate);
        }

        private void GetWidth(ushort id, ReadOnlySpan<ushort> payload)
        {
            _sendStandardResponse(id, PruManager.Width);
        }

        private void GetHeight(ushort id, ReadOnlySpan<ushort> payload)
        {
            _sendStandardResponse(id, PruManager.Height);
        }

        private unsafe void EnumVideoModes(ushort id, ReadOnlySpan<ushort> payload)
        {
            var response = new VideoListResponse
            {
                id = id,
                responseData = VideoModeHelpers.VideoModeInfoTable.Length
            };

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

        private void GetVideoModeCount(ushort id, ReadOnlySpan<ushort> payload)
        {
            _sendStandardResponse(id, VideoModeHelpers.VideoModeInfoTable.Length);
        }

        private void GetLineMod(ushort id, ReadOnlySpan<ushort> payload)
        {
            _sendStandardResponse(id, PruManager.LinePosMod);
        }

        private void SetLineMod(ushort id, ReadOnlySpan<ushort> receivedData)
        {
            PruManager.SetLinePosMod(receivedData[0]);
            _sendStandardResponse(id);
        }

        private void SetVirtualSync(ushort id, ReadOnlySpan<ushort> receivedData)
        {
            PruManager.SetVsyncLine(receivedData[0]);
            _sendStandardResponse(id);
        }

        private void SetInterlacing(ushort id, ReadOnlySpan<ushort> receivedData)
        {
            PruManager.SetInterlacing(receivedData[0]);
            _sendStandardResponse(id);
        }
        
        private void UpdateStart(ushort id, ReadOnlySpan<ushort> receivedData) {}
        private void UpdatePacket(ushort id, ReadOnlySpan<ushort> receivedData) {}
        private void UpdateEnd(ushort id, ReadOnlySpan<ushort> receivedData) {}
    }
}