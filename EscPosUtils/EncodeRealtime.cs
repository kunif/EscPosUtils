/*

   Copyright (C) 2022 Kunio Fukuchi

   This software is provided 'as-is', without any express or implied
   warranty. In no event will the authors be held liable for any damages
   arising from the use of this software.

   Permission is granted to anyone to use this software for any purpose,
   including commercial applications, and to alter it and redistribute it
   freely, subject to the following restrictions:

   1. The origin of this software must not be misrepresented; you must not
      claim that you wrote the original software. If you use this software
      in a product, an acknowledgment in the product documentation would be
      appreciated but is not required.

   2. Altered source versions must be plainly marked as such, and must not be
      misrepresented as being the original software.

   3. This notice may not be removed or altered from any source distribution.

   Kunio Fukuchi

 */

namespace kunif.EscPosUtils
{
    using System.Linq;

    public partial class EscPosEncoder
    {
        //--------------------------------------

        public enum RealtimeStatusType
        {
            Printer = 1,
            OfflineCause = 2,
            ErrorCause = 3,
            RollPaperSensor = 4,
            InkA = 0x701,
            InkB = 0x702,
            Peeler = 0x803,
            Interface = 0x1801,
            DM_D = 0x1802
        }

        public void TransmitRealtimeStatus(RealtimeStatusType statusType)
        {
            byte[] cmd = { 0x10, 0x04, (byte)statusType };
            if ((int)statusType >= 0x701)
            {
                cmd = cmd.Concat(new[] { (byte)0x00 }).ToArray();
                switch (statusType)
                {
                    case RealtimeStatusType.InkA:
                        cmd[2] = 7;
                        cmd[3] = 1;
                        break;

                    case RealtimeStatusType.InkB:
                        cmd[2] = 7;
                        cmd[3] = 2;
                        break;

                    case RealtimeStatusType.Peeler:
                        cmd[2] = 8;
                        cmd[3] = 3;
                        break;

                    case RealtimeStatusType.Interface:
                        cmd[2] = 18;
                        cmd[3] = 1;
                        break;

                    case RealtimeStatusType.DM_D:
                        cmd[2] = 18;
                        cmd[3] = 2;
                        break;
                }
            }
            CommandList.Add(new EscPosCmd(EscPosCmdType.DleTransmitRealtimeStatus, cmd));
        }

        //--------------------------------------

        public enum RealtimeEnqType
        {
            FeedButtonWhenWaitResume = 0,
            RetryPrintOnErrorResume = 1,
            BufferClearAndErrorResume = 2
        }

        public void TransmitRealtimeEnq(RealtimeEnqType realtimeEnq)
        {
            byte[] cmd = { 0x10, 0x05, (byte)realtimeEnq };
            CommandList.Add(new EscPosCmd(EscPosCmdType.DleSendRealtimeRequest, cmd));
        }

        //--------------------------------------

        public void RealtimeDrawerKickPulse(int drawer, int duration)
        {
            if (((drawer & 0xFFFFFFFE) != 0)
              || ((duration < 1) || (duration > 8))
            )
            { return; }
            byte[] cmd = { 0x10, 0x14, 0x01, (byte)drawer, (byte)duration };
            CommandList.Add(new EscPosCmd(EscPosCmdType.DleGeneratePulseRealtime, cmd));
        }

        //--------------------------------------

        public void RealtimeExecPowerOff()
        {
            byte[] cmd = { 0x10, 0x14, 0x02, 0x01, 0x08 };
            CommandList.Add(new EscPosCmd(EscPosCmdType.DleExecPowerOff, cmd));
        }

        //--------------------------------------

        public void RealtimeBuzzerRinging(int pattern, int repeat, int onduration, int offduration)
        {
            if (((pattern & 0xFFFFFFF8) != 0)
              || ((repeat < 0) || (repeat > 8))
              || (onduration != 1) || (offduration != 0)
            )
            { return; }
            byte[] cmd = { 0x10, 0x14, 0x03, (byte)pattern, 0x00, (byte)repeat, (byte)onduration, (byte)onduration };
            CommandList.Add(new EscPosCmd(EscPosCmdType.DleSoundBuzzRealtime, cmd));
        }

        //--------------------------------------

        public enum RealtimeSpecifiedStatusType
        {
            BasicASB = 1,
            ExtendedASB = 2,
            OfflineResponse = 4,
            BatteryStatus = 5
        }

        public void TransmitRealtimeSpecifiedStatus(RealtimeSpecifiedStatusType realtimeStatus)
        {
            byte[] cmd = { 0x10, 0x14, 0x07, (byte)realtimeStatus };
            CommandList.Add(new EscPosCmd(EscPosCmdType.DleTransmitSpcifiedStatusRealtime, cmd));
        }

        //--------------------------------------

        public void RealtimeClearBuffer()
        {
            byte[] cmd = { 0x10, 0x14, 0x08, 0x01, 0x03, 0x14, 0x01, 0x06, 0x02, 0x08 };
            CommandList.Add(new EscPosCmd(EscPosCmdType.DleClearBuffer, cmd));
        }

        //--------------------------------------

        public enum RealtimeEnableDisableType
        {
            Disable = 0x30,
            Enable = 0x31,
            UnChange = 0x32
        }

        public void EnableDisableRealtimeCommand(RealtimeEnableDisableType pulse, RealtimeEnableDisableType poweroff)
        {
            if (pulse == RealtimeEnableDisableType.UnChange && poweroff == RealtimeEnableDisableType.UnChange) { return; }
            byte[] data = new byte[((pulse == RealtimeEnableDisableType.UnChange || poweroff == RealtimeEnableDisableType.UnChange) ? 2 : 4)];
            int offset = 0;
            if (pulse != RealtimeEnableDisableType.UnChange)
            {
                data[0] = 1;
                data[1] = (byte)pulse;
                offset = 2;
            }
            if (poweroff != RealtimeEnableDisableType.UnChange)
            {
                data[offset] = 2;
                data[offset + 1] = (byte)poweroff;
            }
            int cmdlen = 1 + data.Length;
            byte[] cmd = { 0x1D, 0x28, 0x44, (byte)cmdlen, (byte)(cmdlen >> 8), 0x14 };
            cmd = cmd.Concat(data).ToArray();
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsEnableDisableRealtimeCommand, cmd));
        }

        //--------------------------------------
    }
}