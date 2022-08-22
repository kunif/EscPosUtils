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

    public static partial class EscPosDecoder
    {
        //  FS  !   1C 21 bx0xx0000
        internal static string DecodeFsSelectPrintModeKanji(EscPosCmd record, int index)
        {
            byte mode = record.cmddata[index];
            string underline = (mode & 0x80) == 0x80 ? "ON" : "OFF";
            string doublewidth = (mode & 0x20) == 0x20 ? "ON" : "OFF";
            string doubleheight = (mode & 0x10) == 0x10 ? "ON" : "OFF";
            return $"Underline:{underline}, DoubleWidth:{doublewidth}, DoubleHeight:{doubleheight}";
        }

        //  ESC M   1B 4D 00-04/30-34/61/62
        internal static string DecodeFsSelectKanjiCharacterFont(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "A",
                48 => "A",
                1 => "B",
                49 => "B",
                2 => "C",
                50 => "C",
                _ => "Undefined",
            };
        }

        //  FS  ( C 1C 28 43 02 00 30 01/02/31/32
        internal static string DecodeFsSelectCharacterEncodeSystem(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                1 => "1 byte character encoding",
                49 => "1 byte character encoding",
                2 => "UTF-8",
                50 => "UTF-8",
                _ => "Undefined",
            };
        }

        //  FS  ( C 1C 28 43 03 00 3C 00/01 00/0B/14/1E/29
        internal static string DecodeFsSetFontPriority(EscPosCmd record, int index)
        {
            string mode = record.cmddata[index] switch
            {
                0 => "1st",
                1 => "2nd",
                _ => "Undefined",
            };
            string font = record.cmddata[index + 1] switch
            {
                0 => "ANK",
                11 => "Japanese",
                20 => "Simplified Chinese",
                30 => "Traditional Chinese",
                41 => "Korean",
                _ => "Undefined",
            };
            return $"Priority:{mode}, Font:{font}";
        }

        //  FS  ( E 1C 28 45 06 00 3C 02 30/31 43 4C 52
        internal static string DecodeFsCancelSetValuesTopBottomLogo(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                48 => "Top Logo",
                49 => "Bottom Logo",
                _ => "Undefined",
            };
        }

        //  FS  ( E 1C 28 45 03 00 3D 02 30-32
        internal static string DecodeFsTransmitSetValuesTopBottomLogo(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                48 => "Top Logo",
                49 => "Bottom Logo",
                50 => "Both Top & Bottom Logo",
                _ => "Undefined",
            };
        }

        //  FS  ( E 1C 28 45 06 00 3E 02 20-7E 20-7E 30-32 00-FF
        internal static string DecodeFsSetTopLogoPrinting(EscPosCmd record, int index)
        {
            byte kc1 = record.cmddata[index];
            byte kc2 = record.cmddata[index + 1];
            string align = record.cmddata[index + 2] switch
            {
                48 => "Left",
                49 => "Center",
                50 => "Right",
                _ => "Undefined",
            };
            byte lines = record.cmddata[index + 3];
            return $"KeyCode1:{kc1:X}, KeyCode2:{kc2:X}, Align:{align}, Remove:{lines:D} Lines";
        }

        //  FS  ( E 1C 28 45 05 00 3F 02 20-7E 20-7E 30-32
        internal static string DecodeFsSetBottomLogoPrinting(EscPosCmd record, int index)
        {
            byte kc1 = record.cmddata[index];
            byte kc2 = record.cmddata[index + 1];
            string align = record.cmddata[index + 2] switch
            {
                48 => "Left",
                49 => "Center",
                50 => "Right",
                _ => "Undefined",
            };
            return $"KeyCode1:{kc1:X}, KeyCode2:{kc2:X}, Align:{align}";
        }

        //  FS  ( E 1C 28 45 0004-000C 40 02 [30/40-43 30/31]...
        internal static string DecodeFsMakeExtendSettingsTopBottomLogoPrinting(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, 3);
            if ((length < 4) || (length > 12))
            {
                return "Out of range";
            }
            else if ((length & 1) == 1)
            {
                return "Odd length";
            }
            int count = (length / 2) - 1;
            List<string> setting = new();
            for (int i = 0, currindex = 7; i < count; i++, currindex += 2)
            {
                string currset = record.cmddata[currindex] switch
                {
                    48 => "While Paper feeding to Cutting position:",
                    64 => "At Power-On:",
                    65 => "When Roll paper cover is Closed:",
                    66 => "While Clearing Buffer to Recover from Recoverble Error:",
                    67 => "After Paper feeding with Paper feed button has Finished:",
                    _ => "Undefined:",
                };
                currset += record.cmddata[currindex + 1] switch
                {
                    48 => "Enable",
                    49 => "Disable",
                    _ => "Undefined",
                };
                setting.Add(currset);
            }
            return string.Join<string>(", ", setting);
        }

        //  FS  ( E 1C 28 45 04 00 41 02 30/31 30/31
        internal static string DecodeFsEnableDisableTopBottomLogoPrinting(EscPosCmd record, int index)
        {
            string loc = record.cmddata[index] switch
            {
                48 => "Top Logo",
                49 => "Bottom Logo",
                _ => "Undefined",
            };
            string mode = record.cmddata[index + 1] switch
            {
                48 => "Enable",
                49 => "Disable",
                _ => "Undefined",
            };
            return $"Location:{loc}, Mode:{mode}";
        }

        //  FS  ( L 1C 28 4C 0008-001a 21 30-33 [[30-39]... 3B]...
        internal static string DecodeFsPaperLayoutSetting(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 8) || (length > 26))
            {
                return "Length out of range";
            }
            string layoutrefs = record.cmddata[index + 3] switch
            {
                48 => "Receipt(no black mark): do not use layout",
                49 => "Die cut label paper(no black mark): Print Label top edge, Eject Label bottom edge",
                50 => "Die cut label paper(black mark): Print Black mark bottom edge, Eject Black mark top edge",
                51 => "Receipt(black mark): Print Black mark top edge, Eject Black mark top edge",
                _ => "Undefined",
            };
            string layoutvalues = ascii.GetString(record.cmddata, (index + 4), (length - 2));
            return $"Layout Reference:{layoutrefs}, Settings:{layoutvalues}";
        }

        //  FS  ( L 1C 28 4C 02 00 22 40/50
        internal static string DecodeFsPaperLayoutInformationTransmission(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                64 => "Setting value",
                80 => "Effective value",
                _ => "Undefined",
            };
        }

        //  FS  ( L 1C 28 4C 02 00 41 30/31
        internal static string DecodeFsFeedPaperLabelPeelingPosition(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                48 => "if the paper is in standby at the label peeling position, the printer does not feed.",
                49 => "if the paper is in standby at the label peeling position, the printer feeds paper to the next label peeling position.",
                _ => "Undefined",
            };
        }

        //  FS  ( L 1C 28 4C 02 00 42 30/31
        internal static string DecodeFsFeedPaperCuttingPosition(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                48 => "if the paper is in standby at the label cutting position, the printer does not feed.",
                49 => "if the paper is in standby at the label cutting position, the printer feeds paper to the next label cutting position.",
                _ => "Undefined",
            };
        }

        //  FS  ( L 1C 28 4C 02 00 43 30-32
        internal static string DecodeFsFeedPaperPrintStartingPosition(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                48 => "Feeds to next label, if the paper is in standby at the print starting position, the printer does not feed.",
                49 => "Feeds to next label, if the paper is in standby at the print starting position, the printer feeds paper to the next print starting position.",
                50 => "Feeds to current label, if the paper is in standby at the print starting position, the printer does not feed.",
                _ => "Undefined",
            };
        }

        //  FS  ( L 1C 28 4C 0002-0003 50 30-39 [30-39]
        internal static string DecodeFsPaperLayoutErrorSpecialMarginSetting(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 2) || (length > 3))
            {
                return "Length out of range";
            }
            string margin = ascii.GetString(record.cmddata, (index + 3), (length - 1));
            return $"Layout Error Special Margin:{margin} x 0.1mm";
        }

        //  FS  ( e 1C 28 65 02 00 33 b0000x000
        internal static string DecodeFsEnableDisableAutomaticStatusBackOptional(EscPosCmd record, int index)
        {
            return (record.cmddata[index] & 0x08) switch
            {
                0 => "Disabled",
                8 => "Enabled",
                _ => "Undefined",
            };
        }

        //c FS  ( f 1C 28 66 0002-FFFF [00-03/30-33 00-FF|00/01/30/31]...
        internal static string DecodeFsSelectMICRDataHandling(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 2) || ((length & 1) == 1))
            {
                return "Length out of range";
            }
            List<string> config = new() { };
            int count = length / 2;
            int cfgindex = index + 2;
            for (int i = 0; i < count; i++, cfgindex += 2)
            {
                string entry;
                byte n = record.cmddata[cfgindex];
                byte m = record.cmddata[cfgindex + 1];
                if ((n <= 3) || ((n >= 48) && (n <= 51)))
                {
                    entry = (n & 0x03) switch
                    {
                        0 => m switch
                        {
                            0 => "processing for unrecognized characters : Reading is stopped when a character that cannot be recognized is detected.",
                            _ => $"processing for unrecognized characters : The character that cannot be recognized is replaced with the character \"?\" and reading is continued. When the number of characters that are replaced with \"?\" becomes({m} + 1), the reading is stopped.",
                        },
                        1 => m switch
                        {
                            0 => "detailed information for the reading result : Not to add detailed information for an abnormal end.",
                            1 => "detailed information for the reading result : Add detailed information for an abnormal end.",
                            _ => "detailed information for the reading result : Undefined",
                        },
                        2 => m switch
                        {
                            0 => "no addition of the reading result in an abnormal end : The MICR function ends after transmission the reading result.",
                            48 => "no addition of the reading result in an abnormal end : The MICR function ends after transmission the reading result.",
                            1 => "no addition of the reading result in an abnormal end : The MICR function is continued after transmission the reading result only for the following abnormal ends",
                            49 => "no addition of the reading result in an abnormal end : The MICR function is continued after transmission the reading result only for the following abnormal ends",
                            _ => "no addition of the reading result in an abnormal end : Undefined",
                        },
                        3 => m switch
                        {
                            0 => "header for transmission data : The MICR function ends after transmission the reading result.",
                            48 => "header for transmission data : The MICR function ends after transmission the reading result.",
                            1 => "header for transmission data : The MICR function is continued after transmission the reading result only for the following abnormal ends",
                            49 => "header for transmission data : The MICR function is continued after transmission the reading result only for the following abnormal ends",
                            _ => "header for transmission data : Undefined",
                        },
                        _ => "",
                    };
                }
                else
                {
                    entry = "Function out of range";
                }
                config.Add(entry);
            }
            return string.Join<string>(", ", config);
        }

        //c FS  ( g 1C 28 67 02 00 20 30/31
        internal static string DecodeFsSelectImageScannerCommandSettings(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                48 => "Slip Image Scanner [Active Sheet = Check Paper]",
                49 => "Card Image Scanner [Active Sheet = Card]",
                _ => "Undefined",
            };
        }

        //c FS  ( g 1C 28 67 05 00 28 30 01/08 31/32 80-7F
        internal static string DecodeFsSetBasicOperationOfImageScanner(EscPosCmd record, int index)
        {
            string color = record.cmddata[index] switch
            {
                1 => "Monochrome",
                8 => "256 level gray scale",
                _ => "Undefined",
            };
            string sharpness = record.cmddata[index + 1] switch
            {
                49 => "No",
                50 => "Yes",
                _ => "Undefined",
            };
            sbyte[] signed = Array.ConvertAll(record.cmddata, b => unchecked((sbyte)b));
            string threshold = signed[index + 2].ToString("D", invariantculture);
            return $"ColorType:{color}, Sharpness:{sharpness}, ThresholdLevel:{threshold}";
        }

        //c FS  ( g 1C 28 67 05 00 29 00-62 00-E4 00/02-64 00/02-E6
        internal static string DecodeFsSetScanningArea(EscPosCmd record, int index)
        {
            byte x1 = record.cmddata[index];
            byte y1 = record.cmddata[index + 1];
            byte x2 = record.cmddata[index + 2];
            byte y2 = record.cmddata[index + 3];
            if (x1 > 98)
            {
                return "x1 value out of range";
            }
            if (y1 > 228)
            {
                return "y1 value out of range";
            }
            if ((x2 == 1) || (x2 > 100))
            {
                return "x2 value out of range";
            }
            if ((y2 == 1) || (y2 > 230))
            {
                return "y2 value out of range";
            }
            return $"x1:{x1}, y1:{y1}, x2:{x2}, y2:{y2}";
        }

        //c FS  ( g 1C 28 67 03 00 32 30-32 30-32
        internal static string DecodeFsSelectCompressionMethodForImageData(EscPosCmd record, int index)
        {
            byte m = record.cmddata[index];
            byte n = record.cmddata[index + 1];
            return m switch
            {
                48 => n switch
                {
                    48 => "RAW data does not compress",
                    49 => "BMP does not compress",
                    50 => "TIFF does not compress",
                    _ => "Not compress Undefined",
                },

                49 => n switch
                {
                    48 => "TIFF Compression with CCITT(Grp4)",
                    _ => "TIFF Undefined",
                },
                50 => n switch
                {
                    48 => "JPEG High compression rate",
                    49 => "JPEG Standard compression rate",
                    50 => "JPEG Low compression rate",
                    _ => "JPEG Undefined",
                },
                _ => "Undefined",
            };
        }

        //c FS  ( g 1C 28 67 02 00 38 00-0A
        internal static string DecodeFsDeleteCroppingArea(EscPosCmd record, int index)
        {
            byte area = record.cmddata[index];
            if (area > 10)
            {
                return "Area number value out of range";
            }
            return $"Area Number:{area}";
        }

        //c FS  ( g 1C 28 67 06 00 39 00-0A 00-64 00-E4 02-64 02-E6
        internal static string DecodeFsSetCroppingArea(EscPosCmd record, int index)
        {
            byte area = record.cmddata[index];
            byte x1 = record.cmddata[index + 1];
            byte y1 = record.cmddata[index + 2];
            byte x2 = record.cmddata[index + 3];
            byte y2 = record.cmddata[index + 4];
            if (area > 10)
            {
                return "Area number value out of range";
            }
            if (x1 > 98)
            {
                return "x1 value out of range";
            }
            if (y1 > 228)
            {
                return "y1 value out of range";
            }
            if ((x2 == 1) || (x2 > 100))
            {
                return "x2 value out of range";
            }
            if ((y2 == 1) || (y2 > 230))
            {
                return "y2 value out of range";
            }
            return $"Area Number:{area}, x1:{x1}, y1:{y1}, x2:{x2}, y2:{y2}";
        }

        //c FS  ( g 1C 28 67 02 00 3C 30-32
        internal static string DecodeFsSelectTransmissionFormatForImageScanningResult(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                48 => "Binary data format: max 65,535 bytes",
                49 => "Hexadecimal character string format",
                50 => "Binary data format: max 4,294,967,295 bytes",
                _ => "Undefined",
            };
        }

        //  FS  -   1C 2D 00-02/30-32
        internal static string DecodeFsTurnKanjiUnderlineMode(EscPosCmd record, int index)
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

        //  FS  2   1C 32 7721-777E/EC40-EC9E/FEA1-FEFE 00-FF x 72
        internal static string DecodeFsDefineUserDefinedKanjiCharacters2424(EscPosCmd record, int index)
        {
            byte c1 = record.cmddata[index];
            byte c2 = record.cmddata[index + 1];
            int code = ((int)c1 << 8) + (int)c2;
            int width = 24;
            int height = 24;
            record.somebinary = GetBitmap(width, height, ImageDataType.Column, record.cmddata, (index + 2), "1");
            return $"CharacterCode:{code:X}";
        }

        //  FS  2   1C 32 7721-777E/EC40-EC9E/FEA1-FEFE 00-FF x 60
        internal static string DecodeFsDefineUserDefinedKanjiCharacters2024(EscPosCmd record, int index)
        {
            byte c1 = record.cmddata[index];
            byte c2 = record.cmddata[index + 1];
            int code = ((int)c1 << 8) + (int)c2;
            int width = 20;
            int height = 24;
            record.somebinary = GetBitmap(width, height, ImageDataType.Column, record.cmddata, (index + 2), "1");
            return $"CharacterCode:{code:X}";
        }

        //  FS  2   1C 32 7721-777E/EC40-EC9E/FEA1-FEFE 00-FF x 32
        internal static string DecodeFsDefineUserDefinedKanjiCharacters1616(EscPosCmd record, int index)
        {
            byte c1 = record.cmddata[index];
            byte c2 = record.cmddata[index + 1];
            int code = ((int)c1 << 8) + (int)c2;
            int width = 16;
            int height = 16;
            record.somebinary = GetBitmap(width, height, ImageDataType.Column, record.cmddata, (index + 2), "1");
            return $"CharacterCode:{code:X}";
        }

        //  FS  ?   1C 3F 7721-777E/EC40-EC9E/FEA1-FEFE
        internal static string DecodeFsCancelUserDefinedKanjiCharacters(EscPosCmd record, int index)
        {
            byte c1 = record.cmddata[index];
            byte c2 = record.cmddata[index + 1];
            int code = ((int)c1 << 8) + (int)c2;
            return $"CharacterCode:{code:X}";
        }

        //  FS  C   1C 43 00-02/30-32
        internal static string DecodeFsSelectKanjiCharacterCodeSystem(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "JIS",
                48 => "JIS",
                1 => "ShiftJIS",
                49 => "ShiftJIS",
                2 => "ShiftJIS2004",
                50 => "ShiftJIS2004",
                _ => "Undefined",
            };
        }

        //  FS  S   1C 53 00-FF/00-20 00-FF/00-20
        internal static string DecodeFsSetKanjiCharacerSpacing(EscPosCmd record, int index)
        {
            return $"LeftSideSpacing:{record.cmddata[index]} dots, RightSideSpacing:{record.cmddata[index + 1]} dots";
        }

        //c FS  a 0 1C 61 30 b000000xx
        internal static string DecodeFsObsoleteReadCheckPaper(EscPosCmd record, int index)
        {
            return (record.cmddata[index] & 0x03) switch
            {
                0 => "E13B",
                1 => "CMC7",
                _ => "Reserved",
            };
        }

        //  FS  g 1 1C 67 31 00 00000000-000003FF 0001-0400 20-FF...
        internal static string DecodeFsObsoleteWriteNVUserMemory(EscPosCmd record, int index)
        {
            uint start = BitConverter.ToUInt32(record.cmddata, index);
            string startaddress = start < 0x400 ? "0x" + start.ToString("X8", invariantculture) : "Out of range";
            int write = BitConverter.ToUInt16(record.cmddata, (index + 4));
            string writesize = ((write != 0) && (write <= 0x400)) ? "0x" + write.ToString("X4", invariantculture) : "Out of range";
            return $"StartAddress:{startaddress}, Size:{writesize}";
        }

        //  FS  g 2 1C 67 32 00 00000000-000003FF 0001-0400
        internal static string DecodeFsObsoleteReadNVUserMemory(EscPosCmd record, int index)
        {
            uint start = BitConverter.ToUInt32(record.cmddata, index);
            string startaddress = start < 0x400 ? "0x" + start.ToString("X8", invariantculture) : "Out of range";
            int read = BitConverter.ToUInt16(record.cmddata, (index + 4));
            string readsize = ((read != 0) && (read <= 0x400)) ? "0x" + read.ToString("X4", invariantculture) : "Out of range";
            return $"StartAddress:{startaddress}, Size:{readsize}";
        }

        //  FS  p   1C 70 01-FF 00-03/30-33
        internal static string DecodeFsObsoletePrintNVBitimage(EscPosCmd record, int index)
        {
            byte imageno = record.cmddata[index];
            string imagenumber = imageno != 0 ? imageno.ToString("D", invariantculture) : "0=Unsupported";
            string scalling = record.cmddata[index + 1] switch
            {
                0 => "Normal",
                48 => "Normal",
                1 => "DoubleWidth",
                49 => "DoubleWidth",
                2 => "DoubleHeight",
                50 => "DoubleHeight",
                3 => "Quadruple",
                51 => "Quadruple",
                _ => "Undefined",
            };
            return $"NVImageNumber:{imagenumber}, Scalling:{scalling}";
        }

        //  FS  q   1C 71 01-FF [0001-03FF 0001-0240 00-FF...]...
        internal static string DecodeFsObsoleteDefineNVBitimage(EscPosCmd record, int index)
        {
            byte images = record.cmddata[index];
            string imagecount = images != 0 ? images.ToString("D", invariantculture) : "0=Unsupported";
            List<System.Drawing.Bitmap> imagelist = new();
            int i = index + 1;
            for (int n = 0; n < images; n++)
            {
                int width = BitConverter.ToUInt16(record.cmddata, i);
                int heightbytes = BitConverter.ToUInt16(record.cmddata, i + 2);
                int height = heightbytes * 8;
                i += 4;
                int datalength = width * heightbytes;
                if (((heightbytes > 0) && (heightbytes <= 0x240)) && ((width > 0) && (width <= 0x3FF)))
                {
                    imagelist.Add(GetBitmap(width, height, ImageDataType.Column, record.cmddata, i, "1"));
                }
                i += datalength;
            }
            record.somebinary = imagelist.ToArray();
            return $"NVImageCount:{imagecount}";
        }
    }
}