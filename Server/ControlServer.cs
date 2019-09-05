using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using Arvid.Response;

namespace Arvid.Server
{
    internal class ControlServer: BaseServer
    {
        private bool _initialized;

        private delegate void Command(ReadOnlySpan<ushort> payload);
        
        private readonly ConcurrentQueue<ListenerMessage> _listenerMessages;
        private readonly Dictionary<CommandEnum, Command> _commandMap;
        private readonly Dictionary<CommandEnum, Action> _actionMap;

        public ControlServer(Socket socket, ConcurrentQueue<ListenerMessage> listenerMessages) : base(socket)
        {
            _listenerMessages = listenerMessages;
            _commandMap = new Dictionary<CommandEnum, Command>();
            _actionMap = new Dictionary<CommandEnum, Action>();

            var commandEnumValues = Enum.GetValues(typeof(CommandEnum));

            foreach (var commandValue in commandEnumValues)
            {
                var commandEnum = (CommandEnum) commandValue;
                
                // Ignore these for the map
                switch (commandEnum)
                {
                    case CommandEnum.Init:
                    case CommandEnum.Blit:
                        continue;
                }

                // Create delegate and add to the delegateMap
                var method = GetType().GetMethod(commandValue.ToString());

                if (method.GetParameters().Length == 0)
                    _actionMap.Add(commandEnum, Delegate.CreateDelegate(typeof(Action), method) as Action);
                else if (method.GetParameters().Length == 1)
                    _commandMap.Add(commandEnum, Delegate.CreateDelegate(typeof(Command), method) as Command);
                else
                    throw new Exception("Invalid method definition");
            }
        }

        protected override unsafe void DoWork()
        {
            const int bufferSize = 128;
            var receiveBuffer = stackalloc ushort[bufferSize];
            var receiveSpan = new Span<byte>(receiveBuffer, bufferSize * 2);

            while (true)
            {
                var receivedWords = _socket.Receive(receiveSpan) / 2;

                Debug.Assert(receivedWords >= 2);

                var command = (CommandEnum) receiveBuffer[0];

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
        }
        
        // Commands

        private unsafe void _sendEmptyAnswer()
        {
            var sendBuffer = stackalloc ushort[6];
            _socket.Send(new ReadOnlySpan<byte>(sendBuffer, 12));
        }
        
        private unsafe void _sendStandardResponse(int responseData)
        {
            var response = new StandardResponse();
            response.responseData = responseData;
            _socket.Send(new ReadOnlySpan<byte>(response.rawData, sizeof(StandardResponse)));
        }

        private void Init()
        {
            if (!_initialized)
            {
                _listenerMessages.Enqueue(ListenerMessage.Init);
                _initialized = true;
            }
            
            _sendEmptyAnswer();
        }

        private void Close()
        {
            _sendEmptyAnswer();
            _listenerMessages.Enqueue(ListenerMessage.Stop);
        }

        private void PowerOff()
        {
            _sendEmptyAnswer();
            _listenerMessages.Enqueue(ListenerMessage.ShutDown);
        }

        private void GetFrameNumber()
        {
            _sendStandardResponse((int) PruManager.GetFrameNumber());
        }

        private unsafe void Vsync()
        {
            PruManager.WaitForVsync();
            var response = new VsyncResponse();
            response.frameNumber = PruManager.GetFrameNumber();
            response.buttons = PruManager.GetButtons();
            _socket.Send(new ReadOnlySpan<byte>(response.rawData, sizeof(VsyncResponse)));
        }

        private void SetVideoMode(ReadOnlySpan<ushort> payload)
        {
            var mode = payload[0];
            var lines = payload[1];
            
            PruManager.S
        }
        private void GetVideoModeLines(ReadOnlySpan<byte> receivedData) {}
        private void GetVideoModeFrequency(ReadOnlySpan<byte> receivedData) {}
        private void GetWidth(ReadOnlySpan<byte> receivedData) {}
        private void GetHeight(ReadOnlySpan<byte> receivedData) {}
        private void EnumVideoModes(ReadOnlySpan<byte> receivedData) {}
        private void GetVideoModeCount(ReadOnlySpan<byte> receivedData) {}
        private void GetLineMod(ReadOnlySpan<byte> receivedData) {}
        private void SetLineMod(ReadOnlySpan<byte> receivedData) {}
        private void SetVirtualSync(ReadOnlySpan<byte> receivedData) {}
        private void UpdateStart(ReadOnlySpan<byte> receivedData) {}
        private void UpdatePacket(ReadOnlySpan<byte> receivedData) {}
        private void UpdateEnd(ReadOnlySpan<byte> receivedData) {}
    }
}