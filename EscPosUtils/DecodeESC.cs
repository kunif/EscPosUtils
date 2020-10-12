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
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;

    public static partial class EscPosDecoder
    {
        //  ESC !   1B 21 bx0xxx00x
        internal static string DecodeEscSelectPrintMode(EscPosCmd record, int index)
        {
            byte mode = record.cmddata[index];
            string underline = (mode & 0x80) == 0x80 ? "ON" : "OFF";
            string doublewidth = (mode & 0x20) == 0x20 ? "ON" : "OFF";
            string doubleheight = (mode & 0x10) == 0x10 ? "ON" : "OFF";
            string emphasize = (mode & 0x08) == 0x08 ? "ON" : "OFF";
            string font = (mode & 0x01) == 0x01 ? "B" : "A";
            return $"Underline:{underline}, DoubleWidth:{doublewidth}, DoubleHeight:{doubleheight}, Emphasize:{emphasize}, Font:{font}";
        }

        //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
        internal static string DecodeEscDefineUserDefinedCharacters1224(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index, 12, 24);
        }
        internal static string DecodeEscDefineUserDefinedCharacters1024(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index, 10, 24);
        }
        internal static string DecodeEscDefineUserDefinedCharacters0924(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index,  9, 24);
        }
        internal static string DecodeEscDefineUserDefinedCharacters0917(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index,  9, 17);
        }
        internal static string DecodeEscDefineUserDefinedCharacters0909(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index,  9,  9);
        }
        internal static string DecodeEscDefineUserDefinedCharacters0709(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index,  7,  9);
        }
        internal static string DecodeEscDefineUserDefinedCharacters0816(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index,  8, 16);
        }
        internal static string DecodeEscDefineUserDefinedCharacters(EscPosCmd record, int index, int maxwidth, int height)
        {
            byte ybytes = record.cmddata[index];
            byte startcode = record.cmddata[index + 1];
            byte endcode = record.cmddata[index + 2];
            int count = startcode - endcode + 1;
            int i = index + 3;
            List<string> chars = new List<string>();
            List<System.Drawing.Bitmap> glyphs = new List<System.Drawing.Bitmap>();
            for (int n = 0; n < count; n++)
            {
                byte xbytes = record.cmddata[i];
                int size = ybytes * xbytes;
                if (size > 0)
                {
                    glyphs.Add(GetBitmap(xbytes, height, ImageDataType.Column, record.cmddata, (i + 1), "1"));
                }
                else
                {
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(maxwidth, height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                    ColorPalette palette = bitmap.Palette;
                    palette.Entries[0] = Color.White;
                    palette.Entries[1] = Color.Black;
                    bitmap.Palette = palette;
                    glyphs.Add(bitmap);
                }

                i += size + 1;
                chars.Add($"X:{xbytes} bytes, Size:{size}");
            }
            record.somebinary = glyphs.ToArray();
            string charslist = string.Join(", ", chars);
            return $"VerticalBytes:{ybytes}, StartCode:{startcode}, EndCode:{endcode}, Characters:{charslist}";
        }

        //  ESC ( A 1B 28 41 04 00 30 30-3A 01-3F 0A-FF
        internal static string DecodeEscBeeperBuzzer(EscPosCmd record, int index)
        {
            byte pattern = record.cmddata[index];
            byte cycles = record.cmddata[index + 1];
            byte duration = record.cmddata[index + 2];
            string result = $"Cycles:{cycles}, Duration:{duration} x 100ms, Pattern:{pattern} is ";
            result += pattern switch
            {
                48 => "doesn't beep",
                49 => "1320 Hz: 1000 ms beeping",
                50 => "2490 Hz: 1000 ms beeping",
                51 => "1320 Hz: 200 ms beeping",
                52 => "2490 Hz: 200 ms beeping",
                53 => "1320 Hz: 200 ms beeping → 200 ms off → 200 ms beeping",
                54 => "2490 Hz: 200 ms beeping → 200 ms off → 200 ms beeping",
                55 => "1320 Hz: 500 ms beeping",
                56 => "2490 Hz: 500 ms beeping",
                57 => "1320 Hz: 200 ms beeping → 200 ms off → 200 ms beeping → 200 ms off → 200 ms beeping",
                58 => "2490 Hz: 200 ms beeping → 200 ms off → 200 ms beeping → 200 ms off → 200 ms beeping",
                _ => "Undefined",
            };
            return result;
        }

        //  ESC ( A 1B 28 41 03 00 61 01-07 00-FF
        internal static string DecodeEscBeeperBuzzerM1a(EscPosCmd record, int index)
        {
            byte pattern = record.cmddata[index];
            byte cycles = record.cmddata[index + 1];
            string result = $"Cycles:{cycles}, Pattern:{pattern} is ";
            result += pattern switch
            {
                1 => "A",
                2 => "B",
                3 => "C",
                4 => "D",
                5 => "E",
                6 => "Error",
                7 => "Paper-End",
                _ => "Undefined",
            };
            return result;
        }

        //  ESC ( A 1B 28 41 05 00 61 64 00-3F 00-FF 00-FF
        internal static string DecodeEscBeeperBuzzerM1b(EscPosCmd record, int index)
        {
            byte cycles = record.cmddata[index];
            byte onduration = record.cmddata[index + 1];
            byte offduration = record.cmddata[index + 2];
            return $"Cycles:{cycles}, On-Duration:{onduration} x 100ms, Off-Duration:{offduration} x 100ms";
        }

        //  ESC ( A 1B 28 41 07 00 62 30-33 01 64 00/FF 01-32/FF 01-32
        internal static string DecodeEscBeeperBuzzerOffline(EscPosCmd record, int index)
        {
            string factor = record.cmddata[index] switch
            {
                48 => "Cover open",
                49 => "Paper end",
                50 => "Recoverable error",
                51 => "Unrecoverable error",
                _ => "Undefined",
            };
            string beeptype = record.cmddata[index + 3] switch
            {
                0 => "OFF",
                255 => "Infinite",
                _ => "Undefined",
            };
            byte onduration = record.cmddata[index + 4];
            byte offduration = record.cmddata[index + 5];
            return $"Factor:{factor}, Type:{beeptype}, On-Duration:{onduration} x 100ms, Off-Duration:{offduration} x 100ms";
        }

        //  ESC ( A 1B 28 41 07 00 63 30 01 64 00/FF 01-32/FF 01-32
        internal static string DecodeEscBeeperBuzzerNearEnd(EscPosCmd record, int index)
        {
            string beeptype = record.cmddata[index] switch
            {
                0 => "OFF",
                255 => "Infinite",
                _ => "Undefined",
            };
            byte onduration = record.cmddata[index + 1];
            string t1;
            if (onduration == 255)
            {
                t1 = "On-Duration:Infinite";
            }
            else if ((onduration >= 1) && (onduration <= 50))
            {
                t1 = $"On-Duration:{onduration} x 100ms";
            }
            else
            {
                t1 = $"On-Duration:{onduration} Out of range";
            }
            byte offduration = record.cmddata[index + 2];
            string t2;
            if ((offduration >= 1) && (offduration <= 50))
            {
                t2 = $"Off-Duration:{offduration} x 100ms";
            }
            else
            {
                t2 = $"Off-Duration:{offduration} Out of range";
            }
            return $"Type:{beeptype}, {t1}, {t2}";
        }

        //  ESC ( Y 1B 28 59 02 00 00/01/30/31 00/01/30/31
        internal static string DecodeEscSpecifyBatchPrint(EscPosCmd record, int index)
        {
            string fanc = record.cmddata[index] switch
            {
                0 => "Print bufferd batch data",
                48 => "Print bufferd batch data",
                1 => "Start batch buffering",
                49 => "Start batch buffering",
                _ => "Undefined",
            };
            string direction = record.cmddata[index + 1] switch
            {
                0 => "Normal direction",
                48 => "Normal direction",
                1 => "Reverse direction",
                49 => "Reverse direction",
                _ => "Undefined",
            };
            return $"{fanc}, {direction}";
        }

        //  ESC *   1B 2A 00/01/20/21 0001-0960 00-FF...
        internal static string DecodeEscSelectBitImageMode(EscPosCmd record, int index)
        {
            int height = record.cmddata[index] switch
            {
                0 => 8,
                1 => 8,
                32 => 24,
                33 => 24,
                _ => -1,
            };
            string modestr = record.cmddata[index] switch
            {
                0 => "8 dot Single(low) density ",
                1 => "8 dot Double(high) density",
                32 => "24 dot Single(low) density",
                33 => "24 dot Double(high) density",
                _ => "Undefined",
            };
            int width = BitConverter.ToUInt16(record.cmddata, index + 1);
            string widthstr = width.ToString("D", invariantculture);
            if ((height > 0) && ((width > 0)&&(width <= 0x960)))
            {
                record.somebinary = GetBitmap(width, height, ImageDataType.Column, record.cmddata, (index + 3), "1");
            }
            return $"Mode:{modestr}, Width:{widthstr} dot";
        }

        //  ESC -   1B 2D 00-02/30-32
        internal static string DecodeEscUnderlineMode(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "OFF",
                48 => "OFF",
                1 => "ON 1 dot",
                49 => "ON 1 dot",
                2 => "ON 2 dot",
                50 => "ON 2 dot",
                _ => "Undefined",
            };
        }

        //  ESC =   1B 3D 01-03 or 00-FF
        internal static string DecodeEscSelectPeripheralDevice(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                1 => "Printer",
                2 => "LineDisplay",
                3 => "Printer and LineDisplay",
                _ => "Undefined",
            };
        }

        //  ESC ?   1B 3F 20-7E
        internal static string DecodeEscCancelUserDefinedCharacters(EscPosCmd record, int index)
        {
            return $"Code:{record.cmddata[index]}";
        }

        //  ESC D   1B 44 [01-FF]... 00
        internal static string DecodeEscHorizontalTabPosition(EscPosCmd record, int index)
        {
            int length = (int)(record.cmddata[record.cmdlength - 1] == 0 ? record.cmdlength - 3 : record.cmdlength - 2);
            string result;
            if (length > 0)
            {
                result = BitConverter.ToString(record.cmddata, index, length).Replace('-', ',');
            }
            else
            {
                result = "Clear all tab settings.";
            }
            return result;
        }

        //  ESC M   1B 4D 00-04/30-34/61/62
        internal static string DecodeEscSelectCharacterFont(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "A",
                48 => "A",
                1 => "B",
                49 => "B",
                2 => "C",
                50 => "C",
                3 => "D",
                51 => "D",
                4 => "E",
                52 => "E",
                97 => "Special A",
                98 => "Special B",
                _ => "Undefined",
            };
        }

        //  ESC R   1B 52 00-11/42-4B/52
        internal static string DecodeEscSelectInternationalCharacterSet(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "U.S.A.",
                1 => "France",
                2 => "Germany",
                3 => "U.K.",
                4 => "Denmark I",
                5 => "Sweden",
                6 => "Italy",
                7 => "Spain I",
                8 => "Japan",
                9 => "Norway",
                10 => "Denmark II",
                11 => "Spain II",
                12 => "Latin America",
                13 => "Korea",
                14 => "Slovenia / Croatia",
                15 => "China",
                16 => "Vietnam",
                17 => "Arabia",
                66 => "India (Devanagari)",
                67 => "India (Bengali)",
                68 => "India (Tamil)",
                69 => "India (Telugu)",
                70 => "India (Assamese)",
                71 => "India (Oriya)",
                72 => "India (Kannada)",
                73 => "India (Malayalam)",
                74 => "India (Gujarati)",
                75 => "India (Punjabi)",
                82 => "India (Marathi)",
                _ => "Undefined",
            };
        }

        //  ESC T   1B 54 00-03/30-33
        internal static string DecodeEscSelectPrintDirection(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "Left to Right : Normal",
                48 => "Left to Right : Normal",
                1 => "Bottom to Top : Left90",
                49 => "Bottom to Top : Left90",
                2 => "Right to Left : Rotate180",
                50 => "Right to Left : Rotate180",
                3 => "Top to Bottom : Right90",
                51 => "Top to Bottom : Right90",
                _ => "Undefined",
            };
        }

        //  ESC V   1B 56 00-02/30-32
        internal static string DecodeEscTurn90digreeClockwiseRotationMode(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "Turns OFF 90 digree Clockwise Rotation",
                48 => "Turns OFF 90 digree Clockwise Rotation",
                1 => "Turns ON 90 digree Clockwise Rotation : 1 dot Character spacing",
                49 => "Turns ON 90 digree Clockwise Rotation : 1 dot Character spacing",
                2 => "Turns ON 90 digree Clockwise Rotation : 5 dot Character spacing",
                50 => "Turns ON 90 digree Clockwise Rotation : 5 dot Character spacing",
                _ => "Undefined",
            };
        }

        //  ESC W   1B 57 0000-FFFF 0000-FFFF 0001-FFFF 0001-FFFF
        internal static string DecodeEscSetPrintAreaInPageMode(EscPosCmd record, int index)
        {
            int top = BitConverter.ToUInt16(record.cmddata, 2);
            int left = BitConverter.ToUInt16(record.cmddata, 4);
            int width = BitConverter.ToUInt16(record.cmddata, 6);
            int height = BitConverter.ToUInt16(record.cmddata, 8);
            return $"Top:{top} dot, Left:{left} dot, Width:{width} dot, Height:{height} dot";
        }

        //  ESC a   1B 61 00-02/30-32
        internal static string DecodeEscSelectJustification(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "Left",
                48 => "Left",
                1 => "Centered",
                49 => "Centered",
                2 => "Right",
                50 => "Right",
                _ => "Undefined",
            };
        }

        //c ESC c 0 1B 63 30 b0000xxxx
        internal static string DecodeEscSelectPaperTypesPrinting(EscPosCmd record, int index)
        {
            byte mode = record.cmddata[index];
            string validation = (mode & 0x08) == 0x08 ? "Enable" : "Disable";
            string slip = (mode & 0x04) == 0x04 ? "Enable" : "Disable";
            string roll = (mode & 0x03) switch
            {
                0 => "Disable",
                1 => "Active Sheet",
                2 => "Active Sheet",
                3 => "Enable",
                _ => "",
            };
            return $"Validation paper:{validation}, Slip paper:{slip}, Roll paper:{roll}";
        }

        //c ESC c 1 1B 63 31 b0x00xxxx
        internal static string DecodeEscSelectPaperTypesCommandSettings(EscPosCmd record, int index)
        {
            byte mode = record.cmddata[index];
            string slipback = (mode & 0x20) == 0x20 ? "Enable" : "Disable";
            string validation = (mode & 0x08) == 0x08 ? "Enable" : "Disable";
            string slipface = (mode & 0x04) == 0x04 ? "Enable" : "Disable";
            string roll = (mode & 0x03) switch
            {
                0 => "Disable",
                1 => "Active Sheet",
                2 => "Active Sheet",
                3 => "Enable",
                _ => "",
            };
            return $"Validation paper:{validation}, Face of Slip paper:{slipface}, Back of Slip paper:{slipback}, Roll paper:{roll}";
        }

        //  ESC c 3 1B 63 33 bccccxxxx
        internal static string DecodeEscSelectPaperSensorsPaperEndSignals(EscPosCmd record, int index)
        {
            byte mode = record.cmddata[index];
            string validation = (mode & 0xC0) == 0x00 ? "Disable" : "Enable";
            string slipBOF = (mode & 0x20) == 0x00 ? "Disable" : "Enable";
            string slipTOF = (mode & 0x20) == 0x00 ? "Disable" : "Enable";
            string empty = (mode & 0x0C) == 0x00 ? "Disable" : "Enable";
            string nearend = (mode & 0x03) == 0x00 ? "Disable" : "Enable";
            return $"Validation:{validation}, SlipBOF:{slipBOF}, SlipTOF:{slipTOF}, Empty:{empty}, Near End:{nearend}";
        }

        //  ESC c 4 1B 63 34 bccccxxxx
        internal static string DecodeEscSelectPaperSensorsStopPrinting(EscPosCmd record, int index)
        {
            byte mode = record.cmddata[index];
            string validation = (mode & 0xC0) == 0x00 ? "Disable" : "Enable";
            string slipBOF = (mode & 0x20) == 0x00 ? "Disable" : "Enable";
            string slipTOF = (mode & 0x20) == 0x00 ? "Disable" : "Enable";
            string empty = (mode & 0x0C) == 0x00 ? "Disable" : "Enable";
            string nearend = (mode & 0x03) == 0x00 ? "Disable" : "Enable";
            return $"Validation:{validation}, SlipBOF:{slipBOF}, SlipTOF:{slipTOF}, Empty:{empty}, Near End:{nearend}";
        }

        //c ESC f   1B 66 00-0F 00-40
        internal static string DecodeEscCutSheetWaitTime(EscPosCmd record, int index)
        {
            byte insert = record.cmddata[index];
            string inswait = (insert <= 15) ? insert.ToString("D", invariantculture) + " minute" : "Out of range";
            byte detect = record.cmddata[index + 1];
            string detwait = (detect <= 64) ? detect.ToString("D", invariantculture) + " x 100ms" : "Out of range";
            return $"Insertion Wait:{inswait}, Detection Wait:{detwait}";
        }

        //  ESC p   1B 70 00/01/30/31 00-FF 00-FF
        internal static string DecodeEscGeneratePulse(EscPosCmd record, int index)
        {
            string pin = record.cmddata[index] switch
            {
                0 => "2",
                48 => "2",
                1 => "5",
                49 => "5",
                _ => "Undefined",
            };
            byte onduration = record.cmddata[index + 1];
            byte offduration = record.cmddata[index + 2];
            return $"Pin:{pin}, On-Duration:{onduration} x 100ms, Off-Duration:{offduration} x 100ms";
        }

        //  ESC r   1B 72 00/01/30/31
        internal static string DecodeEscSelectPrinterColor(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "Black",
                48 => "Black",
                1 => "Red",
                49 => "Red",
                _ => "Undefined",
            };
        }

        public static Dictionary<byte, string> GetEmbeddedESCtCodePage(int embeddedcodepage)
        {
            return embeddedcodepage switch
            {
                6 => s_cp06_hiragana, // Page 6: Hiragana Not suitable code page but alternative definition
                7 => s_cp07_Kanji01,  // Page 7:One-pass printing Kanji characters
                8 => s_cp08_Kanji02,  // Page 8:One-pass printing Kanji characters
                11 => s_cp851,  // PC851: Greek
                12 => s_cp853,  // PC853: Turkish
                20 => s_cp20_Thai_CC_42, // Page 20 Thai Character Code 42 Not suitable code page but alternative definition
                21 => s_cp21_Thai_CC_11, // Page 21 Thai Character Code 11 Not suitable code page but alternative definition
                22 => s_cp22_Thai_CC_13, // Page 22 Thai Character Code 13 Not suitable code page but alternative definition
                23 => s_cp23_Thai_CC_14, // Page 23 Thai Character Code 14 Not suitable code page but alternative definition
                24 => s_cp24_Thai_CC_16, // Page 24 Thai Character Code 16 Not suitable code page but alternative definition
                25 => s_cp25_Thai_CC_17, // Page 25 Thai Character Code 17 Not suitable code page but alternative definition
                26 => s_cp26_Thai_CC_18, // Page 26 Thai Character Code 18 Not suitable code page but alternative definition
                30 => s_cp30_TCVN_3,  // Page 30 TCVN-3: Vietnamese Not suitable code page but alternative definition
                31 => s_cp31_TCVN_3,  // Page 31 TCVN-3: Vietnamese Not suitable code page but alternative definition
                41 => s_cp1098,  // PC1098: Farsi
                42 => s_cp1118,  // PC1118: Lithuanian
                43 => s_cp1119,  // PC1119: Lithuanian
                44 => s_cp1125,  // PC1125: Ukrainian
                53 => s_kz1048,  // KZ-1048: Kazakhstan Not suitable code page but alternative definition
                254 => s_cpASCII,  // Page254
                255 => s_cpASCII,  // Page255
                _ => s_cpASCII,
            };
        }

        public static readonly Dictionary<byte, int> PrtESCtCodePage = new Dictionary<byte, int>()
        {
            { 0,    437 },  // PC437: USA, Standard Europe
            { 1,    932 },  // PC932: Katakana
            { 2,    850 },  // PC850: Multilingual
            { 3,    860 },  // PC860: Portuguese
            { 4,    863 },  // PC863: Canadian-French
            { 5,    865 },  // PC865: Nordic
            { 6,      6 },  // Page 6: Hiragana Not suitable code page but alternative definition
            { 7,      7 },  // Page 7:One-pass printing Kanji characters
            { 8,      8 },  // Page 8:One-pass printing Kanji characters
            { 11,    11 },  // PC851: Greek
            { 12,    12 },  // PC853: Turkish
            { 13,   857 },  // PC857: Turkish
            { 14,   737 },  // PC737: Greek
            { 15, 28597 },  // ISO8859-7: Greek
            { 16,  1252 },  // WPC1252
            { 17,   866 },  // PC866: Cyrillic #2
            { 18,   852 },  // PC852: Latin 2
            { 19,   858 },  // PC858: Euro
            { 20,    20 },  // Page 20 Thai Character Code 42 Not suitable code page but alternative definition
            { 21,    21 },  // Page 21 Thai Character Code 11 Not suitable code page but alternative definition
            { 22,    22 },  // Page 22 Thai Character Code 13 Not suitable code page but alternative definition
            { 23,    23 },  // Page 23 Thai Character Code 14 Not suitable code page but alternative definition
            { 24,    24 },  // Page 24 Thai Character Code 16 Not suitable code page but alternative definition
            { 25,    25 },  // Page 25 Thai Character Code 17 Not suitable code page but alternative definition
            { 26,    26 },  // Page 26 Thai Character Code 18 Not suitable code page but alternative definition
            { 30,    30 },  // Page 30 TCVN-3: Vietnamese Not suitable code page but alternative definition
            { 31,    31 },  // Page 31 TCVN-3: Vietnamese Not suitable code page but alternative definition
            { 32,   720 },  // PC720: Arabic
            { 33,   775 },  // WPC775: Baltic Rim
            { 34,   855 },  // PC855: Cyrillic
            { 35,   861 },  // PC861: Icelandic
            { 36,   862 },  // PC862: Hebrew
            { 37,   864 },  // PC864: Arabic
            { 38,   869 },  // PC869: Greek
            { 39, 28592 },  // ISO8859-2: Latin 2
            { 40, 28605 },  // ISO8859-15: Latin 9
            { 41,    41 },  // PC1098: Farsi
            { 42,    42 },  // PC1118: Lithuanian
            { 43,    43 },  // PC1119: Lithuanian
            { 44,    44 },  // PC1125: Ukrainian
            { 45,  1250 },  // WPC1250: Latin 2
            { 46,  1251 },  // WPC1251: Cyrillic
            { 47,  1253 },  // WPC1253: Greek
            { 48,  1254 },  // WPC1254: Turkish
            { 49,  1255 },  // WPC1255: Hebrew
            { 50,  1256 },  // WPC1256: Arabic
            { 51,  1257 },  // WPC1257: Baltic Rim
            { 52,  1258 },  // WPC1258: Vietnamese
            { 53,    53 },  // KZ-1048: Kazakhstan Not suitable code page but alternative definition
            { 66, 57002 },  // Devanagari
            { 67, 57003 },  // Bengali
            { 68, 57004 },  // Tamil
            { 69, 57005 },  // Telugu
            { 70, 57006 },  // Assamese
            { 71, 57007 },  // Oriya
            { 72, 57008 },  // Kannada
            { 73, 57009 },  // Malayalam
            { 74, 57010 },  // Gujarati
            { 75, 57011 },  // Punjabi
            { 82, 57002 },  // Marathi Not suitable code page but alternative definition
            { 254,  254 },  // Page254
            { 255,  255 }   // Page255
        };

        //  ESC t   1B 74 00-08/0B-1A/1E-35/42-4B/52/FE/FF
        internal static string DecodeEscSelectCharacterCodeTable(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "PC437: USA, Standard Europe",
                1 => "PC932: Katakana",
                2 => "PC850: Multilingual",
                3 => "PC860: Portuguese",
                4 => "PC863: Canadian-French",
                5 => "PC865: Nordic",
                6 => "Page 6: Hiragana",
                7 => "Page 7: One-pass printing Kanji characters",
                8 => "Page 8: One-pass printing Kanji characters",
                11 => "PC851: Greek",
                12 => "PC853: Turkish",
                13 => "PC857: Turkish",
                14 => "PC737: Greek",
                15 => "ISO8859-7: Greek",
                16 => "WPC1252",
                17 => "PC866: Cyrillic #2",
                18 => "PC852: Latin 2",
                19 => "PC858: Euro",
                20 => "Page 20 Thai Character Code 42",
                21 => "Page 21 Thai Character Code 11",
                22 => "Page 22 Thai Character Code 13",
                23 => "Page 23 Thai Character Code 14",
                24 => "Page 24 Thai Character Code 16",
                25 => "Page 25 Thai Character Code 17",
                26 => "Page 26 Thai Character Code 18",
                30 => "Page 30 TCVN-3: Vietnamese",
                31 => "Page 31 TCVN-3: Vietnamese",
                32 => "PC720: Arabic",
                33 => "WPC775: Baltic Rim",
                34 => "PC855: Cyrillic",
                35 => "PC861: Icelandic",
                36 => "PC862: Hebrew",
                37 => "PC864: Arabic",
                38 => "PC869: Greek",
                39 => "ISO8859-2: Latin 2",
                40 => "ISO8859-15: Latin 9",
                41 => "PC1098: Farsi",
                42 => "PC1118: Lithuanian",
                43 => "PC1119: Lithuanian",
                44 => "PC1125: Ukrainian",
                45 => "WPC1250: Latin 2",
                46 => "WPC1251: Cyrillic",
                47 => "WPC1253: Greek",
                48 => "WPC1254: Turkish",
                49 => "WPC1255: Hebrew",
                50 => "WPC1256: Arabic",
                51 => "WPC1257: Baltic Rim",
                52 => "WPC1258: Vietnamese",
                53 => "Page 53 KZ-1048: Kazakhstan",
                66 => "Devanagari",
                67 => "Bengali",
                68 => "Tamil",
                69 => "Telugu",
                70 => "Assamese",
                71 => "Oriya",
                72 => "Kannada",
                73 => "Malayalam",
                74 => "Gujarati",
                75 => "Punjabi",
                82 => "Marathi",
                254 => "Page 254",
                255 => "Page 255",
                _ => "Undefined",
            };
        }

        //  ESC u   1B 75 00/30
        internal static string DecodeEscObsoleteTransmitPeripheralDeviceStatus(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "DrawerKickConnector Pin 3",
                48 => "DrawerKickConnector Pin 3",
                _ => "Undefined",
            };
        }

        //--------//

        //  ESC &   1B 26 00/01 20-7E 20-7E 00-08 00-FF...
        internal static string DecodeVfdEscDefineUserDefinedCharacters0816(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index, 8, 16);
        }
        internal static string DecodeVfdEscDefineUserDefinedCharacters0507(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index, 5, 7);
        }

        //  ESC =   1B 3D 01/02/03
        internal static string DecodeVfdEscSelectPeripheralDevice(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                1 => "Printer",
                3 => "Printer and LineDisplay",
                2 => "LineDisplay",
                _ => "Undefined",
            };
        }

        //  ESC ?   1B 3F 20-7E
        internal static string DecodeVfdEscCancelUserDefinedCharacters(EscPosCmd record, int index)
        {
            return $"Code:{record.cmddata[index]}";
        }

        //  ESC R   1B 52 00-11
        internal static string DecodeVfdEscSelectInternationalCharacterSet(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "U.S.A.",
                1 => "France",
                2 => "Germany",
                3 => "U.K.",
                4 => "Denmark I",
                5 => "Sweden",
                6 => "Italy",
                7 => "Spain I",
                8 => "Japan",
                9 => "Norway",
                10 => "Denmark II",
                11 => "Spain II",
                12 => "Latin America",
                13 => "Korea",
                14 => "Slovenia / Croatia",
                15 => "China",
                16 => "Vietnam",
                17 => "Arabia",
                _ => "Undefined",
            };
        }

        //  ESC W   1B 57 01-04 00/30
        internal static string DecodeVfdEscCancelWindowArea(EscPosCmd record, int index)
        {
            byte win = record.cmddata[index];
            string winno;
            if ((win >= 1) && (win <= 4))
            {
                winno = win.ToString("D", invariantculture);
            }
            else
            {
                winno = "Out of range";
            }
            string mode = record.cmddata[index + 1] switch
            {
                0 => "Release",
                48 => "Release",
                _ => "Undefined",
            };
            return $"Window number:{winno}, Action:{mode}";
        }

        //  ESC W   1B 57 01-04 01/31 01-14 01-14 01/02 01/02
        internal static string DecodeVfdEscSelectWindowArea(EscPosCmd record, int index)
        {
            byte win = record.cmddata[index];
            string winno = ((win >= 1) && (win <= 4)) ? win.ToString("D", invariantculture) : "Out of range";
            string mode = record.cmddata[index + 1] switch
            {
                1 => "Specify",
                49 => "Specify",
                _ => "Undefined",
            };
            byte x1 = record.cmddata[index + 2];
            string left = ((x1 >= 1) && (x1 <= 20)) ? x1.ToString("D", invariantculture) : "Out of range";
            byte y1 = record.cmddata[index + 3];
            string top = ((y1 >= 1) && (y1 <= 2)) ? y1.ToString("D", invariantculture) : "Out of range";
            byte x2 = record.cmddata[index + 4];
            string right = ((x2 >= 1) && (x2 <= 20) && (x1 <= x2)) ? x2.ToString("D", invariantculture) : "Out of range";
            byte y2 = record.cmddata[index + 5];
            string bottom = ((y2 >= 1) && (y2 <= 2) && (y1 <= y2)) ? y2.ToString("D", invariantculture) : "Out of range";
            return $"Window number:{winno}, Action:{mode}, Left:{left}, Top:{top}, Right:{right}, Bottom:{bottom}";
        }

        public static readonly Dictionary<byte, int> VfdESCtCodePage = new Dictionary<byte, int>()
        {
            { 0,    437 },  // PC437: USA, Standard Europe
            { 1,    932 },  // PC932: Katakana
            { 2,    850 },  // PC850: Multilingual
            { 3,    860 },  // PC860: Portuguese
            { 4,    863 },  // PC863: Canadian-French
            { 5,    865 },  // PC865: Nordic
            { 11,    11 },  // PC851: Greek
            { 12,    12 },  // PC853: Turkish
            { 13,   857 },  // PC857: Turkish
            { 14,   737 },  // PC737: Greek
            { 15, 28597 },  // ISO8859-7: Greek
            { 16,  1252 },  // WPC1252
            { 17,   866 },  // PC866: Cyrillic #2
            { 18,   852 },  // PC852: Latin 2
            { 19,   858 },  // PC858: Euro
            { 30,    30 },  // Page 30 TCVN-3: Vietnamese Not suitable code page but alternative definition
            { 31,    31 },  // Page 31 TCVN-3: Vietnamese Not suitable code page but alternative definition
            { 32,   720 },  // PC720: Arabic
            { 33,   775 },  // WPC775: Baltic Rim
            { 34,   855 },  // PC855: Cyrillic
            { 35,   861 },  // PC861: Icelandic
            { 36,   862 },  // PC862: Hebrew
            { 37,   864 },  // PC864: Arabic
            { 38,   869 },  // PC869: Greek
            { 39, 28592 },  // ISO8859-2: Latin 2
            { 40, 28605 },  // ISO8859-15: Latin 9
            { 41,    41 },  // PC1098: Farsi
            { 42,    42 },  // PC1118: Lithuanian
            { 43,    43 },  // PC1119: Lithuanian
            { 44,    44 },  // PC1125: Ukrainian
            { 45,  1250 },  // WPC1250: Latin 2
            { 46,  1251 },  // WPC1251: Cyrillic
            { 47,  1253 },  // WPC1253: Greek
            { 48,  1254 },  // WPC1254: Turkish
            { 49,  1255 },  // WPC1255: Hebrew
            { 50,  1256 },  // WPC1256: Arabic
            { 51,  1257 },  // WPC1257: Baltic Rim
            { 52,  1258 },  // WPC1258: Vietnamese
            { 53,    53 },  // KZ-1048: Kazakhstan Not suitable code page but alternative definition
            { 254,  254 },  // Page254
            { 255,  255 }   // Page255
        };

        //  ESC t   1B 74 00-13/1E-35/FE/FF
        internal static string DecodeVfdEscSelectCharacterCodeTable(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "PC437: USA, Standard Europe",
                1 => "PC932: Katakana",
                2 => "PC850: Multilingual",
                3 => "PC860: Portuguese",
                4 => "PC863: Canadian-French",
                5 => "PC865: Nordic",
                11 => "PC851: Greek",
                12 => "PC853: Turkish",
                13 => "PC857: Turkish",
                14 => "PC737: Greek",
                15 => "ISO8859-7: Greek",
                16 => "WPC1252",
                17 => "PC866: Cyrillic #2",
                18 => "PC852: Latin 2",
                19 => "PC858: Euro",
                30 => "Page 30 TCVN-3: Vietnamese",
                31 => "Page 31 TCVN-3: Vietnamese",
                32 => "PC720: Arabic",
                33 => "WPC775: Baltic Rim",
                34 => "PC855: Cyrillic",
                35 => "PC861: Icelandic",
                36 => "PC862: Hebrew",
                37 => "PC864: Arabic",
                38 => "PC869: Greek",
                39 => "ISO8859-2: Latin 2",
                40 => "ISO8859-15: Latin 9",
                41 => "PC1098: Farsi",
                42 => "PC1118: Lithuanian",
                43 => "PC1119: Lithuanian",
                44 => "PC1125: Ukrainian",
                45 => "WPC1250: Latin 2",
                46 => "WPC1251: Cyrillic",
                47 => "WPC1253: Greek",
                48 => "WPC1254: Turkish",
                49 => "WPC1255: Hebrew",
                50 => "WPC1256: Arabic",
                51 => "WPC1257: Baltic Rim",
                52 => "WPC1258: Vietnamese",
                53 => "KZ-1048: Kazakhstan",
                254 => "Page 254",
                255 => "Page 255",
                _ => "Undefined",
            };
        }
    }
}