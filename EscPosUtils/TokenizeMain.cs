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
        /// ESC/POS command type and related data length information class
        /// </summary>
        private class SeqInfo
        {
            internal EscPosCmdType seqtype;
            internal long length;
        };

        /// <summary>
        /// single byte control code and command type enum table for printer
        /// </summary>
        private static readonly Dictionary<byte, EscPosCmdType> s_Prt1ByteType = new()
        {
            { 0x09, EscPosCmdType.HorizontalTab },
            { 0x0A, EscPosCmdType.PrintAndLineFeed },
            { 0x0C, EscPosCmdType.FormFeedPrintAndReturnToStandardMode },
            { 0x0D, EscPosCmdType.PrintAndCarriageReturn },
            { 0x18, EscPosCmdType.Cancel }
        };

        /// <summary>
        /// single byte control code and command type enum table for linedisplay(vacuum fluorescent display)
        /// </summary>
        private static readonly Dictionary<byte, EscPosCmdType> s_Vfd1ByteType = new()
        {
            { 0x08, EscPosCmdType.VfdMoveCursorLeft },
            { 0x09, EscPosCmdType.VfdMoveCursorRight },
            { 0x0A, EscPosCmdType.VfdMoveCursorDown },
            { 0x0B, EscPosCmdType.VfdHomePosition },
            { 0x0C, EscPosCmdType.VfdClearScreen },
            { 0x0D, EscPosCmdType.VfdMoveCursorLeftMost },
            { 0x18, EscPosCmdType.VfdClearCursorLine }
        };

        public const long EscPosPrinter = 1;
        public const long EscPosLineDisplay = 2;

        private byte[] baData = Array.Empty<byte>();
        private long dataLength;
        private long curIndex;

        internal Dictionary<byte, SbcsFontSizeInfo> SbcsFontList = new();
        internal Dictionary<byte, KanjiFontSizeInfo> KanjiFontList = new();
        internal SbcsFontSizeInfo CurrentSbcsFontInfo = new();
        internal KanjiFontSizeInfo CurrentKanjiFontInfo = new();
        internal VfdFontSizeInfo CurrentVfdFontInfo = new();

        private EscPosCmdType ctlType;
        private long blockLength;
        private long remainLength;
        private byte ctlByte0;
        private byte ctlByte1;
        private byte ctlByte2;
        private byte ctlByte3;
        private byte ctlByte4;

        /// <summary>
        /// entry point method of EscPosTokenizerr object
        /// </summary>
        /// <param name="data">byte array data</param>
        /// <param name="initialDevice">indicate initial device type</param>
        /// <returns></returns>
        public List<EscPosCmd> Scan(byte[] data, long initialDevice = EscPosPrinter, int SbcsFontPattern = 1, int KanjiFontPattern = 1, int LineDisplayFontPattern = 1)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            List<EscPosCmd> result = new() { };
            long targetDevice = initialDevice;
            long prtHead = -1;
            long ctlHead = -1;
            dataLength = data.Length;
            baData = data;
            ctlType = EscPosCmdType.None;

            SbcsFontList = SbcsFontPattern switch
            {
                1 => DeviceFontType.s_SbcsFontType01,
                2 => DeviceFontType.s_SbcsFontType02,
                3 => DeviceFontType.s_SbcsFontType03,
                4 => DeviceFontType.s_SbcsFontType04,
                5 => DeviceFontType.s_SbcsFontType05,
                6 => DeviceFontType.s_SbcsFontType06,
                7 => DeviceFontType.s_SbcsFontType07,
                8 => DeviceFontType.s_SbcsFontType08,
                9 => DeviceFontType.s_SbcsFontType09,
                _ => DeviceFontType.s_SbcsFontType01,
            };
            CurrentSbcsFontInfo = SbcsFontList[0];

            KanjiFontList = KanjiFontPattern switch
            {
                1 => DeviceFontType.s_KanjiFontType01,
                2 => DeviceFontType.s_KanjiFontType02,
                3 => DeviceFontType.s_KanjiFontType03,
                4 => DeviceFontType.s_KanjiFontType04,
                5 => DeviceFontType.s_KanjiFontType05,
                _ => DeviceFontType.s_KanjiFontType01,
            };
            CurrentKanjiFontInfo = KanjiFontList[0];

            CurrentVfdFontInfo = LineDisplayFontPattern switch
            {
                1 => DeviceFontType.s_VfdFontType01,
                2 => DeviceFontType.s_VfdFontType02,
                _ => DeviceFontType.s_VfdFontType01,
            };

            for (curIndex = 0; curIndex < dataLength; curIndex += blockLength)
            {
                blockLength = 0;
                ctlByte0 = baData[curIndex];
                remainLength = dataLength - curIndex;

                // special handling for select perihperal device sequence to update process target device type
                if ((ctlByte0 == 0x1B) && ((((curIndex + 1) < dataLength) ? baData[curIndex + 1] : 0xFF) == 0x3D))
                {
                    AddPrintablesAndControls(result, ref ctlHead, ref prtHead);
                    if ((curIndex + 2) < dataLength)
                    {
                        targetDevice = baData[curIndex + 2];
                    }
                }
                // special handling for realtime commands
                else if (ctlByte0 == 0x10)
                {
                    AddPrintablesAndControls(result, ref ctlHead, ref prtHead);
                    prtHead = -1;
                    ctlHead = -1;
                    if (remainLength < 2)
                    {
                        ctlType = EscPosCmdType.NotEnough;
                        blockLength = 1;
                    }
                    else
                    {
                        ctlByte1 = (byte)(((curIndex + 1) < dataLength) ? baData[curIndex + 1] : 0xFF);
                        ctlByte2 = (byte)(((curIndex + 2) < dataLength) ? baData[curIndex + 2] : 0xFF);
                        ctlByte3 = (byte)(((curIndex + 3) < dataLength) ? baData[curIndex + 3] : 0xFF);
                        ctlByte4 = (byte)(((curIndex + 4) < dataLength) ? baData[curIndex + 4] : 0xFF);
                        TokenizeDLE();
                        result.Add(new EscPosCmd(ctlType, ref baData, curIndex, blockLength));
                        ctlType = EscPosCmdType.None;
                        prtHead = -1;
                        ctlHead = -1;
                    }
                }
                // for Printer
                if ((targetDevice & EscPosPrinter) == EscPosPrinter)
                {
                    switch (ctlByte0)
                    {
                        // single byte control code
                        case 0x09: // HT
                        case 0x0C: // FF
                        case 0x18: // CAN
                            AddPrintablesAndControls(result, ref ctlHead, ref prtHead);
                            blockLength = 1;
                            ctlType = s_Prt1ByteType[ctlByte0];
                            result.Add(new EscPosCmd(ctlType, ref baData, curIndex, blockLength));
                            ctlType = EscPosCmdType.None;
                            prtHead = -1;
                            ctlHead = -1;
                            break;

                        case 0x0A: // LF
                        case 0x0D: // CR
                            AddPrintablesAndControls(result, ref ctlHead, ref prtHead);
                            blockLength = 1;
                            // Consecutive carriage returns and line feeds and vice versa are processed in combination
                            // so that they do not appear redundant.
                            ctlType = s_Prt1ByteType[ctlByte0];
                            ctlByte1 = (byte)(((curIndex + 1) < dataLength) ? baData[curIndex + 1] : 0);
                            if ((ctlByte0 == 0x0D) && (ctlByte1 == 0x0A))
                            {
                                blockLength = 2;
                                ctlType = EscPosCmdType.PrintAndCarriageReturnLineFeed;
                            }
                            else if ((ctlByte0 == 0x0A) && (ctlByte1 == 0x0D))
                            {
                                blockLength = 2;
                                ctlType = EscPosCmdType.PrintAndLineFeedCarriageReturn;
                            }
                            result.Add(new EscPosCmd(ctlType, ref baData, curIndex, blockLength));
                            ctlType = EscPosCmdType.None;
                            prtHead = -1;
                            ctlHead = -1;
                            break;

                        case 0x10: // DLE
                            break; // Since it is already specially processed as a realtime command, it will not be processed.
                        // muntiple bytes control data sequence
                        case 0x1B: // ESC
                        case 0x1C: // FS
                        case 0x1D: // GS
                            AddPrintablesAndControls(result, ref ctlHead, ref prtHead);
                            if (remainLength < 2)
                            {
                                ctlType = EscPosCmdType.NotEnough;
                                blockLength = 1;
                            }
                            else
                            {
                                ctlByte1 = (byte)(((curIndex + 1) < dataLength) ? baData[curIndex + 1] : 0xFF);
                                ctlByte2 = (byte)(((curIndex + 2) < dataLength) ? baData[curIndex + 2] : 0xFF);
                                ctlByte3 = (byte)(((curIndex + 3) < dataLength) ? baData[curIndex + 3] : 0xFF);
                                ctlByte4 = (byte)(((curIndex + 4) < dataLength) ? baData[curIndex + 4] : 0xFF);
                                switch (ctlByte0)
                                {
                                    case 0x1B:
                                        TokenizeESCprt();
                                        break;

                                    case 0x1C:
                                        TokenizeFS();
                                        break;

                                    case 0x1D:
                                        TokenizeGS();
                                        break;
                                }
                            }
                            if (remainLength < blockLength)
                            {
                                ctlType = EscPosCmdType.NotEnough;
                                blockLength = remainLength;
                            }
                            result.Add(new EscPosCmd(ctlType, ref baData, curIndex, blockLength));
                            ctlType = EscPosCmdType.None;
                            prtHead = -1;
                            ctlHead = -1;
                            break;

                        default:
                            // printable data
                            if (ctlByte0 >= 0x20)
                            {
                                if (ctlHead >= 0)
                                {
                                    result.Add(new EscPosCmd(ctlType, ref baData, ctlHead, (curIndex - ctlHead)));
                                    ctlHead = -1;
                                }
                                if (prtHead < 0)
                                {
                                    prtHead = curIndex;
                                    ctlType = EscPosCmdType.PrtPrintables;
                                }
                            }
                            // unused control code
                            else
                            {
                                if (prtHead >= 0)
                                {
                                    result.Add(new EscPosCmd(ctlType, ref baData, prtHead, (curIndex - prtHead)));
                                    prtHead = -1;
                                }
                                if (ctlHead < 0)
                                {
                                    ctlHead = curIndex;
                                    ctlType = EscPosCmdType.Controls;
                                }
                            }
                            blockLength = 1;
                            break;
                    }
                }
                // for LineDisplay
                else if ((targetDevice & EscPosLineDisplay) == EscPosLineDisplay)
                {
                    switch (ctlByte0)
                    {
                        // single byte control code
                        case 0x08: // BS
                        case 0x09: // HT
                        case 0x0A: // LF
                        case 0x0B: // HOM
                        case 0x0C: // CLR
                        case 0x0D: // CR
                        case 0x18: // CAN
                            AddPrintablesAndControls(result, ref ctlHead, ref prtHead);
                            blockLength = 1;
                            ctlType = s_Vfd1ByteType[ctlByte0];
                            result.Add(new EscPosCmd(ctlType, ref baData, curIndex, blockLength));
                            ctlType = EscPosCmdType.None;
                            prtHead = -1;
                            ctlHead = -1;
                            break;

                        case 0x10: // DLE
                            break; // guard for printer already specially processed as a realtime command, it will not be processed.
                        // muntiple bytes control data sequence
                        case 0x1B: // ESC
                        case 0x1F: // US
                            AddPrintablesAndControls(result, ref ctlHead, ref prtHead);
                            if (remainLength < 2)
                            {
                                ctlType = EscPosCmdType.NotEnough;
                                blockLength = 1;
                            }
                            else
                            {
                                ctlByte1 = (byte)(((curIndex + 1) < dataLength) ? baData[curIndex + 1] : 0xFF);
                                ctlByte2 = (byte)(((curIndex + 2) < dataLength) ? baData[curIndex + 2] : 0xFF);
                                ctlByte3 = (byte)(((curIndex + 3) < dataLength) ? baData[curIndex + 3] : 0xFF);
                                ctlByte4 = (byte)(((curIndex + 4) < dataLength) ? baData[curIndex + 4] : 0xFF);
                                switch (ctlByte0)
                                {
                                    case 0x1B:
                                        TokenizeESCvfd();
                                        break;

                                    case 0x1F:
                                        TokenizeUS();
                                        break;
                                }
                            }
                            if (remainLength < blockLength)
                            {
                                ctlType = EscPosCmdType.NotEnough;
                                blockLength = remainLength;
                            }
                            result.Add(new EscPosCmd(ctlType, ref baData, curIndex, blockLength));
                            ctlType = EscPosCmdType.None;
                            prtHead = -1;
                            ctlHead = -1;
                            break;

                        default:
                            // printable data
                            if (ctlByte0 >= 0x20)
                            {
                                if (ctlHead >= 0)
                                {
                                    result.Add(new EscPosCmd(ctlType, ref baData, ctlHead, (curIndex - ctlHead)));
                                    ctlHead = -1;
                                }
                                if (prtHead < 0)
                                {
                                    prtHead = curIndex;
                                    ctlType = EscPosCmdType.VfdDisplayables;
                                }
                            }
                            // unused control code
                            else
                            {
                                if (prtHead >= 0)
                                {
                                    result.Add(new EscPosCmd(ctlType, ref baData, prtHead, (curIndex - prtHead)));
                                    prtHead = -1;
                                }
                                if (ctlHead < 0)
                                {
                                    ctlHead = curIndex;
                                    ctlType = EscPosCmdType.Controls;
                                }
                            }
                            blockLength = 1;
                            break;
                    }
                }
                else
                {
                    blockLength = 1;
                }
            }
            AddPrintablesAndControls(result, ref ctlHead, ref prtHead);
            baData = Array.Empty<byte>();
            return result;
        }

        /// <summary>
        /// general printable data or unused control code putting to list process
        /// </summary>
        /// <param name="result">process target EscPosCmd List</param>
        /// <param name="ctlHead">head index of control codes</param>
        /// <param name="prtHead">head index of printable data</param>
        private void AddPrintablesAndControls(List<EscPosCmd> result, ref long ctlHead, ref long prtHead)
        {
            long headIndex = 0;
            long dataSize = 0;
            // printable data exists
            if (prtHead >= 0)
            {
                headIndex = prtHead;
                dataSize = curIndex - prtHead;
            }
            // unused control code exists
            else if (ctlHead >= 0)
            {
                headIndex = ctlHead;
                dataSize = curIndex - ctlHead;
            }
            // both process
            if (dataSize > 0)
            {
                result.Add(new EscPosCmd(ctlType, ref baData, headIndex, dataSize));
                ctlType = EscPosCmdType.None;
                prtHead = -1;
                ctlHead = -1;
            }
            return;
        }
    }
}