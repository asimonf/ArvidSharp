using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using Arvid.Response;

namespace Arvid.Server
{
    internal class ControlServer: BaseServer
    {
        private bool _initialized;

        private delegate void Command(RegularResponse regularResponse);
        
        private readonly ConcurrentQueue<ListenerMessage> _listenerMessages;
        private readonly Dictionary<CommandEnum, Command> _commandMap;

        public ControlServer(Socket socket, ConcurrentQueue<ListenerMessage> listenerMessages) : base(socket)
        {
            _listenerMessages = listenerMessages;
            _commandMap = new Dictionary<CommandEnum, Command>();

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
                var action = Delegate.CreateDelegate(typeof(Command), method);
                _commandMap.Add(commandEnum, action as Command);
            }
        }

        protected override unsafe void DoWork()
        {
            var response = new RegularResponse();
            var receiveSpan = new Span<byte>(response.rawData, sizeof(RegularResponse));

            while (true)
            {
                var receivedBytes = _socket.Receive(receiveSpan);

                var command = (CommandEnum) response.id;

                if (command == CommandEnum.Init)
                {
                    Init(response);
                    continue;
                }

                if (!_initialized) continue;

                _commandMap[command](response);
            }
        }
        
        // Commands

        private unsafe void Init(RegularResponse regularResponse)
        {
            if (!_initialized)
            {
                _listenerMessages.Enqueue(ListenerMessage.Init);
                _initialized = true;
            }
            
            var sendBuffer = stackalloc ushort[6];
            _socket.Send(new ReadOnlySpan<byte>(sendBuffer, 12));
        }

        private unsafe void Close(RegularResponse regularResponse)
        {
            _listenerMessages.Enqueue(ListenerMessage.Stop);
            
            
            var sendBuffer = stackalloc ushort[6];
            _socket.Send(new ReadOnlySpan<byte>(sendBuffer, 12));
        }

        private void PowerOff(RegularResponse regularResponse)
        {
            _listenerMessages.Enqueue(ListenerMessage.ShutDown);
        }

        private unsafe void GetFrameNumber(RegularResponse regularResponse)
        {
            var response = new RegularResponse();
            response.responseData = (int)PruManager.GetFrameNumber();
            _socket.Send(new ReadOnlySpan<byte>(response.rawData, sizeof(RegularResponse)));
        }

        private unsafe void Vsync(ReadOnlySpan<byte> receivedData)
        {
            PruManager.WaitForVsync();
            var response = new VsyncResponse();
            response.frameNumber = PruManager.GetFrameNumber();
            response.buttons = PruManager.GetButtons();
            _socket.Send(new ReadOnlySpan<byte>(response.rawData, sizeof(VsyncResponse)));
        }
        private void SetVideoMode(ReadOnlySpan<byte> receivedData) {}
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