/*
 * prussdrv.h
 *
 * Describes PRUSS userspace driver for Industrial Communications
 *
 * Copyright (C) 2010 Texas Instruments Incorporated - http://www.ti.com/
 *
 *
 *  Redistribution and use in source and binary forms, with or without
 *  modification, are permitted provided that the following conditions
 *  are met:
 *
 *    Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 *
 *    Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the
 *    distribution.
 *
 *    Neither the name of Texas Instruments Incorporated nor the names of
 *    its contributors may be used to endorse or promote products derived
 *    from this software without specific prior written permission.
 *
 *  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 *  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 *  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 *  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 *  OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 *  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 *  LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 *  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 *  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 *  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
*/

/*
 * ============================================================================
 * Copyright (c) Texas Instruments Inc 2010-11
 *
 * Use of this software is controlled by the terms and conditions found in the
 * license agreement under which this software has been supplied or provided.
 * ============================================================================
*/

using System;
using System.Runtime.InteropServices;
using size_t = System.UIntPtr;
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace NetPRUSSDriver
{
    public class NativeDriver
    {
        private const string DllName = "libprussdrv";

        public const int NUM_PRU_HOSTIRQS = 8;
        public const int NUM_PRU_HOSTS = 10;
        public const int NUM_PRU_CHANNELS = 10;
        public const int NUM_PRU_SYS_EVTS = 64;

        public const int PRUSS0_PRU0_DATARAM = 0;
        public const int PRUSS0_PRU1_DATARAM = 1;
        public const int PRUSS0_PRU0_IRAM = 2;
        public const int PRUSS0_PRU1_IRAM = 3;

        public const int PRUSS_V1 = 1; // AM18XX
        public const int PRUSS_V2 = 2; // AM33XX

        //Available in AM33xx series - begin
        public const int PRUSS0_SHARED_DATARAM = 4;
        public const int PRUSS0_CFG = 5;
        public const int PRUSS0_UART = 6;
        public const int PRUSS0_IEP = 7;
        public const int PRUSS0_ECAP = 8;
        public const int PRUSS0_MII_RT = 9;

        public const int PRUSS0_MDIO = 10;
        //Available in AM33xx series - end

        public const int PRU_EVTOUT_0 = 0;
        public const int PRU_EVTOUT_1 = 1;
        public const int PRU_EVTOUT_2 = 2;
        public const int PRU_EVTOUT_3 = 3;
        public const int PRU_EVTOUT_4 = 4;
        public const int PRU_EVTOUT_5 = 5;
        public const int PRU_EVTOUT_6 = 6;
        public const int PRU_EVTOUT_7 = 7;
        
        public const int PRU0_PRU1_INTERRUPT       = 17;
        public const int PRU1_PRU0_INTERRUPT       = 18;
        public const int PRU0_ARM_INTERRUPT        = 19;
        public const int PRU1_ARM_INTERRUPT        = 20;
        public const int ARM_PRU0_INTERRUPT        = 21;
        public const int ARM_PRU1_INTERRUPT        = 22;
        
        public const int CHANNEL0                  = 0;
        public const int CHANNEL1                  = 1;
        public const int CHANNEL2                  = 2;
        public const int CHANNEL3                  = 3;
        public const int CHANNEL4                  = 4;
        public const int CHANNEL5                  = 5;
        public const int CHANNEL6                  = 6;
        public const int CHANNEL7                  = 7;
        public const int CHANNEL8                  = 8;
        public const int CHANNEL9                  = 9;
        
        public const int PRU0                      = 0;
        public const int PRU1                      = 1;
        public const int PRU_EVTOUT0               = 2;
        public const int PRU_EVTOUT1               = 3;
        public const int PRU_EVTOUT2               = 4;
        public const int PRU_EVTOUT3               = 5;
        public const int PRU_EVTOUT4               = 6;
        public const int PRU_EVTOUT5               = 7;
        public const int PRU_EVTOUT6               = 8;
        public const int PRU_EVTOUT7               = 9;
        
        public const int PRU0_HOSTEN_MASK          = 0x0001;
        public const int PRU1_HOSTEN_MASK          = 0x0002;
        
        public const int PRU_EVTOUT0_HOSTEN_MASK   = 0x0004;
        public const int PRU_EVTOUT1_HOSTEN_MASK   = 0x0008;
        public const int PRU_EVTOUT2_HOSTEN_MASK   = 0x0010;
        public const int PRU_EVTOUT3_HOSTEN_MASK   = 0x0020;
        public const int PRU_EVTOUT4_HOSTEN_MASK   = 0x0040;
        public const int PRU_EVTOUT5_HOSTEN_MASK   = 0x0080;
        public const int PRU_EVTOUT6_HOSTEN_MASK   = 0x0100;
        public const int PRU_EVTOUT7_HOSTEN_MASK   = 0x0200;

        [StructLayout(LayoutKind.Sequential)]
        public struct tsysevt_to_channel_map
        {
            public short sysevt;
            public short channel;

            public tsysevt_to_channel_map(short sysevt, short channel)
            {
                this.sysevt = sysevt;
                this.channel = channel;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct tchannel_to_host_map
        {
            public short channel;
            public short host;

            public tchannel_to_host_map(short channel, short host)
            {
                this.channel = channel;
                this.host = host;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct tpruss_intc_initdata
        {
            //Enabled SYSEVTs - Range:0..63
            //{-1} indicates end of list
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NUM_PRU_SYS_EVTS)]
            public sbyte[] sysevts_enabled;

            //SysEvt to Channel map. SYSEVTs - Range:0..63 Channels -Range: 0..9
            //{-1, -1} indicates end of list
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NUM_PRU_SYS_EVTS)]
            public tsysevt_to_channel_map[] sysevt_to_channel_map;

            //Channel to Host map.Channels -Range: 0..9  HOSTs - Range:0..9
            //{-1, -1} indicates end of list
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NUM_PRU_CHANNELS)]
            public tchannel_to_host_map[] channel_to_host_map;

            //10-bit mask - Enable Host0-Host9 {Host0/1:PRU0/1, Host2..9 : PRUEVT_OUT0..7}
            public uint host_enable_bitmask;

            public void Init()
            {
                sysevts_enabled = new sbyte[NUM_PRU_SYS_EVTS];
                sysevt_to_channel_map = new tsysevt_to_channel_map[NUM_PRU_SYS_EVTS];
                channel_to_host_map = new tchannel_to_host_map[NUM_PRU_CHANNELS];
            }

            public static tpruss_intc_initdata PRUSS_INTC_INITDATA()
            {
                var ret = new tpruss_intc_initdata();

                ret.Init();

                Array.Copy(new sbyte[]
                {
                    PRU1_PRU0_INTERRUPT, 
                    PRU0_ARM_INTERRUPT, 
                    PRU1_ARM_INTERRUPT,
                    -1
                }, 0, ret.sysevts_enabled, 0, 4);

                Array.Copy(new[]
                {
                    new tsysevt_to_channel_map(PRU1_PRU0_INTERRUPT, CHANNEL0), 
                    new tsysevt_to_channel_map(PRU0_ARM_INTERRUPT, CHANNEL2), 
                    new tsysevt_to_channel_map(PRU1_ARM_INTERRUPT, CHANNEL3), 
                    new tsysevt_to_channel_map(-1,-1)
                }, 0, ret.sysevt_to_channel_map, 0, 4);

                Array.Copy(new[]
                {
                    new tchannel_to_host_map(CHANNEL0, PRU_EVTOUT2), 
                    new tchannel_to_host_map(CHANNEL2, PRU_EVTOUT0), 
                    new tchannel_to_host_map(CHANNEL3, PRU_EVTOUT1), 
                    new tchannel_to_host_map(-1,-1)
                }, 0, ret.channel_to_host_map, 0, 4);


                /*Enable PRU0, PRU1, PRU_EVTOUT0 */
                ret.host_enable_bitmask = (PRU_EVTOUT0_HOSTEN_MASK | PRU_EVTOUT1_HOSTEN_MASK | PRU_EVTOUT2_HOSTEN_MASK);

                return ret;
            }
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_init();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_open(uint host_interrupt);

        /** Return version of PRU.  This must be called after prussdrv_open. */
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_version();

        /** Return string description of PRU version. */
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr prussdrv_strversion(int version);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_pru_reset(uint prunum);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_pru_disable(uint prunum);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_pru_enable(uint prunum);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_pru_enable_at(uint prunum, size_t addr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_pru_write_memory(
            uint pru_ram_id,
            uint wordoffset,
            IntPtr memarea,
            uint bytelength);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_pruintc_init(ref tpruss_intc_initdata prussintc_init_data);

        /** Find and return the channel a specified event is mapped to.
         * Note that this only searches for the first channel mapped and will not
         * detect error cases where an event is mapped erroneously to multiple
         * channels.
         * @return channel-number to which a system event is mapped.
         * @return -1 for no mapping found
         */
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern short prussdrv_get_event_to_channel_map(uint eventnum);

        /** Find and return the host interrupt line a specified channel is mapped
         * to.  Note that this only searches for the first host interrupt line
         * mapped and will not detect error cases where a channel is mapped
         * erroneously to multiple host interrupt lines.
         * @return host-interrupt-line to which a channel is mapped.
         * @return -1 for no mapping found
         */
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern short prussdrv_get_channel_to_host_map(uint channel);

        /** Find and return the host interrupt line a specified event is mapped
         * to.  This first finds the intermediate channel and then the host.
         * @return host-interrupt-line to which a system event is mapped.
         * @return -1 for no mapping found
         */
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern short prussdrv_get_event_to_host_map(uint eventnum);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_map_l3mem(out IntPtr address);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_map_extmem(out IntPtr address);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint prussdrv_extmem_size();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_map_prumem(uint pru_ram_id, out IntPtr address);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_map_peripheral_io(uint per_id, out IntPtr address);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint prussdrv_get_phys_addr(IntPtr address);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr prussdrv_get_virt_addr(uint phyaddr);

        /** Wait for the specified host interrupt.
         * @return the number of times the event has happened. */
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint prussdrv_pru_wait_event(uint host_interrupt);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint prussdrv_pru_wait_event_timeout(uint host_interrupt, int time_us);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_pru_event_fd(uint host_interrupt);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_pru_send_event(uint eventnum);

        /** Clear the specified event and re-enable the host interrupt. */
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_pru_clear_event(
            uint host_interrupt,
            uint sysevent);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_pru_send_wait_clear_event(
            uint send_eventnum,
            uint host_interrupt,
            uint ack_eventnum);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_exit();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int prussdrv_exec_program(int prunum, [MarshalAs(UnmanagedType.LPStr)] string filename);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int prussdrv_exec_program_at(
            int prunum, 
            [MarshalAs(UnmanagedType.LPStr)] string filename,
            size_t addr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_exec_code(int prunum, IntPtr code, int codelen);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_exec_code_at(int prunum, IntPtr code, int codelen, size_t addr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int prussdrv_load_data(int prunum, IntPtr code, int codelen);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int prussdrv_load_datafile(int prunum, [MarshalAs(UnmanagedType.LPStr)] string filename);
    }
}