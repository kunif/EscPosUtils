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
    using System;
    using System.Collections.Generic;

    public partial class EscPosTokenizer
    {
        /// <summary>
        /// FS(0x1C) XX started fixed size printer command type and related length information
        /// </summary>
        private static readonly Dictionary<byte, SeqInfo> s_PrtFSType = new()
        {
            { 0x21, new SeqInfo { seqtype = EscPosCmdType.FsSelectPrintModeKanji, length = 3 } },
            { 0x26, new SeqInfo { seqtype = EscPosCmdType.FsSelectKanjiCharacterMode, length = 2 } },
            { 0x2D, new SeqInfo { seqtype = EscPosCmdType.FsTurnKanjiUnderlineMode, length = 3 } },
            { 0x2E, new SeqInfo { seqtype = EscPosCmdType.FsCancelKanjiCharacterMode, length = 2 } },
            { 0x3F, new SeqInfo { seqtype = EscPosCmdType.FsCancelUserDefinedKanjiCharacters, length = 4 } },
            { 0x43, new SeqInfo { seqtype = EscPosCmdType.FsSelectKanjiCharacterCodeSystem, length = 3 } },
            { 0x4C, new SeqInfo { seqtype = EscPosCmdType.FsSelectDoubleDensityPageMode, length = 2 } },
            { 0x53, new SeqInfo { seqtype = EscPosCmdType.FsSetKanjiCharacerSpacing, length = 4 } },
            { 0x57, new SeqInfo { seqtype = EscPosCmdType.FsTurnQuadrupleSizeMode, length = 3 } },
            { 0x62, new SeqInfo { seqtype = EscPosCmdType.FsObsoleteRequestTransmissionOfCheckPaperReadingResult, length = 2 } },
            { 0x63, new SeqInfo { seqtype = EscPosCmdType.FsCleanMICRMechanism, length = 2 } },
            { 0x70, new SeqInfo { seqtype = EscPosCmdType.FsObsoletePrintNVBitimage, length = 4 } }
        };

        /// <summary>
        /// FS(0x1C) ( E pL pH XX started printer command type information
        /// </summary>
        private static readonly Dictionary<byte, EscPosCmdType> s_PrtFSEType = new()
        {
            { 0x3C, EscPosCmdType.FsCancelSetValuesTopBottomLogo },
            { 0x3D, EscPosCmdType.FsTransmitSetValuesTopBottomLogo },
            { 0x3E, EscPosCmdType.FsSetTopLogoPrinting },
            { 0x3F, EscPosCmdType.FsSetBottomLogoPrinting },
            { 0x40, EscPosCmdType.FsMakeExtendSettingsTopBottomLogoPrinting },
            { 0x41, EscPosCmdType.FsEnableDisableTopBottomLogoPrinting }
        };

        /// <summary>
        /// FS(0x1C) (or8 L pL pH/p0,p1,p2,p3 XX  started printer command type information
        /// </summary>
        private static readonly Dictionary<byte, EscPosCmdType> s_PrtFSLType = new()
        {
            { 0x21, EscPosCmdType.FsPaperLayoutSetting },
            { 0x22, EscPosCmdType.FsPaperLayoutInformationTransmission },
            { 0x30, EscPosCmdType.FsTransmitPositioningInformation },
            { 0x41, EscPosCmdType.FsFeedPaperLabelPeelingPosition },
            { 0x42, EscPosCmdType.FsFeedPaperCuttingPosition },
            { 0x43, EscPosCmdType.FsFeedPaperPrintStartingPosition },
            { 0x50, EscPosCmdType.FsPaperLayoutErrorSpecialMarginSetting }
        };

        /// <summary>
        /// FS(0x1C) ( g pL pH XX  started printer command type information
        /// </summary>
        private static readonly Dictionary<byte, EscPosCmdType> s_PrtFSgType = new()
        {
            { 0x20, EscPosCmdType.FsSelectImageScannerCommandSettings },
            { 0x28, EscPosCmdType.FsSetBasicOperationOfImageScanner },
            { 0x29, EscPosCmdType.FsSetScanningArea },
            { 0x32, EscPosCmdType.FsSelectCompressionMethodForImageData },
            { 0x38, EscPosCmdType.FsDeleteCroppingArea },
            { 0x39, EscPosCmdType.FsSetCroppingArea },
            { 0x3C, EscPosCmdType.FsSelectTransmissionFormatForImageScanningResult },
            { 0x50, EscPosCmdType.FsTransmitSettingValueForBasicOperationsOfImageScanner },
            { 0x51, EscPosCmdType.FsTransmitSettingValueOfScanningArea },
            { 0x5A, EscPosCmdType.FsTransmitSettingValueOfCompressionMethdForImageData },
            { 0x61, EscPosCmdType.FsTransmitSettingValueOfCroppingArea },
            { 0x64, EscPosCmdType.FsTransmitSettingValueOfTransmissionFormatForImageScanningResult }
        };

        /// <summary>
        /// FS(0x1C) started variable bytes printer command sequence tokenize routine
        /// </summary>
        private void TokenizeFS()
        {
            if (s_PrtFSType.ContainsKey(ctlByte1))
            {
                ctlType = s_PrtFSType[ctlByte1].seqtype;
                blockLength = s_PrtFSType[ctlByte1].length;
            }
            else
            {
                blockLength = 2;
                switch (ctlByte1)
                {
                    case 0x28: // FS (
                        blockLength = 3;
                        ctlType = ctlByte2 switch
                        {
                            // FS ( A,C,E,L,e,f,g
                            0x41 or 0x43 or 0x45 or 0x4C or 0x65 or 0x66 or 0x67 => EscPosCmdType.None,
                            // FS ( ??
                            _ => EscPosCmdType.FsUnknown,
                        };
                        if (ctlType != EscPosCmdType.FsUnknown)
                        {
                            byte ctlByte5 = (byte)(((curIndex + 5) < dataLength) ? baData[curIndex + 5] : 0xFF);
                            // byte ctlByte6 = (byte)(((curIndex + 6) < dataLength) ? baData[curIndex + 6] : 0xFF);
                            blockLength = 5 + ctlByte4 * 0x100 + ctlByte3;
                            switch (ctlByte2)
                            {
                                case 0x41: // FS ( A 02 00 '0' xx
                                    if (blockLength == 7)
                                    {
                                        if (KanjiFontList.ContainsKey(baData[curIndex + 6]))
                                        {
                                            KanjiFontSizeInfo info = KanjiFontList[(baData[curIndex + 6])];
                                            ctlType = EscPosCmdType.FsSelectKanjiCharacterFont;
                                            CurrentKanjiFontInfo = info;
                                        }
                                        else
                                        {
                                            ctlType = EscPosCmdType.FsUnknown;
                                        }
                                    }
                                    else
                                    {
                                        ctlType = EscPosCmdType.FsUnknown;
                                    }
                                    break;

                                case 0x43: // FS ( C
                                    ctlType = ctlByte5 switch
                                    {
                                        // FS ( C 02 00 '0' xx
                                        0x30 => (blockLength == 7) ? EscPosCmdType.FsSelectCharacterEncodeSystem : EscPosCmdType.FsUnknown,
                                        // FS ( C 03 00 '<' xx xx
                                        0x3C => (blockLength == 8) ? EscPosCmdType.FsSetFontPriority : EscPosCmdType.FsUnknown,
                                        _ => EscPosCmdType.FsUnknown,
                                    };
                                    break;

                                case 0x45: // FS ( E
                                    if (remainLength >= blockLength)
                                    {
                                        if (s_PrtFSEType.ContainsKey(ctlByte5))
                                        {
                                            ctlType = s_PrtFSEType[ctlByte5];
                                        }
                                        else // FS ( E dL dH ??
                                        {
                                            ctlType = EscPosCmdType.FsUnknown;
                                        }
                                    }
                                    break;

                                case 0x4C: // FS ( L
                                    if (remainLength >= blockLength)
                                    {
                                        if (s_PrtFSLType.ContainsKey(ctlByte5))
                                        {
                                            ctlType = s_PrtFSLType[ctlByte5];
                                        }
                                        else // FS ( L dL dH ??
                                        {
                                            ctlType = EscPosCmdType.FsUnknown;
                                        }
                                    }
                                    break;

                                case 0x65: // FS ( e
                                    ctlType = (blockLength == 7) ? EscPosCmdType.FsEnableDisableAutomaticStatusBackOptional : EscPosCmdType.FsUnknown;
                                    break;

                                case 0x66: // FS ( f
                                    ctlType = EscPosCmdType.FsSelectMICRDataHandling;
                                    break;

                                case 0x67: // FS ( g
                                    if (remainLength >= blockLength)
                                    {
                                        if (s_PrtFSgType.ContainsKey(ctlByte5))
                                        {
                                            ctlType = s_PrtFSgType[ctlByte5];
                                        }
                                        else // FS ( g dL dH ??
                                        {
                                            ctlType = EscPosCmdType.FsUnknown;
                                        }
                                    }
                                    break;

                                default: // FS ( ??
                                    ctlType = EscPosCmdType.FsUnknown;
                                    break;
                            }
                        }
                        break;

                    case 0x32: // FS 2 --depends on KanjiFontType 72(FontA)/60(FontB)/32(FontC)
                        ctlType = CurrentKanjiFontInfo.seqtype;
                        blockLength = 4 + (CurrentKanjiFontInfo.xbytes * CurrentKanjiFontInfo.ybytes);
                        break;

                    case 0x61: // FS a
                        ctlType = ctlByte2 switch
                        {
                            // FS a 0
                            0x30 => EscPosCmdType.FsObsoleteReadCheckPaper,
                            // FS a 1
                            0x31 => EscPosCmdType.FsObsoleteLoadCheckPaperToPrintStartingPosition,
                            // FS a 2
                            0x32 => EscPosCmdType.FsObsoleteEjectCheckPaper,
                            // FS ( ??
                            _ => EscPosCmdType.FsUnknown,
                        };
                        blockLength = ctlType == EscPosCmdType.FsObsoleteReadCheckPaper ? 4 : 3;
                        break;

                    case 0x67: // FS g
                        if ((ctlByte2 == 0x31) && (ctlByte3 == 0x00))
                        {
                            if (remainLength < 10)
                            {
                                blockLength = 10;
                                break;
                            }
                            ctlType = EscPosCmdType.FsObsoleteWriteNVUserMemory;
                            blockLength = 10 + BitConverter.ToUInt16(baData, (int)(curIndex + 8));
                        }
                        else if ((ctlByte2 == 0x32) && (ctlByte3 == 0x00))
                        {
                            ctlType = EscPosCmdType.FsObsoleteReadNVUserMemory;
                            blockLength = 10;
                        }
                        else // FS g ?? ??
                        {
                            ctlType = EscPosCmdType.FsUnknown;
                            blockLength = 4;
                        }
                        break;

                    case 0x71: // FS q
                        if ((ctlByte2 > 0) && (remainLength >= 15))
                        {
                            ctlType = EscPosCmdType.None;
                            long imageCount = ctlByte2;
                            long count;
                            long workIndex = curIndex + 3;
                            for (count = 0; (count < imageCount) && (workIndex < dataLength); count++)
                            {
                                long Xbytes = BitConverter.ToUInt16(baData, (int)workIndex);
                                long Ybytes = BitConverter.ToUInt16(baData, (int)(workIndex + 2));
                                workIndex += 4 + (Xbytes * Ybytes * 8);
                                if (workIndex > dataLength)
                                {
                                    ctlType = EscPosCmdType.NotEnough;
                                    blockLength = remainLength;
                                    break;
                                }
                            }
                            if (ctlType == EscPosCmdType.None)
                            {
                                if (count >= imageCount)
                                {
                                    ctlType = EscPosCmdType.FsObsoleteDefineNVBitimage;
                                    blockLength = workIndex - curIndex;
                                }
                                else
                                {
                                    ctlType = EscPosCmdType.NotEnough;
                                    blockLength = remainLength;
                                }
                            }
                        }
                        else // FS 00 or NotEnough
                        {
                            ctlType = EscPosCmdType.FsUnknown;
                            blockLength = 3;
                        }
                        break;

                    default: // FS ??
                        ctlType = EscPosCmdType.FsUnknown;
                        break;
                }
            }
        }
    }
}