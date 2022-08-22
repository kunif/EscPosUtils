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
    using System.Collections.Generic;

    public partial class EscPosTokenizer
    {
        /// <summary>
        /// US(0x1F) XX started fixed size linedisplay command type and related length information
        /// </summary>
        private static readonly Dictionary<byte, SeqInfo> s_VfdUSType = new()
        {
            { 0x01, new SeqInfo { seqtype = EscPosCmdType.VfdUsOverwriteMode, length = 2 } },
            { 0x02, new SeqInfo { seqtype = EscPosCmdType.VfdUsVerticalScrollMode, length = 2 } },
            { 0x03, new SeqInfo { seqtype = EscPosCmdType.VfdUsHorizontalScrollMode, length = 2 } },
            { 0x0A, new SeqInfo { seqtype = EscPosCmdType.VfdUsMoveCursorUp, length = 2 } },
            { 0x0D, new SeqInfo { seqtype = EscPosCmdType.VfdUsMoveCursorRightMost, length = 2 } },
            { 0x23, new SeqInfo { seqtype = EscPosCmdType.VfdUsTurnAnnounciatorOnOff, length = 4 } },
            { 0x24, new SeqInfo { seqtype = EscPosCmdType.VfdUsMoveCursorSpecifiedPosition, length = 4 } },
            { 0x2C, new SeqInfo { seqtype = EscPosCmdType.VfdUsDisplayCharWithComma, length = 3 } },
            { 0x2E, new SeqInfo { seqtype = EscPosCmdType.VfdUsDisplayCharWithPeriod, length = 3 } },
            { 0x3A, new SeqInfo { seqtype = EscPosCmdType.VfdUsStartEndMacroDefinition, length = 2 } },
            { 0x3B, new SeqInfo { seqtype = EscPosCmdType.VfdUsDisplayCharWithSemicolon, length = 3 } },
            { 0x40, new SeqInfo { seqtype = EscPosCmdType.VfdUsExecuteSelfTest, length = 2 } },
            { 0x42, new SeqInfo { seqtype = EscPosCmdType.VfdUsMoveCursorBottom, length = 2 } },
            { 0x43, new SeqInfo { seqtype = EscPosCmdType.VfdUsTurnCursorDisplayModeOnOff, length = 3 } },
            { 0x45, new SeqInfo { seqtype = EscPosCmdType.VfdUsSetDisplayBlinkInterval, length = 3 } },
            { 0x54, new SeqInfo { seqtype = EscPosCmdType.VfdUsSetAndDisplayCountTime, length = 4 } },
            { 0x55, new SeqInfo { seqtype = EscPosCmdType.VfdUsDisplayCounterTime, length = 2 } },
            { 0x58, new SeqInfo { seqtype = EscPosCmdType.VfdUsBrightnessAdjustment, length = 3 } },
            { 0x5E, new SeqInfo { seqtype = EscPosCmdType.VfdUsExecuteMacro, length = 4 } },
            { 0x72, new SeqInfo { seqtype = EscPosCmdType.VfdUsTurnReverseMode, length = 3 } },
            { 0x76, new SeqInfo { seqtype = EscPosCmdType.VfdUsStatusConfirmationByDTRSignal, length = 3 } }
        };

        /// <summary>
        /// US(0x1F) started variable bytes linedisplay command sequence tokenize routine
        /// </summary>
        private void TokenizeUS()
        {
            if (s_VfdUSType.ContainsKey(ctlByte1))
            {
                ctlType = s_VfdUSType[ctlByte1].seqtype;
                blockLength = s_VfdUSType[ctlByte1].length;
            }
            else
            {
                byte ctlByte5 = (byte)(((curIndex + 5) < dataLength) ? baData[curIndex + 5] : 0xFF);
                blockLength = 2;
                switch (ctlByte1)
                {
                    case 0x28: // US (
                        blockLength = 5 + ctlByte4 * 0x100 + ctlByte3;
                        ctlType = ctlByte2 switch
                        {
                            // US ( A
                            0x41 => EscPosCmdType.VfdUsSelectDisplays,
                            // US ( E
                            0x45 => ctlByte5 switch
                            {
                                0x01 => EscPosCmdType.VfdUsChangeIntoUserSettingMode,
                                0x02 => EscPosCmdType.VfdUsEndUserSettingMode,
                                0x03 => EscPosCmdType.VfdUsSetMemorySwitchValues,
                                0x04 => EscPosCmdType.VfdUsSendingDisplayingMemorySwitchValues,
                                _ => EscPosCmdType.VfdUsUnknown,
                            },
                            // US ( G
                            0x47 => ctlByte5 switch
                            {
                                0x60 => EscPosCmdType.VfdUsKanjiCharacterModeOnOff,
                                0x61 => EscPosCmdType.VfdUsSelectKanjiCharacterCodeSystem,
                                _ => EscPosCmdType.VfdUsUnknown,
                            },
                            _ => EscPosCmdType.VfdUsUnknown,
                        };
                        break;

                    default:
                        ctlType = EscPosCmdType.VfdUsUnknown;
                        break;
                }
            }
        }
    }
}