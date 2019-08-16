using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Arvid.Server
{
    internal class ControlServer: BaseServer
    {
        public bool Initialized { get; private set; }

        private ConcurrentQueue<ListenerMessage> _listenerMessages;
        
        public ControlServer(Socket socket, ConcurrentQueue<ListenerMessage> listenerMessages) : base(socket)
        {
            _listenerMessages = listenerMessages;
        }

        protected override unsafe void DoWork()
        {
            var receiveBuffer = stackalloc ushort[6];
            var receiveSpan = new Span<byte>(receiveBuffer, 6);
            
            while (true)
            {
                var receivedBytes = _socket.Receive(receiveSpan);

                var command = (CommandEnum)receiveBuffer[0];

                switch (command)
                {
                    case CommandEnum.Init:
                        Init();
                        break;
                    case CommandEnum.Close:
                        Close();                        
                        break;
                    case CommandEnum.Blit:
                        Blit();
                        break;
                    case CommandEnum.GetFrameNumber:
                        break;
                    case CommandEnum.Vsync:
                        break;
                    case CommandEnum.SetVideoMode:
                        break;
                    case CommandEnum.GetVideoModeLines:
                        break;
                    case CommandEnum.GetVideoModeFrequency:
                        break;
                    case CommandEnum.GetWidth:
                        break;
                    case CommandEnum.GetHeight:
                        break;
                    case CommandEnum.EnumVideoModes:
                        break;
                    case CommandEnum.GetVideoModeCount:
                        break;
                    case CommandEnum.GetLineMod:
                        break;
                    case CommandEnum.SetLineMod:
                        break;
                    case CommandEnum.SetVirtualSync:
                        break;
                    case CommandEnum.UpdateStart:
                        break;
                    case CommandEnum.UpdatePacket:
                        break;
                    case CommandEnum.UpdateEnd:
                        break;
                    case CommandEnum.PowerOff:
                        PowerOff();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void Init()
        {
            if (Initialized) return;

            Initialized = true;
            
            _listenerMessages.Enqueue(ListenerMessage.Init);
        }

        private void Close()
        {
            _listenerMessages.Enqueue(ListenerMessage.Stop);
        }

        private void PowerOff()
        {
            _listenerMessages.Enqueue(ListenerMessage.ShutDown);
        }
        
        private void Blit() {}

        private int GetFrameNumber()
        {
            return 0;
        }
        
        private void Vsync() {}
        private void SetVideoMode() {}
        private void GetVideoModeLines() {}
        private void GetVideoModeFrequency() {}
        private void GetWidth() {}
        private void GetHeight() {}
        private void EnumVideoModes() {}
        private void GetVideoModeCount() {}
        private void GetLineMod() {}
        private void SetLineMod() {}
        private void SetVirtualSync() {}
        private void UpdateStart() {}
        private void UpdatePacket() {}
        private void UpdateEnd() {}
    }
}