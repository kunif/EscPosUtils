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
    using System.IO;

    public static partial class EscPosDecoder
    {
        //  GS  !   1D 21 b0xxx0yyy
        internal static string DecodeGsSelectCharacterSize(EscPosCmd record, int index)
        {
            byte mode = record.cmddata[index];
            if ((mode & 0x88) != 0)
            {
                return "Undefined pattern value";
            }
            int h = (mode >> 4) + 1;
            int v = (mode & 7) + 1;
            return $"Horizontal:{h}, Vertical:{v}";
        }

        //  GS  ( A 1D 28 41 02 00 00-02/30-32 01-03/31-33/40
        internal static string DecodeGsExecuteTestPrint(EscPosCmd record, int index)
        {
            string paper = record.cmddata[index] switch
            {
                0 => "Basic sheet",
                48 => "Basic sheet",
                1 => "Roll paper",
                49 => "Roll paper",
                2 => "Roll paper",
                50 => "Roll paper",
                3 => "Slip(face)",
                51 => "Slip(face)",
                4 => "Validation",
                52 => "Validation",
                5 => "Slip(back)",
                53 => "Slip(back)",
                _ => "Undefined",
            };
            string pattern = record.cmddata[index + 1] switch
            {
                1 => "Hexadecimal dump",
                49 => "Hexadecimal dump",
                2 => "Printer status",
                50 => "Printer status",
                3 => "Rolling pattern",
                51 => "Rolling pattern",
                64 => "Automatic setting of paper layout",
                _ => "Undefined",
            };
            return $"Print to:{paper}, Test pattern:{pattern}";
        }

        //c GS  ( B 1D 28 42 0002/0003/0005/0007/0009 61 00|[31/33/45/46 2C/2D/37/38]...
        internal static string DecodeGsCustomizeASBStatusBits(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length == 2)
            {
                return (record.cmddata[index + 4] == 0) ? "All Disabled" : "Undefined";
            }
            else if ((length < 2) || (length > 9) || ((length & 1) == 0))
            {
                return "Length out of range";
            }
            // 49,44 "1,", 51,45 "3-", 69,56 "E8", 70,55 "F7", xx,54 "?6"
            int count = (length - 1) / 2;
            List<string> status = new();
            for (int i = 0, currindex = 6; i < count; i++, currindex += 2)
            {
                string entry = ascii.GetString(record.cmddata, currindex, 2) switch
                {
                    "1," => "cut sheet insertion waiting status",
                    "3-" => "cut sheet removal waiting status",
                    "E8" => "card sensor status",
                    "F7" => "slip paper ejection sensor status",
                    //"?6" => "paper width sensor status",
                    _ => "Undefined",
                };
                status.Add(entry);
            }
            return string.Join<string>(", ", status);
        }

        //  GS  ( C 1D 28 43 05 00 00 00/30 00 20-7E 20-7E
        internal static string DecodeGsDeleteSpecifiedRecord(EscPosCmd record, int index)
        {
            byte c1 = record.cmddata[index];
            byte c2 = record.cmddata[index + 1];
            return $"Code1:{c1:X}, Code2:{c2:X}";
        }

        //  GS  ( C 1D 28 43 0006-FFFF 00 01/31 00 20-7E 20-7E 20-FF...
        internal static string DecodeGsStoreDataSpecifiedRecord(EscPosCmd record, int index)
        {
            byte c1 = record.cmddata[index];
            byte c2 = record.cmddata[index + 1];
            return $"Code1:{c1:X}, Code2:{c2:X}";
        }

        //  GS  ( C 1D 28 43 05 00 00 02/32 00 20-7E 20-7E
        internal static string DecodeGsTransmitDataSpecifiedRecord(EscPosCmd record, int index)
        {
            byte c1 = record.cmddata[index];
            byte c2 = record.cmddata[index + 1];
            return $"Code1:{c1:X}, Code2:{c2:X}";
        }

        //  GS  ( D 1D 28 44 0003/0005 14 [01/02 00/01/30/31]...
        internal static string DecodeGsEnableDisableRealtimeCommand(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length != 3) && (length != 5))
            {
                return "Length out of range";
            }
            int count = (length - 1) / 2;
            List<string> cmds = new();
            for (int i = 0, currindex = 6; i < count; i++, currindex += 2)
            {
                string cmdtype = record.cmddata[currindex] switch
                {
                    1 => "Generate pulse in real-time",
                    2 => "Execute power-off sequence",
                    _ => "Undefined",
                };
                string enable = record.cmddata[currindex + 1] switch
                {
                    0 => "Disable",
                    48 => "Disable",
                    1 => "Enable",
                    49 => "Enable",
                    _ => "Undefined",
                };
                cmds.Add($"Type:{cmdtype}, {enable}");
            }
            return string.Join<string>(", ", cmds);
        }

        //  GS  ( E 1D 28 45 000A-FFFA 03 [01-08 30-32...]...
        internal static string DecodeGsChangeMeomorySwitch(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, 3);
            if ((length < 10) && (length > 65530))
            {
                return "Out of range";
            }
            else if (((length - 1) % 9) != 0)
            {
                return "Miss align length";
            }
            int count = (length - 1) / 9;
            List<string> memorys = new();
            for (int i = 0, currindex = 6; i < count; i++, currindex += 9)
            {
                string msw = record.cmddata[currindex] switch
                {
                    1 => "Msw1",
                    2 => "Msw2",
                    3 => "Msw3",
                    4 => "Msw4",
                    5 => "Msw5",
                    6 => "Msw6",
                    7 => "Msw7",
                    8 => "Msw8",
                    _ => "Undefined",
                };
                string setting = ascii.GetString(record.cmddata, (currindex + 1), 8).Replace('2', '_');
                memorys.Add($"MemorySwitch:{msw} Setting:{setting}");
            }
            return string.Join<string>(", ", memorys);
        }

        //  GS  ( E 1D 28 45 02 00 04 01-08
        internal static string DecodeGsTransmitSettingsMemorySwitch(EscPosCmd record, int index)
        {
            byte m = record.cmddata[index];
            if ((m >= 1) && (m <= 8))
            {
                return "MemorySwitch:Msw" + m.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( E 1D 28 45 0004-FFFD 05 [01-03/05-0D/14-16/46-48/61/62/64-69/6F/70/74-C2 0000-FFFF]...
        internal static string DecodeGsSetCustomizeSettingValues(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, 3);
            if ((length < 4) && (length > 65533))
            {
                return "Out of range";
            }
            else if (((length - 1) % 3) != 0)
            {
                return "Miss align length";
            }
            int count = (length - 1) / 3;
            List<string> memorys = new();
            for (int i = 0, currindex = 6; i < count; i++, currindex += 3)
            {
                byte swno = record.cmddata[currindex];
                string swnostr;
                if (((swno >= 1) && (swno <= 14))
                    || ((swno >= 20) && (swno <= 22))
                    || (swno == 70) || (swno == 71) || (swno == 73)
                    || (swno == 97) || (swno == 98)
                    || ((swno >= 100) && (swno <= 106))
                    || (swno == 111) || (swno == 112)
                    || ((swno >= 116) && (swno <= 194)))
                {
                    swnostr = swno.ToString("D", invariantculture);
                }
                else
                {
                    return "Out of range";
                }
                int msv = BitConverter.ToUInt16(record.cmddata, (currindex + 1));
                memorys.Add($"MemorySwitch:{swnostr} Setting:{msv:D}");
            }
            return string.Join<string>(", ", memorys);
        }

        //  GS  ( E 1D 28 45 02 00 06 01-03/05-0D/14-16/46-48/61/62/64-69/6F-71/74-C1
        internal static string DecodeGsTransmitCustomizeSettingValues(EscPosCmd record, int index)
        {
            byte swno = record.cmddata[index];
            if (((swno >= 1) && (swno <= 14))
                || ((swno >= 20) && (swno <= 22))
                || (swno == 70) || (swno == 71) || (swno == 73)
                || (swno == 97) || (swno == 98)
                || ((swno >= 100) && (swno <= 106))
                || (swno >= 111) || (swno <= 113)
                || ((swno >= 116) && (swno <= 195)))
            {
                return swno.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( E 1D 28 45 04 00 07 0A/0C/11/12 1D/1E 1E/1D
        internal static string DecodeGsCopyUserDefinedPage(EscPosCmd record, int index)
        {
            string font = record.cmddata[index] switch
            {
                10 => "Width 9 dot, Hwight 17 dot",
                12 => "Width 12 dot, Hwight 24 dot",
                17 => "Width 8 dot, Hwight 16 dot",
                18 => "Width 10 dot, Hwight 24 dot",
                _ => "Undefined",
            };
            string direction = ascii.GetString(record.cmddata, (index + 1), 2) switch
            {
                "\x1E\x1D" => "FromStorage ToWork",
                "\x1D\x1E" => "FromWork ToStorage",
                _ => "Undefined",
            };
            return $"Font size:{font}, Direction:{direction}";
        }

        //  GS  ( E 1D 28 45 0005-FFFF 08 02/03 20-7E 20-7E [08/09/0A/0C 00-FF...]...
        internal static string DecodeGsDefineColumnFormatCharacterCodePage(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length <= 5)
            {
                return "Length out of range";
            }
            int y = record.cmddata[index + 3];
            string ysize = y switch
            {
                2 => "2",
                3 => "3",
                _ => "Undefined",
            };
            byte c1 = record.cmddata[index + 4];
            byte c2 = record.cmddata[index + 5];
            int count = c2 - c1 + 1;
            List<string> fonts = new();
            List<System.Drawing.Bitmap> glyphs = new();
            for (int i = 0, currindex = 9; (i < count) && (currindex < record.cmdlength); i++)
            {
                int x = record.cmddata[currindex];
                string xsize = x switch
                {
                    8 => "8",
                    9 => "9",
                    10 => "10",
                    12 => "12",
                    _ => "Undefined",
                };
                int ybits = x switch
                {
                    8 => 16,
                    9 => 17,
                    10 => 24,
                    12 => 24,
                    _ => 24,
                };
                int fdsize = (x * y);
                System.Drawing.Bitmap bitmap = new(ybits, x, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                ColorPalette palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = Color.Black;
                bitmap.Palette = palette;
                if (fdsize > 0)
                {
                    System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                    IntPtr ptr = bmpData.Scan0;
                    int stride = bmpData.Stride;
                    if (stride == y)
                    {
                        System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (i + 1), ptr, fdsize);
                    }
                    else
                    {
                        for (int j = 0; j < x; j++)
                        {
                            IntPtr curptr = bmpData.Scan0 + stride * j;
                            System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (i + 1 + (y * j)), curptr, y);
                        }
                    }
                    bitmap.UnlockBits(bmpData);
                }
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                glyphs.Add(bitmap);

                fonts.Add($"X size:{xsize} dot, this Length:{fdsize}");
                currindex += fdsize + 1;
            }
            record.somebinary = glyphs.ToArray();
            string fdlist = string.Join<string>(", ", fonts);
            return $"Length:{length}, Y size:{ysize} byte, 1st code:{c1:X}, Last code:{c2:X}, Each data: {fdlist}";
        }

        //  GS  ( E 1D 28 45 0005-FFFF 09 01/02 20-7E 20-7E [10/11/18 00-FF...]...
        internal static string DecodeGsDefineRasterFormatCharacterCodePage(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length <= 5)
            {
                return "Length out of range";
            }
            int x = record.cmddata[index + 3];
            string xsize = x switch
            {
                1 => "1",
                2 => "2",
                _ => "Undefined",
            };
            byte c1 = record.cmddata[index + 4];
            byte c2 = record.cmddata[index + 5];
            int count = c2 - c1 + 1;
            List<string> fonts = new();
            List<System.Drawing.Bitmap> glyphs = new();
            for (int i = 0, currindex = 9; (i < count) && (currindex < record.cmdlength); i++)
            {
                int y = record.cmddata[currindex];
                string ysize = y switch
                {
                    16 => "16",
                    17 => "17",
                    24 => "24",
                    _ => "Undefined",
                };
                int fdsize = (x * y);
                System.Drawing.Bitmap bitmap = new((x * 8), y, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                ColorPalette palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = Color.Black;
                bitmap.Palette = palette;
                System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                IntPtr ptr = bmpData.Scan0;
                int stride = bmpData.Stride;
                if (stride == x)
                {
                    System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (i + 1), ptr, fdsize);
                }
                else
                {
                    for (int j = 0; j < y; j++)
                    {
                        IntPtr curptr = bmpData.Scan0 + stride * j;
                        System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (i + 1 + (x * j)), curptr, x);
                    }
                }
                bitmap.UnlockBits(bmpData);
                glyphs.Add(bitmap);

                fonts.Add($"Y size:{ysize} dot, this Length:{fdsize}");
                currindex += fdsize + 1;
            }
            record.somebinary = glyphs.ToArray();
            string fdlist = string.Join<string>(", ", fonts);
            return $"Length:{length}, X size:{xsize} byte, 1st code:{c1:X}, Last code:{c2:X}, Each data: {fdlist}";
        }

        //  GS  ( E 1D 28 45 03 00 0A 80-FF 80-FF
        internal static string DecodeGsDeleteCharacterCodePage(EscPosCmd record, int index)
        {
            byte c1 = record.cmddata[index];
            byte c2 = record.cmddata[index + 1];
            return $"1st code:{c1:X}, Last code:{c2:X}";
        }

        //  GS  ( E 1D 28 45 0003-0008 0B 01-04 30-39...
        internal static string DecodeGsSetSerialInterface(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            int mode = record.cmddata[index + 3];
            int data = record.cmddata[index + 4];
            string config = mode switch
            {
                1 => "Transmission speed",
                2 => "Parity",
                3 => "Flow control",
                4 => "Data bits length",
                _ => "Undefined",
            };
            string value = mode switch
            {
                1 => ascii.GetString(record.cmddata, 4, (length - 2)),
                2 => data switch
                {
                    48 => "None parity",
                    49 => "Odd parity",
                    50 => "Even parity",
                    _ => "Undefined parity",
                },
                3 => data switch
                {
                    48 => "Flow control of DTR/DSR",
                    49 => "Flow control of XON/XOFF",
                    _ => "Undefined flow control",
                },
                4 => data switch
                {
                    55 => "7 bits length",
                    56 => "8 bits length",
                    _ => "Undefined bits length",
                },
                _ => "Undefined",
            };
            return $"Setting:{config}, Value:{value}";
        }

        //  GS  ( E 1D 28 45 02 00 0C 01-04
        internal static string DecodeGsTransmitSerialInterface(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                1 => "Transmission speed",
                2 => "Parity",
                3 => "Flow control",
                4 => "Data bits length",
                _ => "Undefined",
            };
        }

        //  GS  ( E 1D 28 45 0003-0021 0D [31/41/46/49 20-7E...]...
        internal static string DecodeGsSetBluetoothInterface(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            string config = record.cmddata[index + 3] switch
            {
                49 => "Passkey",
                65 => "Device name",
                70 => "Bundle Seed ID",
                73 => "Automatic reconnection with iOS device",
                _ => "Undefined",
            };
            string value = ascii.GetString(record.cmddata, 4, (length - 2));
            return $"Setting:{config}, Value:{value}";
        }

        //  GS  ( E 1D 28 45 02 00 0E 30/31/41/46/49
        internal static string DecodeGsTransmitBluetoothInterface(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                48 => "Device address",
                49 => "Passkey",
                65 => "Device name",
                70 => "Bundle Seed ID",
                73 => "Automatic reconnection with iOS device",
                _ => "Undefined",
            };
        }

        //  GS  ( E 1D 28 45 03 00 0F 01/20 30/31
        internal static string DecodeGsSetUSBInterface(EscPosCmd record, int index)
        {
            int value = record.cmddata[index + 1];
            return record.cmddata[index] switch
            {
                1 => value switch
                {
                    48 => "Class settings: Vendor-defined class",
                    49 => "Class settings: Printer class",
                    _ => "Undefined class settings",
                },
                32 => value switch
                {
                    48 => "IEEE1284 DeviceID settings: Do not transmit",
                    49 => "IEEE1284 DeviceID settings: Transmits",
                    _ => "Undefined IEEE1284 DeviceID settings",
                },
                _ => "Undefined",
            };
        }

        //  GS  ( E 1D 28 45 02 00 10 01/20
        internal static string DecodeGsTransmitUSBInterface(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                1 => "Class settings",
                32 => "IEEE1284 DeviceID settings",
                _ => "Undefined",
            };
        }

        //  GS  ( E 1D 28 45 0009-0024 31 {34 38/34 39/36 34} 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B
        internal static string DecodeGsSetPaperLayout(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            string mode = record.cmddata[index + 3] switch
            {
                48 => "None(does not use layout)",
                49 => "Top of black mark",
                64 => "Bottom of label",
                _ => "Undefined",
            };
            string value = ascii.GetString(record.cmddata, 8, (length - 3));
            return $"Setting:{mode}, Value:{value}";
        }

        //  GS  ( E 1D 28 45 02 00 32 40/50
        internal static string DecodeGsTransmitPaperLayout(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                64 => "Setting value of the paper layout (unit: 0.1 mm {0.004\"})",
                80 => "Actual value of the paper layout (unit: dot)",
                _ => "Undefined",
            };
        }

        //  GS  ( E 1D 28 45 002D-0048 33 20-7E...
        internal static string DecodeGsSetControlLabelPaperAndBlackMarks(EscPosCmd record, int index)
        {
            return "T.B.D.";
        }

        //  GS  ( E 1D 28 45 11 00 34 20-7E... 00
        internal static string DecodeGsTransmitControlSettingsLabelPaperAndBlackMarks(EscPosCmd record, int index)
        {
            return "T.B.D.";
        }

        //  GS  ( E 1D 28 45 000E-FFFF 63 [01-05 [00/01 00-64]...]...
        internal static string DecodeGsSetInternalBuzzerPatterns(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 14) || (((length - 1) % 13) != 0))
            {
                return "Length out of range or alignment";
            }
            int count = (length - 1) / 13;
            List<string> beeps = new();
            for (int i = 0, currindex = 6; (i < count) && (currindex < record.cmdlength); i++)
            {
                string n = record.cmddata[currindex] switch
                {
                    1 => "A",
                    2 => "B",
                    3 => "C",
                    4 => "D",
                    5 => "E",
                    _ => "Undefined",
                };
                List<string> onoff = new();
                currindex++;
                for (int j = 0; j < 6; j++, currindex += 2)
                {
                    string sound = record.cmddata[currindex] switch
                    {
                        0 => "Off",
                        1 => "On",
                        _ => "Undefined",
                    };
                    string duration = record.cmddata[currindex + 1] <= 100 ? record.cmddata[currindex + 1].ToString("D", invariantculture) : "Out of range";
                    onoff.Add($"Sound:{sound}, Duration:{duration} x 100ms");
                }
                beeps.Add($"Pattern:{n} dot, this Length:{string.Join<string>(", ", onoff)}");
            }
            string beeplist = string.Join<string>(", ", beeps);
            return $"Length:{length}, Each data: {beeplist}";
        }

        //  GS  ( E 1D 28 45 02 00 64 01-05
        internal static string DecodeGsTransmitInternalBuzzerPatterns(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                1 => "A",
                2 => "B",
                3 => "C",
                4 => "D",
                5 => "E",
                _ => "Undefined",
            };
        }

        //c GS  ( G 1D 28 47 02 00 30 04/44
        internal static string DecodeGsSelectSideOfSlipFaceOrBack(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                4 => "Face of slip",
                68 => "Back of slip",
                _ => "Undefined",
            };
        }

        //c GS  ( G 1D 28 47 04 00 3C 01 00 00/01
        internal static string DecodeGsReadMagneticInkCharacterAndTransmitReadingResult(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "E13B.",
                1 => "CMC7.",
                _ => "Undefined",
            };
        }

        //c GS  ( G 1D 28 47 0005-0405 40 0000-FFFF 30 01-03 00/01 30 0000 00-FF...
        internal static string DecodeGsReadDataAndTransmitResultingInformation(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 5) || (length > 1029))
            {
                return "Length out of range";
            }
            int dataid = BitConverter.ToUInt16(record.cmddata, (index + 3));
            string scanning = record.cmddata[index + 6] switch
            {
                1 => "Magnetic ink character",
                2 => "Image data",
                3 => "Magnetic ink character and Image data",
                _ => "Undefined",
            };
            string font = record.cmddata[index] switch
            {
                0 => "E13B.",
                1 => "CMC7.",
                _ => "Undefined",
            };
            return $"Length:{length}, Data ID:{dataid}, Scanning:{scanning}, Font:{font}";
        }

        //c GS  ( G 1D 28 47 0005-0405 41 0001-FFFF 30/31 30 00-FF...
        internal static string DecodeGsScanImageDataAndTransmitImageScanningResult(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 5) || (length > 1029))
            {
                return "Length out of range";
            }
            int dataid = BitConverter.ToUInt16(record.cmddata, (index + 3));
            string store = record.cmddata[index + 5] switch
            {
                48 => "To Work area temporarily",
                49 => "To NV Memory Image data storage",
                _ => "Undefined",
            };
            return $"Length:{length}, Data ID:{dataid}, Store:{store}";
        }

        //c GS  ( G 1D 28 47 0003-0004 42 0001-FFFF [30/31]
        internal static string DecodeGsRetransmitImageScanningResult(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 3) || (length > 4))
            {
                return "Length out of range";
            }
            int dataid = BitConverter.ToUInt16(record.cmddata, (index + 3));
            string side = length switch
            {
                3 => "No specified side",
                4 => record.cmddata[index + 5] switch
                {
                    48 => "Face side",
                    49 => "Back side",
                    _ => "Undefined",
                },
                _ => "Undefined",
            };
            return $"Length:{length}, Data ID:{dataid}, Side:{side}";
        }

        //c GS  ( G 1D 28 47 04 00 44 30 0001-FFFF
        internal static string DecodeGsDeleteImageScanningResultWithSpecifiedDataID(EscPosCmd record, int index)
        {
            int dataid = BitConverter.ToUInt16(record.cmddata, index);
            return $"Data ID:{dataid}";
        }

        //c GS  ( G 1D 28 47 02 00 50 b00xxxxxx
        internal static string DecodeGsSelectActiveSheet(EscPosCmd record, int index)
        {
            byte mode = record.cmddata[index];
            string check = (mode & 0x20) == 0x20 ? "Select" : "Do not Select";
            string card = (mode & 0x10) == 0x10 ? "Select" : "Do not Select";
            string validation = (mode & 0x08) == 0x08 ? "Select" : "Do not Select";
            string slip = (mode & 0x04) == 0x04 ? "Select" : "Do not Select";
            string roll = (mode & 0x03) switch
            {
                0 => "Disable",
                1 => "Active Sheet",
                2 => "Active Sheet",
                3 => "Enable",
                _ => "",
            };
            return $"Check:{check}, Card:{card}, Validation:{validation}, Slip:{slip}, Roll paper:{roll}";
        }

        //c GS  ( G 1D 28 47 02 00 55 30/31
        internal static string DecodeGsFinishProcessingOfCutSheet(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                48 => "Ejecting operation",
                49 => "Releasing operation",
                _ => "Undefined",
            };
        }

        //  GS  ( H 1D 28 48 06 00 30 30 20-7E 20-7E 20-7E 20-7E
        internal static string DecodeGsSpecifiesProcessIDResponse(EscPosCmd record, int index)
        {
            return ascii.GetString(record.cmddata, index, 4);
        }

        //  GS  ( H 1D 28 48 03 00 31 30 00-02/30-32
        internal static string DecodeGsSpecifiesOfflineResponse(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "Turns off the offline response transmission.",
                48 => "Turns off the offline response transmission.",
                1 => "Specifies the offline response transmission (not including the offline cause).",
                49 => "Specifies the offline response transmission (not including the offline cause).",
                2 => "Specifies the offline response transmission (including the offline cause).",
                50 => "Specifies the offline response transmission (including the offline cause).",
                _ => "Undefined",
            };
        }

        //  GS  ( K 1D 28 4B 02 00 30 00-04/30-34
        internal static string DecodeGsSelectPrintControlMode(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "Print mode when power is turned on",
                48 => "Print mode when power is turned on",
                1 => "Print control mode 1",
                49 => "Print control mode 1",
                2 => "Print control mode 2",
                50 => "Print control mode 2",
                3 => "Print control mode 3",
                51 => "Print control mode 3",
                4 => "Print control mode 4",
                52 => "Print control mode 4",
                _ => "Undefined",
            };
        }

        //  GS  ( K 1D 28 4B 02 00 31 80-7F
        internal static string DecodeGsSelectPrintDensity(EscPosCmd record, int index)
        {
            return "Criterion density" + record.cmddata[index] switch
            {
                250 => " x 70%",
                251 => " x 75%",
                252 => " x 80%",
                253 => " x 85%",
                254 => " x 90%",
                255 => " x 95%",
                0 => "",
                1 => " x 105%",
                2 => " x 110%",
                3 => " x 115%",
                4 => " x 120%",
                5 => " x 125%",
                6 => " x 130%",
                7 => " x 135%",
                8 => " x 140%",
                _ => " Undefined",
            };
        }

        //  GS  ( K 1D 28 4B 02 00 32 00-0D/30-39
        internal static string DecodeGsSelectPrintSpeed(EscPosCmd record, int index)
        {
            return "Print speed level " + record.cmddata[index] switch
            {
                0 => "Customized value",
                48 => "Customized value",
                1 => "1",
                49 => "1",
                2 => "2",
                50 => "2",
                3 => "3",
                51 => "3",
                4 => "4",
                52 => "4",
                5 => "5",
                53 => "5",
                6 => "6",
                54 => "6",
                7 => "7",
                55 => "7",
                8 => "8",
                56 => "8",
                9 => "9",
                57 => "9",
                10 => "10",
                58 => "10",
                11 => "11",
                59 => "11",
                12 => "12",
                13 => "13",
                14 => "14",
                _ => "Undefined",
            };
        }

        //  GS  ( K 1D 28 4B 02 00 61 00-04/30-34/80
        internal static string DecodeGsSelectNumberOfPartsThermalHeadEnergizing(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "Customized value",
                48 => "Customized value",
                1 => "One-part",
                49 => "One-part",
                2 => "Two-part",
                50 => "Two-part",
                3 => "Three-part",
                51 => "Three-part",
                4 => "Four-part",
                52 => "Four-part",
                14 => "Automatic control",
                _ => "Undefined",
            } + " energizing";
        }

        //  GS  ( L 1D 28 4C 000C-FFFF 30 43 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
        internal static string DecodeGsDefineNVGraphicsDataRasterW(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length < 12)
            {
                return "Length out of range";
            }
            string tone = record.cmddata[index + 4] switch
            {
                48 => "Monochrome",
                52 => "Multiple tone",
                _ => "Undefined",
            };
            string keycode = ascii.GetString(record.cmddata, (index + 5), 2);
            int plane = record.cmddata[index + 7];
            int width = BitConverter.ToUInt16(record.cmddata, (index + 8));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int size = ((width + 7) / 8) * height;
            List<string> buffers = new();
            List<System.Drawing.Bitmap> planes = new();
            for (int i = 0, currindex = (index + 12); (i < plane) && (currindex < record.cmdlength); i++)
            {
                string c = record.cmddata[currindex] switch
                {
                    49 => "1",
                    50 => "2",
                    51 => "3",
                    52 => "4",
                    _ => "Undefined",
                };
                System.Drawing.Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                ColorPalette palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = c switch
                {
                    "1" => Color.Black,
                    "2" => Color.Red,
                    "3" => Color.Green,
                    "4" => Color.Blue,
                    _ => Color.Yellow,
                };
                bitmap.Palette = palette;
                System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                IntPtr ptr = bmpData.Scan0;
                int stride = bmpData.Stride;
                int linesize = (width + 7) / 8;
                if (stride == linesize)
                {
                    System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1), ptr, size);
                }
                else
                {
                    for (int j = 0; j < height; j++)
                    {
                        IntPtr curptr = bmpData.Scan0 + stride * j;
                        System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1 + (linesize * j)), curptr, linesize);
                    }
                }
                bitmap.UnlockBits(bmpData);
                planes.Add(bitmap);

                currindex += size + 1;
                buffers.Add($"Color:{c}, Size:{size}");
            }
            record.somebinary = planes.ToArray();
            string bufferlist = string.Join<string>(", ", buffers);
            return $"Length:{length}, Tone:{tone}, KeyCode:{keycode}, Width:{width}, Height:{height}, Plane:{plane}, BufferList:{bufferlist}";
        }

        //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 43 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
        internal static string DecodeGsDefineNVGraphicsDataRasterDW(EscPosCmd record, int index)
        {
            long length = BitConverter.ToUInt32(record.cmddata, index);
            if (length < 12)
            {
                return "Length out of range";
            }
            string tone = record.cmddata[index + 6] switch
            {
                48 => "Monochrome",
                52 => "Multiple tone",
                _ => "Undefined",
            };
            string keycode = ascii.GetString(record.cmddata, (index + 7), 2);
            int plane = record.cmddata[index + 9];
            int width = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 12));
            int size = ((width + 7) / 8) * height;
            List<string> buffers = new();
            List<System.Drawing.Bitmap> planes = new();
            for (int i = 0, currindex = (index + 14); (i < plane) && (currindex < record.cmdlength); i++)
            {
                string c = record.cmddata[currindex] switch
                {
                    49 => "1",
                    50 => "2",
                    51 => "3",
                    52 => "4",
                    _ => "Undefined",
                };
                System.Drawing.Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                ColorPalette palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = c switch
                {
                    "1" => Color.Black,
                    "2" => Color.Red,
                    "3" => Color.Green,
                    "4" => Color.Blue,
                    _ => Color.Yellow,
                };
                bitmap.Palette = palette;
                System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                IntPtr ptr = bmpData.Scan0;
                int stride = bmpData.Stride;
                int linesize = (width + 7) / 8;
                if (stride == linesize)
                {
                    System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1), ptr, size);
                }
                else
                {
                    for (int j = 0; j < height; j++)
                    {
                        IntPtr curptr = bmpData.Scan0 + stride * j;
                        System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1 + (linesize * j)), curptr, linesize);
                    }
                }
                bitmap.UnlockBits(bmpData);
                planes.Add(bitmap);

                currindex += size + 1;
                buffers.Add($"Color:{c}, Size:{size}");
            }
            record.somebinary = planes.ToArray();
            string bufferlist = string.Join<string>(", ", buffers);
            return $"Length:{length}, Tone:{tone}, KeyCode:{keycode}, Width:{width}, Height:{height}, Plane:{plane}, BufferList:{bufferlist}";
        }

        //  GS  ( L 1D 28 4C 000C-FFFF 30 44 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
        internal static string DecodeGsDefineNVGraphicsDataColumnW(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length < 12)
            {
                return "Length out of range";
            }
            string tone = record.cmddata[index + 4] switch
            {
                48 => "Monochrome",
                52 => "Multiple tone",
                _ => "Undefined",
            };
            string keycode = ascii.GetString(record.cmddata, (index + 5), 2);
            int plane = record.cmddata[index + 7];
            int width = BitConverter.ToUInt16(record.cmddata, (index + 8));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int size = width * ((height + 7) / 8);
            List<string> buffers = new();
            List<System.Drawing.Bitmap> planes = new();
            for (int i = 0, currindex = (index + 12); (i < plane) && (currindex < record.cmdlength); i++)
            {
                string c = record.cmddata[currindex] switch
                {
                    49 => "1",
                    50 => "2",
                    51 => "3",
                    52 => "4",
                    _ => "Undefined",
                };
                System.Drawing.Bitmap bitmap = new(height, width, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                ColorPalette palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = c switch
                {
                    "1" => Color.Black,
                    "2" => Color.Red,
                    "3" => Color.Green,
                    "4" => Color.Blue,
                    _ => Color.Yellow,
                };
                bitmap.Palette = palette;
                System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                IntPtr ptr = bmpData.Scan0;
                int stride = bmpData.Stride;
                int linesize = (height + 7) / 8;
                if (stride == linesize)
                {
                    System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1), ptr, size);
                }
                else
                {
                    for (int j = 0; j < width; j++)
                    {
                        IntPtr curptr = bmpData.Scan0 + stride * j;
                        System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1 + (linesize * j)), curptr, linesize);
                    }
                }
                bitmap.UnlockBits(bmpData);
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                planes.Add(bitmap);

                currindex += size + 1;
                buffers.Add($"Color:{c}, Size:{size}");
            }
            record.somebinary = planes.ToArray();
            string bufferlist = string.Join<string>(", ", buffers);
            return $"Length:{length}, Tone:{tone}, KeyCode:{keycode}, Width:{width}, Height:{height}, Plane:{plane}, BufferList:{bufferlist}";
        }

        //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 44 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
        internal static string DecodeGsDefineNVGraphicsDataColumnDW(EscPosCmd record, int index)
        {
            long length = BitConverter.ToUInt32(record.cmddata, index);
            if (length < 12)
            {
                return "Length out of range";
            }
            string tone = record.cmddata[index + 6] switch
            {
                48 => "Monochrome",
                52 => "Multiple tone",
                _ => "Undefined",
            };
            string keycode = ascii.GetString(record.cmddata, (index + 7), 2);
            int plane = record.cmddata[index + 9];
            int width = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 12));
            int size = width * ((height + 7) / 8);
            List<string> buffers = new();
            List<System.Drawing.Bitmap> planes = new();
            for (int i = 0, currindex = (index + 14); (i < plane) && (currindex < record.cmdlength); i++)
            {
                string c = record.cmddata[currindex] switch
                {
                    49 => "1",
                    50 => "2",
                    51 => "3",
                    52 => "4",
                    _ => "Undefined",
                };
                System.Drawing.Bitmap bitmap = new(height, width, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                ColorPalette palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = c switch
                {
                    "1" => Color.Black,
                    "2" => Color.Red,
                    "3" => Color.Green,
                    "4" => Color.Blue,
                    _ => Color.Yellow,
                };
                bitmap.Palette = palette;
                System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                IntPtr ptr = bmpData.Scan0;
                int stride = bmpData.Stride;
                int linesize = (height + 7) / 8;
                if (stride == linesize)
                {
                    System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1), ptr, size);
                }
                else
                {
                    for (int j = 0; j < width; j++)
                    {
                        IntPtr curptr = bmpData.Scan0 + stride * j;
                        System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1 + (linesize * j)), curptr, linesize);
                    }
                }
                bitmap.UnlockBits(bmpData);
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                planes.Add(bitmap);

                currindex += size + 1;
                buffers.Add($"Color:{c}, Size:{size}");
            }
            record.somebinary = planes.ToArray();
            string bufferlist = string.Join<string>(", ", buffers);
            return $"Length:{length}, Tone:{tone}, KeyCode:{keycode}, Width:{width}, Height:{height}, Plane:{plane}, BufferList:{bufferlist}";
        }

        //  GS  ( L 1D 28 4C 000C-FFFF 30 53 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
        internal static string DecodeGsDefineDownloadGraphicsDataRasterW(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length < 12)
            {
                return "Length out of range";
            }
            string tone = record.cmddata[index + 4] switch
            {
                48 => "Monochrome",
                52 => "Multiple tone",
                _ => "Undefined",
            };
            string keycode = ascii.GetString(record.cmddata, (index + 5), 2);
            int plane = record.cmddata[index + 7];
            int width = BitConverter.ToUInt16(record.cmddata, (index + 8));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int size = ((width + 7) / 8) * height;
            List<string> buffers = new();
            List<System.Drawing.Bitmap> planes = new();
            for (int i = 0, currindex = (index + 12); (i < plane) && (currindex < record.cmdlength); i++)
            {
                string c = record.cmddata[currindex] switch
                {
                    49 => "1",
                    50 => "2",
                    51 => "3",
                    52 => "4",
                    _ => "Undefined",
                };
                System.Drawing.Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                ColorPalette palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = c switch
                {
                    "1" => Color.Black,
                    "2" => Color.Red,
                    "3" => Color.Green,
                    "4" => Color.Blue,
                    _ => Color.Yellow,
                };
                bitmap.Palette = palette;
                System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                IntPtr ptr = bmpData.Scan0;
                int stride = bmpData.Stride;
                int linesize = (width + 7) / 8;
                if (stride == linesize)
                {
                    System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1), ptr, size);
                }
                else
                {
                    for (int j = 0; j < height; j++)
                    {
                        IntPtr curptr = bmpData.Scan0 + stride * j;
                        System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1 + (linesize * j)), curptr, linesize);
                    }
                }
                bitmap.UnlockBits(bmpData);
                planes.Add(bitmap);

                currindex += size + 1;
                buffers.Add($"Color:{c}, Size:{size}");
            }
            record.somebinary = planes.ToArray();
            string bufferlist = string.Join<string>(", ", buffers);
            return $"Length:{length}, Tone:{tone}, KeyCode:{keycode}, Width:{width}, Height:{height}, Plane:{plane}, BufferList:{bufferlist}";
        }

        //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 53 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
        internal static string DecodeGsDefineDownloadGraphicsDataRasterDW(EscPosCmd record, int index)
        {
            long length = BitConverter.ToUInt32(record.cmddata, index);
            if (length < 12)
            {
                return "Length out of range";
            }
            string tone = record.cmddata[index + 6] switch
            {
                48 => "Monochrome",
                52 => "Multiple tone",
                _ => "Undefined",
            };
            string keycode = ascii.GetString(record.cmddata, (index + 7), 2);
            int plane = record.cmddata[index + 9];
            int width = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 12));
            int size = ((width + 7) / 8) * height;
            List<string> buffers = new();
            List<System.Drawing.Bitmap> planes = new();
            for (int i = 0, currindex = (index + 14); (i < plane) && (currindex < record.cmdlength); i++)
            {
                string c = record.cmddata[currindex] switch
                {
                    49 => "1",
                    50 => "2",
                    51 => "3",
                    52 => "4",
                    _ => "Undefined",
                };
                System.Drawing.Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                ColorPalette palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = c switch
                {
                    "1" => Color.Black,
                    "2" => Color.Red,
                    "3" => Color.Green,
                    "4" => Color.Blue,
                    _ => Color.Yellow,
                };
                bitmap.Palette = palette;
                System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                IntPtr ptr = bmpData.Scan0;
                int stride = bmpData.Stride;
                int linesize = (width + 7) / 8;
                if (stride == linesize)
                {
                    System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1), ptr, size);
                }
                else
                {
                    for (int j = 0; j < height; j++)
                    {
                        IntPtr curptr = bmpData.Scan0 + stride * j;
                        System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1 + (linesize * j)), curptr, linesize);
                    }
                }
                bitmap.UnlockBits(bmpData);
                planes.Add(bitmap);

                currindex += size + 1;
                buffers.Add($"Color:{c}, Size:{size}");
            }
            record.somebinary = planes.ToArray();
            string bufferlist = string.Join<string>(", ", buffers);
            return $"Length:{length}, Tone:{tone}, KeyCode:{keycode}, Width:{width}, Height:{height}, Plane:{plane}, BufferList:{bufferlist}";
        }

        //  GS  ( L 1D 28 4C 000C-FFFF 30 54 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
        internal static string DecodeGsDefineDownloadGraphicsDataColumnW(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length < 12)
            {
                return "Length out of range";
            }
            string tone = record.cmddata[index + 4] switch
            {
                48 => "Monochrome",
                52 => "Multiple tone",
                _ => "Undefined",
            };
            string keycode = ascii.GetString(record.cmddata, (index + 5), 2);
            int plane = record.cmddata[index + 7];
            int width = BitConverter.ToUInt16(record.cmddata, (index + 8));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int size = width * ((height + 7) / 8);
            List<string> buffers = new();
            List<System.Drawing.Bitmap> planes = new();
            for (int i = 0, currindex = (index + 12); (i < plane) && (currindex < record.cmdlength); i++)
            {
                string c = record.cmddata[currindex] switch
                {
                    49 => "1",
                    50 => "2",
                    51 => "3",
                    52 => "4",
                    _ => "Undefined",
                };
                System.Drawing.Bitmap bitmap = new(height, width, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                ColorPalette palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = c switch
                {
                    "1" => Color.Black,
                    "2" => Color.Red,
                    "3" => Color.Green,
                    "4" => Color.Blue,
                    _ => Color.Yellow,
                };
                bitmap.Palette = palette;
                System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                IntPtr ptr = bmpData.Scan0;
                int stride = bmpData.Stride;
                int linesize = (height + 7) / 8;
                if (stride == linesize)
                {
                    System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1), ptr, size);
                }
                else
                {
                    for (int j = 0; j < width; j++)
                    {
                        IntPtr curptr = bmpData.Scan0 + stride * j;
                        System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1 + (linesize * j)), curptr, linesize);
                    }
                }
                bitmap.UnlockBits(bmpData);
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                planes.Add(bitmap);

                currindex += size + 1;
                buffers.Add($"Color:{c}, Size:{size}");
            }
            record.somebinary = planes.ToArray();
            string bufferlist = string.Join<string>(", ", buffers);
            return $"Length:{length}, Tone:{tone}, KeyCode:{keycode}, Width:{width}, Height:{height}, Plane:{plane}, BufferList:{bufferlist}";
        }

        //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 54 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
        internal static string DecodeGsDefineDownloadGraphicsDataColumnDW(EscPosCmd record, int index)
        {
            long length = BitConverter.ToUInt32(record.cmddata, index);
            if (length < 12)
            {
                return "Length out of range";
            }
            string tone = record.cmddata[index + 6] switch
            {
                48 => "Monochrome",
                52 => "Multiple tone",
                _ => "Undefined",
            };
            string keycode = ascii.GetString(record.cmddata, (index + 7), 2);
            int plane = record.cmddata[index + 9];
            int width = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 12));
            int size = width * ((height + 7) / 8);
            List<string> buffers = new();
            List<System.Drawing.Bitmap> planes = new();
            for (int i = 0, currindex = (index + 14); (i < plane) && (currindex < record.cmdlength); i++)
            {
                string c = record.cmddata[currindex] switch
                {
                    49 => "1",
                    50 => "2",
                    51 => "3",
                    52 => "4",
                    _ => "Undefined",
                };
                System.Drawing.Bitmap bitmap = new(height, width, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                ColorPalette palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = c switch
                {
                    "1" => Color.Black,
                    "2" => Color.Red,
                    "3" => Color.Green,
                    "4" => Color.Blue,
                    _ => Color.Yellow,
                };
                bitmap.Palette = palette;
                System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                IntPtr ptr = bmpData.Scan0;
                int stride = bmpData.Stride;
                int linesize = (height + 7) / 8;
                if (stride == linesize)
                {
                    System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1), ptr, size);
                }
                else
                {
                    for (int j = 0; j < width; j++)
                    {
                        IntPtr curptr = bmpData.Scan0 + stride * j;
                        System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1 + (linesize * j)), curptr, linesize);
                    }
                }
                bitmap.UnlockBits(bmpData);
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                planes.Add(bitmap);

                currindex += size + 1;
                buffers.Add($"Color:{c}, Size:{size}");
            }
            record.somebinary = planes.ToArray();
            string bufferlist = string.Join<string>(", ", buffers);
            return $"Length:{length}, Tone:{tone}, KeyCode:{keycode}, Width:{width}, Height:{height}, Plane:{plane}, BufferList:{bufferlist}";
        }

        //  GS  ( L 1D 28 4C 000B-FFFF 30 70 30/34 01/02 01/02 31-34 0001-0960 0001-0960 00-FF...
        internal static string DecodeGsStoreGraphicsDataToPrintBufferRasterW(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length < 11)
            {
                return "Length out of range";
            }
            string tone = record.cmddata[index + 4] switch
            {
                48 => "Monochrome",
                52 => "Multiple tone",
                _ => "Undefined",
            };
            string bx = record.cmddata[index + 5] switch
            {
                1 => "1",
                2 => "2",
                _ => "Undefined",
            };
            string by = record.cmddata[index + 6] switch
            {
                1 => "1",
                2 => "2",
                _ => "Undefined",
            };
            string color = record.cmddata[index + 7] switch
            {
                49 => "1",
                50 => "2",
                51 => "3",
                52 => "4",
                _ => "Undefined",
            };
            int width = BitConverter.ToUInt16(record.cmddata, (index + 8));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int size = ((width + 7) / 8) * height;
            record.somebinary = GetBitmap(width, height, ImageDataType.Raster, record.cmddata, (index + 12), color);
            return $"Length:{length}, Tone:{tone}, X times:{bx}, Y times:{by}, Color:{color}, Width:{width}, Height:{height}, Size:{size}";
        }

        //  GS  8 L 1D 38 4C 0000000B-FFFFFFFF 30 70 30/34 01/02 01/02 31-34 0001-0960 0001-0960 00-FF...
        internal static string DecodeGsStoreGraphicsDataToPrintBufferRasterDW(EscPosCmd record, int index)
        {
            long length = BitConverter.ToUInt32(record.cmddata, index);
            if (length < 11)
            {
                return "Length out of range";
            }
            string tone = record.cmddata[index + 6] switch
            {
                48 => "Monochrome",
                52 => "Multiple tone",
                _ => "Undefined",
            };
            string bx = record.cmddata[index + 7] switch
            {
                1 => "1",
                2 => "2",
                _ => "Undefined",
            };
            string by = record.cmddata[index + 8] switch
            {
                1 => "1",
                2 => "2",
                _ => "Undefined",
            };
            string color = record.cmddata[index + 9] switch
            {
                49 => "1",
                50 => "2",
                51 => "3",
                52 => "4",
                _ => "Undefined",
            };
            int width = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 12));
            int size = ((width + 7) / 8) * height;
            record.somebinary = GetBitmap(width, height, ImageDataType.Raster, record.cmddata, (index + 14), color);
            return $"Length:{length}, Tone:{tone}, X times:{bx}, Y times:{by}, Color:{color}, Width:{width}, Height:{height}, Size:{size}";
        }

        //  GS  ( L 1D 28 4C 000B-FFFF 30 71 30 01/02 01/02 31-33 0001-0800 0001-0080 00-FF...
        internal static string DecodeGsStoreGraphicsDataToPrintBufferColumnW(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length < 11)
            {
                return "Length out of range";
            }
            string tone = record.cmddata[index + 4] switch
            {
                48 => "Monochrome",
                52 => "Multiple tone",
                _ => "Undefined",
            };
            string bx = record.cmddata[index + 5] switch
            {
                1 => "1",
                2 => "2",
                _ => "Undefined",
            };
            string by = record.cmddata[index + 6] switch
            {
                1 => "1",
                2 => "2",
                _ => "Undefined",
            };
            string color = record.cmddata[index + 7] switch
            {
                49 => "1",
                50 => "2",
                51 => "3",
                52 => "4",
                _ => "Undefined",
            };
            int width = BitConverter.ToUInt16(record.cmddata, (index + 8));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int size = width * ((height + 7) / 8);
            record.somebinary = GetBitmap(width, height, ImageDataType.Column, record.cmddata, (index + 12), color);
            return $"Length:{length}, Tone:{tone}, X times:{bx}, Y times:{by}, Color:{color}, Width:{width}, Height:{height}, Size:{size}";
        }

        //  GS  8 L 1D 38 4C 0000000B-FFFFFFFF 30 71 30 01/02 01/02 31-33 0001-0800 0001-0080 00-FF...
        internal static string DecodeGsStoreGraphicsDataToPrintBufferColumnDW(EscPosCmd record, int index)
        {
            long length = BitConverter.ToUInt32(record.cmddata, index);
            if (length < 11)
            {
                return "Length out of range";
            }
            string tone = record.cmddata[index + 6] switch
            {
                48 => "Monochrome",
                52 => "Multiple tone",
                _ => "Undefined",
            };
            string bx = record.cmddata[index + 7] switch
            {
                1 => "1",
                2 => "2",
                _ => "Undefined",
            };
            string by = record.cmddata[index + 8] switch
            {
                1 => "1",
                2 => "2",
                _ => "Undefined",
            };
            string color = record.cmddata[index + 9] switch
            {
                49 => "1",
                50 => "2",
                51 => "3",
                52 => "4",
                _ => "Undefined",
            };
            int width = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 12));
            int size = width * ((height + 7) / 8);
            record.somebinary = GetBitmap(width, height, ImageDataType.Column, record.cmddata, (index + 14), color);
            return $"Length:{length}, Tone:{tone}, X times:{bx}, Y times:{by}, Color:{color}, Width:{width}, Height:{height}, Size:{size}";
        }

        //  GS  ( L 1D 28 4C 04 00 30 01/31 32/33 32/33
        internal static string DecodeGsSetReferenceDotDensityGraphics(EscPosCmd record, int index)
        {
            string dpi = "Undefined";
            if ((record.cmddata[7]) == (record.cmddata[8]))
            {
                dpi = record.cmddata[7] switch
                {
                    50 => "180",
                    51 => "360",
                    _ => "Undefined",
                };
            }
            return dpi;
        }

        //  GS  ( L 1D 28 4C 04 00 30 42 20-7E 20-7E
        internal static string DecodeGsDeleteSpecifiedNVGraphicsData(EscPosCmd record, int index)
        {
            return ascii.GetString(record.cmddata, index, 2);
        }

        //  GS  ( L 1D 28 4C 06 00 30 45 20-7E 20-7E 01/02 01/02
        internal static string DecodeGsPrintSpecifiedNVGraphicsData(EscPosCmd record, int index)
        {
            string keycode = ascii.GetString(record.cmddata, index, 2);
            string x = record.cmddata[index + 2] switch
            {
                1 => "1",
                2 => "2",
                _ => "Undefined",
            };
            string y = record.cmddata[index + 3] switch
            {
                1 => "1",
                2 => "2",
                _ => "Undefined",
            };
            return $"Keycode:{keycode}, X times:{x}, Y times:{y}";
        }

        //  GS  ( L 1D 28 4C 04 00 30 52 20-7E 20-7E
        internal static string DecodeGsDeleteSpecifiedDownloadGraphicsData(EscPosCmd record, int index)
        {
            return ascii.GetString(record.cmddata, index, 2);
        }

        //  GS  ( L 1D 28 4C 06 00 30 55 20-7E 20-7E 01/02 01/02
        internal static string DecodeGsPrintSpecifiedDownloadGraphicsData(EscPosCmd record, int index)
        {
            string keycode = ascii.GetString(record.cmddata, index, 2);
            string x = record.cmddata[index + 2] switch
            {
                1 => "1",
                2 => "2",
                _ => "Undefined",
            };
            string y = record.cmddata[index + 3] switch
            {
                1 => "1",
                2 => "2",
                _ => "Undefined",
            };
            return $"Keycode:{keycode}, X times:{x}, Y times:{y}";
        }

        //  GS  ( M 1D 28 4D 02 00 01/31 01/31
        internal static string DecodeGsSaveSettingsValuesFromWorkToStorage(EscPosCmd record, int index)
        {
            return ((record.cmddata[index] == 1) || (record.cmddata[index] == 49)) ? "" : "Undefined";
        }

        //  GS  ( M 1D 28 4D 02 00 02/32 00/01/30/31
        internal static string DecodeGsLoadSettingsValuesFromStorageToWork(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "Initial value loaded",
                48 => "Initial value loaded",
                1 => "1st saved value loaded",
                49 => "1st saved value loaded",
                _ => "Undefined",
            };
        }

        //  GS  ( M 1D 28 4D 02 00 03/33 00/01/30/31
        internal static string DecodeGsSelectSettingsValuesToWorkAfterInitialize(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "Select initial value",
                48 => "Select initial value",
                1 => "Select 1st saved value",
                49 => "Select 1st saved value",
                _ => "Undefined",
            };
        }

        //  GS  ( N 1D 28 4E 02 00 30 30-33
        internal static string DecodeGsSetCharacterColor(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                48 => "None(not print)",
                49 => "Color 1",
                50 => "Color 2",
                51 => "Color 3",
                _ => "Undefined",
            };
        }

        //  GS  ( N 1D 28 4E 02 00 31 30-33
        internal static string DecodeGsSetBackgroundColor(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                48 => "None(not print)",
                49 => "Color 1",
                50 => "Color 2",
                51 => "Color 3",
                _ => "Undefined",
            };
        }

        //  GS  ( N 1D 28 4E 03 00 32 00/01/30/31 30-33
        internal static string DecodeGsTurnShadingMode(EscPosCmd record, int index)
        {
            string onoff = record.cmddata[index] switch
            {
                0 => "OFF",
                48 => "OFF",
                1 => "ON",
                49 => "ON",
                _ => "Undefined",
            };
            string color = record.cmddata[index] switch
            {
                48 => "None(not print)",
                49 => "1",
                50 => "2",
                51 => "3",
                _ => "Undefined",
            };
            return $"Shadow mode:{onoff}, Color:{color}";
        }

        //  GS  ( P 1D 28 50 08 00 30 FFFF 0001-FFFF 0000 01
        internal static string DecodeGsSetPrinttableArea(EscPosCmd record, int index)
        {
            int wx = BitConverter.ToUInt16(record.cmddata, index);
            int wy = BitConverter.ToUInt16(record.cmddata, index + 2);
            int ox = BitConverter.ToUInt16(record.cmddata, index + 4);
            string c = record.cmddata[6] switch
            {
                1 => "1",
                _ => "Undefined",
            };
            return $"Horizontal size:{wx}, Vertical size:{wy}, Horizontal offset:{ox}, c:{c}";
        }

        //  GS  ( Q 1D 28 51 0C 00 30 0000-FFFF 0000-FFFF 0000-FFFF 0000-FFFF 01 01-06 30
        internal static string DecodeGsDrawLineInPageMode(EscPosCmd record, int index)
        {
            int x1 = BitConverter.ToUInt16(record.cmddata, index);
            int y1 = BitConverter.ToUInt16(record.cmddata, index + 2);
            int x2 = BitConverter.ToUInt16(record.cmddata, index + 4);
            int y2 = BitConverter.ToUInt16(record.cmddata, index + 6);
            string c = record.cmddata[8] switch
            {
                1 => "1",
                _ => "Undefined",
            };
            string m1 = record.cmddata[9] switch
            {
                1 => "Single line : Thin",
                2 => "Single line : Moderately Thick",
                3 => "Single line : Thick",
                4 => "Double line : Thin",
                5 => "Double line : Moderately Thick",
                6 => "Double line : Thick",
                _ => "Undefined",
            };
            string m2 = record.cmddata[10] switch
            {
                48 => "0",
                _ => "Undefined",
            };
            return $"X start:{x1}, Y start:{y1}, X end:{x2}, Y end:{y2}, c:{c}, Line style:{m1}, m2:{m2}";
        }

        //  GS  ( Q 1D 28 51 0E 00 31 0000-FFFF 0000-FFFF 0000-FFFF 0000-FFFF 01 01-06 30 30 01
        internal static string DecodeGsDrawRectangleInPageMode(EscPosCmd record, int index)
        {
            int x1 = BitConverter.ToUInt16(record.cmddata, index);
            int y1 = BitConverter.ToUInt16(record.cmddata, index + 2);
            int x2 = BitConverter.ToUInt16(record.cmddata, index + 4);
            int y2 = BitConverter.ToUInt16(record.cmddata, index + 6);
            string c = record.cmddata[8] switch
            {
                1 => "1",
                _ => "Undefined",
            };
            string m1 = record.cmddata[9] switch
            {
                1 => "Single line : Thin",
                2 => "Single line : Moderately Thick",
                3 => "Single line : Thick",
                4 => "Double line : Thin",
                5 => "Double line : Moderately Thick",
                6 => "Double line : Thick",
                _ => "Undefined",
            };
            string m2 = record.cmddata[10] switch
            {
                48 => "0",
                _ => "Undefined",
            };
            string m3 = record.cmddata[11] switch
            {
                48 => "0",
                _ => "Undefined",
            };
            string m4 = record.cmddata[12] switch
            {
                1 => "1",
                _ => "Undefined",
            };
            return $"X start:{x1}, Y start:{y1}, X end:{x2}, Y end:{y2}, c:{c}, Line style:{m1}, m2:{m2}, m3:{m3}, m4:{m4}";
        }

        //  GS  ( Q 1D 28 51 09 00 32 0000-023F 0000-023F 01-FF 01 01-06 30
        internal static string DecodeGsDrawHorizontalLineInStandardMode(EscPosCmd record, int index)
        {
            int x1 = BitConverter.ToUInt16(record.cmddata, index);
            int x2 = BitConverter.ToUInt16(record.cmddata, index + 2);
            byte n = record.cmddata[4];
            string c = record.cmddata[5] switch
            {
                1 => "1",
                _ => "Undefined",
            };
            string m1 = record.cmddata[6] switch
            {
                1 => "Single line : Thin",
                2 => "Single line : Moderately Thick",
                3 => "Single line : Thick",
                4 => "Double line : Thin",
                5 => "Double line : Moderately Thick",
                6 => "Double line : Thick",
                _ => "Undefined",
            };
            string m2 = record.cmddata[7] switch
            {
                48 => "0",
                _ => "Undefined",
            };
            return $"X start:{x1}, X end:{x2}, Feed:{n}, c:{c}, Line style:{m1}, m2:{m2}";
        }

        //  GS  ( Q 1D 28 51 07 00 33 0000-023F 00/01 01 01-06 30
        internal static string DecodeGsDrawVerticalLineInStandardMode(EscPosCmd record, int index)
        {
            int x = BitConverter.ToUInt16(record.cmddata, index);
            string a = record.cmddata[2] switch
            {
                0 => "Draw stop",
                1 => "Draw start",
                _ => "Undefined",
            };
            string c = record.cmddata[3] switch
            {
                1 => "1",
                _ => "Undefined",
            };
            string m1 = record.cmddata[4] switch
            {
                1 => "Single line : Thin",
                2 => "Single line : Moderately Thick",
                3 => "Single line : Thick",
                4 => "Double line : Thin",
                5 => "Double line : Moderately Thick",
                6 => "Double line : Thick",
                _ => "Undefined",
            };
            string m2 = record.cmddata[5] switch
            {
                48 => "0",
                _ => "Undefined",
            };
            return $"X position:{x}, Action:{a}, c:{c}, Line style:{m1}, m2:{m2}";
        }

        //  GS  ( k 1D 28 6B 03 00 30 41 00-1E
        internal static string DecodeGsPDF417SetNumberOfColumns(EscPosCmd record, int index)
        {
            return record.cmddata[index] <= 30 ? record.cmddata[index].ToString("D", invariantculture) : "Out of range";
        }

        //  GS  ( k 1D 28 6B 03 00 30 42 00/3-5A
        internal static string DecodeGsPDF417SetNumberOfRows(EscPosCmd record, int index)
        {
            byte rows = record.cmddata[index];
            if ((rows == 0) || ((rows >= 3) && (rows <= 90)))
            {
                return rows.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 03 00 30 43 01-08
        internal static string DecodeGsPDF417SetWidthOfModule(EscPosCmd record, int index)
        {
            byte modules = record.cmddata[index];
            if ((modules >= 1) && (modules <= 8))
            {
                return modules.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 03 00 30 44 02-08
        internal static string DecodeGsPDF417SetRowHeight(EscPosCmd record, int index)
        {
            byte height = record.cmddata[index];
            if ((height >= 2) && (height <= 8))
            {
                return height.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 04 00 30 45 30/31 30-38/00-28
        internal static string DecodeGsPDF417SetErrorCollectionLevel(EscPosCmd record, int index)
        {
            byte m = record.cmddata[index];
            byte n = record.cmddata[index + 1];
            string collection;
            string value = "";
            switch (m)
            {
                case 48:
                    collection = "Level";
                    value = ((n >= 48) && (n <= 56)) ? "Level " + ascii.GetString(record.cmddata, (index + 1), 1) : "Out of range";
                    break;

                case 49:
                    collection = "Ratio";
                    value = ((n >= 1) && (n <= 40)) ? (n * 10).ToString("D", invariantculture) + " %" : "Out of range";
                    break;

                default:
                    collection = "Undefined";
                    break;
            };
            return $"Error collection type:{collection}, Value:{value}";
        }

        //  GS  ( k 1D 28 6B 03 00 30 46 00/01
        internal static string DecodeGsPDF417SelectOptions(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "Standard PDF417",
                1 => "Truncated PDF417",
                _ => "Undefined",
            };
        }

        //  GS  ( k 1D 28 6B 0004-FFFF 30 50 30 00-FF...
        internal static string DecodeGsPDF417StoreData(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length < 4)
            {
                return "Length out of range";
            }
            return (length - 3).ToString("D", invariantculture);
        }

        //  GS  ( k 1D 28 6B 04 00 31 41 31-33 00
        internal static string DecodeGsQRCodeSelectModel(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                48 => "Model 1",
                49 => "Model 2",
                50 => "Micro QR Code",
                _ => "Undefined",
            };
        }

        //  GS  ( k 1D 28 6B 03 00 31 43 01-10
        internal static string DecodeGsQRCodeSetSizeOfModule(EscPosCmd record, int index)
        {
            byte modules = record.cmddata[index];
            if ((modules >= 1) && (modules <= 16))
            {
                return modules.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 03 00 31 45 30-33
        internal static string DecodeGsQRCodeSetErrorCollectionLevel(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                48 => "L",
                49 => "M",
                50 => "Q",
                51 => "H",
                _ => "Undefined",
            };
        }

        //  GS  ( k 1D 28 6B 0004-1BB4 31 50 30 00-FF...
        internal static string DecodeGsQRCodeStoreData(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 4) || (length > 7092))
            {
                return "Length out of range";
            }
            return (length - 3).ToString("D", invariantculture);
        }

        //  GS  ( k 1D 28 6B 03 00 32 41 32-36
        internal static string DecodeGsMaxiCodeSelectMode(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                50 => "2",
                51 => "3",
                52 => "4",
                53 => "5",
                54 => "6",
                _ => "Undefined",
            };
        }

        //  GS  ( k 1D 28 6B 0004-008D 32 50 30 00-FF...
        internal static string DecodeGsMaxiCodeStoreData(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 4) || (length > 141))
            {
                return "Length out of range";
            }
            return (length - 3).ToString("D", invariantculture);
        }

        //  GS  ( k 1D 28 6B 03 00 33 43 02-08
        internal static string DecodeGsD2GS1DBSetWidthOfModule(EscPosCmd record, int index)
        {
            byte modules = record.cmddata[index];
            if ((modules >= 2) && (modules <= 8))
            {
                return modules.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 04 00 33 47 0000/006A-0F70
        internal static string DecodeGsD2GS1DBSetExpandStackedMaximumWidth(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            return ((length == 0) || ((length >= 106) && (length <= 3952))) ? length.ToString("D", invariantculture) : "Length out of range";
        }

        //  GS  ( k 1D 28 6B 0006-0103 33 50 30 20-22/25-2F/30-39/3A-3F/41-5A/61-7A...
        internal static string DecodeGsD2GS1DBStoreData(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 6) || (length > 259))
            {
                return "Length out of range";
            }
            string symbol = record.cmddata[index + 5] switch
            {
                72 => "GS1 DataBar Stacked",
                73 => "GS1 DataBar Stacked Onmidirectional",
                76 => "GS1 DataBar Expanded Stacked",
                _ => "Undefined",
            };
            length -= 4;
            return $"Type:{symbol}, Length:{length}";
        }

        //  GS  ( k 1D 28 6B 03 00 34 43 02-08
        internal static string DecodeGsCompositeSetWidthOfModule(EscPosCmd record, int index)
        {
            byte modules = record.cmddata[index];
            if ((modules >= 2) && (modules <= 8))
            {
                return modules.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 04 00 34 47 0000/006A-0F70
        internal static string DecodeGsCompositeSetExpandStackedMaximumWidth(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            return ((length == 0) || ((length >= 106) && (length <= 3952))) ? length.ToString("D", invariantculture) : "Length out of range";
        }

        //  GS  ( k 1D 28 6B 03 00 34 48 00-05/30-35/61/62
        internal static string DecodeGsCompositeSelectHRICharacterFont(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "No HRI",
                48 => "No HRI",
                1 => "With Font A HRI",
                49 => "With Font A HRI",
                2 => "With Font B HRI",
                50 => "With Font B HRI",
                3 => "With Font C HRI",
                51 => "With Font C HRI",
                4 => "With Font D HRI",
                52 => "With Font D HRI",
                5 => "With Font E HRI",
                53 => "With Font E HRI",
                97 => "With Special Font A HRI",
                98 => "With Special Font B HRI",
                _ => "Undefined",
            };
        }

        //  GS  ( k 1D 28 6B 0006-093E 34 50 30 00-FF...
        internal static string DecodeGsCompositeStoreData(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 6) || (length > 2366))
            {
                return "Length out of range";
            }
            byte a = record.cmddata[index + 5];
            byte b = record.cmddata[index + 6];
            string element;
            string symbol = "";
            string data = "";
            int k = length - 5;
            int bcindex = 10;
            switch (a)
            {
                case 48:
                    element = "Linear element";
                    switch (b)
                    {
                        case 65:
                            symbol = "EAN8";
                            data = (k == 7) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 66:
                            symbol = "EAN13";
                            data = (k == 12) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 67:
                            symbol = "UPC-A";
                            data = (k == 11) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 68:
                            symbol = "UPC-E 6 digits";
                            data = (k == 6) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 69:
                            symbol = "UPC-E 11 digits";
                            data = (k == 11) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 70:
                            symbol = "GS1 DataBar Omnidirectional";
                            data = (k == 13) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 71:
                            symbol = "GS1 DataBar Truncated";
                            data = (k == 13) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 72:
                            symbol = "GS1 DataBar Stacked";
                            data = (k == 13) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 73:
                            symbol = "GS1 DataBar Stacked Omnidirectional";
                            data = (k == 13) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 74:
                            symbol = "GS1 DataBar Limited";
                            data = (k == 13) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 75:
                            symbol = "GS1 DataBar Expanded";
                            data = ((k >= 2) && (k <= 255)) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 76:
                            symbol = "GS1 DataBar Expanded Stacked";
                            data = ((k >= 2) && (k <= 255)) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 77:
                            symbol = "GS1-128";
                            data = ((k >= 2) && (k <= 255)) ? BitConverter.ToString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        default:
                            symbol = "Undefined";
                            break;
                    };
                    break;

                case 49:
                    element = "2D Composite element";
                    switch (b)
                    {
                        case 65:
                            symbol = "Automatic selection according to number of digits";
                            data = ((k >= 1) && (k <= 2361)) ? BitConverter.ToString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 66:
                            symbol = "CC-C";
                            data = ((k >= 1) && (k <= 2361)) ? BitConverter.ToString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        default:
                            symbol = "Undefined";
                            break;
                    };
                    break;

                default:
                    element = "Undefined";
                    break;
            };
            return $"Length:{length}, Element:{element}, Symbol:{symbol}, Data:{data}";
        }

        //  GS  ( k 1D 28 6B 04 00 35 42 00/01/30/31 00-20
        internal static string DecodeGsAztecCodeSetModeTypesAndDataLayer(EscPosCmd record, int index)
        {
            string mode = record.cmddata[index] switch
            {
                0 => "Full-Range",
                48 => "Full-Range",
                1 => "Compact",
                49 => "Compact",
                _ => "Undefined",
            };
            byte n2 = record.cmddata[index + 1];
            string layers;
            if (n2 == 0)
            {
                layers = "Automatic processing";
            }
            else if ((n2 >= 1) && (n2 <= 32))
            {
                layers = n2.ToString("D", invariantculture);
            }
            else
            {
                layers = "Out of range";
            }
            return $"Mode:{mode}, Data Layers:{layers}";
        }

        //  GS  ( k 1D 28 6B 03 00 35 43 02-10
        internal static string DecodeGsAztecCodeSetSizeOfModule(EscPosCmd record, int index)
        {
            byte modules = record.cmddata[index];
            if ((modules >= 2) && (modules <= 16))
            {
                return modules.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 03 00 35 45 05-5F
        internal static string DecodeGsAztecCodeSetErrorCollectionLevel(EscPosCmd record, int index)
        {
            byte level = record.cmddata[index];
            if ((level >= 5) && (level <= 95))
            {
                return level.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 0004-0EFB 35 50 30 00-FF...
        internal static string DecodeGsAztecCodeStoreData(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 4) || (length > 3835))
            {
                return "Length out of range";
            }
            int k = length - 3;
            return $"Length:{length}, Data:{BitConverter.ToString(record.cmddata, 8, k)}";
        }

        private static readonly List<byte> s_DMSquare = new()
        {
            0,
            10,
            12,
            14,
            16,
            18,
            20,
            22,
            24,
            26,
            32,
            36,
            40,
            44,
            48,
            52,
            64,
            72,
            80,
            88,
            96,
            104,
            120,
            132,
            144
        };

        private static readonly List<byte> s_DMRectCol = new() { 8, 12, 16 };
        private static readonly List<byte> s_DMRect08 = new() { 0, 18, 32 };
        private static readonly List<byte> s_DMRect12 = new() { 0, 26, 36 };
        private static readonly List<byte> s_DMRect16 = new() { 0, 36, 48 };

        //  GS  ( k 1D 28 6B 05 00 36 42 00/01/30/31 00-90 00-90
        internal static string DecodeGsDataMatrixSetSymbolTypeColumnsRows(EscPosCmd record, int index)
        {
            byte m = record.cmddata[index];
            byte d1 = record.cmddata[index + 1];
            byte d2 = record.cmddata[index + 2];
            string symboltype;
            string columnsrows;
            switch (m)
            {
                case 0:
                case 48:
                    symboltype = "Suare";
                    if (d1 != d2) { columnsrows = "No square Columns, Rows"; break; }
                    if (d1 == 0) { columnsrows = "Automatic processing"; break; }
                    columnsrows = s_DMSquare.Contains(d1) ? $"{d1}, {d2}" : "Out of range";
                    break;

                case 1:
                case 49:
                    symboltype = "Rectangle";
                    if (!s_DMRectCol.Contains(d1)
                        || ((d1 == 8) && !s_DMRect08.Contains(d2))
                        || ((d1 == 12) && !s_DMRect12.Contains(d2))
                        || ((d1 == 16) && !s_DMRect16.Contains(d2))
                    )
                    {
                        columnsrows = "Out of range"; break;
                    }
                    columnsrows = (d2 == 0) ? $"Columns:{d1}, Rows:Automatic processing" : $"Columns:{d1}, Rows:{d2}";
                    break;

                default:
                    symboltype = "Undefined";
                    columnsrows = "";
                    break;
            };
            return $"SymbolType:{symboltype}, {columnsrows}";
        }

        //  GS  ( k 1D 28 6B 03 00 36 43 02-10
        internal static string DecodeGsDataMatrixSetSizeOfModule(EscPosCmd record, int index)
        {
            byte modules = record.cmddata[index];
            if ((modules >= 2) && (modules <= 16))
            {
                return modules.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 0004-0C2F 36 50 30 00-FF...
        internal static string DecodeGsDataMatrixStoreData(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 4) || (length > 3119))
            {
                return "Length out of range";
            }
            int k = length - 3;
            return $"Length:{length}, Data:{BitConverter.ToString(record.cmddata, 8, k)}";
        }

        //c GS  ( z 1D 28 7A 0003-FFFF 2A [3C/40-42/46 00/01/30/31]...
        internal static string DecodeGsSetReadOperationsOfCheckPaper(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length < 3)
            {
                return "Length out of range";
            }
            else if ((length & 1) != 1)
            {
                return "Invalid alignment";
            }
            int count = (length - 1) / 2;
            List<string> ops = new();
            for (int i = 0, currindex = 6; i < count; i++, currindex += 2)
            {
                byte n = record.cmddata[currindex];
                byte m = record.cmddata[currindex + 1];
                string detection = n switch
                {
                    60 => "Multi feed detected : read check paper",
                    64 => "Magnetic waveforms cannot detected : read check paper",
                    65 => "Number of unrecognizable characters has exceeded specified number : Magnetic waveforms analysis",
                    66 => "Abnormality detected : noise measurement",
                    70 => "reading process check paper",
                    _ => "Undefined",
                };
                string operation = m switch
                {
                    0 => "Continues",
                    48 => "Continues",
                    1 => "Cancels",
                    49 => "Cancels",
                    _ => "Undefined",
                };
                if (n == 70)
                {
                    operation = m switch
                    {
                        0 => "Paperjam detection level : High",
                        48 => "Paperjam detection level : High",
                        1 => "Paperjam detection level : Low",
                        49 => "Paperjam detection level : Low",
                        _ => "Undefined",
                    };
                }
                ops.Add($"Type:{detection}, {operation}");
            }
            return string.Join<string>(", ", ops);
        }

        //c GS  ( z 1D 28 7A 0C 00 3E 33 01-09 00000000-3B9ACAFF 00/20/30 00000000-3B9ACAFF
        internal static string DecodeGsSetCounterForReverseSidePrint(EscPosCmd record, int index)
        {
            byte k = record.cmddata[index];
            long n = BitConverter.ToUInt32(record.cmddata, index + 1);
            byte d1 = record.cmddata[index + 5];
            long c = BitConverter.ToUInt32(record.cmddata, index + 6);
            string digits = ((k >= 1) && (k <= 9)) ? k.ToString("D", invariantculture) : "Digits out of range";
            string layout = d1 switch
            {
                32 => "Right align with leading spaces",
                48 => "Right align with leading 0",
                0 => "Left align with trailing spaces",
                _ => "Undefined",
            };
            return $"Digits:{digits}, Layout:{layout}, Default Value:{n}, Inclemental Value:{c}";
        }

        //  GS  *   1D 2A 01-FF 01-FF 00-FF...
        internal static string DecodeGsObsoleteDefineDownloadedBitimage(EscPosCmd record, int index)
        {
            byte x = record.cmddata[index];
            byte y = record.cmddata[index + 1];
            if ((x == 0) || (y == 0))
            {
                return $"Invalid value Width:{x} dots, Height:{y} x 8 dots";
            }
            int length = x * y * 8;
            System.Drawing.Bitmap bitmap = new((y * 8), x, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            ColorPalette palette = bitmap.Palette;
            palette.Entries[0] = Color.White;
            palette.Entries[1] = Color.Black;
            bitmap.Palette = palette;
            System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            IntPtr ptr = bmpData.Scan0;
            int stride = bmpData.Stride;
            if (stride == y)
            {
                System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (index + 2), ptr, length);
            }
            else
            {
                for (int j = 0; j < x; j++)
                {
                    IntPtr curptr = bmpData.Scan0 + stride * j;
                    System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (index + 2 + (y * j)), curptr, y);
                }
            }
            bitmap.UnlockBits(bmpData);
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
            record.somebinary = bitmap.Clone();
            return $"Width:{x} dots, Height:{y} x 8 dots, Length:{length}";
        }

        //  GS  /   1D 2F 00-03/30-33
        internal static string DecodeGsObsoletePrintDownloadedBitimage(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "Normal",
                48 => "Normal",
                1 => "Double Width",
                49 => "Double Width",
                2 => "Double Hight",
                50 => "Double Hight",
                3 => "Quadruple",
                51 => "Quadruple",
                _ => "Undefined",
            };
        }

        //  GS  C 0 1D 43 30 00-05 00-02/30-32
        internal static string DecodeGsObsoleteSelectCounterPrintMode(EscPosCmd record, int index)
        {
            string digits = record.cmddata[index] switch
            {
                0 => "actual digits",
                1 => "1",
                2 => "2",
                3 => "3",
                4 => "4",
                5 => "5",
                _ => "Out of range",
            };
            string layout = record.cmddata[index + 1] switch
            {
                0 => "Right align with leading spaces",
                48 => "Right align with leading spaces",
                1 => "Right align with leading 0",
                49 => "Right align with leading 0",
                2 => "Left align with trailing spaces",
                50 => "Left align with trailing spaces",
                _ => "Undefined",
            };
            return $"Digits:{digits}, Layout:{layout}";
        }

        //  GS  C 1 1D 43 31 0000-FFFF 0000-FFFF 00-FF 00-FF
        internal static string DecodeGsObsoleteSelectCounterModeA(EscPosCmd record, int index)
        {
            int a = BitConverter.ToUInt16(record.cmddata, index);
            int b = BitConverter.ToUInt16(record.cmddata, index + 2);
            byte n = record.cmddata[4];
            byte r = record.cmddata[5];
            string mode = "";
            if ((a < b) && (n != 0) && (r != 0))
            {
                mode = "Count Up";
            }
            else if ((a > b) && (n != 0) && (r != 0))
            {
                mode = "Count Down";
            }
            else if ((a == b) || (n == 0) || (r == 0))
            {
                mode = "Count Stop";
            }
            return $"Count mode:{mode}, Range:{a}, {b}, Stepping amount:{n}, Reputation number:{r}";
        }

        //  GS  C ; 1D 43 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B
        internal static string DecodeGsObsoleteSelectCounterModeB(EscPosCmd record, int index)
        {
            return ascii.GetString(record.cmddata, index, (int)(record.cmdlength - index));
        }

        //  GS  D   1D 44 30 43 30 20-7E 20-7E 30/34 31 42 4D 00000042-FFFFFFFF 00-FF...
        internal static string DecodeGsDefineWindowsBMPNVGraphicsData(EscPosCmd record, int index)
        {
            string keycode = ascii.GetString(record.cmddata, index, 2);
            string b = record.cmddata[index + 2] switch
            {
                48 => "Monochrome(digital)",
                52 => "Multiple tone",
                _ => "Undefined",
            };
            string c = (record.cmddata[index + 3] == 49) ? "Color 1" : "Undefined";
            using MemoryStream stream = new(record.cmddata, (index + 4), (int)(record.cmdlength - 9), false);
            using System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
            record.somebinary = stream.ToArray();
            int width = img.Width;
            int height = img.Height;
            long bmpsize = BitConverter.ToUInt32(record.cmddata, (index + 6));
            return $"Length:{record.cmdlength}, Tone:{b}, KeyCode:{keycode}, Width:{width}, Height:{height}, Color:{c}, BMPsize:{bmpsize}";
        }

        //  GS  D   1D 44 30 53 30 20-7E 20-7E 30/34 31 42 4D 00000042-FFFFFFFF 00-FF...
        internal static string DecodeGsDefineWindowsBMPDownloadGraphicsData(EscPosCmd record, int index)
        {
            string keycode = ascii.GetString(record.cmddata, index, 2);
            string b = record.cmddata[index + 2] switch
            {
                48 => "Monochrome(digital)",
                52 => "Multiple tone",
                _ => "Undefined",
            };
            string c = (record.cmddata[index + 3] == 49) ? "Color 1" : "Undefined";
            using MemoryStream stream = new(record.cmddata, (index + 4), (int)(record.cmdlength - 9), false);
            using System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
            record.somebinary = stream.ToArray();
            int width = img.Width;
            int height = img.Height;
            long bmpsize = BitConverter.ToUInt32(record.cmddata, (index + 6));
            return $"Length:{record.cmdlength}, Tone:{b}, KeyCode:{keycode}, Width:{width}, Height:{height}, Color:{c}, BMPsize:{bmpsize}";
        }

        //c GS  E   1D 45 b000x0x0x
        internal static string DecodeGsObsoleteSelectHeadControlMethod(EscPosCmd record, int index)
        {
            byte mode = record.cmddata[index];
            string head = (mode & 1) == 1 ? "Normal" : "Copy";
            string quality = (mode & 4) == 4 ? "Fine" : "Economy";
            string speed = (mode & 0x10) == 0x10 ? "Low" : "High";
            return $"Head energizing time:{head}, Print quality:{quality}, Printing speed:{speed}";
        }

        //  GS  H   1D 48 00-03/30-33
        internal static string DecodeGsSelectPrintPositionHRICharacters(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "Not Printed",
                48 => "Not Printed",
                1 => "Above Barcode",
                49 => "Above Barcode",
                2 => "Below Barcode",
                50 => "Below Barcode",
                3 => "Both Above and Below Barcode",
                51 => "Both Above and Below Barcode",
                _ => "Undefined",
            };
        }

        //  GS  I   1D 49 01-03/31-33/21/23/24/41-45/60/6E-70
        internal static string DecodeGsTransmitPrinterID(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                1 => "Printer Model ID",
                49 => "Printer Model ID",
                2 => "Type ID",
                50 => "Type ID",
                3 => "Version ID",
                51 => "Version ID",
                33 => "Type Information",
                35 => "Model specific information 35",
                36 => "Model specific information 36",
                65 => "Firmware Version",
                66 => "Maker name",
                67 => "Model name",
                68 => "Serial number",
                69 => "Font language",
                96 => "Model specific information 96",
                110 => "Model specific information 110",
                111 => "Model specific information 111",
                112 => "Model specific information 112",
                _ => "Undefined",
            };
        }

        //  GS  P   1D 50 00-FF 00-FF
        internal static string DecodeGsSetHorizontalVerticalMotionUnits(EscPosCmd record, int index)
        {
            string x = record.cmddata[index] == 0 ? "Initial value" : record.cmddata[index].ToString("D", invariantculture);
            string y = record.cmddata[index + 1] == 0 ? "Initial value" : record.cmddata[index + 1].ToString("D", invariantculture);
            return $"Basic motion units Horizontal:{x}, Vertical:{y}";
        }

        //  GS  Q 0 1D 51 30 00-03/30-33 0001-10A0 0001-0010 00-FF...
        internal static string DecodeGsObsoletePrintVariableVerticalSizeBitimage(EscPosCmd record, int index)
        {
            string m = record.cmddata[index] switch
            {
                0 => "Normal",
                48 => "Normal",
                1 => "Double Width",
                49 => "Double Width",
                2 => "Double Hight",
                50 => "Double Hight",
                3 => "Quadruple",
                51 => "Quadruple",
                _ => "Undefined",
            };
            int x = BitConverter.ToUInt16(record.cmddata, index + 1);
            string xvalue = ((x >= 1) && (x <= 4256)) ? x.ToString("D", invariantculture) : "Out of range";
            int y = BitConverter.ToUInt16(record.cmddata, index + 3);
            string yvalue = ((y >= 1) && (y <= 16)) ? y.ToString("D", invariantculture) : "Out of range";
            int k = x * y;
            if (((y > 0) && (y <= 16)) && ((x > 0) && (x <= 4256)))
            {
                System.Drawing.Bitmap bitmap = new((y * 8), x, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                ColorPalette palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = Color.Black;
                bitmap.Palette = palette;
                System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                IntPtr ptr = bmpData.Scan0;
                int stride = bmpData.Stride;
                if (stride == y)
                {
                    System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (index + 5), ptr, k);
                }
                else
                {
                    for (int j = 0; j < x; j++)
                    {
                        IntPtr curptr = bmpData.Scan0 + stride * j;
                        System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (index + 5 + (y * j)), curptr, y);
                    }
                }
                bitmap.UnlockBits(bmpData);
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                record.somebinary = bitmap.Clone();
            }
            return $"Mode:{m}, Width:{xvalue} dots, Height:{yvalue} bytes, Size:{k} bytes";
        }

        //  GS  T   1D 54 00/01/30/31
        internal static string DecodeGsSetPrintPositionBeginningOfPrintLine(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "Erases buffer data, then move print position to beginning of line",
                48 => "Erases buffer data, then move print position to beginning of line",
                1 => "Prints buffer data, then move print position to beginning of line",
                49 => "Prints buffer data, then move print position to beginning of line",
                _ => "Undefined",
            };
        }

        //  GS  ^   1D 5E 01-FF 00-FF 00/01
        internal static string DecodeGsExecuteMacro(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                0 => "Continuous execution",
                1 => "Execution by button",
                _ => "Undefined",
            };
        }

        //  GS  a   1D 61 b0x00xxxx
        internal static string DecodeGsEnableDisableAutomaticStatusBack(EscPosCmd record, int index)
        {
            byte mode = record.cmddata[index];
            string panel = (mode & 0x40) == 0x40 ? "Enabled" : "Disabled";
            string rollpaper = (mode & 0x08) == 0x08 ? "Enabled" : "Disabled";
            string error = (mode & 0x04) == 0x04 ? "Enabled" : "Disabled";
            string online = (mode & 0x02) == 0x02 ? "Enabled" : "Disabled";
            string drawer = (mode & 0x01) == 0x01 ? "Enabled" : "Disabled";
            return $"Panel switch:{panel}, Roll Paper Sensor:{rollpaper}, Error:{error}, Online/Offline:{online}, Drawer kick out connector:{drawer}";
        }

        //  GS  f   1D 66 00-04/30-34/61/62
        internal static string DecodeGsSelectFontHRICharacters(EscPosCmd record, int index)
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

        //  GS  g 0 1D 67 30 00 000A-004F
        internal static string DecodeGsInitializeMaintenanceCounter(EscPosCmd record, int index)
        {
            int n = BitConverter.ToUInt16(record.cmddata, index);
            string category = (n / 10) switch
            {
                1 => "Serial impact head",
                2 => "Thermal head",
                3 => "Ink jet head",
                4 => "Shuttle head",
                5 => "Standard devices",
                6 => "Optional devices",
                7 => "Time",
                _ => "Undefined",
            };
            return $"Value:{n}, Category:{category}";
        }

        //  GS  g 2 1D 67 32 00 000A-004F/008A-00CF
        internal static string DecodeGsTransmitMaintenanceCounter(EscPosCmd record, int index)
        {
            int n = BitConverter.ToUInt16(record.cmddata, index);
            string countertype = (n & 0x80) == 0x80 ? "Comulative" : "Resettable";
            string category = ((n & 0x7F) / 10) switch
            {
                1 => "Serial impact head",
                2 => "Thermal head",
                3 => "Ink jet head",
                4 => "Shuttle head",
                5 => "Standard devices",
                6 => "Optional devices",
                7 => "Time",
                _ => "Undefined",
            };
            return $"Value:{n}, Type:{countertype}, Category:{category}";
        }

        //  GS  j   1D 6A b000000xx
        internal static string DecodeGsEnableDisableAutomaticStatusBackInk(EscPosCmd record, int index)
        {
            byte mode = record.cmddata[index];
            string mechanism = (mode & 0x02) == 0x02 ? "Enabled" : "Disabled";
            string capacity = (mode & 0x01) == 0x01 ? "Enabled" : "Disabled";
            return $"NotifyByMechanism:{mechanism}, NotifyByInkCapacity:{capacity}";
        }

        //  GS  k   1D 6B 00-06 20/24/25/2A/2B/2D-2F/30-39/41-5A/61-64... 00
        internal static string DecodeGsPrintBarcodeAsciiz(EscPosCmd record, int index)
        {
            byte m = record.cmddata[index];
            string symbol = m switch
            {
                0 => "UPC-A",
                1 => "UPC-E",
                2 => "EAN13",
                3 => "EAN8",
                4 => "CODE39",
                5 => "ITF",
                6 => "CODABAR",
                _ => "Undefined",
            };
            string barcode = ascii.GetString(record.cmddata, (index + 1), (int)(record.cmdlength - 4));
            return $"Barcode Type:{symbol}, Data:{barcode}";
        }

        //  GS  k   1D 6B 41-4F 01-FF 00-FF...
        internal static string DecodeGsPrintBarcodeSpecifiedLength(EscPosCmd record, int index)
        {
            byte m = record.cmddata[index];
            string symbol = m switch
            {
                65 => "UPC-A",
                66 => "UPC-E",
                67 => "EAN13",
                68 => "EAN8",
                69 => "CODE39",
                70 => "ITF",
                71 => "CODABAR",
                72 => "CODE93",
                73 => "CODE128",
                74 => "GS1-128",
                75 => "GS1 DataBar Omnidirectional",
                76 => "GS1 DataBar Truncated",
                77 => "GS1 DataBar Limited",
                78 => "GS1 DataBar Expanded",
                79 => "Code128 auto",
                _ => "Undefined",
            };
            byte n = record.cmddata[index + 1];
            string barcode = ascii.GetString(record.cmddata, (index + 2), n);
            return $"Barcode Type:{symbol}, Data:{barcode}";
        }

        //  GS  r   1D 72 01/02/04/31/32/34
        internal static string DecodeGsTransmitStatus(EscPosCmd record, int index)
        {
            return record.cmddata[index] switch
            {
                1 => "Paper sensor",
                49 => "Paper sensor",
                2 => "Drawer kick out connector",
                50 => "Drawer kick out connector",
                4 => "Ink",
                52 => "Ink",
                _ => "Undefined",
            };
        }

        //  GS  v 0 1D 76 30 00-03/30-33 0001-FFFF 0001-11FF 00-FF...
        internal static string DecodeGsObsoletePrintRasterBitimage(EscPosCmd record, int index)
        {
            string m = record.cmddata[index] switch
            {
                0 => "Normal",
                48 => "Normal",
                1 => "Double Width",
                49 => "Double Width",
                2 => "Double Hight",
                50 => "Double Hight",
                3 => "Quadruple",
                51 => "Quadruple",
                _ => "Undefined",
            };
            int x = BitConverter.ToUInt16(record.cmddata, index + 1);
            string xvalue = (x >= 1) ? x.ToString("D", invariantculture) : "Out of range";
            int y = BitConverter.ToUInt16(record.cmddata, index + 3);
            string yvalue = ((y >= 1) && (y <= 4607)) ? y.ToString("D", invariantculture) : "Out of range";
            int k = x * y;
            if (((y > 0) && (y <= 4607)) && (x > 0))
            {
                System.Drawing.Bitmap bitmap = new((x * 8), y, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                ColorPalette palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = Color.Black;
                bitmap.Palette = palette;
                System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                IntPtr ptr = bmpData.Scan0;
                int stride = bmpData.Stride;
                if (stride == x)
                {
                    System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (index + 5), ptr, k);
                }
                else
                {
                    for (int j = 0; j < y; j++)
                    {
                        IntPtr curptr = bmpData.Scan0 + stride * j;
                        System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (index + 5 + (x * j)), curptr, x);
                    }
                }
                bitmap.UnlockBits(bmpData);
                record.somebinary = bitmap.Clone();
            }
            return $"Mode:{m}, Width:{xvalue} bytes, Height:{yvalue} dots, Size:{k} bytes";
        }

        //  GS  w   1D 77 02-06/44-4C
        internal static string DecodeGsSetBarcodeWidth(EscPosCmd record, int index)
        {
            byte modules = record.cmddata[index];
            if ((modules >= 2) && (modules <= 6))
            {
                return modules.ToString("D", invariantculture);
            }
            else if ((modules >= 68) && (modules <= 76))
            {
                int i = (modules - 64) / 2;
                string f = ((modules & 1) == 1) ? ".5" : ".0";
                return i.ToString("D", invariantculture) + f;
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  z 0 1D 7A 30 00-FF 00-FF
        internal static string DecodeGsSetOnlineRecoveryWaitTime(EscPosCmd record, int index)
        {
            return $"Paper loading wait:{record.cmddata[index]} x 500ms, Recovery confirmation time:{record.cmddata[index + 1]} x 500ms";
        }
    }
}