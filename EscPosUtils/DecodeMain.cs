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
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Text;

    public static partial class EscPosDecoder
    {
        internal static ASCIIEncoding ascii = new();
        internal static CultureInfo invariantculture = CultureInfo.InvariantCulture;

        internal delegate string DecodeDetail(EscPosCmd record, int index);

        /// <summary>
        /// ESC/POS decode type and related detail function delegate structure
        /// </summary>
        //private class DecInfo
        private class DecInfo
        {
            internal DecodeDetail? decodedetail;
            internal int index;
            internal string leader = string.Empty;
            internal string trailer = string.Empty;
        };

        /// <summary>
        ///
        /// </summary>
        /// <param name="record"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static string DecodeLsbOnOff(EscPosCmd record, int index)
        {
            return (record.cmddata[index] & 1) == 1 ? "ON" : "OFF";
        }

        internal static string DecodeValueOnOff(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0x00 => "OFF",
                0x30 => "OFF",
                0x01 => "ON",
                0x31 => "ON",
                _ => "Undefined",
            };
        }

        internal static string DecodeValueSInt08(EscPosCmd record, int index)
        {
            sbyte[] signed = Array.ConvertAll(record.cmddata, b => unchecked((sbyte)b));
            return signed[index].ToString("D", invariantculture);
        }

        internal static string DecodeValueSInt16(EscPosCmd record, int index)
        {
            return BitConverter.ToInt16(record.cmddata, index).ToString("D", invariantculture);
        }

        internal static string DecodeValueSInt32(EscPosCmd record, int index)
        {
            return BitConverter.ToInt32(record.cmddata, index).ToString("D", invariantculture);
        }

        internal static string DecodeValueUInt08(EscPosCmd record, int index)
        {
            return record.cmddata[index].ToString("D", invariantculture);
        }

        internal static string DecodeValueUInt16(EscPosCmd record, int index)
        {
            return BitConverter.ToUInt16(record.cmddata, index).ToString("D", invariantculture);
        }

        internal static string DecodeValueUInt32(EscPosCmd record, int index)
        {
            return BitConverter.ToUInt32(record.cmddata, index).ToString("D", invariantculture);
        }

        //  DLE EOT 10 04 01/02/03/04/07/08/12 [01/02/03]
        internal static string DecodeDleTransmitRealtimeStatus(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                1 => "Printer",
                2 => "Offline cause",
                3 => "Error cause",
                4 => "Roll paper sensor",
                7 => record.cmddata[index + 1] switch
                {
                    1 => "Ink A",
                    2 => "Ink B",
                    _ => "Undefined",
                },
                8 => record.cmddata[index + 1] == 3 ? "Peeler" : "Undefined",
                18 => record.cmddata[index + 1] switch
                {
                    1 => "Interface",
                    2 => "DM-D",
                    _ => "Undefined",
                },
                _ => "Undefined",
            };
        }

        //  DLE ENQ 10 05 00/01/02
        internal static string DecodeDleSendRealtimeRequest(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "Equivalent to pressing the FEED button when waiting for online recovery.",
                1 => "After returning from the error, printing resumes from the beginning of the line in which the error occurred.",
                2 => "After clearing the receive buffer and print buffer, recover from the error.",
                _ => "Undefined",
            };
        }

        //  DLE DC4 10 14 01 00/01 01-08
        internal static string DecodeDleGeneratePulseRealtime(EscPosCmd record, int index)
        {
            string result = record.cmddata[index] switch
            {
                0 => "Pin 2",
                1 => "Pin 5",
                _ => "Undefined Pin",
            };
            byte duration = record.cmddata[index + 1];
            if (duration >= 1 && duration <= 8)
            {
                result += ", Pulse " + duration.ToString("D", invariantculture) + " x 100ms";
            }
            else
            {
                result += ", Pulse value out of range";
            }
            return result;
        }

        //  DLE DC4 10 14 03 00-07 00 00-08 01 00
        internal static string DecodeDleSoundBuzzRealtime(EscPosCmd record, int index)
        {
            string result;
            byte pattern = record.cmddata[index];
            if (pattern <= 7)
            {
                result = "Buzzer pattern No." + pattern.ToString("D", invariantculture);
                byte cycles = record.cmddata[index + 2];
                if (cycles == 0)
                {
                    result += ", Repeat infinite.";
                }
                else if (cycles <= 8)
                {
                    result += ", Repeat " + cycles.ToString("D", invariantculture) + " times.";
                }
                else
                {
                    result += ", Repeat value out of range.";
                }
            }
            else
            {
                result = "Buzzer pattern No. value out of range";
            }
            return result;
        }

        //  DLE DC4 10 14 07 01/02/04/05
        internal static string DecodeDleTransmitSpcifiedStatusRealtime(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                1 => "Basic ASB",
                2 => "Extended ASB",
                4 => "Offline response",
                5 => "Battery",
                _ => "Undefined",
            };
        }

        internal enum ImageDataType
        {
            Raster,
            Column
        }

        internal static System.Drawing.Bitmap GetBitmap(int width, int height, ImageDataType imageDataType, byte[] imageData, int srcindex, string color)
        {
            int x = imageDataType == ImageDataType.Raster ? width : height;
            int y = imageDataType == ImageDataType.Raster ? height : width;
            System.Drawing.Bitmap bitmap = new(x, y, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            ColorPalette palette = bitmap.Palette;
            palette.Entries[0] = Color.White;
            palette.Entries[1] = color switch
            {
                "1" => Color.Black,
                "2" => Color.Red,
                "3" => Color.Green,
                "4" => Color.Blue,
                _ => Color.Yellow,
            };
            bitmap.Palette = palette;
            System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            int workBufferSize = bmpData.Stride * bmpData.Height;
            byte[] workBuffer = new byte[workBufferSize];
            int xbytes = ((x + 7) / 8);
            int dstindex = 0;
            for (int yindex = 0; yindex < bmpData.Height; yindex++)
            {
                Buffer.BlockCopy(imageData, srcindex, workBuffer, dstindex, xbytes);
                srcindex += xbytes;
                dstindex += bmpData.Stride;
            }
            IntPtr ptr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(workBuffer, 0, ptr, workBufferSize);
            bitmap.UnlockBits(bmpData);
            if (imageDataType == ImageDataType.Column)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
            }
            return bitmap;
        }

        /// <summary>
        ///
        /// </summary>
        private static readonly Dictionary<EscPosCmdType, DecInfo?> s_DecodeType = new()
        {
            { EscPosCmdType.None, null },
            { EscPosCmdType.NotEnough, null },
            { EscPosCmdType.Unknown, null },
            { EscPosCmdType.Controls, null },                               //  0x00-0x1F Code not used for the following control sequences
            { EscPosCmdType.PrtPrintables, null },                          //  0x20-0xFF
            { EscPosCmdType.HorizontalTab, null },                          //  HT    09
            { EscPosCmdType.PrintAndLineFeed, null },                       //  LF    0A
            { EscPosCmdType.FormFeedPrintAndReturnToStandardMode, null },   //  FF    0C  // in PageMode
            { EscPosCmdType.EndOfJob, null },                               //  FF    0C  // in StandardMode
            { EscPosCmdType.PrintAndReturnToStandardMode, null },           //c FF    0C  // in PageMode
            { EscPosCmdType.PrintAndEjectCutSheet, null },                  //c FF    0C  // in Standard Mode
            { EscPosCmdType.PrintAndCarriageReturn, null },                 //  CR    0D
            { EscPosCmdType.Cancel, null },                                 //  CAN   18
            { EscPosCmdType.PrintAndCarriageReturnLineFeed, null },         //  CR LF 0D 0A
            { EscPosCmdType.PrintAndLineFeedCarriageReturn, null },         //  LF CR 0A 0D
                                                                            // VFD
            { EscPosCmdType.VfdDisplayables, null },        //  0x20-0xFF
            { EscPosCmdType.VfdMoveCursorLeft, null },      //  BS    08
            { EscPosCmdType.VfdMoveCursorRight, null },     //  HT    09
            { EscPosCmdType.VfdMoveCursorDown, null },      //  LF    0A
            { EscPosCmdType.VfdHomePosition, null },        //  HOME  0B
            { EscPosCmdType.VfdClearScreen, null },         //  CLR   0C
            { EscPosCmdType.VfdMoveCursorLeftMost, null },  //  CR    0D
            { EscPosCmdType.VfdClearCursorLine, null },     //  CAN   18
                                                            // DLE 0x10
            { EscPosCmdType.DleTransmitRealtimeStatus, new DecInfo { decodedetail = new DecodeDetail(DecodeDleTransmitRealtimeStatus), index = 2 } },                 //  DLE EOT 10 04 01/02/03/04/07/08/12 [01/02/03]
            { EscPosCmdType.DleSendRealtimeRequest, new DecInfo { decodedetail = new DecodeDetail(DecodeDleSendRealtimeRequest), index = 2 } },                       //  DLE ENQ 10 05 00/01/02
            { EscPosCmdType.DleGeneratePulseRealtime, new DecInfo { decodedetail = new DecodeDetail(DecodeDleGeneratePulseRealtime), index = 3 } },                  //  DLE DC4 10 14 01 00/01 01-08
            { EscPosCmdType.DleExecPowerOff, null },                                                                                                                  //  DLE DC4 10 14 02 01 08
            { EscPosCmdType.DleSoundBuzzRealtime, new DecInfo { decodedetail = new DecodeDetail(DecodeDleSoundBuzzRealtime), index = 3 } },                           //  DLE DC4 10 14 03 00-07 00 00-08 01 00
            { EscPosCmdType.DleTransmitSpcifiedStatusRealtime, new DecInfo { decodedetail = new DecodeDetail(DecodeDleTransmitSpcifiedStatusRealtime), index = 3 } }, //  DLE DC4 10 14 07 01/02/04/05
            { EscPosCmdType.DleClearBuffer, null },                                                                                                                   //  DLE DC4 10 14 08 01 03 14 01 06 02 08
            { EscPosCmdType.DleUnknown, null },
            // ESC 0x1B
            { EscPosCmdType.EscPageModeFormFeed, null },                                                                                                                              //  ESC FF  1B 0C  // PrintData in Page Mode
            { EscPosCmdType.EscRightSideSpacing, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt08), index = 2, trailer = " dots" } },                                  //  ESC SP  1B 20 00-FF
            { EscPosCmdType.EscSelectPrintMode, new DecInfo { decodedetail = new DecodeDetail(DecodeEscSelectPrintMode), index = 2 } },                                               //  ESC !   1B 21 bx0xxx00x
            { EscPosCmdType.EscAbsoluteHorizontalPrintPosition, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt16), index = 2, trailer = " dots" } },                   //  ESC $   1B 24 0000-FFFF
            { EscPosCmdType.EscSelectUserDefinedCharacterSet, new DecInfo { decodedetail = new DecodeDetail(DecodeLsbOnOff), index = 2 } },                                           //  ESC %   1B 25 bnnnnnnnx
            { EscPosCmdType.EscDefineUserDefinedCharacters1224, new DecInfo { decodedetail = new DecodeDetail(DecodeEscDefineUserDefinedCharacters1224), index = 2 } },              //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
            { EscPosCmdType.EscDefineUserDefinedCharacters1024, new DecInfo { decodedetail = new DecodeDetail(DecodeEscDefineUserDefinedCharacters1024), index = 2 } },              //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
            { EscPosCmdType.EscDefineUserDefinedCharacters0924, new DecInfo { decodedetail = new DecodeDetail(DecodeEscDefineUserDefinedCharacters0924), index = 2 } },              //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
            { EscPosCmdType.EscDefineUserDefinedCharacters0917, new DecInfo { decodedetail = new DecodeDetail(DecodeEscDefineUserDefinedCharacters0917), index = 2 } },              //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
            { EscPosCmdType.EscDefineUserDefinedCharacters0909, new DecInfo { decodedetail = new DecodeDetail(DecodeEscDefineUserDefinedCharacters0909), index = 2 } },              //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
            { EscPosCmdType.EscDefineUserDefinedCharacters0709, new DecInfo { decodedetail = new DecodeDetail(DecodeEscDefineUserDefinedCharacters0709), index = 2 } },              //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
            { EscPosCmdType.EscDefineUserDefinedCharacters0816, new DecInfo { decodedetail = new DecodeDetail(DecodeEscDefineUserDefinedCharacters0816), index = 2 } },              //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
            { EscPosCmdType.EscBeeperBuzzer, new DecInfo { decodedetail = new DecodeDetail(DecodeEscBeeperBuzzer), index = 6 } },                                                     //  ESC ( A 1B 28 41 04 00 30 30-3A 01-3F 0A-FF
            { EscPosCmdType.EscBeeperBuzzerM1a, new DecInfo { decodedetail = new DecodeDetail(DecodeEscBeeperBuzzerM1a), index = 6 } },                                               //  ESC ( A 1B 28 41 03 00 61 01-07 00-FF
            { EscPosCmdType.EscBeeperBuzzerM1b, new DecInfo { decodedetail = new DecodeDetail(DecodeEscBeeperBuzzerM1b), index = 7 } },                                               //  ESC ( A 1B 28 41 05 00 61 64 00-3F 00-FF 00-FF
            { EscPosCmdType.EscBeeperBuzzerOffline, new DecInfo { decodedetail = new DecodeDetail(DecodeEscBeeperBuzzerOffline), index = 6 } },                                       //  ESC ( A 1B 28 41 07 00 62 30-33 01 64 00/FF 01-32/FF 01-32
            { EscPosCmdType.EscBeeperBuzzerNearEnd, new DecInfo { decodedetail = new DecodeDetail(DecodeEscBeeperBuzzerNearEnd), index = 9 } },                                       //  ESC ( A 1B 28 41 07 00 63 30 01 64 00/FF 01-32/FF 01-32
            { EscPosCmdType.EscSpecifyBatchPrint, new DecInfo { decodedetail = new DecodeDetail(DecodeEscSpecifyBatchPrint), index = 5 } },                                           //  ESC ( Y 1B 28 59 02 00 00/01/30/31 00/01/30/31
            { EscPosCmdType.EscSelectBitImageMode, new DecInfo { decodedetail = new DecodeDetail(DecodeEscSelectBitImageMode), index = 2 } },                                         //  ESC *   1B 2A 00/01/20/21 0001-0960 00-FF...
            { EscPosCmdType.EscUnderlineMode, new DecInfo { decodedetail = new DecodeDetail(DecodeEscUnderlineMode), index = 2 } },                                                   //  ESC -   1B 2D 00-02/30-32
            { EscPosCmdType.EscSelectDefaultLineSpacing, null },                                                                                                                      //  ESC 2   1B 32
            { EscPosCmdType.EscLineSpacing, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt08), index = 2, trailer = " dots" } },                                       //  ESC 3   1B 33 00-FF
            { EscPosCmdType.EscReturnHome, null },                                                                                                                                    //  ESC <   1B 3C
            { EscPosCmdType.EscSelectPeripheralDevice, new DecInfo { decodedetail = new DecodeDetail(DecodeEscSelectPeripheralDevice), index = 2 } },                                 //  ESC =   1B 3D 01-03 or 00-FF
            { EscPosCmdType.EscCancelUserDefinedCharacters, new DecInfo { decodedetail = new DecodeDetail(DecodeEscCancelUserDefinedCharacters), index = 2 } },                       //  ESC ?   1B 3F 20-7E
            { EscPosCmdType.EscInitialize, null },                                                                                                                                    //  ESC @   1B 40
            { EscPosCmdType.EscSetCutSheetEjectLength, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt08), index = 2, trailer = " lines" } },                           //c ESC C   1B 43 00-FF
            { EscPosCmdType.EscHorizontalTabPosition, new DecInfo { decodedetail = new DecodeDetail(DecodeEscHorizontalTabPosition), index = 2 } },                                   //  ESC D   1B 44 [01-FF]... 00
            { EscPosCmdType.EscTurnEmphasizedMode, new DecInfo { decodedetail = new DecodeDetail(DecodeLsbOnOff), index = 2 } },                                                      //  ESC E   1B 45 bnnnnnnnx
            { EscPosCmdType.EscSetCancelCutSheetReverseEject, new DecInfo { decodedetail = new DecodeDetail(DecodeLsbOnOff), index = 2 } },                                           //c ESC F   1B 46 bnnnnnnnx
            { EscPosCmdType.EscTurnDoubleStrikeMode, new DecInfo { decodedetail = new DecodeDetail(DecodeLsbOnOff), index = 2 } },                                                    //  ESC G   1B 47 bnnnnnnnx
            { EscPosCmdType.EscPrintAndFeedPaper, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt08), index = 2, trailer = " dots" } },                                 //  ESC J   1B 4A 00-FF
            { EscPosCmdType.EscPrintAndReverseFeed, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt08), index = 2, trailer = " dots" } },                               //  ESC K   1B 4B 00-30
            { EscPosCmdType.EscSelectPageMode, null },                                                                                                                                //  ESC L   1B 4C
            { EscPosCmdType.EscSelectCharacterFont, new DecInfo { decodedetail = new DecodeDetail(DecodeEscSelectCharacterFont), index = 2 } },                                       //  ESC M   1B 4D 00-04/30-34/61/62
            { EscPosCmdType.EscSelectInternationalCharacterSet, new DecInfo { decodedetail = new DecodeDetail(DecodeEscSelectInternationalCharacterSet), index = 2 } },               //  ESC R   1B 52 00-11/42-4B/52
            { EscPosCmdType.EscSelectStandardMode, null },                                                                                                                            //  ESC S   1B 53
            { EscPosCmdType.EscSelectPrintDirection, new DecInfo { decodedetail = new DecodeDetail(DecodeEscSelectPrintDirection), index = 2 } },                                     //  ESC T   1B 54 00-03/30-33
            { EscPosCmdType.EscTurnUnidirectionalPrintMode, new DecInfo { decodedetail = new DecodeDetail(DecodeLsbOnOff), index = 2 } },                                             //  ESC U   1B 55 bnnnnnnnx
            { EscPosCmdType.EscTurn90digreeClockwiseRotationMode, new DecInfo { decodedetail = new DecodeDetail(DecodeEscTurn90digreeClockwiseRotationMode), index = 2 } },           //  ESC V   1B 56 00-02/30-32
            { EscPosCmdType.EscSetPrintAreaInPageMode, new DecInfo { decodedetail = new DecodeDetail(DecodeEscSetPrintAreaInPageMode), index = 2 } },                                 //  ESC W   1B 57 0000-FFFF 0000-FFFF 0001-FFFF 0001-FFFF
            { EscPosCmdType.EscSetRelativeHorizontalPrintPosition, new DecInfo { decodedetail = new DecodeDetail(DecodeValueSInt16), index = 2, trailer = " dots" } },                //  ESC \   1B 5C 8000-7FFF
            { EscPosCmdType.EscSelectJustification, new DecInfo { decodedetail = new DecodeDetail(DecodeEscSelectJustification), index = 2 } },                                       //  ESC a   1B 61 00-02/30-32
            { EscPosCmdType.EscSelectPaperTypesPrinting, new DecInfo { decodedetail = new DecodeDetail(DecodeEscSelectPaperTypesPrinting), index = 3 } },                             //c ESC c 0 1B 63 30 b0000xxxx
            { EscPosCmdType.EscSelectPaperTypesCommandSettings, new DecInfo { decodedetail = new DecodeDetail(DecodeEscSelectPaperTypesCommandSettings), index = 3 } },               //c ESC c 1 1B 63 31 b0x00xxxx
            { EscPosCmdType.EscSelectPaperSensorsPaperEndSignals, new DecInfo { decodedetail = new DecodeDetail(DecodeEscSelectPaperSensorsPaperEndSignals), index = 3 } },           //  ESC c 3 1B 63 33 bccccxxxx
            { EscPosCmdType.EscSelectPaperSensorsStopPrinting, new DecInfo { decodedetail = new DecodeDetail(DecodeEscSelectPaperSensorsStopPrinting), index = 3 } },                 //  ESC c 4 1B 63 34 bccccxxxx
            { EscPosCmdType.EscEnableDisablePanelButton, new DecInfo { decodedetail = new DecodeDetail(DecodeLsbOnOff), index = 3 } },                                                //  ESC c 5 1B 63 35 bnnnnnnnx
            { EscPosCmdType.EscPrintAndFeedNLines, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt08), index = 2, trailer = " lines" } },                               //  ESC d   1B 64 00-FF
            { EscPosCmdType.EscPrintAndReverseFeedNLines, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt08), index = 2, trailer = " lines" } },                        //  ESC e   1B 65 00-02
            { EscPosCmdType.EscCutSheetWaitTime, new DecInfo { decodedetail = new DecodeDetail(DecodeEscCutSheetWaitTime), index = 2 } },                                             //c ESC f   1B 66 00-0F 00-40
            { EscPosCmdType.EscObsoletePartialCut1Point, null },                                                                                                                      //  ESC i   1B 69
            { EscPosCmdType.EscObsoletePartialCut3Point, null },                                                                                                                      //  ESC m   1B 6D
            { EscPosCmdType.EscGeneratePulse, new DecInfo { decodedetail = new DecodeDetail(DecodeEscGeneratePulse), index = 2 } },                                                   //  ESC p   1B 70 00/01/30/31 00-FF 00-FF
            { EscPosCmdType.EscReleasePaper, null },                                                                                                                                  //c ESC q   1B 71
            { EscPosCmdType.EscSelectPrinterColor, new DecInfo { decodedetail = new DecodeDetail(DecodeEscSelectPrinterColor), index = 2 } },                                         //  ESC r   1B 72 00/01/30/31
            { EscPosCmdType.EscSelectCharacterCodeTable, new DecInfo { decodedetail = new DecodeDetail(DecodeEscSelectCharacterCodeTable), index = 2 } },                             //  ESC t   1B 74 00-08/0B-1A/1E-35/42-4B/52/FE/FF
            { EscPosCmdType.EscObsoleteTransmitPeripheralDeviceStatus, new DecInfo { decodedetail = new DecodeDetail(DecodeEscObsoleteTransmitPeripheralDeviceStatus), index = 2 } }, //  ESC u   1B 75 00/30
            { EscPosCmdType.EscObsoleteTransmitPaperSensorStatus, null },                                                                                                             //  ESC v   1B 76
            { EscPosCmdType.EscTurnUpsideDownPrintMode, new DecInfo { decodedetail = new DecodeDetail(DecodeLsbOnOff), index = 2 } },                                                 //  ESC {   1B 7B bnnnnnnnx
            { EscPosCmdType.EscUnknown, null },
            // ESC 0x1B VFD
            { EscPosCmdType.VfdEscSelectCancelUserDefinedCharacterSet, new DecInfo { decodedetail = new DecodeDetail(DecodeLsbOnOff), index = 2 } },                          //  ESC %   1B 25 bnnnnnnnx
            { EscPosCmdType.VfdEscDefineUserDefinedCharacters0816, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdEscDefineUserDefinedCharacters0816), index = 2 } }, //  ESC &   1B 26 00/01 20-7E 20-7E 00-08 00-FF...
            { EscPosCmdType.VfdEscDefineUserDefinedCharacters0507, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdEscDefineUserDefinedCharacters0507), index = 2 } }, //  ESC &   1B 26 00/01 20-7E 20-7E 00-08 00-FF...
            { EscPosCmdType.VfdEscSelectPeripheralDevice, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdEscSelectPeripheralDevice), index = 2 } },                   //  ESC =   1B 3D 01/02/03
            { EscPosCmdType.VfdEscCancelUserDefinedCharacters, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdEscCancelUserDefinedCharacters), index = 2 } },         //  ESC ?   1B 3F 20-7E
            { EscPosCmdType.VfdEscInitialize, null },                                                                                                                         //  ESC @   1B 40
            { EscPosCmdType.VfdEscSelectInternationalCharacterSet, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdEscSelectInternationalCharacterSet), index = 2 } }, //  ESC R   1B 52 00-11
            { EscPosCmdType.VfdEscCancelWindowArea, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdEscCancelWindowArea), index = 2 } },                               //  ESC W   1B 57 01-04 00/30
            { EscPosCmdType.VfdEscSelectWindowArea, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdEscSelectWindowArea), index = 2 } },                               //  ESC W   1B 57 01-04 01/31 01-14 01-14 01/02 01/02
            { EscPosCmdType.VfdEscSelectCharacterCodeTable, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdEscSelectCharacterCodeTable), index = 2 } },               //  ESC t   1B 74 00-13/1E-35/FE/FF
            { EscPosCmdType.VfdEscUnknown, null },
            // FS 0x1C
            { EscPosCmdType.FsSelectPrintModeKanji, new DecInfo { decodedetail = new DecodeDetail(DecodeFsSelectPrintModeKanji), index = 2 } },                                                     //  FS  !   1C 21 bx000xx00
            { EscPosCmdType.FsSelectKanjiCharacterMode, null },                                                                                                                                     //  FS  &   1C 26
            { EscPosCmdType.FsSelectKanjiCharacterFont, new DecInfo { decodedetail = new DecodeDetail(DecodeFsSelectKanjiCharacterFont), index = 6 } },                                             //  FS  ( A 1C 28 41 02 00 30 00-02/30-32
            { EscPosCmdType.FsSelectCharacterEncodeSystem, new DecInfo { decodedetail = new DecodeDetail(DecodeFsSelectCharacterEncodeSystem), index = 6 } },                                       //  FS  ( C 1C 28 43 02 00 30 01/02/31/32
            { EscPosCmdType.FsSetFontPriority, new DecInfo { decodedetail = new DecodeDetail(DecodeFsSetFontPriority), index = 6 } },                                                               //  FS  ( C 1C 28 43 03 00 3C 00/01 00/0B/14/1E/29
            { EscPosCmdType.FsCancelSetValuesTopBottomLogo, new DecInfo { decodedetail = new DecodeDetail(DecodeFsCancelSetValuesTopBottomLogo), index = 7 } },                                     //  FS  ( E 1C 28 45 06 00 3C 02 30/31 43 4C 52
            { EscPosCmdType.FsTransmitSetValuesTopBottomLogo, new DecInfo { decodedetail = new DecodeDetail(DecodeFsTransmitSetValuesTopBottomLogo), index = 7 } },                                 //  FS  ( E 1C 28 45 03 00 3D 02 30-32
            { EscPosCmdType.FsSetTopLogoPrinting, new DecInfo { decodedetail = new DecodeDetail(DecodeFsSetTopLogoPrinting), index = 7 } },                                                         //  FS  ( E 1C 28 45 06 00 3E 02 20-7E 20-7E 30-32 00-FF
            { EscPosCmdType.FsSetBottomLogoPrinting, new DecInfo { decodedetail = new DecodeDetail(DecodeFsSetBottomLogoPrinting), index = 7 } },                                                   //  FS  ( E 1C 28 45 05 00 3F 02 20-7E 20-7E 30-32
            { EscPosCmdType.FsMakeExtendSettingsTopBottomLogoPrinting, new DecInfo { decodedetail = new DecodeDetail(DecodeFsMakeExtendSettingsTopBottomLogoPrinting), index = 3 } },               //  FS  ( E 1C 28 45 0004-000C 40 02 [30/40-43 30/31]...
            { EscPosCmdType.FsEnableDisableTopBottomLogoPrinting, new DecInfo { decodedetail = new DecodeDetail(DecodeFsEnableDisableTopBottomLogoPrinting), index = 7 } },                         //  FS  ( E 1C 28 45 04 00 41 02 30/31 30/31
            { EscPosCmdType.FsPaperLayoutSetting, new DecInfo { decodedetail = new DecodeDetail(DecodeFsPaperLayoutSetting), index = 3 } },                                                         //  FS  ( L 1C 28 4C 0008-001a 21 30-33 [[30-39]... 3B]...
            { EscPosCmdType.FsPaperLayoutInformationTransmission, new DecInfo { decodedetail = new DecodeDetail(DecodeFsPaperLayoutInformationTransmission), index = 6 } },                         //  FS  ( L 1C 28 4C 02 00 22 40/50
            { EscPosCmdType.FsTransmitPositioningInformation, null },                                                                                                                               //  FS  ( L 1C 28 4C 02 00 30 30
            { EscPosCmdType.FsFeedPaperLabelPeelingPosition, new DecInfo { decodedetail = new DecodeDetail(DecodeFsFeedPaperLabelPeelingPosition), index = 6 } },                                   //  FS  ( L 1C 28 4C 02 00 41 30/31
            { EscPosCmdType.FsFeedPaperCuttingPosition, new DecInfo { decodedetail = new DecodeDetail(DecodeFsFeedPaperCuttingPosition), index = 6 } },                                             //  FS  ( L 1C 28 4C 02 00 42 30/31
            { EscPosCmdType.FsFeedPaperPrintStartingPosition, new DecInfo { decodedetail = new DecodeDetail(DecodeFsFeedPaperPrintStartingPosition), index = 6 } },                                 //  FS  ( L 1C 28 4C 02 00 43 30-32
            { EscPosCmdType.FsPaperLayoutErrorSpecialMarginSetting, new DecInfo { decodedetail = new DecodeDetail(DecodeFsPaperLayoutErrorSpecialMarginSetting), index = 2 } },                     //  FS  ( L 1C 28 4C 0002-0003 50 30-39 [30-39]
            { EscPosCmdType.FsEnableDisableAutomaticStatusBackOptional, new DecInfo { decodedetail = new DecodeDetail(DecodeFsEnableDisableAutomaticStatusBackOptional), index = 6 } },             //  FS  ( e 1C 28 65 02 00 33 b0000x000
            { EscPosCmdType.FsSelectMICRDataHandling, new DecInfo { decodedetail = new DecodeDetail(DecodeFsSelectMICRDataHandling), index = 3 } },                                                 //c FS  ( f 1C 28 66 0002-FFFF [00-03/30-33 00-FF|00/01/30/31]...
            { EscPosCmdType.FsSelectImageScannerCommandSettings, new DecInfo { decodedetail = new DecodeDetail(DecodeFsSelectImageScannerCommandSettings), index = 6 } },                           //c FS  ( g 1C 28 67 02 00 20 30/31
            { EscPosCmdType.FsSetBasicOperationOfImageScanner, new DecInfo { decodedetail = new DecodeDetail(DecodeFsSetBasicOperationOfImageScanner), index = 7 } },                               //c FS  ( g 1C 28 67 05 00 28 30 01/08 31/32 80-7F
            { EscPosCmdType.FsSetScanningArea, new DecInfo { decodedetail = new DecodeDetail(DecodeFsSetScanningArea), index = 6 } },                                                               //c FS  ( g 1C 28 67 05 00 29 00-62 00-E4 00/02-64 00/02-E6
            { EscPosCmdType.FsSelectCompressionMethodForImageData, new DecInfo { decodedetail = new DecodeDetail(DecodeFsSelectCompressionMethodForImageData), index = 6 } },                       //c FS  ( g 1C 28 67 03 00 32 30-32 30-32
            { EscPosCmdType.FsDeleteCroppingArea, new DecInfo { decodedetail = new DecodeDetail(DecodeFsDeleteCroppingArea), index = 6 } },                                                         //c FS  ( g 1C 28 67 02 00 38 00-0A
            { EscPosCmdType.FsSetCroppingArea, new DecInfo { decodedetail = new DecodeDetail(DecodeFsSetCroppingArea), index = 6 } },                                                               //c FS  ( g 1C 28 67 06 00 39 00-0A 00-64 00-E4 02-64 02-E6
            { EscPosCmdType.FsSelectTransmissionFormatForImageScanningResult, new DecInfo { decodedetail = new DecodeDetail(DecodeFsSelectTransmissionFormatForImageScanningResult), index = 6 } }, //c FS  ( g 1C 28 67 02 00 3C 30-32
            { EscPosCmdType.FsTransmitSettingValueForBasicOperationsOfImageScanner, null },                                                                                                         //c FS  ( g 1C 28 67 02 00 50 30
            { EscPosCmdType.FsTransmitSettingValueOfScanningArea, null },                                                                                                                           //c FS  ( g 1C 28 67 02 00 51 30
            { EscPosCmdType.FsTransmitSettingValueOfCompressionMethdForImageData, null },                                                                                                           //c FS  ( g 1C 28 67 02 00 5A 30
            { EscPosCmdType.FsTransmitSettingValueOfCroppingArea, null },                                                                                                                           //c FS  ( g 1C 28 67 02 00 61 30
            { EscPosCmdType.FsTransmitSettingValueOfTransmissionFormatForImageScanningResult, null },                                                                                               //c FS  ( g 1C 28 67 02 00 64 30
            { EscPosCmdType.FsTurnKanjiUnderlineMode, new DecInfo { decodedetail = new DecodeDetail(DecodeFsTurnKanjiUnderlineMode), index = 2 } },                                                 //  FS  -   1C 2D 00-02/30-32
            { EscPosCmdType.FsCancelKanjiCharacterMode, null },                                                                                                                                     //  FS  .   1C 2E
            { EscPosCmdType.FsDefineUserDefinedKanjiCharacters2424, new DecInfo { decodedetail = new DecodeDetail(DecodeFsDefineUserDefinedKanjiCharacters2424), index = 2 } },                     //  FS  2   1C 32 7721-777E/EC40-EC9E/FEA1-FEFE 00-FF x 72
            { EscPosCmdType.FsDefineUserDefinedKanjiCharacters2024, new DecInfo { decodedetail = new DecodeDetail(DecodeFsDefineUserDefinedKanjiCharacters2024), index = 2 } },                     //  FS  2   1C 32 7721-777E/EC40-EC9E/FEA1-FEFE 00-FF x 60
            { EscPosCmdType.FsDefineUserDefinedKanjiCharacters1616, new DecInfo { decodedetail = new DecodeDetail(DecodeFsDefineUserDefinedKanjiCharacters1616), index = 2 } },                     //  FS  2   1C 32 7721-777E/EC40-EC9E/FEA1-FEFE 00-FF x 32
            { EscPosCmdType.FsCancelUserDefinedKanjiCharacters, new DecInfo { decodedetail = new DecodeDetail(DecodeFsCancelUserDefinedKanjiCharacters), index = 2 } },                             //  FS  ?   1C 3F 7721-777E/EC40-EC9E/FEA1-FEFE
            { EscPosCmdType.FsSelectKanjiCharacterCodeSystem, new DecInfo { decodedetail = new DecodeDetail(DecodeFsSelectKanjiCharacterCodeSystem), index = 2 } },                                 //  FS  C   1C 43 00-02/30-32
            { EscPosCmdType.FsSelectDoubleDensityPageMode, null },                                                                                                                                  //c FS  L   1C 4C
            { EscPosCmdType.FsSetKanjiCharacerSpacing, new DecInfo { decodedetail = new DecodeDetail(DecodeFsSetKanjiCharacerSpacing), index = 2 } },                                               //  FS  S   1C 53 00-FF/00-20 00-FF/00-20
            { EscPosCmdType.FsTurnQuadrupleSizeMode, new DecInfo { decodedetail = new DecodeDetail(DecodeLsbOnOff), index = 2 } },                                                                  //  FS  W   1C 57 bnnnnnnnx
            { EscPosCmdType.FsObsoleteReadCheckPaper, new DecInfo { decodedetail = new DecodeDetail(DecodeFsObsoleteReadCheckPaper), index = 3 } },                                                 //c FS  a 0 1C 61 30 b000000xx
            { EscPosCmdType.FsObsoleteLoadCheckPaperToPrintStartingPosition, null },                                                                                                                //c FS  a 1 1C 61 31
            { EscPosCmdType.FsObsoleteEjectCheckPaper, null },                                                                                                                                      //c FS  a 2 1C 61 32
            { EscPosCmdType.FsObsoleteRequestTransmissionOfCheckPaperReadingResult, null },                                                                                                         //c FS  b  1C 62
            { EscPosCmdType.FsCleanMICRMechanism, null },                                                                                                                                           //c FS  c   1C 63
            { EscPosCmdType.FsObsoleteWriteNVUserMemory, new DecInfo { decodedetail = new DecodeDetail(DecodeFsObsoleteWriteNVUserMemory), index = 4 } },                                           //  FS  g 1 1C 67 31 00 00000000-000003FF 0001-0400 20-FF...
            { EscPosCmdType.FsObsoleteReadNVUserMemory, new DecInfo { decodedetail = new DecodeDetail(DecodeFsObsoleteReadNVUserMemory), index = 4 } },                                             //  FS  g 2 1C 67 32 00 00000000-000003FF 0001-0400
            { EscPosCmdType.FsObsoletePrintNVBitimage, new DecInfo { decodedetail = new DecodeDetail(DecodeFsObsoletePrintNVBitimage), index = 2 } },                                               //  FS  p   1C 70 01-FF 00-03/30-33
            { EscPosCmdType.FsObsoleteDefineNVBitimage, new DecInfo { decodedetail = new DecodeDetail(DecodeFsObsoleteDefineNVBitimage), index = 2 } },                                             //  FS  q   1C 71 01-FF [0001-03FF 0001-0240 00-FF...]...
            { EscPosCmdType.FsUnknown, null },
            // GS 0x1D
            { EscPosCmdType.GsSelectCharacterSize, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSelectCharacterSize), index = 2 } },                                                             //  GS  !   1D 21 b0xxx0yyy
            { EscPosCmdType.GsSetAbsoluteVerticalPrintPositionInPageMode, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt16), index = 2, trailer = " dots" } },                             //  GS  $   1D 24 0000-FFFF
            { EscPosCmdType.GsExecuteTestPrint, new DecInfo { decodedetail = new DecodeDetail(DecodeGsExecuteTestPrint), index = 5 } },                                                                   //  GS  ( A 1D 28 41 02 00 00-02/30-32 01-03/31-33/40
            { EscPosCmdType.GsCustomizeASBStatusBits, new DecInfo { decodedetail = new DecodeDetail(DecodeGsCustomizeASBStatusBits), index = 3 } },                                                       //c GS  ( B 1D 28 42 0002/0003/0005/0007/0009 61 00|[31/33/45/46 2C/2D/37/38]...
            { EscPosCmdType.GsDeleteSpecifiedRecord, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDeleteSpecifiedRecord), index = 6 } },                                                         //  GS  ( C 1D 28 43 05 00 00 00/30 00 20-7E 20-7E
            { EscPosCmdType.GsStoreDataSpecifiedRecord, new DecInfo { decodedetail = new DecodeDetail(DecodeGsStoreDataSpecifiedRecord), index = 3 } },                                                   //  GS  ( C 1D 28 43 0006-FFFF 00 01/31 00 20-7E 20-7E 20-FF...
            { EscPosCmdType.GsTransmitDataSpecifiedRecord, new DecInfo { decodedetail = new DecodeDetail(DecodeGsTransmitDataSpecifiedRecord), index = 6 } },                                             //  GS  ( C 1D 28 43 05 00 00 02/32 00 20-7E 20-7E
            { EscPosCmdType.GsTransmitCapacityNVUserMemory, null },                                                                                                                                       //  GS  ( C 1D 28 43 03 00 00 03/33 00
            { EscPosCmdType.GsTransmitRemainingCapacityNVUserMemory, null },                                                                                                                              //  GS  ( C 1D 28 43 03 00 00 04/34 00
            { EscPosCmdType.GsTransmitKeycodeList, null },                                                                                                                                                //  GS  ( C 1D 28 43 03 00 00 05/35 00
            { EscPosCmdType.GsDeleteAllDataNVMemory, null },                                                                                                                                              //  GS  ( C 1D 28 43 06 00 00 06/36 00 43 4C 52
            { EscPosCmdType.GsEnableDisableRealtimeCommand, new DecInfo { decodedetail = new DecodeDetail(DecodeGsEnableDisableRealtimeCommand), index = 3 } },                                           //  GS  ( D 1D 28 44 0003/0005 14 [01/02 00/01/30/31]...
            { EscPosCmdType.GsChangeIntoUserSettingMode, null },                                                                                                                                          //  GS  ( E 1D 28 45 03 00 01 49 4E
            { EscPosCmdType.GsEndUserSettingMode, null },                                                                                                                                                 //  GS  ( E 1D 28 45 04 00 02 4F 55 54
            { EscPosCmdType.GsChangeMeomorySwitch, new DecInfo { decodedetail = new DecodeDetail(DecodeGsChangeMeomorySwitch), index = 3 } },                                                             //  GS  ( E 1D 28 45 000A-FFFA 03 [01-08 30-32...]...
            { EscPosCmdType.GsTransmitSettingsMemorySwitch, new DecInfo { decodedetail = new DecodeDetail(DecodeGsTransmitSettingsMemorySwitch), index = 6 } },                                           //  GS  ( E 1D 28 45 02 00 04 01-08
            { EscPosCmdType.GsSetCustomizeSettingValues, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetCustomizeSettingValues), index = 3 } },                                                 //  GS  ( E 1D 28 45 0004-FFFD 05 [01-03/05-0D/14-16/46-48/61/62/64-69/6F/70/74-C2 0000-FFFF]...
            { EscPosCmdType.GsTransmitCustomizeSettingValues, new DecInfo { decodedetail = new DecodeDetail(DecodeGsTransmitCustomizeSettingValues), index = 6 } },                                       //  GS  ( E 1D 28 45 02 00 06 01-03/05-0D/14-16/46-48/61/62/64-69/6F-71/74-C1
            { EscPosCmdType.GsCopyUserDefinedPage, new DecInfo { decodedetail = new DecodeDetail(DecodeGsCopyUserDefinedPage), index = 6 } },                                                             //  GS  ( E 1D 28 45 04 00 07 0A/0C/11/12 1D/1E 1E/1D
            { EscPosCmdType.GsDefineColumnFormatCharacterCodePage, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDefineColumnFormatCharacterCodePage), index = 3 } },                             //  GS  ( E 1D 28 45 0005-FFFF 08 02/03 20-7E 20-7E [08/09/0A/0C 00-FF...]...
            { EscPosCmdType.GsDefineRasterFormatCharacterCodePage, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDefineRasterFormatCharacterCodePage), index = 3 } },                             //  GS  ( E 1D 28 45 0005-FFFF 09 01/02 20-7E 20-7E [10/11/18 00-FF...]...
            { EscPosCmdType.GsDeleteCharacterCodePage, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDeleteCharacterCodePage), index = 6 } },                                                     //  GS  ( E 1D 28 45 03 00 0A 80-FF 80-FF
            { EscPosCmdType.GsSetSerialInterface, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetSerialInterface), index = 3 } },                                                               //  GS  ( E 1D 28 45 0003-0008 0B 01-04 30-39...
            { EscPosCmdType.GsTransmitSerialInterface, new DecInfo { decodedetail = new DecodeDetail(DecodeGsTransmitSerialInterface), index = 6 } },                                                     //  GS  ( E 1D 28 45 02 00 0C 01-04
            { EscPosCmdType.GsSetBluetoothInterface, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetBluetoothInterface), index = 3 } },                                                         //  GS  ( E 1D 28 45 0003-0021 0D [31/41/46/49 20-7E...]...
            { EscPosCmdType.GsTransmitBluetoothInterface, new DecInfo { decodedetail = new DecodeDetail(DecodeGsTransmitBluetoothInterface), index = 6 } },                                               //  GS  ( E 1D 28 45 02 00 0E 30/31/41/46/49
            { EscPosCmdType.GsSetUSBInterface, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetUSBInterface), index = 6 } },                                                                     //  GS  ( E 1D 28 45 03 00 0F 01/20 30/31
            { EscPosCmdType.GsTransmitUSBInterface, new DecInfo { decodedetail = new DecodeDetail(DecodeGsTransmitUSBInterface), index = 6 } },                                                           //  GS  ( E 1D 28 45 02 00 10 01/20
            { EscPosCmdType.GsDeletePaperLayout, null },                                                                                                                                                  //  GS  ( E 1D 28 45 04 00 30 43 4C 52
            { EscPosCmdType.GsSetPaperLayout, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetPaperLayout), index = 3 } },                                                                       //  GS  ( E 1D 28 45 0009-0024 31 {34 38/34 39/36 34} 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B
            { EscPosCmdType.GsTransmitPaperLayout, new DecInfo { decodedetail = new DecodeDetail(DecodeGsTransmitPaperLayout), index = 6 } },                                                             //  GS  ( E 1D 28 45 02 00 32 40/50
            { EscPosCmdType.GsSetControlLabelPaperAndBlackMarks, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetControlLabelPaperAndBlackMarks), index = 3 } },                                 //  GS  ( E 1D 28 45 002D-0048 33 20-7E...
            { EscPosCmdType.GsTransmitControlSettingsLabelPaperAndBlackMarks, new DecInfo { decodedetail = new DecodeDetail(DecodeGsTransmitControlSettingsLabelPaperAndBlackMarks), index = 6 } },       //  GS  ( E 1D 28 45 11 00 34 20-7E... 00
            { EscPosCmdType.GsSetInternalBuzzerPatterns, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetInternalBuzzerPatterns), index = 3 } },                                                 //  GS  ( E 1D 28 45 000E-FFFF 63 [01-05 [00/01 00-64]...]...
            { EscPosCmdType.GsTransmitInternalBuzzerPatterns, new DecInfo { decodedetail = new DecodeDetail(DecodeGsTransmitInternalBuzzerPatterns), index = 6 } },                                       //  GS  ( E 1D 28 45 02 00 64 01-05
            { EscPosCmdType.GsTransmitStatusOfCutSheet, null },                                                                                                                                           //c GS  ( G 1D 28 47 02 00 20 30
            { EscPosCmdType.GsSelectSideOfSlipFaceOrBack, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSelectSideOfSlipFaceOrBack), index = 6 } },                                               //c GS  ( G 1D 28 47 02 00 30 04/44
            { EscPosCmdType.GsReadMagneticInkCharacterAndTransmitReadingResult, new DecInfo { decodedetail = new DecodeDetail(DecodeGsReadMagneticInkCharacterAndTransmitReadingResult), index = 8 } },   //c GS  ( G 1D 28 47 04 00 3C 01 00 00/01
            { EscPosCmdType.GsRetransmitMagneticInkCharacterReadingResult, null },                                                                                                                        //c GS  ( G 1D 28 47 03 00 3D 01 00
            { EscPosCmdType.GsReadDataAndTransmitResultingInformation, new DecInfo { decodedetail = new DecodeDetail(DecodeGsReadDataAndTransmitResultingInformation), index = 3 } },                     //c GS  ( G 1D 28 47 0005-0405 40 0000-FFFF 30 01-03 00/01 30 0000 00-FF...
            { EscPosCmdType.GsScanImageDataAndTransmitImageScanningResult, new DecInfo { decodedetail = new DecodeDetail(DecodeGsScanImageDataAndTransmitImageScanningResult), index = 3 } },             //c GS  ( G 1D 28 47 0005-0405 41 0001-FFFF 30/31 30 00-FF...
            { EscPosCmdType.GsRetransmitImageScanningResult, new DecInfo { decodedetail = new DecodeDetail(DecodeGsRetransmitImageScanningResult), index = 3 } },                                         //c GS  ( G 1D 28 47 0003-0004 42 0001-FFFF [30/31]
            { EscPosCmdType.GsExecutePreScan, null },                                                                                                                                                     //c GS  ( G 1D 28 47 02 00 43 30
            { EscPosCmdType.GsDeleteImageScanningResultWithSpecifiedDataID, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDeleteImageScanningResultWithSpecifiedDataID), index = 7 } },           //c GS  ( G 1D 28 47 04 00 44 30 0001-FFFF
            { EscPosCmdType.GsDeleteAllImageScanningResult, null },                                                                                                                                       //c GS  ( G 1D 28 47 05 00 45 30 43 4C 52
            { EscPosCmdType.GsTransmitDataIDListOfImageScanningResult, null },                                                                                                                            //c GS  ( G 1D 28 47 04 00 46 30 49 44
            { EscPosCmdType.GsTransmitRemainingCapacityOfNVMemoryForImageDataStorage, null },                                                                                                             //c GS  ( G 1D 28 47 02 00 47 30
            { EscPosCmdType.GsSelectActiveSheet, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSelectActiveSheet), index = 6 } },                                                                 //c GS  ( G 1D 28 47 02 00 50 b00xxxxxx
            { EscPosCmdType.GsStartPreProcessForCutSheetInsertion, null },                                                                                                                                //c GS  ( G 1D 28 47 02 00 51 30
            { EscPosCmdType.GsEndPreProcessForCutSheetInsertion, null },                                                                                                                                  //c GS  ( G 1D 28 47 02 00 52 30
            { EscPosCmdType.GsExecuteWaitingProcessForCutSheetInsertion, null },                                                                                                                          //c GS  ( G 1D 28 47 02 00 53 30
            { EscPosCmdType.GsFeedToPrintStartingPositionForSlip, null },                                                                                                                                 //c GS  ( G 1D 28 47 02 00 54 01
            { EscPosCmdType.GsFinishProcessingOfCutSheet, new DecInfo { decodedetail = new DecodeDetail(DecodeGsFinishProcessingOfCutSheet), index = 6 } },                                               //c GS  ( G 1D 28 47 02 00 55 30/31
            { EscPosCmdType.GsSpecifiesProcessIDResponse, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSpecifiesProcessIDResponse), index = 7 } },                                               //  GS  ( H 1D 28 48 06 00 30 30 20-7E 20-7E 20-7E 20-7E
            { EscPosCmdType.GsSpecifiesOfflineResponse, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSpecifiesOfflineResponse), index = 7 } },                                                   //  GS  ( H 1D 28 48 03 00 31 30 00-02/30-32
            { EscPosCmdType.GsSelectPrintControlMode, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSelectPrintControlMode), index = 6 } },                                                       //  GS  ( K 1D 28 4B 02 00 30 00-04/30-34
            { EscPosCmdType.GsSelectPrintDensity, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSelectPrintDensity), index = 6 } },                                                               //  GS  ( K 1D 28 4B 02 00 31 80-7F
            { EscPosCmdType.GsSelectPrintSpeed, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSelectPrintSpeed), index = 6 } },                                                                   //  GS  ( K 1D 28 4B 02 00 32 00-0D/30-39
            { EscPosCmdType.GsSelectNumberOfPartsThermalHeadEnergizing, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSelectNumberOfPartsThermalHeadEnergizing), index = 6 } },                   //  GS  ( K 1D 28 4B 02 00 61 00-04/30-34/80
            { EscPosCmdType.GsDefineNVGraphicsDataRasterW, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDefineNVGraphicsDataRasterW), index = 3 } },                                             //  GS  ( L 1D 28 4C 000C-FFFF 30 43 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
            { EscPosCmdType.GsDefineNVGraphicsDataRasterDW, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDefineNVGraphicsDataRasterDW), index = 3 } },                                           //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 43 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
            { EscPosCmdType.GsDefineNVGraphicsDataColumnW, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDefineNVGraphicsDataColumnW), index = 3 } },                                             //  GS  ( L 1D 28 4C 000C-FFFF 30 44 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
            { EscPosCmdType.GsDefineNVGraphicsDataColumnDW, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDefineNVGraphicsDataColumnDW), index = 3 } },                                           //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 44 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
            { EscPosCmdType.GsDefineDownloadGraphicsDataRasterW, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDefineDownloadGraphicsDataRasterW), index = 3 } },                                 //  GS  ( L 1D 28 4C 000C-FFFF 30 53 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
            { EscPosCmdType.GsDefineDownloadGraphicsDataRasterDW, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDefineDownloadGraphicsDataRasterDW), index = 3 } },                               //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 53 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
            { EscPosCmdType.GsDefineDownloadGraphicsDataColumnW, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDefineDownloadGraphicsDataColumnW), index = 3 } },                                 //  GS  ( L 1D 28 4C 000C-FFFF 30 54 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
            { EscPosCmdType.GsDefineDownloadGraphicsDataColumnDW, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDefineDownloadGraphicsDataColumnDW), index = 3 } },                               //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 54 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
            { EscPosCmdType.GsStoreGraphicsDataToPrintBufferRasterW, new DecInfo { decodedetail = new DecodeDetail(DecodeGsStoreGraphicsDataToPrintBufferRasterW), index = 3 } },                         //  GS  ( L 1D 28 4C 000B-FFFF 30 70 30/34 01/02 01/02 31-34 0001-0960 0001-0960 00-FF...
            { EscPosCmdType.GsStoreGraphicsDataToPrintBufferRasterDW, new DecInfo { decodedetail = new DecodeDetail(DecodeGsStoreGraphicsDataToPrintBufferRasterDW), index = 3 } },                       //  GS  8 L 1D 38 4C 0000000B-FFFFFFFF 30 70 30/34 01/02 01/02 31-34 0001-0960 0001-0960 00-FF...
            { EscPosCmdType.GsStoreGraphicsDataToPrintBufferColumnW, new DecInfo { decodedetail = new DecodeDetail(DecodeGsStoreGraphicsDataToPrintBufferColumnW), index = 3 } },                         //  GS  ( L 1D 28 4C 000B-FFFF 30 71 30 01/02 01/02 31-33 0001-0800 0001-0080 00-FF...
            { EscPosCmdType.GsStoreGraphicsDataToPrintBufferColumnDW, new DecInfo { decodedetail = new DecodeDetail(DecodeGsStoreGraphicsDataToPrintBufferColumnDW), index = 3 } },                       //  GS  8 L 1D 38 4C 0000000B-FFFFFFFF 30 71 30 01/02 01/02 31-33 0001-0800 0001-0080 00-FF...
            { EscPosCmdType.GsTransmitNVGraphicsMemoryCapacity, null },                                                                                                                                   //  GS  ( L 1D 28 4C 02 00 30 00/30
            { EscPosCmdType.GsSetReferenceDotDensityGraphics, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetReferenceDotDensityGraphics), index = 6 } },                                       //  GS  ( L 1D 28 4C 04 00 30 01/31 32/33 32/33
            { EscPosCmdType.GsPrintGraphicsDataInPrintBuffer, null },                                                                                                                                     //  GS  ( L 1D 28 4C 02 00 30 02/32
            { EscPosCmdType.GsTransmitRemainingCapacityNVGraphicsMemory, null },                                                                                                                          //  GS  ( L 1D 28 4C 02 00 30 03/33
            { EscPosCmdType.GsTransmitRemainingCapacityDownloadGraphicsMemory, null },                                                                                                                    //  GS  ( L 1D 28 4C 02 00 30 04/34
            { EscPosCmdType.GsTransmitKeycodeListDefinedNVGraphics, null },                                                                                                                               //  GS  ( L 1D 28 4C 04 00 30 40 4B 43
            { EscPosCmdType.GsDeleteAllNVGraphicsData, null },                                                                                                                                            //  GS  ( L 1D 28 4C 05 00 30 41 43 4C 52
            { EscPosCmdType.GsDeleteSpecifiedNVGraphicsData, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDeleteSpecifiedNVGraphicsData), index = 7 } },                                         //  GS  ( L 1D 28 4C 04 00 30 42 20-7E 20-7E
            { EscPosCmdType.GsPrintSpecifiedNVGraphicsData, new DecInfo { decodedetail = new DecodeDetail(DecodeGsPrintSpecifiedNVGraphicsData), index = 7 } },                                           //  GS  ( L 1D 28 4C 06 00 30 45 20-7E 20-7E 01/02 01/02
            { EscPosCmdType.GsTransmitKeycodeListDefinedDownloadGraphics, null },                                                                                                                         //  GS  ( L 1D 28 4C 04 00 30 50 4B 43
            { EscPosCmdType.GsDeleteAllDownloadGraphicsData, null },                                                                                                                                      //  GS  ( L 1D 28 4C 05 00 30 51 43 4C 52
            { EscPosCmdType.GsDeleteSpecifiedDownloadGraphicsData, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDeleteSpecifiedDownloadGraphicsData), index = 7 } },                             //  GS  ( L 1D 28 4C 04 00 30 52 20-7E 20-7E
            { EscPosCmdType.GsPrintSpecifiedDownloadGraphicsData, new DecInfo { decodedetail = new DecodeDetail(DecodeGsPrintSpecifiedDownloadGraphicsData), index = 7 } },                               //  GS  ( L 1D 28 4C 06 00 30 55 20-7E 20-7E 01/02 01/02
            { EscPosCmdType.GsSaveSettingsValuesFromWorkToStorage, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSaveSettingsValuesFromWorkToStorage), index = 5 } },                             //  GS  ( M 1D 28 4D 02 00 01/31 01/31
            { EscPosCmdType.GsLoadSettingsValuesFromStorageToWork, new DecInfo { decodedetail = new DecodeDetail(DecodeGsLoadSettingsValuesFromStorageToWork), index = 5 } },                             //  GS  ( M 1D 28 4D 02 00 02/32 00/01/30/31
            { EscPosCmdType.GsSelectSettingsValuesToWorkAfterInitialize, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSelectSettingsValuesToWorkAfterInitialize), index = 5 } },                 //  GS  ( M 1D 28 4D 02 00 03/33 00/01/30/31
            { EscPosCmdType.GsSetCharacterColor, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetCharacterColor), index = 6 } },                                                                 //  GS  ( N 1D 28 4E 02 00 30 30-33
            { EscPosCmdType.GsSetBackgroundColor, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetBackgroundColor), index = 6 } },                                                               //  GS  ( N 1D 28 4E 02 00 31 30-33
            { EscPosCmdType.GsTurnShadingMode, new DecInfo { decodedetail = new DecodeDetail(DecodeGsTurnShadingMode), index = 6 } },                                                                     //  GS  ( N 1D 28 4E 03 00 32 00/01/30/31 30-33
            { EscPosCmdType.GsSetPrinttableArea, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetPrinttableArea), index = 6 } },                                                                 //  GS  ( P 1D 28 50 08 00 30 FFFF 0001-FFFF 0000 01
            { EscPosCmdType.GsDrawLineInPageMode, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDrawLineInPageMode), index = 6 } },                                                               //  GS  ( Q 1D 28 51 0C 00 30 0000-FFFF 0000-FFFF 0000-FFFF 0000-FFFF 01 01-06 30
            { EscPosCmdType.GsDrawRectangleInPageMode, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDrawRectangleInPageMode), index = 6 } },                                                     //  GS  ( Q 1D 28 51 0E 00 31 0000-FFFF 0000-FFFF 0000-FFFF 0000-FFFF 01 01-06 30 30 01
            { EscPosCmdType.GsDrawHorizontalLineInStandardMode, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDrawHorizontalLineInStandardMode), index = 6 } },                                   //  GS  ( Q 1D 28 51 09 00 32 0000-023F 0000-023F 01-FF 01 01-06 30
            { EscPosCmdType.GsDrawVerticalLineInStandardMode, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDrawVerticalLineInStandardMode), index = 6 } },                                       //  GS  ( Q 1D 28 51 07 00 33 0000-023F 00/01 01 01-06 30
            { EscPosCmdType.GsPDF417SetNumberOfColumns, new DecInfo { decodedetail = new DecodeDetail(DecodeGsPDF417SetNumberOfColumns), index = 7 } },                                                   //  GS  ( k 1D 28 6B 03 00 30 41 00-1E
            { EscPosCmdType.GsPDF417SetNumberOfRows, new DecInfo { decodedetail = new DecodeDetail(DecodeGsPDF417SetNumberOfRows), index = 7 } },                                                         //  GS  ( k 1D 28 6B 03 00 30 42 00/3-5A
            { EscPosCmdType.GsPDF417SetWidthOfModule, new DecInfo { decodedetail = new DecodeDetail(DecodeGsPDF417SetWidthOfModule), index = 7 } },                                                       //  GS  ( k 1D 28 6B 03 00 30 43 01-08
            { EscPosCmdType.GsPDF417SetRowHeight, new DecInfo { decodedetail = new DecodeDetail(DecodeGsPDF417SetRowHeight), index = 7 } },                                                               //  GS  ( k 1D 28 6B 03 00 30 44 02-08
            { EscPosCmdType.GsPDF417SetErrorCollectionLevel, new DecInfo { decodedetail = new DecodeDetail(DecodeGsPDF417SetErrorCollectionLevel), index = 7 } },                                         //  GS  ( k 1D 28 6B 04 00 30 45 30/31 30-38/00-28
            { EscPosCmdType.GsPDF417SelectOptions, new DecInfo { decodedetail = new DecodeDetail(DecodeGsPDF417SelectOptions), index = 7 } },                                                             //  GS  ( k 1D 28 6B 03 00 30 46 00/01
            { EscPosCmdType.GsPDF417StoreData, new DecInfo { decodedetail = new DecodeDetail(DecodeGsPDF417StoreData), index = 3 } },                                                                     //  GS  ( k 1D 28 6B 0004-FFFF 30 50 30 00-FF...
            { EscPosCmdType.GsPDF417PrintSymbolData, null },                                                                                                                                              //  GS  ( k 1D 28 6B 03 00 30 51 30
            { EscPosCmdType.GsPDF417TransmitSizeInformation, null },                                                                                                                                      //  GS  ( k 1D 28 6B 03 00 30 52 30
            { EscPosCmdType.GsQRCodeSelectModel, new DecInfo { decodedetail = new DecodeDetail(DecodeGsQRCodeSelectModel), index = 7 } },                                                                 //  GS  ( k 1D 28 6B 04 00 31 41 31-33 00
            { EscPosCmdType.GsQRCodeSetSizeOfModule, new DecInfo { decodedetail = new DecodeDetail(DecodeGsQRCodeSetSizeOfModule), index = 7 } },                                                         //  GS  ( k 1D 28 6B 03 00 31 43 01-10
            { EscPosCmdType.GsQRCodeSetErrorCollectionLevel, new DecInfo { decodedetail = new DecodeDetail(DecodeGsQRCodeSetErrorCollectionLevel), index = 7 } },                                         //  GS  ( k 1D 28 6B 03 00 31 45 30-33
            { EscPosCmdType.GsQRCodeStoreData, new DecInfo { decodedetail = new DecodeDetail(DecodeGsQRCodeStoreData), index = 3 } },                                                                     //  GS  ( k 1D 28 6B 0004-1BB4 31 50 30 00-FF...
            { EscPosCmdType.GsQRCodePrintSymbolData, null },                                                                                                                                              //  GS  ( k 1D 28 6B 03 00 31 51 30
            { EscPosCmdType.GsQRCodeTransmitSizeInformation, null },                                                                                                                                      //  GS  ( k 1D 28 6B 03 00 31 52 30
            { EscPosCmdType.GsMaxiCodeSelectMode, new DecInfo { decodedetail = new DecodeDetail(DecodeGsMaxiCodeSelectMode), index = 7 } },                                                               //  GS  ( k 1D 28 6B 03 00 32 41 32-36
            { EscPosCmdType.GsMaxiCodeStoreData, new DecInfo { decodedetail = new DecodeDetail(DecodeGsMaxiCodeStoreData), index = 3 } },                                                                 //  GS  ( k 1D 28 6B 0004-008D 32 50 30 00-FF...
            { EscPosCmdType.GsMaxiCodePrintSymbolData, null },                                                                                                                                            //  GS  ( k 1D 28 6B 03 00 32 51 30
            { EscPosCmdType.GsMaxiCodeTransmitSizeInformation, null },                                                                                                                                    //  GS  ( k 1D 28 6B 03 00 32 52 30
            { EscPosCmdType.GsD2GS1DBSetWidthOfModule, new DecInfo { decodedetail = new DecodeDetail(DecodeGsD2GS1DBSetWidthOfModule), index = 7 } },                                                     //  GS  ( k 1D 28 6B 03 00 33 43 02-08
            { EscPosCmdType.GsD2GS1DBSetExpandStackedMaximumWidth, new DecInfo { decodedetail = new DecodeDetail(DecodeGsD2GS1DBSetExpandStackedMaximumWidth), index = 7 } },                             //  GS  ( k 1D 28 6B 04 00 33 47 0000/006A-0F70
            { EscPosCmdType.GsD2GS1DBStoreData, new DecInfo { decodedetail = new DecodeDetail(DecodeGsD2GS1DBStoreData), index = 3 } },                                                                   //  GS  ( k 1D 28 6B 0006-0103 33 50 30 20-22/25-2F/30-39/3A-3F/41-5A/61-7A...
            { EscPosCmdType.GsD2GS1DBPrintSymbolData, null },                                                                                                                                             //  GS  ( k 1D 28 6B 03 00 33 51 30
            { EscPosCmdType.GsD2GS1DBTransmitSizeInformation, null },                                                                                                                                     //  GS  ( k 1D 28 6B 03 00 33 52 30
            { EscPosCmdType.GsCompositeSetWidthOfModule, new DecInfo { decodedetail = new DecodeDetail(DecodeGsCompositeSetWidthOfModule), index = 7 } },                                                 //  GS  ( k 1D 28 6B 03 00 34 43 02-08
            { EscPosCmdType.GsCompositeSetExpandStackedMaximumWidth, new DecInfo { decodedetail = new DecodeDetail(DecodeGsCompositeSetExpandStackedMaximumWidth), index = 7 } },                         //  GS  ( k 1D 28 6B 04 00 34 47 0000/006A-0F70
            { EscPosCmdType.GsCompositeSelectHRICharacterFont, new DecInfo { decodedetail = new DecodeDetail(DecodeGsCompositeSelectHRICharacterFont), index = 7 } },                                     //  GS  ( k 1D 28 6B 03 00 34 48 00-05/30-35/61/62
            { EscPosCmdType.GsCompositeStoreData, new DecInfo { decodedetail = new DecodeDetail(DecodeGsCompositeStoreData), index = 3 } },                                                               //  GS  ( k 1D 28 6B 0006-093E 34 50 30 00-FF...
            { EscPosCmdType.GsCompositePrintSymbolData, null },                                                                                                                                           //  GS  ( k 1D 28 6B 03 00 34 51 30
            { EscPosCmdType.GsCompositeTransmitSizeInformation, null },                                                                                                                                   //  GS  ( k 1D 28 6B 03 00 34 52 30
            { EscPosCmdType.GsAztecCodeSetModeTypesAndDataLayer, new DecInfo { decodedetail = new DecodeDetail(DecodeGsAztecCodeSetModeTypesAndDataLayer), index = 7 } },                                 //  GS  ( k 1D 28 6B 04 00 35 42 00/01/30/31 00-20
            { EscPosCmdType.GsAztecCodeSetSizeOfModule, new DecInfo { decodedetail = new DecodeDetail(DecodeGsAztecCodeSetSizeOfModule), index = 7 } },                                                   //  GS  ( k 1D 28 6B 03 00 35 43 02-10
            { EscPosCmdType.GsAztecCodeSetErrorCollectionLevel, new DecInfo { decodedetail = new DecodeDetail(DecodeGsAztecCodeSetErrorCollectionLevel), index = 7 } },                                   //  GS  ( k 1D 28 6B 03 00 35 45 05-5F
            { EscPosCmdType.GsAztecCodeStoreData, new DecInfo { decodedetail = new DecodeDetail(DecodeGsAztecCodeStoreData), index = 3 } },                                                               //  GS  ( k 1D 28 6B 0004-0EFB 35 50 30 00-FF...
            { EscPosCmdType.GsAztecCodePrintSymbolData, null },                                                                                                                                           //  GS  ( k 1D 28 6B 03 00 35 51 30
            { EscPosCmdType.GsAztecCodeTransmitSizeInformation, null },                                                                                                                                   //  GS  ( k 1D 28 6B 03 00 35 52 30
            { EscPosCmdType.GsDataMatrixSetSymbolTypeColumnsRows, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDataMatrixSetSymbolTypeColumnsRows), index = 7 } },                               //  GS  ( k 1D 28 6B 05 00 36 42 00/01/30/31 00-90 00-90
            { EscPosCmdType.GsDataMatrixSetSizeOfModule, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDataMatrixSetSizeOfModule), index = 7 } },                                                 //  GS  ( k 1D 28 6B 03 00 36 43 02-10
            { EscPosCmdType.GsDataMatrixStoreData, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDataMatrixStoreData), index = 3 } },                                                             //  GS  ( k 1D 28 6B 0004-0C2F 36 50 30 00-FF...
            { EscPosCmdType.GsDataMatrixPrintSymbolData, null },                                                                                                                                          //  GS  ( k 1D 28 6B 03 00 36 51 30
            { EscPosCmdType.GsDataMatrixTransmitSizeInformation, null },                                                                                                                                  //  GS  ( k 1D 28 6B 03 00 36 52 30
            { EscPosCmdType.GsSetReadOperationsOfCheckPaper, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetReadOperationsOfCheckPaper), index = 3 } },                                         //c GS  ( z 1D 28 7A 0003-FFFF 2A [3C/40-42/46 00/01/30/31]...
            { EscPosCmdType.GsSetsCancelsOperationToFeedCutSheetsToPrintStartingPosition, null },                                                                                                         //c GS  ( z 1D 28 7A 02 00 30 30
            { EscPosCmdType.GsStartSavingReverseSidePrintData, null },                                                                                                                                    //c GS  ( z 1D 28 7A 02 00 3E 30
            { EscPosCmdType.GsFinishSavingReverseSidePrintData, null },                                                                                                                                   //c GS  ( z 1D 28 7A 02 00 3E 31
            { EscPosCmdType.GsSetCounterForReverseSidePrint, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetCounterForReverseSidePrint), index = 7 } },                                         //c GS  ( z 1D 28 7A 0C 00 3E 33 01-09 00000000-3B9ACAFF 00/20/30 00000000-3B9ACAFF
            { EscPosCmdType.GsReadCheckDataContinuouslyAndTransmitDataRead, null },                                                                                                                       //c GS  ( z 1D 28 7A 09 00 3F 01 00 30 00-03 00 30 00 00
            { EscPosCmdType.GsObsoleteDefineDownloadedBitimage, new DecInfo { decodedetail = new DecodeDetail(DecodeGsObsoleteDefineDownloadedBitimage), index = 2 } },                                   //  GS  *   1D 2A 01-FF 01-FF 00-FF...
            { EscPosCmdType.GsObsoletePrintDownloadedBitimage, new DecInfo { decodedetail = new DecodeDetail(DecodeGsObsoletePrintDownloadedBitimage), index = 2 } },                                     //  GS  /   1D 2F 00-03/30-33
            { EscPosCmdType.GsStartEndMacroDefinition, null },                                                                                                                                            //  GS  :   1D 3A
            { EscPosCmdType.GsTurnWhiteBlackReversePrintMode, new DecInfo { decodedetail = new DecodeDetail(DecodeLsbOnOff), index = 2 } },                                                               //  GS  B   1D 42 bnnnnnnnx
            { EscPosCmdType.GsObsoleteSelectCounterPrintMode, new DecInfo { decodedetail = new DecodeDetail(DecodeGsObsoleteSelectCounterPrintMode), index = 3 } },                                       //  GS  C 0 1D 43 30 00-05 00-02/30-32
            { EscPosCmdType.GsObsoleteSelectCounterModeA, new DecInfo { decodedetail = new DecodeDetail(DecodeGsObsoleteSelectCounterModeA), index = 3 } },                                               //  GS  C 1 1D 43 31 0000-FFFF 0000-FFFF 00-FF 00-FF
            { EscPosCmdType.GsObsoleteSetCounter, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt16), index = 3 } },                                                                       //  GS  C 2 1D 43 32 0000-FFFF
            { EscPosCmdType.GsObsoleteSelectCounterModeB, new DecInfo { decodedetail = new DecodeDetail(DecodeGsObsoleteSelectCounterModeB), index = 3 } },                                               //  GS  C ; 1D 43 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B
            { EscPosCmdType.GsDefineWindowsBMPNVGraphicsData, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDefineWindowsBMPNVGraphicsData), index = 5 } },                                       //  GS  D   1D 44 30 43 30 20-7E 20-7E 30/34 31 42 4D 00000042-FFFFFFFF 00-FF...
            { EscPosCmdType.GsDefineWindowsBMPDownloadGraphicsData, new DecInfo { decodedetail = new DecodeDetail(DecodeGsDefineWindowsBMPDownloadGraphicsData), index = 5 } },                           //  GS  D   1D 44 30 53 30 20-7E 20-7E 30/34 31 42 4D 00000042-FFFFFFFF 00-FF...
            { EscPosCmdType.GsObsoleteSelectHeadControlMethod, new DecInfo { decodedetail = new DecodeDetail(DecodeGsObsoleteSelectHeadControlMethod), index = 2 } },                                     //c GS  E   1D 45 b000x0x0x
            { EscPosCmdType.GsSelectPrintPositionHRICharacters, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSelectPrintPositionHRICharacters), index = 2 } },                                   //  GS  H   1D 48 00-03/30-33
            { EscPosCmdType.GsTransmitPrinterID, new DecInfo { decodedetail = new DecodeDetail(DecodeGsTransmitPrinterID), index = 2 } },                                                                 //  GS  I   1D 49 01-03/31-33/21/23/24/41-45/60/6E-70
            { EscPosCmdType.GsSetLeftMargin, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt16), index = 2, trailer = " dots" } },                                                          //  GS  L   1D 4C 0000-FFFF
            { EscPosCmdType.GsSetHorizontalVerticalMotionUnits, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetHorizontalVerticalMotionUnits), index = 2 } },                                   //  GS  P   1D 50 00-FF 00-FF
            { EscPosCmdType.GsObsoletePrintVariableVerticalSizeBitimage, new DecInfo { decodedetail = new DecodeDetail(DecodeGsObsoletePrintVariableVerticalSizeBitimage), index = 3 } },                 //  GS  Q 0 1D 51 30 00-03/30-33 0001-10A0 0001-0010 00-FF...
            { EscPosCmdType.GsSetPrintPositionBeginningOfPrintLine, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetPrintPositionBeginningOfPrintLine), index = 2 } },                           //  GS  T   1D 54 00/01/30/31
            { EscPosCmdType.GsPaperFullCut, null },                                                                                                                                                       //  GS  V   1D 56 00/30
            { EscPosCmdType.GsPaperPartialCut, null },                                                                                                                                                    //  GS  V   1D 56 01/31
            { EscPosCmdType.GsPaperFeedAndFullCut, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt08), index = 2, trailer = " dots" } },                                                    //  GS  V   1D 56 41 00-FF
            { EscPosCmdType.GsPaperFeedAndPartialCut, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt08), index = 2, trailer = " dots" } },                                                 //  GS  V   1D 56 42 00-FF
            { EscPosCmdType.GsPaperReservedFeedAndFullCut, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt08), index = 2, trailer = " dots" } },                                            //  GS  V   1D 56 61 00-FF
            { EscPosCmdType.GsPaperReservedFeedAndPartialCut, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt08), index = 2, trailer = " dots" } },                                         //  GS  V   1D 56 62 00-FF
            { EscPosCmdType.GsPaperFeedAndFullCutAndTopOfForm, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt08), index = 2, trailer = " dots" } },                                        //  GS  V   1D 56 67 00-FF
            { EscPosCmdType.GsPaperFeedAndPartialCutAndTopOfForm, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt08), index = 2, trailer = " dots" } },                                     //  GS  V   1D 56 68 00-FF
            { EscPosCmdType.GsSetPrintAreaWidth, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt16), index = 2, trailer = " dots" } },                                                      //  GS  W   1D 57 0000-FFFF
            { EscPosCmdType.GsSetRelativeVerticalPrintPosition, new DecInfo { decodedetail = new DecodeDetail(DecodeValueSInt16), index = 2, trailer = " dots" } },                                       //  GS  \   1D 5C 8000-7FFF
            { EscPosCmdType.GsExecuteMacro, new DecInfo { decodedetail = new DecodeDetail(DecodeGsExecuteMacro), index = 2 } },                                                                           //  GS  ^   1D 5E 01-FF 00-FF 00/01
            { EscPosCmdType.GsEnableDisableAutomaticStatusBack, new DecInfo { decodedetail = new DecodeDetail(DecodeGsEnableDisableAutomaticStatusBack), index = 2 } },                                   //  GS  a   1D 61 b0x00xxxx
            { EscPosCmdType.GsTurnSmoothingMode, new DecInfo { decodedetail = new DecodeDetail(DecodeLsbOnOff), index = 2 } },                                                                            //  GS  b   1D 62 bnnnnnnnx
            { EscPosCmdType.GsObsoletePrintCounter, null },                                                                                                                                               //  GS  c   1D 63
            { EscPosCmdType.GsSelectFontHRICharacters, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSelectFontHRICharacters), index = 2 } },                                                     //  GS  f   1D 66 00-04/30-34/61/62
            { EscPosCmdType.GsInitializeMaintenanceCounter, new DecInfo { decodedetail = new DecodeDetail(DecodeGsInitializeMaintenanceCounter), index = 4 } },                                           //  GS  g 0 1D 67 30 00 000A-004F
            { EscPosCmdType.GsTransmitMaintenanceCounter, new DecInfo { decodedetail = new DecodeDetail(DecodeGsTransmitMaintenanceCounter), index = 4 } },                                               //  GS  g 2 1D 67 32 00 000A-004F/008A-00CF
            { EscPosCmdType.GsSetBarcodeHight, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt08), index = 2, trailer = " dots" } },                                                        //  GS  h   1D 68 01-FF
            { EscPosCmdType.GsEnableDisableAutomaticStatusBackInk, new DecInfo { decodedetail = new DecodeDetail(DecodeGsEnableDisableAutomaticStatusBackInk), index = 2 } },                             //  GS  j   1D 6A b000000xx
            { EscPosCmdType.GsPrintBarcodeAsciiz, new DecInfo { decodedetail = new DecodeDetail(DecodeGsPrintBarcodeAsciiz), index = 2 } },                                                               //  GS  k   1D 6B 00-06 20/24/25/2A/2B/2D-2F/30-39/41-5A/61-64... 00
            { EscPosCmdType.GsPrintBarcodeSpecifiedLength, new DecInfo { decodedetail = new DecodeDetail(DecodeGsPrintBarcodeSpecifiedLength), index = 2 } },                                             //  GS  k   1D 6B 41-4F 01-FF 00-FF...
            { EscPosCmdType.GsTransmitStatus, new DecInfo { decodedetail = new DecodeDetail(DecodeGsTransmitStatus), index = 2 } },                                                                       //  GS  r   1D 72 01/02/04/31/32/34
            { EscPosCmdType.GsObsoletePrintRasterBitimage, new DecInfo { decodedetail = new DecodeDetail(DecodeGsObsoletePrintRasterBitimage), index = 3 } },                                             //  GS  v 0 1D 76 30 00-03/30-33 0001-FFFF 0001-11FF 00-FF...
            { EscPosCmdType.GsSetBarcodeWidth, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetBarcodeWidth), index = 2 } },                                                                     //  GS  w   1D 77 02-06/44-4C
            { EscPosCmdType.GsSetOnlineRecoveryWaitTime, new DecInfo { decodedetail = new DecodeDetail(DecodeGsSetOnlineRecoveryWaitTime), index = 3 } },                                                 //  GS  z 0 1D 7A 30 00-FF 00-FF
            { EscPosCmdType.GsUnknown, null },
            // US 0x1F VFD
            { EscPosCmdType.VfdUsOverwriteMode, null },                                                                                                                             //  US  \1  1F 01
            { EscPosCmdType.VfdUsVerticalScrollMode, null },                                                                                                                        //  US  \1  1F 02
            { EscPosCmdType.VfdUsHorizontalScrollMode, null },                                                                                                                      //  US  \1  1F 03
            { EscPosCmdType.VfdUsMoveCursorUp, null },                                                                                                                              //  US  LF  1F 0A
            { EscPosCmdType.VfdUsMoveCursorRightMost, null },                                                                                                                       //  US  LF  1F 0D
            { EscPosCmdType.VfdUsTurnAnnounciatorOnOff, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdUsTurnAnnounciatorOnOff), index = 2 } },                             //  US  #   1F 23 00/01/30/31 00-14
            { EscPosCmdType.VfdUsMoveCursorSpecifiedPosition, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdUsMoveCursorSpecifiedPosition), index = 2 } },                 //  US  $   1F 24 01-14 01/02
            { EscPosCmdType.VfdUsSelectDisplays, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdUsSelectDisplays), index = 3 } },                                           //  US  ( A 1F 28 41 0003-FFFF 30 [30/31 00-FF]...
            { EscPosCmdType.VfdUsChangeIntoUserSettingMode, null },                                                                                                                 //  US  ( E 1F 28 45 03 00 01 49 4E
            { EscPosCmdType.VfdUsEndUserSettingMode, null },                                                                                                                        //  US  ( E 1F 28 45 04 00 02 4F 55 54
            { EscPosCmdType.VfdUsSetMemorySwitchValues, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdUsSetMemorySwitchValues), index = 3 } },                             //  US  ( E 1F 28 45 000A-FFFA 03 [09-0F 30-32]...
            { EscPosCmdType.VfdUsSendingDisplayingMemorySwitchValues, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdUsSendingDisplayingMemorySwitchValues), index = 6 } }, //  US  ( E 1F 28 45 02 00 04 09-0F/6D-70/72/73
            { EscPosCmdType.VfdUsKanjiCharacterModeOnOff, new DecInfo { decodedetail = new DecodeDetail(DecodeValueOnOff), index = 6 } },                                           //  US  ( G 1F 28 47 02 00 60 00/01/30/31
            { EscPosCmdType.VfdUsSelectKanjiCharacterCodeSystem, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdUsSelectKanjiCharacterCodeSystem), index = 6 } },           //  US  ( G 1F 28 47 02 00 61 00/01/30/31
            { EscPosCmdType.VfdUsDisplayCharWithComma, null },                                                                                                                      //  US  ,   1F 2C 20-7E/80-FF
            { EscPosCmdType.VfdUsDisplayCharWithPeriod, null },                                                                                                                     //  US  .   1F 2E 20-7E/80-FF
            { EscPosCmdType.VfdUsStartEndMacroDefinition, null },                                                                                                                   //  US  :   1F 3A
            { EscPosCmdType.VfdUsDisplayCharWithSemicolon, null },                                                                                                                  //  US  ;   1F 3B 20-7E/80-FF
            { EscPosCmdType.VfdUsExecuteSelfTest, null },                                                                                                                           //  US  @   1F 40
            { EscPosCmdType.VfdUsMoveCursorBottom, null },                                                                                                                          //  US  B   1F 42
            { EscPosCmdType.VfdUsTurnCursorDisplayModeOnOff, new DecInfo { decodedetail = new DecodeDetail(DecodeValueOnOff), index = 2 } },                                        //  US  C   1F 43 00/01/30/31
            { EscPosCmdType.VfdUsSetDisplayBlinkInterval, new DecInfo { decodedetail = new DecodeDetail(DecodeValueUInt08), index = 2, trailer = " x 50 ms" } },                    //  US  E   1F 45 00-FF
            { EscPosCmdType.VfdUsSetAndDisplayCountTime, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdUsSetAndDisplayCountTime), index = 2 } },                           //  US  T   1F 54 00-17 00-3B
            { EscPosCmdType.VfdUsDisplayCounterTime, null },                                                                                                                        //  US  U   1F 55
            { EscPosCmdType.VfdUsBrightnessAdjustment, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdUsBrightnessAdjustment), index = 2 } },                               //  US  X   1F 58 01-04
            { EscPosCmdType.VfdUsExecuteMacro, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdUsExecuteMacro), index = 2 } },                                               //  US  ^   1F 5E 00-FF 00-FF
            { EscPosCmdType.VfdUsTurnReverseMode, new DecInfo { decodedetail = new DecodeDetail(DecodeValueOnOff), index = 2 } },                                                   //  US  r   1F 72 00/01/30/31
            { EscPosCmdType.VfdUsStatusConfirmationByDTRSignal, new DecInfo { decodedetail = new DecodeDetail(DecodeVfdUsStatusConfirmationByDTRSignal), index = 2 } },             //  US  v   1F 76 00/01/30/31
            { EscPosCmdType.VfdUsUnknown, null }
        };

        /// <summary>
        /// entry point method of EscPosDecoder object
        /// </summary>
        /// <param name="data">byte array data</param>
        /// <param name="initialDevice">indicate initial device type</param>
        /// <returns></returns>
        public static List<EscPosCmd> Convert(List<EscPosCmd> data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            int count = data.Count;
#pragma warning disable CS8629 // Nullable value type may be null
            for (int i = 0; i < count; i++)
            {
                DecInfo? currDec = s_DecodeType[data[i].cmdtype];
                if (currDec == null)
                {
                    data[i].paramdetail = "";
                    continue;
                }
                string detailed = currDec?.leader ?? "";
                if (currDec?.decodedetail != null)
                {
                    detailed += currDec?.decodedetail(data[i], (int)currDec?.index);
                }
                detailed += currDec?.trailer ?? "";
                data[i].paramdetail = detailed;
            }
#pragma warning restore CS8629 // Nullable value type may be null
            return data;
        }
    }
}