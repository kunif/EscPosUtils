/*

   Copyright (C) 2020 Kunio Fukuchi

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
        /// ESC(0x1B) XX started fixed size printer command type and related length information
        /// </summary>
        private static readonly Dictionary<byte, SeqInfo> s_PrtESCType = new Dictionary<byte, SeqInfo>()
        {
            { 0x0C, new SeqInfo { seqtype = EscPosCmdType.EscPageModeFormFeed,                         length = 2 } },
            { 0x20, new SeqInfo { seqtype = EscPosCmdType.EscRightSideSpacing,                         length = 3 } },
            { 0x21, new SeqInfo { seqtype = EscPosCmdType.EscSelectPrintMode,                          length = 3 } },
            { 0x24, new SeqInfo { seqtype = EscPosCmdType.EscAbsoluteVerticalPosition,                 length = 4 } },
            { 0x25, new SeqInfo { seqtype = EscPosCmdType.EscSelectUserDefinedCharacterSet,            length = 3 } },
            { 0x2D, new SeqInfo { seqtype = EscPosCmdType.EscUnderlineMode,                            length = 3 } },
            { 0x32, new SeqInfo { seqtype = EscPosCmdType.EscSelectDefaultLineSpacing,                 length = 2 } },
            { 0x33, new SeqInfo { seqtype = EscPosCmdType.EscLineSpacing,                              length = 3 } },
            { 0x3C, new SeqInfo { seqtype = EscPosCmdType.EscReturnHome,                               length = 2 } },
            { 0x3D, new SeqInfo { seqtype = EscPosCmdType.EscSelectPeripheralDevice,                   length = 3 } },
            { 0x3F, new SeqInfo { seqtype = EscPosCmdType.EscCancelUserDefinedCharacters,              length = 3 } },
            { 0x40, new SeqInfo { seqtype = EscPosCmdType.EscInitialize,                               length = 2 } },
            { 0x43, new SeqInfo { seqtype = EscPosCmdType.EscSetCutSheetEjectLength,                   length = 3 } },
            { 0x45, new SeqInfo { seqtype = EscPosCmdType.EscTurnEmphasizedMode,                       length = 3 } },
            { 0x46, new SeqInfo { seqtype = EscPosCmdType.EscSetCancelCutSheetReverseEject,            length = 3 } },
            { 0x47, new SeqInfo { seqtype = EscPosCmdType.EscTurnDoubleStrikeMode,                     length = 3 } },
            { 0x4A, new SeqInfo { seqtype = EscPosCmdType.EscPrintAndFeedPaper,                        length = 3 } },
            { 0x4B, new SeqInfo { seqtype = EscPosCmdType.EscPrintAndReverseFeed,                      length = 3 } },
            { 0x4C, new SeqInfo { seqtype = EscPosCmdType.EscSelectPageMode,                           length = 2 } },
            { 0x4D, new SeqInfo { seqtype = EscPosCmdType.EscSelectCharacterFont,                      length = 3 } },
            { 0x52, new SeqInfo { seqtype = EscPosCmdType.EscSelectInternationalCharacterSet,          length = 3 } },
            { 0x53, new SeqInfo { seqtype = EscPosCmdType.EscSelectStandardMode,                       length = 2 } },
            { 0x54, new SeqInfo { seqtype = EscPosCmdType.EscSelectPrintDirection,                     length = 3 } },
            { 0x55, new SeqInfo { seqtype = EscPosCmdType.EscTurnUnidirectionalPrintMode,              length = 3 } },
            { 0x56, new SeqInfo { seqtype = EscPosCmdType.EscTurn90digreeClockwiseRotationMode,        length = 3 } },
            { 0x57, new SeqInfo { seqtype = EscPosCmdType.EscSetPrintAreaInPageMode,                   length = 10 } },
            { 0x5C, new SeqInfo { seqtype = EscPosCmdType.EscSetRelativeHprizontalPrintPosition,       length = 4 } },
            { 0x61, new SeqInfo { seqtype = EscPosCmdType.EscSelectJustification,                      length = 3 } },
            { 0x64, new SeqInfo { seqtype = EscPosCmdType.EscPrintAndFeedNLines,                       length = 3 } },
            { 0x65, new SeqInfo { seqtype = EscPosCmdType.EscPrintAndReverseFeedNLines,                length = 3 } },
            { 0x66, new SeqInfo { seqtype = EscPosCmdType.EscCutSheetWaitTime,                         length = 4 } },
            { 0x69, new SeqInfo { seqtype = EscPosCmdType.EscObsoletePartialCut1Point,                 length = 2 } },
            { 0x6D, new SeqInfo { seqtype = EscPosCmdType.EscObsoletePartialCut3Point,                 length = 2 } },
            { 0x70, new SeqInfo { seqtype = EscPosCmdType.EscGeneratePulse,                            length = 5 } },
            { 0x71, new SeqInfo { seqtype = EscPosCmdType.EscReleasePaper,                             length = 2 } },
            { 0x72, new SeqInfo { seqtype = EscPosCmdType.EscSelectPrinterColor,                       length = 3 } },
            { 0x74, new SeqInfo { seqtype = EscPosCmdType.EscSelectCharacterCodeTable,                 length = 3 } },
            { 0x75, new SeqInfo { seqtype = EscPosCmdType.EscObsoleteTransmitPeripheralDeviceStatus,   length = 3 } },
            { 0x76, new SeqInfo { seqtype = EscPosCmdType.EscObsoleteTransmitPaperSensorStatus,        length = 2 } },
            { 0x7B, new SeqInfo { seqtype = EscPosCmdType.EscTurnUpsideDownPrintMode,                  length = 3 } }
        };

        /// <summary>
        /// ESC(0x1B) started variable bytes or special handling printer command sequence tokenize routine
        /// </summary>
        private void TokenizeESCprt()
        {
            if (s_PrtESCType.ContainsKey(ctlByte1))
            {
                ctlType = s_PrtESCType[ctlByte1].seqtype;
                blockLength = s_PrtESCType[ctlByte1].length;
                if (ctlType == EscPosCmdType.EscInitialize)
                {
                    CurrentSbcsFontInfo = SbcsFontList[0];
                    CurrentKanjiFontInfo = KanjiFontList[0];
                }
                else if (ctlType == EscPosCmdType.EscSelectCharacterFont)
                {
                    if (SbcsFontList.ContainsKey(ctlByte2))
                    {
                        CurrentSbcsFontInfo = SbcsFontList[ctlByte2];
                    }
                }
            }
            else
            {
                blockLength = 2;
                switch (ctlByte1)
                {
                    case 0x26: // ESC &
                        if (remainLength < 6)
                        {
                            blockLength = 6;
                        }
                        else
                        {
                            long fontHeight = ctlByte2;
                            if (
                                (fontHeight == CurrentSbcsFontInfo.ybytes)
                                && ((ctlByte3 >= 0x20) && (ctlByte3 <= 0x7E))
                                && ((ctlByte4 >= 0x20) && (ctlByte4 <= 0x7E))
                                && (ctlByte3 <= ctlByte4)
                                )
                            {
                                ctlType = EscPosCmdType.None;
                                long codeCount = ctlByte4 - ctlByte3 + 1;
                                long maxFontWidth = CurrentSbcsFontInfo.xbytes;
                                long count;
                                long workIndex = curIndex + 5;
                                for (count = 0; (count < codeCount) && (workIndex < dataLength); count++)
                                {
                                    long fontWidth = baData[workIndex];
                                    if (fontWidth > maxFontWidth)
                                    {
                                        ctlType = EscPosCmdType.EscUnknown;
                                        break;
                                    }
                                    workIndex += 1 + (fontHeight * fontWidth);
                                    if (workIndex > dataLength)
                                    {
                                        ctlType = EscPosCmdType.NotEnough;
                                        blockLength = remainLength;
                                        break;
                                    }
                                }
                                if (ctlType == EscPosCmdType.None)
                                {
                                    if (count >= codeCount)
                                    {
                                        ctlType = CurrentSbcsFontInfo.seqtype;
                                        blockLength = workIndex - curIndex;
                                    }
                                    else
                                    {
                                        ctlType = EscPosCmdType.NotEnough;
                                        blockLength = remainLength;
                                    }
                                }
                            }
                            else // ESC & ?? ?? ?? xx
                            {
                                ctlType = EscPosCmdType.EscUnknown;
                                blockLength = 5;
                            }
                        }
                        break;

                    case 0x28: // ESC (
                        blockLength = 5 + ctlByte4 * 0x100 + ctlByte3;
                        if (ctlByte2 == 0x41) // ESC ( A
                        {
                            ctlType = (((curIndex + 5) < dataLength) ? baData[curIndex + 5] : 0xFF) switch
                            {
                                // ESC ( A dL dH 0
                                0x30 => EscPosCmdType.EscBeeperBuzzer,
                                // ESC ( A dL dH a
                                0x61 => blockLength switch
                                {
                                    8 => EscPosCmdType.EscBeeperBuzzerM1a,
                                    10 => EscPosCmdType.EscBeeperBuzzerM1b,
                                    _ => EscPosCmdType.EscUnknown,
                                },
                                // ESC ( A dL dH b
                                0x62 => EscPosCmdType.EscBeeperBuzzerOffline,
                                // ESC ( A dL dH c
                                0x63 => EscPosCmdType.EscBeeperBuzzerNearEnd,
                                // ESC ( A dL dH ??
                                _ => EscPosCmdType.EscUnknown,
                            };
                        }
                        else if (ctlByte2 == 0x59) // ESC ( Y
                        {               // 5 + 2
                            ctlType = blockLength == 7 ? EscPosCmdType.EscSpecifyBatchPrint : EscPosCmdType.EscUnknown;
                        }
                        else // ESC ( ??
                        {
                            ctlType = EscPosCmdType.EscUnknown;
                        }
                        break;

                    case 0x2A: // ESC *
                        ctlType = EscPosCmdType.EscSelectBitImageMode;
                        switch (ctlByte2)
                        {
                            case 0x00: // ESC * 00
                            case 0x01: // ESC * 01
                                blockLength = 5 + ctlByte4 * 0x100 + ctlByte3;
                                break;

                            case 0x30: // ESC * 0
                            case 0x31: // ESC * 1
                                blockLength = 5 + ((ctlByte4 * 0x100 + ctlByte3) * 3);
                                break;

                            default: // ESC * ??
                                ctlType = EscPosCmdType.EscUnknown;
                                blockLength = 5;
                                break;
                        }
                        break;

                    case 0x3D: // ESC =
                        ctlType = EscPosCmdType.EscSelectPeripheralDevice;
                        blockLength = 3;
                        break;

                    case 0x44: // ESC D
                        {
                            blockLength = 0;
                            ctlType = EscPosCmdType.EscHorizontalTabPosition;
                            byte prevPosition = 0;
                            long workIndex = curIndex + 2;
                            for (long i = 0; (i < 32) && (workIndex < dataLength); i++, workIndex++)
                            {
                                byte c = baData[workIndex];
                                if (c == 0x00)
                                {
                                    blockLength = workIndex - curIndex + 1;
                                    break;
                                }
                                else if (c <= prevPosition)
                                {
                                    blockLength = workIndex - curIndex;
                                    break;
                                }
                                prevPosition = c;
                            }
                            if (blockLength == 0)
                            {
                                blockLength = workIndex - curIndex;
                            }
                        }
                        break;

                    case 0x63: // ESC c
                        blockLength = 4;
                        ctlType = ctlByte2 switch
                        {
                            // ESC c 0
                            0x30 => EscPosCmdType.EscSelectPaperTypesPrinting,
                            // ESC c 1
                            0x31 => EscPosCmdType.EscSelectPaperTypesCommandSettings,
                            // ESC c 3
                            0x33 => EscPosCmdType.EscSelectPaperSensorsPaperEndSignals,
                            // ESC c 4
                            0x34 => EscPosCmdType.EscSelectPaperSensorsStopPrinting,
                            // ESC c 5
                            0x35 => EscPosCmdType.EscEnableDisablePanelButton,
                            // ESC c ??
                            _ => EscPosCmdType.EscUnknown,
                        };
                        break;

                    default: // ESC ??
                        ctlType = EscPosCmdType.EscUnknown;
                        break;
                }
            }
        }

        /// <summary>
        /// ESC(0x1B) XX started fixed size linedisplay command type and related length information
        /// </summary>
        private static readonly Dictionary<byte, SeqInfo> s_VfdESCType = new Dictionary<byte, SeqInfo>()
        {
            { 0x25, new SeqInfo { seqtype = EscPosCmdType.VfdEscSelectCancelUserDefinedCharacterSet, length = 3 } },
            { 0x3D, new SeqInfo { seqtype = EscPosCmdType.VfdEscSelectPeripheralDevice,              length = 3 } },
            { 0x3F, new SeqInfo { seqtype = EscPosCmdType.VfdEscCancelUserDefinedCharacters,         length = 3 } },
            { 0x40, new SeqInfo { seqtype = EscPosCmdType.VfdEscInitialize,                          length = 2 } },
            { 0x52, new SeqInfo { seqtype = EscPosCmdType.VfdEscSelectInternationalCharacterSet,     length = 3 } },
            { 0x74, new SeqInfo { seqtype = EscPosCmdType.VfdEscSelectCharacterCodeTable,            length = 3 } }
        };

        /// <summary>
        /// ESC(0x1B) started variable bytes or special handling linedisplay command sequence tokenize routine
        /// </summary>
        private void TokenizeESCvfd()
        {
            if (s_VfdESCType.ContainsKey(ctlByte1))
            {
                ctlType = s_VfdESCType[ctlByte1].seqtype;
                blockLength = s_VfdESCType[ctlByte1].length;
            }
            else
            {
                blockLength = 2;
                switch (ctlByte1)
                {
                    case 0x26: // ESC &
                        if (remainLength < 7)
                        {
                            blockLength = remainLength;
                        }
                        else
                        {
                            long fontHeight = ctlByte2;
                            if (
                                (fontHeight == CurrentVfdFontInfo.ybytes)
                                && ((ctlByte3 >= 0x20) && (ctlByte3 <= 0x7E))
                                && ((ctlByte4 >= 0x20) && (ctlByte4 <= 0x7E))
                                && (ctlByte3 <= ctlByte4)
                                )
                            {
                                ctlType = EscPosCmdType.None;
                                long codeCount = ctlByte4 - ctlByte3 + 1;
                                long maxFontWidth = CurrentVfdFontInfo.xbytes;
                                long count;
                                long workIndex = curIndex + 5;
                                for (count = 0; (count < codeCount) && (workIndex < dataLength); count++)
                                {
                                    long fontWidth = baData[workIndex];
                                    if (fontWidth > maxFontWidth)
                                    {
                                        ctlType = EscPosCmdType.VfdEscUnknown;
                                        blockLength = workIndex - curIndex;
                                        break;
                                    }
                                    workIndex += 1 + (fontHeight * fontWidth);
                                    if (workIndex > dataLength)
                                    {
                                        ctlType = EscPosCmdType.NotEnough;
                                        blockLength = remainLength;
                                        break;
                                    }
                                }
                                if (ctlType == EscPosCmdType.None)
                                {
                                    if (count >= codeCount)
                                    {
                                        ctlType = CurrentVfdFontInfo.seqtype;
                                        blockLength = workIndex - curIndex;
                                    }
                                    else
                                    {
                                        ctlType = EscPosCmdType.NotEnough;
                                        blockLength = remainLength;
                                    }
                                }
                            }
                            else // ESC & ?? ?? ?? xx
                            {
                                ctlType = EscPosCmdType.VfdEscUnknown;
                                blockLength = 5;
                            }
                        }
                        break;

                    case 0x3D: // ESC =
                        ctlType = EscPosCmdType.VfdEscSelectPeripheralDevice;
                        blockLength = 3;
                        break;

                    case 0x57: // ESC W
                        if ((ctlByte2 == 0) || (ctlByte2 > 4)) // ESC W ??
                        {
                            ctlType = EscPosCmdType.VfdEscUnknown;
                            blockLength = 3;
                        }
                        else
                        {
                            blockLength = 4;
                            switch (ctlByte3)
                            {
                                case 0x00: // ESC W xx 00
                                case 0x30: // ESC W xx 0
                                    ctlType = EscPosCmdType.VfdEscCancelWindowArea;
                                    break;

                                case 0x01: // ESC W xx 01
                                case 0x31: // ESC W xx 1
                                    ctlType = EscPosCmdType.VfdEscSelectWindowArea;
                                    blockLength = 8;
                                    break;

                                default: // ESC W xx ??
                                    ctlType = EscPosCmdType.VfdEscUnknown;
                                    break;
                            }
                        }
                        break;

                    default: // ESC ??
                        ctlType = EscPosCmdType.VfdEscUnknown;
                        break;
                }
            }
        }
    }
}