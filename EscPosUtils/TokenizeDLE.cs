/*

   Copyright (C) 2020-2022 Kunio Fukuchi

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
    public partial class EscPosTokenizer
    {
        /// <summary>
        /// DLE(0x10) started realtime command sequence tokenize routine
        /// </summary>
        private void TokenizeDLE()
        {
            blockLength = 2;
            switch (ctlByte1)
            {
                case 0x04: // DLE EOT
                    blockLength = 3;
                    switch (ctlByte2)
                    {
                        case 0x01: // DLE EOT 01
                        case 0x02: // DLE EOT 02
                        case 0x03: // DLE EOT 03
                        case 0x04: // DLE EOT 04
                        case 0x05: // DLE EOT 05
                        case 0x06: // DLE EOT 06
                            ctlType = EscPosCmdType.DleTransmitRealtimeStatus;
                            break;

                        case 0x00: // DLE EOT 00 01
                        case 0x07: // DLE EOT 07 xx
                        case 0x08: // DLE EOT 08 03
                        case 0x12: // DLE EOT 12 xx
                            ctlType = EscPosCmdType.DleTransmitRealtimeStatus;
                            blockLength = 4;
                            break;

                        default: // DLE EOT ??
                            ctlType = EscPosCmdType.DleUnknown;
                            break;
                    }
                    break;

                case 0x05: // DLE ENQ
                    blockLength = 3;
                    switch (ctlByte2)
                    {
                        case 0x00: // DLE ENQ 00
                        case 0x01: // DLE ENQ 01
                        case 0x02: // DLE ENQ 02
                            ctlType = EscPosCmdType.DleSendRealtimeRequest;
                            break;

                        default: // DLE ENQ ??
                            ctlType = EscPosCmdType.DleUnknown;
                            break;
                    }
                    break;

                case 0x14: // DLE DC4
                    switch (ctlByte2)
                    {
                        case 0x01: // DLE DC4 01 xx xx
                            ctlType = EscPosCmdType.DleGeneratePulseRealtime;
                            blockLength = 5;
                            break;

                        case 0x02: // DLE DC4 02 01 08
                            ctlType = EscPosCmdType.DleExecPowerOff;
                            blockLength = 5;
                            break;

                        case 0x03: // DLE DC4 03 xx xx xx xx xx
                            ctlType = EscPosCmdType.DleSoundBuzzRealtime;
                            blockLength = 8;
                            break;

                        case 0x07: // DLE DC4 07 xx
                            ctlType = EscPosCmdType.DleTransmitSpcifiedStatusRealtime;
                            blockLength = 4;
                            break;

                        case 0x08: // DLE DC4 08 01 03 14 01 06 02 08
                            ctlType = EscPosCmdType.DleClearBuffer;
                            blockLength = 10;
                            break;

                        default: // DLE DC4 ??
                            ctlType = EscPosCmdType.DleUnknown;
                            blockLength = 3;
                            break;
                    }
                    break;

                default: // DLE ??
                    ctlType = EscPosCmdType.DleUnknown;
                    break;
            }
        }
    }
}