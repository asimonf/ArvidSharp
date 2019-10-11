using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using static Zstandard.ExternMethods;

namespace Arvid.Server
{
    internal class DataServer: BaseServer
    {
        public DataServer(Socket socket) : base(socket)
        {
        }

        protected override unsafe void DoWork()
        {
            const int bufferSize = 8 + Helper.MaxSegmentSize;
            var receiveBuffer = stackalloc ushort[bufferSize];
            var receiveSpan = new Span<byte>(receiveBuffer, bufferSize * 2);
            var zContext = ZSTD_createDCtx();

            try
            {
                while (true)
                {
                    var receivedWords = Socket.Receive(receiveSpan) / 2;

                    if (receivedWords < 8) continue;

                    var srcSize = receiveBuffer[1];
                    var y = receiveBuffer[2];
                    var stride = receiveBuffer[3];
                    var dstSize = receiveBuffer[4];
                    var src = &receiveBuffer[8];

                    // Ignore segments that don't match current width
                    if (stride != PruManager.Width) continue;

                    var dst = &PruManager.FrameBuffers[0][y * stride];

                    var res = ZSTD_decompressDCtx(
                        zContext,
                        new IntPtr(dst),
                        new UIntPtr(dstSize),
                        new IntPtr(src),
                        new UIntPtr(srcSize)
                    );

                    if (ZSTD_isError(res) > 0) Console.WriteLine("Error decompressing data");
                }
            }
            finally
            {
                ZSTD_freeDCtx(zContext);
            }
        }
    }
}