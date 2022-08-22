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
    using System;

    public partial class EscPosEncoder
    {
        //--------------------------------------

        public enum BuzzerType
        {
            Off = '0',
            Pattern01 = '1',
            Pattern02 = '2',
            Pattern03 = '3',
            Pattern04 = '4',
            Pattern05 = '5',
            Pattern06 = '6',
            Pattern07 = '7',
            Pattern08 = '8',
            Pattern09 = '9',
            Pattern10 = ':',
        }

        public void BuzzerRinging(BuzzerType buzzerType, int repeat, int duration)
        {
            if (((repeat < 0) || (repeat > 63))
              || (duration < 10) || (duration > 255)
            )
            { return; }
            byte[] cmd = { 0x1B, 0x28, 0x41, 0x04, 0x00, 0x30, (byte)buzzerType, (byte)repeat, (byte)duration };
            CommandList.Add(new EscPosCmd(EscPosCmdType.EscBeeperBuzzer, cmd));
        }

        //--------------------------------------

        public enum SpecificBuzzerType
        {
            A = 1,
            B = 2,
            C = 3,
            D = 4,
            E = 5,
            Error = 6,
            PaperEmpty = 7
        }

        public void BuzzerRingingSpecificM1a(SpecificBuzzerType specificBuzzer, int count)
        {
            if ((count < 0) || (count > 255))
            { return; }
            byte[] cmd = { 0x1B, 0x28, 0x41, 0x03, 0x00, 0x61, (byte)specificBuzzer, (byte)count };
            CommandList.Add(new EscPosCmd(EscPosCmdType.EscBeeperBuzzerM1a, cmd));
        }

        //--------------------------------------

        public void BuzzerRingingSpecificM1b(int count, int onduration, int offduration)
        {
            if (((count < 0) || (count > 63))
              || (onduration < 0) || (onduration > 255)
              || (offduration < 0) || (offduration > 255)
            )
            { return; }
            byte[] cmd = { 0x1B, 0x28, 0x41, 0x05, 0x00, 0x61, 0x64, (byte)count, (byte)onduration, (byte)offduration };
            CommandList.Add(new EscPosCmd(EscPosCmdType.EscBeeperBuzzerM1b, cmd));
        }

        //--------------------------------------

        public enum OfflineBuzzerType
        {
            CoverOpen = '0',
            PaperEmpty = '1',
            RecoverableError = '2',
            UnrecoverableError = '3'
        }

        public void BuzzerRingingOffline(OfflineBuzzerType offlineBuzzer, Boolean ringing, int onduration, int offduration)
        {
            if ((onduration < 1) || ((onduration > 50) && (onduration != 255))
              || (offduration < 1) || (offduration > 50)
            )
            { return; }
            byte[] cmd = { 0x1B, 0x28, 0x41, 0x07, 0x00, 0x62, (byte)offlineBuzzer, 0x01, 0x64, (byte)(ringing ? 0xFF : 0x00), (byte)onduration, (byte)offduration };
            CommandList.Add(new EscPosCmd(EscPosCmdType.EscBeeperBuzzerOffline, cmd));
        }

        //--------------------------------------

        public void BuzzerRingingNearEnd(Boolean ringing, int onduration, int offduration)
        {
            if ((onduration < 1) || ((onduration > 50) && (onduration != 255))
              || (offduration < 1) || (offduration > 50)
            )
            { return; }
            byte[] cmd = { 0x1B, 0x28, 0x41, 0x07, 0x00, 0x63, 0x30, 0x01, 0x64, (byte)(ringing ? 0xFF : 0x00), (byte)onduration, (byte)offduration };
            CommandList.Add(new EscPosCmd(EscPosCmdType.EscBeeperBuzzerNearEnd, cmd));
        }

        //--------------------------------------
    }
}