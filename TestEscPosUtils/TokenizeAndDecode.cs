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

namespace kunif.TestEscPosUtils
{
    using kunif.EscPosUtils;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    [TestClass]
    public class TokenizeAndDecode
    {
#pragma warning disable CS8618 // Non-nullable property '<propertyname>' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        public TestContext TestContext;
#pragma warning restore CS8618 // Non-nullable property '<propertyname>' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

        //[TestInitialize]
        //public void SetupTest()
        //{
        //    if (!Directory.Exists(TestContext.TestResultsDirectory))
        //    {
        //        Directory.CreateDirectory(TestContext.TestResultsDirectory);
        //    }
        //}
        //private static long EscPosCompare(List<EscPosCmd> expected, List<EscPosCmd> result)
        //{
        //    long diffs = 0;
        //    if (expected.Count != result.Count)
        //    {
        //        System.Diagnostics.Trace.WriteLine("Different Record Count Expected: " + expected.Count + ",  Result: " + result.Count + "\n");
        //        diffs++;
        //    }
        //    for (int i = 0; i < expected.Count; i++)
        //    {
        //        if (expected[i].cmdtype != result[i].cmdtype)
        //        {
        //            System.Diagnostics.Trace.WriteLine("Different CmdType Expected: " + expected[i].cmdtype + ",  Result: " + result[i].cmdtype + "\n");
        //            diffs++;
        //        }
        //        if (expected[i].cmdlength != result[i].cmdlength)
        //        {
        //            System.Diagnostics.Trace.WriteLine("Different CmdLength Expected: " + expected[i].cmdlength + ",  Result: " + result[i].cmdlength + "\n");
        //            diffs++;
        //        }
        //        for (int j = 0; j < expected[i].cmdlength; j++)
        //        {
        //            if (expected[i].cmddata[j] != result[i].cmddata[j])
        //            {
        //                System.Diagnostics.Trace.WriteLine("Different CmdData Expected: " + expected[i].cmddata[j].ToString("X2") + ",  Result: " + result[i].cmddata[j].ToString("X2") + "\n");
        //                diffs++;
        //            }
        //        }
        //    }
        //    return diffs;
        //}

        //private static void DisplayList(List<EscPosCmd> escPosCmds)
        //{
        //    string result = string.Empty;

        //    foreach (EscPosCmd item in escPosCmds)
        //    {
        //        if (string.IsNullOrEmpty(item.paramdetail))
        //        {
        //            result += string.Format(CultureInfo.InvariantCulture, "<! {0}\n  | {1} !>\n", item.cmdtype.ToString(), BitConverter.ToString(item.cmddata));
        //        }
        //        else
        //        {
        //            result += string.Format(CultureInfo.InvariantCulture, "<! {0}\n  | {1}\n  # {2} !>\n", item.cmdtype.ToString(), BitConverter.ToString(item.cmddata), item.paramdetail);
        //        }
        //    }
        //    Console.Write(result);
        //}

        private static void FileOutList(List<EscPosCmd> escPosCmds, string outpath)
        {
            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;
            try
            {
                FileMode mode = File.Exists(outpath) ? FileMode.Truncate : FileMode.Create;
                ostrm = new FileStream(outpath, mode, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Cannot open {outpath} for writing");
                Console.WriteLine(e.Message);
                return;
            }
            Console.SetOut(writer);
            string result = string.Empty;

            foreach (EscPosCmd item in escPosCmds)
            {
                if (string.IsNullOrEmpty(item.paramdetail))
                {
                    result += string.Format(CultureInfo.InvariantCulture, "<! {0}\n  | {1} !>\n", item.cmdtype.ToString(), BitConverter.ToString(item.cmddata));
                }
                else
                {
                    result += string.Format(CultureInfo.InvariantCulture, "<! {0}\n  | {1}\n  # {2} !>\n", item.cmdtype.ToString(), BitConverter.ToString(item.cmddata), item.paramdetail);
                }
            }
            Console.Write(result);
            Console.SetOut(oldOut);
            writer.Close();
            ostrm.Close();
            Console.WriteLine("Done");
        }

        public static readonly List<EscPosCmd> prtsbcsResult = new()
        {
            new EscPosCmd(EscPosCmdType.Controls, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }),
            new EscPosCmd(EscPosCmdType.HorizontalTab, new byte[] { 9 }),
            new EscPosCmd(EscPosCmdType.PrintAndLineFeed, new byte[] { 0x0a }),
            new EscPosCmd(EscPosCmdType.Controls, new byte[] { 0x0b }),
            new EscPosCmd(EscPosCmdType.FormFeedPrintAndReturnToStandardMode, new byte[] { 0x0c }),
            new EscPosCmd(EscPosCmdType.PrintAndCarriageReturn, new byte[] { 0x0d }),
            new EscPosCmd(EscPosCmdType.Controls, new byte[] { 0x0e, 0x0f }),
            new EscPosCmd(EscPosCmdType.DleUnknown, new byte[] { 0x10, 0x11 }),
            new EscPosCmd(EscPosCmdType.Controls, new byte[] { 0x12, 0x13, 0x14, 0x15, 0x16, 0x17 }),
            new EscPosCmd(EscPosCmdType.Cancel, new byte[] { 0x18 }),
            new EscPosCmd(EscPosCmdType.Controls, new byte[] { 0x19, 0x1a }),
            new EscPosCmd(EscPosCmdType.EscUnknown, new byte[] { 0x1b, 0x1c }),
            new EscPosCmd(EscPosCmdType.GsUnknown, new byte[] { 0x1d, 0x1e }),
            new EscPosCmd(EscPosCmdType.Controls, new byte[] { 0x1f }),
            new EscPosCmd(EscPosCmdType.PrtPrintables, Enumerable.Range(32, 224).Select(n => (byte)n).ToArray())
        };

        [TestMethod]
        public void TestMethodPrtSbcs()
        {
            byte[] linqprtsbcs = prtsbcsResult.SelectMany(v => v.cmddata).ToArray<byte>();
            var EPTkn = new EscPosTokenizer();
            List<EscPosCmd> escposlist = EPTkn.Scan(linqprtsbcs, EscPosTokenizer.EscPosPrinter);
            escposlist = EscPosDecoder.Convert(escposlist);
            //FileOutList(escposlist, $"{TestContext.TestResultsDirectory}\\prt.txt");
            FileOutList(escposlist, ".\\prt.txt");

            Assert.AreEqual(prtsbcsResult.Count, escposlist.Count);
            for (int i = 0; i < prtsbcsResult.Count; i++)
            {
                Assert.AreEqual(prtsbcsResult[i].cmdtype, escposlist[i].cmdtype);
                Assert.AreEqual(prtsbcsResult[i].cmdlength, escposlist[i].cmdlength);
                for (long j = 0; j < prtsbcsResult[i].cmdlength; j++)
                {
                    Assert.AreEqual(prtsbcsResult[i].cmddata[j], escposlist[i].cmddata[j]);
                }
            }
        }

        public static readonly List<EscPosCmd> vfdsbcsResult = new()
        {
            new EscPosCmd(EscPosCmdType.Controls, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 }),
            new EscPosCmd(EscPosCmdType.VfdMoveCursorLeft, new byte[] { 8 }),
            new EscPosCmd(EscPosCmdType.VfdMoveCursorRight, new byte[] { 9 }),
            new EscPosCmd(EscPosCmdType.VfdMoveCursorDown, new byte[] { 0x0a }),
            new EscPosCmd(EscPosCmdType.VfdHomePosition, new byte[] { 0x0b }),
            new EscPosCmd(EscPosCmdType.VfdClearScreen, new byte[] { 0x0c }),
            new EscPosCmd(EscPosCmdType.VfdMoveCursorLeftMost, new byte[] { 0x0d }),
            new EscPosCmd(EscPosCmdType.Controls, new byte[] { 0x0e, 0x0f }),
            new EscPosCmd(EscPosCmdType.DleUnknown, new byte[] { 0x10, 0x11 }),
            new EscPosCmd(EscPosCmdType.Controls, new byte[] { 0x12, 0x13, 0x14, 0x15, 0x16, 0x17 }),
            new EscPosCmd(EscPosCmdType.VfdClearCursorLine, new byte[] { 0x18 }),
            new EscPosCmd(EscPosCmdType.Controls, new byte[] { 0x19, 0x1a }),
            new EscPosCmd(EscPosCmdType.VfdEscUnknown, new byte[] { 0x1b, 0x1c }),
            new EscPosCmd(EscPosCmdType.Controls, new byte[] { 0x1d, 0x1e }),
            new EscPosCmd(EscPosCmdType.VfdUsUnknown, new byte[] { 0x1f, 0x20 }),
            new EscPosCmd(EscPosCmdType.VfdDisplayables, Enumerable.Range(33, 223).Select(n => (byte)n).ToArray())
        };

        [TestMethod]
        public void TestMethodVfdSbcs()
        {
            byte[] linqvfdsbcs = vfdsbcsResult.SelectMany(v => v.cmddata).ToArray<byte>();
            var EPTkn = new EscPosTokenizer();
            List<EscPosCmd> escposlist = EPTkn.Scan(linqvfdsbcs, EscPosTokenizer.EscPosLineDisplay);
            escposlist = EscPosDecoder.Convert(escposlist);
            //FileOutList(escposlist, $"{TestContext.TestResultsDirectory}\\vfd.txt");
            FileOutList(escposlist, ".\\vfd.txt");

            Assert.AreEqual(vfdsbcsResult.Count, escposlist.Count);
            for (int i = 0; i < vfdsbcsResult.Count; i++)
            {
                Assert.AreEqual(vfdsbcsResult[i].cmdtype, escposlist[i].cmdtype);
                Assert.AreEqual(vfdsbcsResult[i].cmdlength, escposlist[i].cmdlength);
                for (long j = 0; j < vfdsbcsResult[i].cmdlength; j++)
                {
                    Assert.AreEqual(vfdsbcsResult[i].cmddata[j], escposlist[i].cmddata[j]);
                }
            }
        }

        public static readonly List<EscPosCmd> prtdlesResult = new()
        {
            new EscPosCmd(EscPosCmdType.DleTransmitRealtimeStatus, new byte[] { 0x10, 0x04, 0x01 }),                                           //  DLE EOT 10 04 01/02/03/04/07/08/12 [01/02/03]
            new EscPosCmd(EscPosCmdType.DleTransmitRealtimeStatus, new byte[] { 0x10, 0x04, 0x02 }),
            new EscPosCmd(EscPosCmdType.DleTransmitRealtimeStatus, new byte[] { 0x10, 0x04, 0x03 }),
            new EscPosCmd(EscPosCmdType.DleTransmitRealtimeStatus, new byte[] { 0x10, 0x04, 0x04 }),
            new EscPosCmd(EscPosCmdType.DleTransmitRealtimeStatus, new byte[] { 0x10, 0x04, 0x07, 0x01 }),
            new EscPosCmd(EscPosCmdType.DleTransmitRealtimeStatus, new byte[] { 0x10, 0x04, 0x07, 0x02 }),
            new EscPosCmd(EscPosCmdType.DleTransmitRealtimeStatus, new byte[] { 0x10, 0x04, 0x08, 0x03 }),
            new EscPosCmd(EscPosCmdType.DleTransmitRealtimeStatus, new byte[] { 0x10, 0x04, 0x12, 0x01 }),
            new EscPosCmd(EscPosCmdType.DleTransmitRealtimeStatus, new byte[] { 0x10, 0x04, 0x12, 0x02 }),
            new EscPosCmd(EscPosCmdType.DleSendRealtimeRequest, new byte[] { 0x10, 0x05, 0x00 }),                                           //  DLE ENQ 10 05 00/01/02
            new EscPosCmd(EscPosCmdType.DleSendRealtimeRequest, new byte[] { 0x10, 0x05, 0x01 }),
            new EscPosCmd(EscPosCmdType.DleSendRealtimeRequest, new byte[] { 0x10, 0x05, 0x02 }),
            new EscPosCmd(EscPosCmdType.DleGeneratePulseRealtime, new byte[] { 0x10, 0x14, 0x01, 0x00, 0x01 }),                               //  DLE DC4 10 14 01 00/01 01-08
            new EscPosCmd(EscPosCmdType.DleGeneratePulseRealtime, new byte[] { 0x10, 0x14, 0x01, 0x00, 0x08 }),
            new EscPosCmd(EscPosCmdType.DleGeneratePulseRealtime, new byte[] { 0x10, 0x14, 0x01, 0x01, 0x01 }),
            new EscPosCmd(EscPosCmdType.DleGeneratePulseRealtime, new byte[] { 0x10, 0x14, 0x01, 0x01, 0x08 }),
            new EscPosCmd(EscPosCmdType.DleExecPowerOff, new byte[] { 0x10, 0x14, 0x02, 0x01, 0x08 }),                               //  DLE DC4 10 14 02 01 08
            new EscPosCmd(EscPosCmdType.DleSoundBuzzRealtime, new byte[] { 0x10, 0x14, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00 }),             //  DLE DC4 10 14 03 00-07 00 00-08 01 00
            new EscPosCmd(EscPosCmdType.DleSoundBuzzRealtime, new byte[] { 0x10, 0x14, 0x03, 0x00, 0x00, 0x01, 0x01, 0x00 }),
            new EscPosCmd(EscPosCmdType.DleSoundBuzzRealtime, new byte[] { 0x10, 0x14, 0x03, 0x00, 0x00, 0x08, 0x01, 0x00 }),
            new EscPosCmd(EscPosCmdType.DleSoundBuzzRealtime, new byte[] { 0x10, 0x14, 0x03, 0x01, 0x00, 0x00, 0x01, 0x00 }),
            new EscPosCmd(EscPosCmdType.DleSoundBuzzRealtime, new byte[] { 0x10, 0x14, 0x03, 0x01, 0x00, 0x01, 0x01, 0x00 }),
            new EscPosCmd(EscPosCmdType.DleSoundBuzzRealtime, new byte[] { 0x10, 0x14, 0x03, 0x01, 0x00, 0x08, 0x01, 0x00 }),
            new EscPosCmd(EscPosCmdType.DleSoundBuzzRealtime, new byte[] { 0x10, 0x14, 0x03, 0x07, 0x00, 0x00, 0x01, 0x00 }),
            new EscPosCmd(EscPosCmdType.DleSoundBuzzRealtime, new byte[] { 0x10, 0x14, 0x03, 0x07, 0x00, 0x01, 0x01, 0x00 }),
            new EscPosCmd(EscPosCmdType.DleSoundBuzzRealtime, new byte[] { 0x10, 0x14, 0x03, 0x07, 0x00, 0x08, 0x01, 0x00 }),
            new EscPosCmd(EscPosCmdType.DleTransmitSpcifiedStatusRealtime, new byte[] { 0x10, 0x14, 0x07, 0x01 }),                                     //  DLE DC4 10 14 07 01/02/04/05
            new EscPosCmd(EscPosCmdType.DleTransmitSpcifiedStatusRealtime, new byte[] { 0x10, 0x14, 0x07, 0x02 }),
            new EscPosCmd(EscPosCmdType.DleTransmitSpcifiedStatusRealtime, new byte[] { 0x10, 0x14, 0x07, 0x04 }),
            new EscPosCmd(EscPosCmdType.DleTransmitSpcifiedStatusRealtime, new byte[] { 0x10, 0x14, 0x07, 0x05 }),
            new EscPosCmd(EscPosCmdType.DleClearBuffer, new byte[] { 0x10, 0x14, 0x08, 0x01, 0x03, 0x14, 0x01, 0x06, 0x02, 0x08 })  //  DLE DC4 10 14 08 01 03 14 01 06 02 08
        };

        [TestMethod]
        public void TestMethodPrtDles()
        {
            byte[] linqdles = prtdlesResult.SelectMany(v => v.cmddata).ToArray<byte>();
            var EPTkn = new EscPosTokenizer();
            List<EscPosCmd> escposlist = EPTkn.Scan(linqdles, EscPosTokenizer.EscPosPrinter);
            escposlist = EscPosDecoder.Convert(escposlist);
            //FileOutList(escposlist, $"{TestContext.TestResultsDirectory}\\dleP.txt");
            FileOutList(escposlist, ".\\dleP.txt");

            Assert.AreEqual(prtdlesResult.Count, escposlist.Count);
            for (int i = 0; i < prtdlesResult.Count; i++)
            {
                Assert.AreEqual(prtdlesResult[i].cmdtype, escposlist[i].cmdtype);
                Assert.AreEqual(prtdlesResult[i].cmdlength, escposlist[i].cmdlength);
                for (int j = 0; j < prtdlesResult[i].cmdlength; j++)
                {
                    Assert.AreEqual(prtdlesResult[i].cmddata[j], escposlist[i].cmddata[j]);
                }
            }
        }

        [TestMethod]
        public void TestMethodVfdDles()
        {
            byte[] linqdles = prtdlesResult.SelectMany(v => v.cmddata).ToArray<byte>();
            var EPTkn = new EscPosTokenizer();
            List<EscPosCmd> escposlist = EPTkn.Scan(linqdles, EscPosTokenizer.EscPosLineDisplay);
            //FileOutList(escposlist, $"{TestContext.TestResultsDirectory}\\dleV.txt");
            FileOutList(escposlist, ".\\dleV.txt");

            Assert.AreEqual(prtdlesResult.Count, escposlist.Count);
            for (int i = 0; i < prtdlesResult.Count; i++)
            {
                Assert.AreEqual(prtdlesResult[i].cmdtype, escposlist[i].cmdtype);
                Assert.AreEqual(prtdlesResult[i].cmdlength, escposlist[i].cmdlength);
                for (int j = 0; j < prtdlesResult[i].cmdlength; j++)
                {
                    Assert.AreEqual(prtdlesResult[i].cmddata[j], escposlist[i].cmddata[j]);
                }
            }
        }

        public static readonly List<EscPosCmd> prtescsResult = new()
        {
            new EscPosCmd(EscPosCmdType.EscPageModeFormFeed, new byte[] { 0x1b, 0x0c }),                                                                     //  ESC FF  1B 0C  // PrintData in Page Mode
            new EscPosCmd(EscPosCmdType.EscRightSideSpacing, new byte[] { 0x1b, 0x20, 0x00 }),                                                               //  ESC SP  1B 20 00-FF
            new EscPosCmd(EscPosCmdType.EscRightSideSpacing, new byte[] { 0x1b, 0x20, 0xff }),
            new EscPosCmd(EscPosCmdType.EscSelectPrintMode, new byte[] { 0x1b, 0x21, 0x00 }),                                                               //  ESC !   1B 21 bx0xxx00x
            new EscPosCmd(EscPosCmdType.EscSelectPrintMode, new byte[] { 0x1b, 0x21, 0xB9 }),
            new EscPosCmd(EscPosCmdType.EscAbsoluteHorizontalPrintPosition, new byte[] { 0x1b, 0x24, 0x00, 0x00 }),                                                         //  ESC $   1B 24 0000-FFFF
            new EscPosCmd(EscPosCmdType.EscAbsoluteHorizontalPrintPosition, new byte[] { 0x1b, 0x24, 0xff, 0xff }),
            new EscPosCmd(EscPosCmdType.EscSelectUserDefinedCharacterSet, new byte[] { 0x1b, 0x25, 0x00 }),                                                               //  ESC %   1B 25 bnnnnnnnx
            new EscPosCmd(EscPosCmdType.EscSelectUserDefinedCharacterSet, new byte[] { 0x1b, 0x25, 0x01 }),
            new EscPosCmd(EscPosCmdType.EscDefineUserDefinedCharacters1224, new byte[] { 0x1b, 0x26, 0x03, 0x20, 0x20, 0x01, 0x00, 0x00, 0x00 }),                           //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
            new EscPosCmd(EscPosCmdType.EscBeeperBuzzer, new byte[] { 0x1b, 0x28, 0x41, 0x04, 0x00, 0x30, 0x30, 0x01, 0x0a }),                           //  ESC ( A 1B 28 41 04 00 30 30-3A 01-3F 0A-FF
            new EscPosCmd(EscPosCmdType.EscBeeperBuzzerM1a, new byte[] { 0x1b, 0x28, 0x41, 0x03, 0x00, 0x61, 0x01, 0x01 }),                                 //  ESC ( A 1B 28 41 03 00 61 01-07 00-FF
            new EscPosCmd(EscPosCmdType.EscBeeperBuzzerM1b, new byte[] { 0x1b, 0x28, 0x41, 0x05, 0x00, 0x61, 0x64, 0x01, 0x01, 0x01 }),                     //  ESC ( A 1B 28 41 05 00 61 64 00-3F 00-FF 00-FF
            new EscPosCmd(EscPosCmdType.EscBeeperBuzzerOffline, new byte[] { 0x1b, 0x28, 0x41, 0x07, 0x00, 0x62, 0x30, 0x01, 0x64, 0x00, 0x01, 0x01 }),         //  ESC ( A 1B 28 41 07 00 62 30-33 01 64 00/FF 01-32/FF 01-32
            new EscPosCmd(EscPosCmdType.EscBeeperBuzzerNearEnd, new byte[] { 0x1b, 0x28, 0x41, 0x07, 0x00, 0x63, 0x30, 0x01, 0x64, 0x00, 0x01, 0x01 }),         //  ESC ( A 1B 28 41 07 00 63 30 01 64 00/FF 01-32/FF 01-32
            new EscPosCmd(EscPosCmdType.EscSpecifyBatchPrint, new byte[] { 0x1b, 0x28, 0x59, 0x02, 0x00, 0x01, 0x00 }),                                       //  ESC ( Y 1B 28 59 02 00 00/01/30/31 00/01/30/31
            new EscPosCmd(EscPosCmdType.EscSelectBitImageMode, new byte[] { 0x1b, 0x2a, 0x00, 0x08, 0x00, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55 }),   //  ESC *   1B 2A 00/01/20/21 0001-0960 00-FF...
            new EscPosCmd(EscPosCmdType.EscUnderlineMode, new byte[] { 0x1b, 0x2d, 0x02 }),                                                               //  ESC -   1B 2D 00-02/30-32
            new EscPosCmd(EscPosCmdType.EscSelectDefaultLineSpacing, new byte[] { 0x1b, 0x32 }),                                                                     //  ESC 2   1B 32
            new EscPosCmd(EscPosCmdType.EscLineSpacing, new byte[] { 0x1b, 0x33, 0x1e }),                                                               //  ESC 3   1B 33 00-FF
            new EscPosCmd(EscPosCmdType.EscReturnHome, new byte[] { 0x1b, 0x3c }),                                                                     //  ESC <   1B 3C
            new EscPosCmd(EscPosCmdType.EscSelectPeripheralDevice, new byte[] { 0x1b, 0x3d, 0x01 }),                                                               //  ESC =   1B 3D 01-03 or 00-FF
            new EscPosCmd(EscPosCmdType.EscCancelUserDefinedCharacters, new byte[] { 0x1b, 0x3f, 0x20 }),                                                               //  ESC ?   1B 3F 20-7E
            new EscPosCmd(EscPosCmdType.EscInitialize, new byte[] { 0x1b, 0x40 }),                                                                     //  ESC @   1B 40
            new EscPosCmd(EscPosCmdType.EscSetCutSheetEjectLength, new byte[] { 0x1b, 0x43, 0x42 }),                                                               //c ESC C   1B 43 00-FF
            new EscPosCmd(EscPosCmdType.EscHorizontalTabPosition, new byte[] { 0x1b, 0x44, 0x08, 0x10, 0x18, 0x20, 0x28, 0x30, 0x00 }),                           //  ESC D   1B 44 [01-FF]... 00
            new EscPosCmd(EscPosCmdType.EscTurnEmphasizedMode, new byte[] { 0x1b, 0x45, 0x00 }),                                                               //  ESC E   1B 45 bnnnnnnnx
            new EscPosCmd(EscPosCmdType.EscTurnEmphasizedMode, new byte[] { 0x1b, 0x45, 0x01 }),
            new EscPosCmd(EscPosCmdType.EscSetCancelCutSheetReverseEject, new byte[] { 0x1b, 0x46, 0x00 }),                                                               //c ESC F   1B 46 bnnnnnnnx
            new EscPosCmd(EscPosCmdType.EscSetCancelCutSheetReverseEject, new byte[] { 0x1b, 0x46, 0x01 }),
            new EscPosCmd(EscPosCmdType.EscTurnDoubleStrikeMode, new byte[] { 0x1b, 0x47, 0x00 }),                                                               //  ESC G   1B 47 bnnnnnnnx
            new EscPosCmd(EscPosCmdType.EscTurnDoubleStrikeMode, new byte[] { 0x1b, 0x47, 0x01 }),
            new EscPosCmd(EscPosCmdType.EscPrintAndFeedPaper, new byte[] { 0x1b, 0x4a, 0x00 }),                                                               //  ESC J   1B 4A 00-FF
            new EscPosCmd(EscPosCmdType.EscPrintAndFeedPaper, new byte[] { 0x1b, 0x4a, 0x01 }),
            new EscPosCmd(EscPosCmdType.EscPrintAndFeedPaper, new byte[] { 0x1b, 0x4a, 0xff }),
            new EscPosCmd(EscPosCmdType.EscPrintAndReverseFeed, new byte[] { 0x1b, 0x4b, 0x00 }),                                                               //  ESC K   1B 4B 00-30
            new EscPosCmd(EscPosCmdType.EscPrintAndReverseFeed, new byte[] { 0x1b, 0x4b, 0x01 }),
            new EscPosCmd(EscPosCmdType.EscPrintAndReverseFeed, new byte[] { 0x1b, 0x4b, 0x30 }),
            new EscPosCmd(EscPosCmdType.EscSelectPageMode, new byte[] { 0x1b, 0x4c }),                                                                     //  ESC L   1B 4C
            new EscPosCmd(EscPosCmdType.EscSelectCharacterFont, new byte[] { 0x1b, 0x4d, 0x00 }),                                                               //  ESC M   1B 4D 00-04/30-34/61/62
            new EscPosCmd(EscPosCmdType.EscSelectCharacterFont, new byte[] { 0x1b, 0x4d, 0x00 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterFont, new byte[] { 0x1b, 0x4d, 0x00 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterFont, new byte[] { 0x1b, 0x4d, 0x00 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterFont, new byte[] { 0x1b, 0x4d, 0x00 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterFont, new byte[] { 0x1b, 0x4d, 0x30 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterFont, new byte[] { 0x1b, 0x4d, 0x31 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterFont, new byte[] { 0x1b, 0x4d, 0x32 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterFont, new byte[] { 0x1b, 0x4d, 0x33 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterFont, new byte[] { 0x1b, 0x4d, 0x34 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterFont, new byte[] { 0x1b, 0x4d, 0x61 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterFont, new byte[] { 0x1b, 0x4d, 0x62 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterFont, new byte[] { 0x1b, 0x4d, 0x00 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x01 }),                                                               //  ESC R   1B 52 00-11/42-4B/52
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x02 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x03 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x04 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x05 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x06 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x07 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x08 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x09 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x0a }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x0b }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x0c }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x0d }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x0e }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x0f }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x10 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x11 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x42 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x43 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x44 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x45 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x46 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x47 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x48 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x49 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x4a }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x4b }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x52 }),
            new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x00 }),
            new EscPosCmd(EscPosCmdType.EscSelectStandardMode, new byte[] { 0x1b, 0x53 }),                                                                     //  ESC S   1B 53
            new EscPosCmd(EscPosCmdType.EscSelectPrintDirection, new byte[] { 0x1b, 0x54, 0x00 }),                                                               //  ESC T   1B 54 00-03/30-33
            new EscPosCmd(EscPosCmdType.EscSelectPrintDirection, new byte[] { 0x1b, 0x54, 0x01 }),
            new EscPosCmd(EscPosCmdType.EscSelectPrintDirection, new byte[] { 0x1b, 0x54, 0x02 }),
            new EscPosCmd(EscPosCmdType.EscSelectPrintDirection, new byte[] { 0x1b, 0x54, 0x03 }),
            new EscPosCmd(EscPosCmdType.EscSelectPrintDirection, new byte[] { 0x1b, 0x54, 0x30 }),
            new EscPosCmd(EscPosCmdType.EscSelectPrintDirection, new byte[] { 0x1b, 0x54, 0x31 }),
            new EscPosCmd(EscPosCmdType.EscSelectPrintDirection, new byte[] { 0x1b, 0x54, 0x32 }),
            new EscPosCmd(EscPosCmdType.EscSelectPrintDirection, new byte[] { 0x1b, 0x54, 0x33 }),
            new EscPosCmd(EscPosCmdType.EscTurnUnidirectionalPrintMode, new byte[] { 0x1b, 0x55, 0x00 }),                                                               //  ESC U   1B 55 bnnnnnnnx
            new EscPosCmd(EscPosCmdType.EscTurnUnidirectionalPrintMode, new byte[] { 0x1b, 0x55, 0x01 }),
            new EscPosCmd(EscPosCmdType.EscTurn90digreeClockwiseRotationMode, new byte[] { 0x1b, 0x56, 0x00 }),                                                               //  ESC V   1B 56 00-02/30-32
            new EscPosCmd(EscPosCmdType.EscTurn90digreeClockwiseRotationMode, new byte[] { 0x1b, 0x56, 0x01 }),
            new EscPosCmd(EscPosCmdType.EscTurn90digreeClockwiseRotationMode, new byte[] { 0x1b, 0x56, 0x02 }),
            new EscPosCmd(EscPosCmdType.EscTurn90digreeClockwiseRotationMode, new byte[] { 0x1b, 0x56, 0x30 }),
            new EscPosCmd(EscPosCmdType.EscTurn90digreeClockwiseRotationMode, new byte[] { 0x1b, 0x56, 0x31 }),
            new EscPosCmd(EscPosCmdType.EscTurn90digreeClockwiseRotationMode, new byte[] { 0x1b, 0x56, 0x32 }),
            new EscPosCmd(EscPosCmdType.EscTurn90digreeClockwiseRotationMode, new byte[] { 0x1b, 0x56, 0x00 }),
            new EscPosCmd(EscPosCmdType.EscSetPrintAreaInPageMode, new byte[] { 0x1b, 0x57, 0x00, 0x00, 0x00, 0x00, 0x40, 0x02, 0xc4, 0x05 }),                     //  ESC W   1B 57 0000-FFFF 0000-FFFF 0001-FFFF 0001-FFFF
            new EscPosCmd(EscPosCmdType.EscSetRelativeHorizontalPrintPosition, new byte[] { 0x1b, 0x5c, 0x00, 0x80 }),                                                         //  ESC \   1B 5C 8000-7FFF
            new EscPosCmd(EscPosCmdType.EscSetRelativeHorizontalPrintPosition, new byte[] { 0x1b, 0x5c, 0x01, 0x00 }),
            new EscPosCmd(EscPosCmdType.EscSetRelativeHorizontalPrintPosition, new byte[] { 0x1b, 0x5c, 0xff, 0x7f }),
            new EscPosCmd(EscPosCmdType.EscSelectJustification, new byte[] { 0x1b, 0x61, 0x00 }),                                                               //  ESC a   1B 61 00-02/30-32
            new EscPosCmd(EscPosCmdType.EscSelectJustification, new byte[] { 0x1b, 0x61, 0x01 }),
            new EscPosCmd(EscPosCmdType.EscSelectJustification, new byte[] { 0x1b, 0x61, 0x02 }),
            new EscPosCmd(EscPosCmdType.EscSelectJustification, new byte[] { 0x1b, 0x61, 0x30 }),
            new EscPosCmd(EscPosCmdType.EscSelectJustification, new byte[] { 0x1b, 0x61, 0x31 }),
            new EscPosCmd(EscPosCmdType.EscSelectJustification, new byte[] { 0x1b, 0x61, 0x32 }),
            new EscPosCmd(EscPosCmdType.EscSelectJustification, new byte[] { 0x1b, 0x61, 0x00 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesPrinting, new byte[] { 0x1b, 0x63, 0x30, 0x01 }),                                                         //c ESC c 0 1B 63 30 b0000xxxx
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesPrinting, new byte[] { 0x1b, 0x63, 0x30, 0x02 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesPrinting, new byte[] { 0x1b, 0x63, 0x30, 0x03 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesPrinting, new byte[] { 0x1b, 0x63, 0x30, 0x04 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesPrinting, new byte[] { 0x1b, 0x63, 0x30, 0x08 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesPrinting, new byte[] { 0x1b, 0x63, 0x30, 0x09 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesPrinting, new byte[] { 0x1b, 0x63, 0x30, 0x0a }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesPrinting, new byte[] { 0x1b, 0x63, 0x30, 0x0b }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesPrinting, new byte[] { 0x1b, 0x63, 0x30, 0x01 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x01 }),                                                         //c ESC c 1 1B 63 31 b0x00xxxx
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x02 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x03 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x04 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x05 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x06 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x07 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x08 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x09 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x0a }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x0b }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x0c }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x0d }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x0e }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x0f }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x40 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x41 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x42 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x43 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x44 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x45 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x46 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperTypesCommandSettings, new byte[] { 0x1b, 0x63, 0x31, 0x47 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperSensorsPaperEndSignals, new byte[] { 0x1b, 0x63, 0x33, 0x0c }),                                                         //  ESC c 3 1B 63 33 bccccxxxx
            new EscPosCmd(EscPosCmdType.EscSelectPaperSensorsPaperEndSignals, new byte[] { 0x1b, 0x63, 0x33, 0x10 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperSensorsPaperEndSignals, new byte[] { 0x1b, 0x63, 0x33, 0x20 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperSensorsPaperEndSignals, new byte[] { 0x1b, 0x63, 0x33, 0xc0 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperSensorsStopPrinting, new byte[] { 0x1b, 0x63, 0x34, 0x0c }),                                                         //  ESC c 4 1B 63 34 bccccxxxx
            new EscPosCmd(EscPosCmdType.EscSelectPaperSensorsStopPrinting, new byte[] { 0x1b, 0x63, 0x34, 0x10 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperSensorsStopPrinting, new byte[] { 0x1b, 0x63, 0x34, 0x20 }),
            new EscPosCmd(EscPosCmdType.EscSelectPaperSensorsStopPrinting, new byte[] { 0x1b, 0x63, 0x34, 0xc0 }),
            new EscPosCmd(EscPosCmdType.EscEnableDisablePanelButton, new byte[] { 0x1b, 0x63, 0x35, 0x00 }),                                                         //  ESC c 5 1B 63 35 bnnnnnnnx
            new EscPosCmd(EscPosCmdType.EscEnableDisablePanelButton, new byte[] { 0x1b, 0x63, 0x35, 0x01 }),
            new EscPosCmd(EscPosCmdType.EscPrintAndFeedNLines, new byte[] { 0x1b, 0x64, 0x00 }),                                                               //  ESC d   1B 64 00-FF
            new EscPosCmd(EscPosCmdType.EscPrintAndFeedNLines, new byte[] { 0x1b, 0x64, 0x01 }),
            new EscPosCmd(EscPosCmdType.EscPrintAndFeedNLines, new byte[] { 0x1b, 0x64, 0xff }),
            new EscPosCmd(EscPosCmdType.EscPrintAndReverseFeedNLines, new byte[] { 0x1b, 0x65, 0x00 }),                                                               //  ESC e   1B 65 00-02
            new EscPosCmd(EscPosCmdType.EscPrintAndReverseFeedNLines, new byte[] { 0x1b, 0x65, 0x01 }),
            new EscPosCmd(EscPosCmdType.EscPrintAndReverseFeedNLines, new byte[] { 0x1b, 0x65, 0x02 }),
            new EscPosCmd(EscPosCmdType.EscCutSheetWaitTime, new byte[] { 0x1b, 0x66, 0x00, 0x00 }),                                                         //c ESC f   1B 66 00-0F 00-40
            new EscPosCmd(EscPosCmdType.EscCutSheetWaitTime, new byte[] { 0x1b, 0x66, 0x0f, 0x40 }),
            new EscPosCmd(EscPosCmdType.EscCutSheetWaitTime, new byte[] { 0x1b, 0x66, 0x0f, 0x00 }),
            new EscPosCmd(EscPosCmdType.EscCutSheetWaitTime, new byte[] { 0x1b, 0x66, 0x00, 0x40 }),
            new EscPosCmd(EscPosCmdType.EscObsoletePartialCut1Point, new byte[] { 0x1b, 0x69 }),                                                                     //  ESC i   1B 69
            new EscPosCmd(EscPosCmdType.EscObsoletePartialCut3Point, new byte[] { 0x1b, 0x6d }),                                                                     //  ESC m   1B 6D
            new EscPosCmd(EscPosCmdType.EscGeneratePulse, new byte[] { 0x1b, 0x70, 0x00, 0x32, 0x64 }),                                                   //  ESC p   1B 70 00/01/30/31 00-FF 00-FF
            new EscPosCmd(EscPosCmdType.EscGeneratePulse, new byte[] { 0x1b, 0x70, 0x01, 0x32, 0x64 }),
            new EscPosCmd(EscPosCmdType.EscGeneratePulse, new byte[] { 0x1b, 0x70, 0x30, 0x32, 0x64 }),
            new EscPosCmd(EscPosCmdType.EscGeneratePulse, new byte[] { 0x1b, 0x70, 0x31, 0x32, 0x64 }),
            new EscPosCmd(EscPosCmdType.EscReleasePaper, new byte[] { 0x1b, 0x71 }),                                                                     //c ESC q   1B 71
            new EscPosCmd(EscPosCmdType.EscSelectPrinterColor, new byte[] { 0x1b, 0x72, 0x00 }),                                                               //  ESC r   1B 72 00/01/30/31
            new EscPosCmd(EscPosCmdType.EscSelectPrinterColor, new byte[] { 0x1b, 0x72, 0x01 }),
            new EscPosCmd(EscPosCmdType.EscSelectPrinterColor, new byte[] { 0x1b, 0x72, 0x30 }),
            new EscPosCmd(EscPosCmdType.EscSelectPrinterColor, new byte[] { 0x1b, 0x72, 0x31 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x00 }),                                                               //  ESC t   1B 74 00-08/0B-1A/1E-35/42-4B/52/FE/FF
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x01 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x02 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x03 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x04 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x05 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x06 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x07 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x08 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x0b }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x0c }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x0d }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x0e }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x0f }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x10 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x11 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x12 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x13 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x14 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x15 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x16 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x17 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x18 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x19 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x1a }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x1e }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x1f }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x20 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x21 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x22 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x23 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x24 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x25 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x26 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x27 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x28 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x29 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x2a }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x2b }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x2c }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x2d }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x2e }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x2f }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x30 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x31 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x32 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x33 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x34 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x35 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x42 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x43 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x44 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x45 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x46 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x47 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x48 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x49 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x4a }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x4b }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x52 }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0xfe }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0xff }),
            new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x00 }),
            new EscPosCmd(EscPosCmdType.EscObsoleteTransmitPeripheralDeviceStatus, new byte[] { 0x1b, 0x75, 0x00 }),                                                               //  ESC u   1B 75 00/30
            new EscPosCmd(EscPosCmdType.EscObsoleteTransmitPeripheralDeviceStatus, new byte[] { 0x1b, 0x75, 0x30 }),
            new EscPosCmd(EscPosCmdType.EscObsoleteTransmitPaperSensorStatus, new byte[] { 0x1b, 0x76 }),                                                                     //  ESC v   1B 76
            new EscPosCmd(EscPosCmdType.EscTurnUpsideDownPrintMode, new byte[] { 0x1b, 0x7b, 0x00 }),                                                               //  ESC {   1B 7B bnnnnnnnx
            new EscPosCmd(EscPosCmdType.EscTurnUpsideDownPrintMode, new byte[] { 0x1b, 0x7b, 0x01 })
        };

        [TestMethod]
        public void TestMethodPrtEscs()
        {
            byte[] linqprtescs = prtescsResult.SelectMany(v => v.cmddata).ToArray<byte>();
            var EPTkn = new EscPosTokenizer();
            List<EscPosCmd> escposlist = EPTkn.Scan(linqprtescs, EscPosTokenizer.EscPosPrinter);
            escposlist = EscPosDecoder.Convert(escposlist);
            //FileOutList(escposlist, $"{TestContext.TestResultsDirectory}\\escP.txt");
            FileOutList(escposlist, ".\\escP.txt");

            Assert.AreEqual(prtescsResult.Count, escposlist.Count);
            for (int i = 0; i < prtescsResult.Count; i++)
            {
                Assert.AreEqual(prtescsResult[i].cmdtype, escposlist[i].cmdtype);
                Assert.AreEqual(prtescsResult[i].cmdlength, escposlist[i].cmdlength);
                for (int j = 0; j < prtescsResult[i].cmdlength; j++)
                {
                    Assert.AreEqual(prtescsResult[i].cmddata[j], escposlist[i].cmddata[j]);
                }
            }
        }

        public static readonly List<EscPosCmd> vfdescsResult = new()
        {
            new EscPosCmd(EscPosCmdType.VfdEscSelectCancelUserDefinedCharacterSet, new byte[] { 0x1b, 0x25, 0x00 }),                               //  ESC %   1B 25 00-FF
            new EscPosCmd(EscPosCmdType.VfdEscSelectCancelUserDefinedCharacterSet, new byte[] { 0x1b, 0x25, 0x01 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCancelUserDefinedCharacterSet, new byte[] { 0x1b, 0x25, 0xff }),
            new EscPosCmd(EscPosCmdType.VfdEscDefineUserDefinedCharacters0816, new byte[] { 0x1b, 0x26, 0x02, 0x20, 0x20, 0x01, 0x00, 0x00 }), //  ESC &   1B 26 00/01 20-7E 20-7E 00-08 00-FF...
            new EscPosCmd(EscPosCmdType.VfdEscSelectPeripheralDevice, new byte[] { 0x1b, 0x3D, 0x02 }),                               //  ESC =   1B 3D 01/02/03
            new EscPosCmd(EscPosCmdType.VfdEscCancelUserDefinedCharacters, new byte[] { 0x1b, 0x3F, 0x20 }),                               //  ESC ?   1B 3F 20-7E
            new EscPosCmd(EscPosCmdType.VfdEscCancelUserDefinedCharacters, new byte[] { 0x1b, 0x3F, 0x7e }),
            new EscPosCmd(EscPosCmdType.VfdEscInitialize, new byte[] { 0x1b, 0x40 }),                                     //  ESC @   1B 40
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x00 }),                               //  ESC R   1B 52 00-11
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x01 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x02 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x03 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x04 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x05 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x06 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x07 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x08 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x09 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x0a }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x0b }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x0c }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x0d }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x0e }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x0f }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x10 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x11 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectInternationalCharacterSet, new byte[] { 0x1b, 0x52, 0x00 }),
            new EscPosCmd(EscPosCmdType.VfdEscCancelWindowArea, new byte[] { 0x1b, 0x57, 0x01, 0x00 }),                         //  ESC W   1B 57 01-04 00/30
            new EscPosCmd(EscPosCmdType.VfdEscCancelWindowArea, new byte[] { 0x1b, 0x57, 0x02, 0x30 }),
            new EscPosCmd(EscPosCmdType.VfdEscCancelWindowArea, new byte[] { 0x1b, 0x57, 0x03, 0x00 }),
            new EscPosCmd(EscPosCmdType.VfdEscCancelWindowArea, new byte[] { 0x1b, 0x57, 0x04, 0x30 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectWindowArea, new byte[] { 0x1b, 0x57, 0x01, 0x01, 0x01, 0x01, 0x14, 0x02 }), //  ESC W   1B 57 01-04 01/31 01-14 01/02 01-14 01/02
            new EscPosCmd(EscPosCmdType.VfdEscSelectWindowArea, new byte[] { 0x1b, 0x57, 0x01, 0x31, 0x01, 0x01, 0x14, 0x02 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectWindowArea, new byte[] { 0x1b, 0x57, 0x02, 0x01, 0x01, 0x01, 0x14, 0x01 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectWindowArea, new byte[] { 0x1b, 0x57, 0x03, 0x31, 0x01, 0x02, 0x14, 0x02 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectWindowArea, new byte[] { 0x1b, 0x57, 0x04, 0x01, 0x0b, 0x01, 0x14, 0x02 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectWindowArea, new byte[] { 0x1b, 0x57, 0x04, 0x31, 0x01, 0x01, 0x0a, 0x02 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x00 }),                               //  ESC t   1B 74 00-13/1E-35/FE/FF
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x01 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x02 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x03 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x04 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x05 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x06 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x07 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x08 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x09 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x0a }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x0b }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x0c }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x0d }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x0e }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x0f }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x10 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x11 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x12 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x13 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x1e }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x1f }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x20 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x21 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x22 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x23 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x24 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x25 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x26 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x27 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x28 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x29 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x2a }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x2b }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x2c }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x2d }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x2e }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x2f }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x30 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x31 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x32 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x33 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x34 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x35 }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0xfe }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0xff }),
            new EscPosCmd(EscPosCmdType.VfdEscSelectCharacterCodeTable, new byte[] { 0x1b, 0x74, 0x00 })
        };

        [TestMethod]
        public void TestMethodVfdEscs()
        {
            byte[] linqvfdescs = vfdescsResult.SelectMany(v => v.cmddata).ToArray<byte>();
            var EPTkn = new EscPosTokenizer();
            List<EscPosCmd> escposlist = EPTkn.Scan(linqvfdescs, EscPosTokenizer.EscPosLineDisplay);
            escposlist = EscPosDecoder.Convert(escposlist);
            //FileOutList(escposlist, $"{TestContext.TestResultsDirectory}\\escV.txt");
            FileOutList(escposlist, ".\\escV.txt");

            Assert.AreEqual(vfdescsResult.Count, escposlist.Count);
            for (int i = 0; i < vfdescsResult.Count; i++)
            {
                Assert.AreEqual(vfdescsResult[i].cmdtype, escposlist[i].cmdtype);
                Assert.AreEqual(vfdescsResult[i].cmdlength, escposlist[i].cmdlength);
                for (int j = 0; j < vfdescsResult[i].cmdlength; j++)
                {
                    Assert.AreEqual(vfdescsResult[i].cmddata[j], escposlist[i].cmddata[j]);
                }
            }
        }

        public static readonly List<EscPosCmd> prtfssResult = new()
        {
            new EscPosCmd(EscPosCmdType.FsSelectPrintModeKanji, new byte[] { 0x1c, 0x21, 0x00 }),                                                                                                               //  FS  !   1C 21 bx000xx00
            new EscPosCmd(EscPosCmdType.FsSelectPrintModeKanji, new byte[] { 0x1c, 0x21, 0x04 }),
            new EscPosCmd(EscPosCmdType.FsSelectPrintModeKanji, new byte[] { 0x1c, 0x21, 0x08 }),
            new EscPosCmd(EscPosCmdType.FsSelectPrintModeKanji, new byte[] { 0x1c, 0x21, 0x0c }),
            new EscPosCmd(EscPosCmdType.FsSelectPrintModeKanji, new byte[] { 0x1c, 0x21, 0x80 }),
            new EscPosCmd(EscPosCmdType.FsSelectPrintModeKanji, new byte[] { 0x1c, 0x21, 0x84 }),
            new EscPosCmd(EscPosCmdType.FsSelectPrintModeKanji, new byte[] { 0x1c, 0x21, 0x88 }),
            new EscPosCmd(EscPosCmdType.FsSelectPrintModeKanji, new byte[] { 0x1c, 0x21, 0x8c }),
            new EscPosCmd(EscPosCmdType.FsSelectKanjiCharacterMode, new byte[] { 0x1c, 0x26 }),                                                                                                                     //  FS  &   1C 26
            new EscPosCmd(EscPosCmdType.FsSelectKanjiCharacterFont, new byte[] { 0x1c, 0x28, 0x41, 0x02, 0x00, 0x30, 0x00 }),                                                                                       //  FS  ( A 1C 28 41 02 00 30 00-02/30-32
            new EscPosCmd(EscPosCmdType.FsSelectKanjiCharacterFont, new byte[] { 0x1c, 0x28, 0x41, 0x02, 0x00, 0x30, 0x01 }),
            new EscPosCmd(EscPosCmdType.FsSelectKanjiCharacterFont, new byte[] { 0x1c, 0x28, 0x41, 0x02, 0x00, 0x30, 0x02 }),
            new EscPosCmd(EscPosCmdType.FsSelectKanjiCharacterFont, new byte[] { 0x1c, 0x28, 0x41, 0x02, 0x00, 0x30, 0x30 }),
            new EscPosCmd(EscPosCmdType.FsSelectKanjiCharacterFont, new byte[] { 0x1c, 0x28, 0x41, 0x02, 0x00, 0x30, 0x31 }),
            new EscPosCmd(EscPosCmdType.FsSelectKanjiCharacterFont, new byte[] { 0x1c, 0x28, 0x41, 0x02, 0x00, 0x30, 0x32 }),
            new EscPosCmd(EscPosCmdType.FsSelectKanjiCharacterFont, new byte[] { 0x1c, 0x28, 0x41, 0x02, 0x00, 0x30, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsSelectCharacterEncodeSystem, new byte[] { 0x1c, 0x28, 0x43, 0x02, 0x00, 0x30, 0x01 }),                                                                                       //  FS  ( C 1C 28 43 02 00 30 01/02/31/32
            new EscPosCmd(EscPosCmdType.FsSelectCharacterEncodeSystem, new byte[] { 0x1c, 0x28, 0x43, 0x02, 0x00, 0x30, 0x02 }),
            new EscPosCmd(EscPosCmdType.FsSelectCharacterEncodeSystem, new byte[] { 0x1c, 0x28, 0x43, 0x02, 0x00, 0x30, 0x31 }),
            new EscPosCmd(EscPosCmdType.FsSelectCharacterEncodeSystem, new byte[] { 0x1c, 0x28, 0x43, 0x02, 0x00, 0x30, 0x32 }),
            new EscPosCmd(EscPosCmdType.FsSelectCharacterEncodeSystem, new byte[] { 0x1c, 0x28, 0x43, 0x02, 0x00, 0x30, 0x01 }),
            new EscPosCmd(EscPosCmdType.FsSetFontPriority, new byte[] { 0x1c, 0x28, 0x43, 0x03, 0x00, 0x3C, 0x00, 0x00 }),                                                                                 //  FS  ( C 1C 28 43 03 00 3C 00/01 00/0B/14/1E/29
            new EscPosCmd(EscPosCmdType.FsSetFontPriority, new byte[] { 0x1c, 0x28, 0x43, 0x03, 0x00, 0x3C, 0x00, 0x0b }),
            new EscPosCmd(EscPosCmdType.FsSetFontPriority, new byte[] { 0x1c, 0x28, 0x43, 0x03, 0x00, 0x3C, 0x00, 0x14 }),
            new EscPosCmd(EscPosCmdType.FsSetFontPriority, new byte[] { 0x1c, 0x28, 0x43, 0x03, 0x00, 0x3C, 0x00, 0x1e }),
            new EscPosCmd(EscPosCmdType.FsSetFontPriority, new byte[] { 0x1c, 0x28, 0x43, 0x03, 0x00, 0x3C, 0x00, 0x29 }),
            new EscPosCmd(EscPosCmdType.FsSetFontPriority, new byte[] { 0x1c, 0x28, 0x43, 0x03, 0x00, 0x3C, 0x00, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsSetFontPriority, new byte[] { 0x1c, 0x28, 0x43, 0x03, 0x00, 0x3C, 0x01, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsSetFontPriority, new byte[] { 0x1c, 0x28, 0x43, 0x03, 0x00, 0x3C, 0x01, 0x0b }),
            new EscPosCmd(EscPosCmdType.FsSetFontPriority, new byte[] { 0x1c, 0x28, 0x43, 0x03, 0x00, 0x3C, 0x01, 0x14 }),
            new EscPosCmd(EscPosCmdType.FsSetFontPriority, new byte[] { 0x1c, 0x28, 0x43, 0x03, 0x00, 0x3C, 0x01, 0x1e }),
            new EscPosCmd(EscPosCmdType.FsSetFontPriority, new byte[] { 0x1c, 0x28, 0x43, 0x03, 0x00, 0x3C, 0x01, 0x29 }),
            new EscPosCmd(EscPosCmdType.FsSetFontPriority, new byte[] { 0x1c, 0x28, 0x43, 0x03, 0x00, 0x3C, 0x01, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsCancelSetValuesTopBottomLogo, new byte[] { 0x1c, 0x28, 0x45, 0x06, 0x00, 0x3C, 0x02, 0x30, 0x43, 0x4C, 0x52 }),                                                               //  FS  ( E 1C 28 45 06 00 3C 02 30/31 43 4C 52
            new EscPosCmd(EscPosCmdType.FsCancelSetValuesTopBottomLogo, new byte[] { 0x1c, 0x28, 0x45, 0x06, 0x00, 0x3C, 0x02, 0x31, 0x43, 0x4C, 0x52 }),
            new EscPosCmd(EscPosCmdType.FsTransmitSetValuesTopBottomLogo, new byte[] { 0x1c, 0x28, 0x45, 0x03, 0x00, 0x3D, 0x02, 0x30 }),                                                                                 //  FS  ( E 1C 28 45 03 00 3D 02 30-32
            new EscPosCmd(EscPosCmdType.FsTransmitSetValuesTopBottomLogo, new byte[] { 0x1c, 0x28, 0x45, 0x03, 0x00, 0x3D, 0x02, 0x31 }),
            new EscPosCmd(EscPosCmdType.FsTransmitSetValuesTopBottomLogo, new byte[] { 0x1c, 0x28, 0x45, 0x03, 0x00, 0x3D, 0x02, 0x32 }),
            new EscPosCmd(EscPosCmdType.FsSetTopLogoPrinting, new byte[] { 0x1c, 0x28, 0x45, 0x06, 0x00, 0x3E, 0x02, 0x20, 0x20, 0x31, 0x00 }),                                                               //  FS  ( E 1C 28 45 06 00 3E 02 20-7E 20-7E 30-32 00-FF
            new EscPosCmd(EscPosCmdType.FsSetBottomLogoPrinting, new byte[] { 0x1c, 0x28, 0x45, 0x05, 0x00, 0x3F, 0x02, 0x21, 0x21, 0x30 }),                                                                     //  FS  ( E 1C 28 45 05 00 3F 02 20-7E 20-7E 30-32
            new EscPosCmd(EscPosCmdType.FsSetBottomLogoPrinting, new byte[] { 0x1c, 0x28, 0x45, 0x05, 0x00, 0x3F, 0x02, 0x50, 0x50, 0x31 }),
            new EscPosCmd(EscPosCmdType.FsSetBottomLogoPrinting, new byte[] { 0x1c, 0x28, 0x45, 0x05, 0x00, 0x3F, 0x02, 0x7e, 0x7e, 0x32 }),
            new EscPosCmd(EscPosCmdType.FsMakeExtendSettingsTopBottomLogoPrinting, new byte[] { 0x1c, 0x28, 0x45, 0x0c, 0x00, 0x40, 0x02, 0x30, 0x31, 0x40, 0x31, 0x41, 0x31, 0x42, 0x31, 0x43, 0x31 }),                           //  FS  ( E 1C 28 45 0004-000C 40 02 [30/40-43 30/31]...
            new EscPosCmd(EscPosCmdType.FsEnableDisableTopBottomLogoPrinting, new byte[] { 0x1c, 0x28, 0x45, 0x04, 0x00, 0x41, 0x02, 0x30, 0x30 }),                                                                           //  FS  ( E 1C 28 45 04 00 41 02 30/31 30/31
            new EscPosCmd(EscPosCmdType.FsEnableDisableTopBottomLogoPrinting, new byte[] { 0x1c, 0x28, 0x45, 0x04, 0x00, 0x41, 0x02, 0x30, 0x31 }),
            new EscPosCmd(EscPosCmdType.FsEnableDisableTopBottomLogoPrinting, new byte[] { 0x1c, 0x28, 0x45, 0x04, 0x00, 0x41, 0x02, 0x31, 0x30 }),
            new EscPosCmd(EscPosCmdType.FsEnableDisableTopBottomLogoPrinting, new byte[] { 0x1c, 0x28, 0x45, 0x04, 0x00, 0x41, 0x02, 0x31, 0x31 }),
            new EscPosCmd(EscPosCmdType.FsPaperLayoutSetting, new byte[] { 0x1c, 0x28, 0x4C, 0x10, 0x00, 0x21, 0x32, 0x30, 0x3b, 0x30, 0x3b, 0x30, 0x3b, 0x30, 0x3b, 0x30, 0x3b, 0x33, 0x30, 0x30, 0x3b }),   //  FS  ( L 1C 28 4C 0008-001a 21 30-33 [[30-39]... 3B]...
            new EscPosCmd(EscPosCmdType.FsPaperLayoutInformationTransmission, new byte[] { 0x1c, 0x28, 0x4C, 0x02, 0x00, 0x22, 0x40 }),                                                                                       //  FS  ( L 1C 28 4C 02 00 22 40/50
            new EscPosCmd(EscPosCmdType.FsPaperLayoutInformationTransmission, new byte[] { 0x1c, 0x28, 0x4C, 0x02, 0x00, 0x22, 0x50 }),
            new EscPosCmd(EscPosCmdType.FsTransmitPositioningInformation, new byte[] { 0x1c, 0x28, 0x4C, 0x02, 0x00, 0x30, 0x30 }),                                                                                       //  FS  ( L 1C 28 4C 02 00 30 30
            new EscPosCmd(EscPosCmdType.FsFeedPaperLabelPeelingPosition, new byte[] { 0x1c, 0x28, 0x4C, 0x02, 0x00, 0x41, 0x30 }),                                                                                       //  FS  ( L 1C 28 4C 02 00 41 30/31
            new EscPosCmd(EscPosCmdType.FsFeedPaperLabelPeelingPosition, new byte[] { 0x1c, 0x28, 0x4C, 0x02, 0x00, 0x41, 0x31 }),
            new EscPosCmd(EscPosCmdType.FsFeedPaperCuttingPosition, new byte[] { 0x1c, 0x28, 0x4C, 0x02, 0x00, 0x42, 0x30 }),                                                                                       //  FS  ( L 1C 28 4C 02 00 42 30/31
            new EscPosCmd(EscPosCmdType.FsFeedPaperCuttingPosition, new byte[] { 0x1c, 0x28, 0x4C, 0x02, 0x00, 0x42, 0x31 }),
            new EscPosCmd(EscPosCmdType.FsFeedPaperPrintStartingPosition, new byte[] { 0x1c, 0x28, 0x4C, 0x02, 0x00, 0x43, 0x30 }),                                                                                       //  FS  ( L 1C 28 4C 02 00 43 30-32
            new EscPosCmd(EscPosCmdType.FsFeedPaperPrintStartingPosition, new byte[] { 0x1c, 0x28, 0x4C, 0x02, 0x00, 0x43, 0x31 }),
            new EscPosCmd(EscPosCmdType.FsFeedPaperPrintStartingPosition, new byte[] { 0x1c, 0x28, 0x4C, 0x02, 0x00, 0x43, 0x32 }),
            new EscPosCmd(EscPosCmdType.FsPaperLayoutErrorSpecialMarginSetting, new byte[] { 0x1c, 0x28, 0x4C, 0x02, 0x00, 0x50, 0x30 }),                                                                                       //  FS  ( L 1C 28 4C 02 00 50 30-39 [30-39]
            new EscPosCmd(EscPosCmdType.FsPaperLayoutErrorSpecialMarginSetting, new byte[] { 0x1c, 0x28, 0x4C, 0x03, 0x00, 0x50, 0x35, 0x30 }),
            new EscPosCmd(EscPosCmdType.FsEnableDisableAutomaticStatusBackOptional, new byte[] { 0x1c, 0x28, 0x65, 0x02, 0x00, 0x33, 0x00 }),                                                                                       //  FS  ( e 1C 28 65 02 00 33 b0000x000
            new EscPosCmd(EscPosCmdType.FsEnableDisableAutomaticStatusBackOptional, new byte[] { 0x1c, 0x28, 0x65, 0x02, 0x00, 0x33, 0x08 }),
            new EscPosCmd(EscPosCmdType.FsSelectMICRDataHandling, new byte[] { 0x1c, 0x28, 0x66, 0x08, 0x00, 0x30, 0x00, 0x31, 0x00, 0x32, 0x00, 0x33, 0x00 }),                                                   //c FS  ( f 1C 28 66 0002-FFFF [00-03/30-33 00-FF|00/01/30/31]...
            new EscPosCmd(EscPosCmdType.FsSelectImageScannerCommandSettings, new byte[] { 0x1c, 0x28, 0x67, 0x02, 0x00, 0x20, 0x30 }),                                                                                       //c FS  ( g 1C 28 67 02 00 20 30/31
            new EscPosCmd(EscPosCmdType.FsSelectImageScannerCommandSettings, new byte[] { 0x1c, 0x28, 0x67, 0x02, 0x00, 0x20, 0x31 }),
            new EscPosCmd(EscPosCmdType.FsSetBasicOperationOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x28, 0x30, 0x01, 0x31, 0x80 }),                                                                     //c FS  ( g 1C 28 67 05 00 28 30 01/08 31/32 80-7F
            new EscPosCmd(EscPosCmdType.FsSetBasicOperationOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x28, 0x30, 0x01, 0x31, 0xff }),
            new EscPosCmd(EscPosCmdType.FsSetBasicOperationOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x28, 0x30, 0x01, 0x31, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsSetBasicOperationOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x28, 0x30, 0x01, 0x31, 0x7f }),
            new EscPosCmd(EscPosCmdType.FsSetBasicOperationOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x28, 0x30, 0x01, 0x32, 0x80 }),
            new EscPosCmd(EscPosCmdType.FsSetBasicOperationOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x28, 0x30, 0x01, 0x32, 0xff }),
            new EscPosCmd(EscPosCmdType.FsSetBasicOperationOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x28, 0x30, 0x01, 0x32, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsSetBasicOperationOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x28, 0x30, 0x01, 0x32, 0x7f }),
            new EscPosCmd(EscPosCmdType.FsSetBasicOperationOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x28, 0x30, 0x08, 0x31, 0x80 }),
            new EscPosCmd(EscPosCmdType.FsSetBasicOperationOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x28, 0x30, 0x08, 0x31, 0xff }),
            new EscPosCmd(EscPosCmdType.FsSetBasicOperationOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x28, 0x30, 0x08, 0x31, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsSetBasicOperationOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x28, 0x30, 0x08, 0x31, 0x7f }),
            new EscPosCmd(EscPosCmdType.FsSetBasicOperationOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x28, 0x30, 0x08, 0x32, 0x80 }),
            new EscPosCmd(EscPosCmdType.FsSetBasicOperationOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x28, 0x30, 0x08, 0x32, 0x80 }),
            new EscPosCmd(EscPosCmdType.FsSetBasicOperationOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x28, 0x30, 0x08, 0x32, 0x80 }),
            new EscPosCmd(EscPosCmdType.FsSetBasicOperationOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x28, 0x30, 0x08, 0x32, 0x80 }),
            new EscPosCmd(EscPosCmdType.FsSetScanningArea, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x29, 0x00, 0x00, 0x00, 0x00 }),                                                                     //c FS  ( g 1C 28 67 05 00 29 00-62 00-E4 00/02-64 00/02-E6
            new EscPosCmd(EscPosCmdType.FsSetScanningArea, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x29, 0x62, 0x00, 0x00, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsSetScanningArea, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x29, 0x00, 0xe4, 0x00, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsSetScanningArea, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x29, 0x00, 0x00, 0x00, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsSetScanningArea, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x29, 0x62, 0xe4, 0x00, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsSetScanningArea, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x29, 0x00, 0x00, 0x64, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsSetScanningArea, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x29, 0x00, 0x00, 0x00, 0xe6 }),
            new EscPosCmd(EscPosCmdType.FsSetScanningArea, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x29, 0x00, 0x00, 0x64, 0xe6 }),
            new EscPosCmd(EscPosCmdType.FsSetScanningArea, new byte[] { 0x1C, 0x28, 0x67, 0x05, 0x00, 0x29, 0x62, 0xe4, 0x64, 0xe6 }),
            new EscPosCmd(EscPosCmdType.FsSelectCompressionMethodForImageData, new byte[] { 0x1C, 0x28, 0x67, 0x03, 0x00, 0x32, 0x30, 0x30 }),                                                                                 //c FS  ( g 1C 28 67 03 00 32 30-32 30-32
            new EscPosCmd(EscPosCmdType.FsSelectCompressionMethodForImageData, new byte[] { 0x1C, 0x28, 0x67, 0x03, 0x00, 0x32, 0x30, 0x31 }),
            new EscPosCmd(EscPosCmdType.FsSelectCompressionMethodForImageData, new byte[] { 0x1C, 0x28, 0x67, 0x03, 0x00, 0x32, 0x30, 0x32 }),
            new EscPosCmd(EscPosCmdType.FsSelectCompressionMethodForImageData, new byte[] { 0x1C, 0x28, 0x67, 0x03, 0x00, 0x32, 0x31, 0x30 }),
            new EscPosCmd(EscPosCmdType.FsSelectCompressionMethodForImageData, new byte[] { 0x1C, 0x28, 0x67, 0x03, 0x00, 0x32, 0x32, 0x30 }),
            new EscPosCmd(EscPosCmdType.FsSelectCompressionMethodForImageData, new byte[] { 0x1C, 0x28, 0x67, 0x03, 0x00, 0x32, 0x32, 0x31 }),
            new EscPosCmd(EscPosCmdType.FsSelectCompressionMethodForImageData, new byte[] { 0x1C, 0x28, 0x67, 0x03, 0x00, 0x32, 0x32, 0x32 }),
            new EscPosCmd(EscPosCmdType.FsDeleteCroppingArea, new byte[] { 0x1C, 0x28, 0x67, 0x02, 0x00, 0x38, 0x00 }),                                                                                       //c FS  ( g 1C 28 67 02 00 38 00-0A
            new EscPosCmd(EscPosCmdType.FsSetCroppingArea, new byte[] { 0x1C, 0x28, 0x67, 0x06, 0x00, 0x39, 0x00, 0x00, 0x00, 0x02, 0x02 }),                                                               //c FS  ( g 1C 28 67 06 00 39 00-0A 00-64 00-E4 02-64 02-E6
            new EscPosCmd(EscPosCmdType.FsSetCroppingArea, new byte[] { 0x1C, 0x28, 0x67, 0x06, 0x00, 0x39, 0x01, 0x00, 0x00, 0x02, 0x02 }),
            new EscPosCmd(EscPosCmdType.FsSetCroppingArea, new byte[] { 0x1C, 0x28, 0x67, 0x06, 0x00, 0x39, 0x02, 0x00, 0x00, 0x02, 0x02 }),
            new EscPosCmd(EscPosCmdType.FsSetCroppingArea, new byte[] { 0x1C, 0x28, 0x67, 0x06, 0x00, 0x39, 0x09, 0x00, 0x00, 0x02, 0x02 }),
            new EscPosCmd(EscPosCmdType.FsSetCroppingArea, new byte[] { 0x1C, 0x28, 0x67, 0x06, 0x00, 0x39, 0x0a, 0x00, 0x00, 0x02, 0x02 }),
            new EscPosCmd(EscPosCmdType.FsSetCroppingArea, new byte[] { 0x1C, 0x28, 0x67, 0x06, 0x00, 0x39, 0x00, 0x62, 0x00, 0x64, 0x02 }),
            new EscPosCmd(EscPosCmdType.FsSetCroppingArea, new byte[] { 0x1C, 0x28, 0x67, 0x06, 0x00, 0x39, 0x00, 0x00, 0xe4, 0x02, 0xe6 }),
            new EscPosCmd(EscPosCmdType.FsSetCroppingArea, new byte[] { 0x1C, 0x28, 0x67, 0x06, 0x00, 0x39, 0x00, 0x00, 0x00, 0x64, 0x02 }),
            new EscPosCmd(EscPosCmdType.FsSetCroppingArea, new byte[] { 0x1C, 0x28, 0x67, 0x06, 0x00, 0x39, 0x00, 0x00, 0x00, 0x02, 0xe6 }),
            new EscPosCmd(EscPosCmdType.FsSetCroppingArea, new byte[] { 0x1C, 0x28, 0x67, 0x06, 0x00, 0x39, 0x00, 0x00, 0x00, 0x64, 0xe6 }),
            new EscPosCmd(EscPosCmdType.FsSetCroppingArea, new byte[] { 0x1C, 0x28, 0x67, 0x06, 0x00, 0x39, 0x00, 0x62, 0xe4, 0x64, 0xe6 }),
            new EscPosCmd(EscPosCmdType.FsSelectTransmissionFormatForImageScanningResult, new byte[] { 0x1C, 0x28, 0x67, 0x02, 0x00, 0x3C, 0x30 }),                                                                                       //c FS  ( g 1C 28 67 02 00 3C 30-32
            new EscPosCmd(EscPosCmdType.FsSelectTransmissionFormatForImageScanningResult, new byte[] { 0x1C, 0x28, 0x67, 0x02, 0x00, 0x3C, 0x31 }),
            new EscPosCmd(EscPosCmdType.FsSelectTransmissionFormatForImageScanningResult, new byte[] { 0x1C, 0x28, 0x67, 0x02, 0x00, 0x3C, 0x32 }),
            new EscPosCmd(EscPosCmdType.FsTransmitSettingValueForBasicOperationsOfImageScanner, new byte[] { 0x1C, 0x28, 0x67, 0x02, 0x00, 0x50, 0x30 }),                                                                                       //c FS  ( g 1C 28 67 02 00 50 30
            new EscPosCmd(EscPosCmdType.FsTransmitSettingValueOfScanningArea, new byte[] { 0x1C, 0x28, 0x67, 0x02, 0x00, 0x51, 0x30 }),                                                                                       //c FS  ( g 1C 28 67 02 00 51 30
            new EscPosCmd(EscPosCmdType.FsTransmitSettingValueOfCompressionMethdForImageData, new byte[] { 0x1C, 0x28, 0x67, 0x02, 0x00, 0x5A, 0x30 }),                                                                                       //c FS  ( g 1C 28 67 02 00 5A 30
            new EscPosCmd(EscPosCmdType.FsTransmitSettingValueOfCroppingArea, new byte[] { 0x1C, 0x28, 0x67, 0x02, 0x00, 0x61, 0x30 }),                                                                                       //c FS  ( g 1C 28 67 02 00 61 30
            new EscPosCmd(EscPosCmdType.FsTransmitSettingValueOfTransmissionFormatForImageScanningResult, new byte[] { 0x1C, 0x28, 0x67, 0x02, 0x00, 0x64, 0x30 }),                                                                                       //c FS  ( g 1C 28 67 02 00 64 30
            new EscPosCmd(EscPosCmdType.FsTurnKanjiUnderlineMode, new byte[] { 0x1c, 0x2D, 0x00 }),                                                                                                               //  FS  -   1C 2D 00-02/30-32
            new EscPosCmd(EscPosCmdType.FsTurnKanjiUnderlineMode, new byte[] { 0x1c, 0x2D, 0x01 }),
            new EscPosCmd(EscPosCmdType.FsTurnKanjiUnderlineMode, new byte[] { 0x1c, 0x2D, 0x02 }),
            new EscPosCmd(EscPosCmdType.FsTurnKanjiUnderlineMode, new byte[] { 0x1c, 0x2D, 0x30 }),
            new EscPosCmd(EscPosCmdType.FsTurnKanjiUnderlineMode, new byte[] { 0x1c, 0x2D, 0x31 }),
            new EscPosCmd(EscPosCmdType.FsTurnKanjiUnderlineMode, new byte[] { 0x1c, 0x2D, 0x32 }),
            new EscPosCmd(EscPosCmdType.FsCancelKanjiCharacterMode, new byte[] { 0x1c, 0x2E }),                                                                                                                     //  FS  .   1C 2E
                                                                                                                                                                                                                    //  FS  2   1C 32 7721-777E/EC40-EC9E/FEA1-FEFE 00-FF...
            new EscPosCmd(EscPosCmdType.FsDefineUserDefinedKanjiCharacters2424, new byte[] { 0x1c, 0x32, 0xec, 0x40, 0x01, 0x03, 0x07, 0x09, 0x0b, 0x0f, 0x11, 0x13, 0x17, 0x19, 0x1b, 0x1f, 0x21, 0x23, 0x27, 0x29, 0x2b, 0x2f, 0x31, 0x33, 0x37, 0x39, 0x3b, 0x3f, 0x41, 0x43, 0x47, 0x49, 0x4b, 0x4f, 0x51, 0x53, 0x57, 0x59, 0x5b, 0x5f, 0x61, 0x63, 0x67, 0x69, 0x6b, 0x6f, 0x71, 0x73, 0x77, 0x79, 0x7b, 0x7f, 0x81, 0x83, 0x87, 0x89, 0x8b, 0x8f, 0x91, 0x93, 0x97, 0x99, 0x9b, 0x9f, 0xa1, 0xa3, 0xa7, 0xa9, 0xab, 0xaf, 0xb1, 0xb3, 0xb7, 0xb9, 0xbb, 0xbf }),
            new EscPosCmd(EscPosCmdType.FsCancelUserDefinedKanjiCharacters, new byte[] { 0x1c, 0x3F, 0xec, 0x40 }),                                                                                                         //  FS  ?   1C 3F 7721-777E/EC40-EC9E/FEA1-FEFE
            new EscPosCmd(EscPosCmdType.FsSelectKanjiCharacterCodeSystem, new byte[] { 0x1c, 0x43, 0x00 }),                                                                                                               //  FS  C   1C 43 00-02/30-32
            new EscPosCmd(EscPosCmdType.FsSelectKanjiCharacterCodeSystem, new byte[] { 0x1c, 0x43, 0x01 }),
            new EscPosCmd(EscPosCmdType.FsSelectKanjiCharacterCodeSystem, new byte[] { 0x1c, 0x43, 0x02 }),
            new EscPosCmd(EscPosCmdType.FsSelectKanjiCharacterCodeSystem, new byte[] { 0x1c, 0x43, 0x30 }),
            new EscPosCmd(EscPosCmdType.FsSelectKanjiCharacterCodeSystem, new byte[] { 0x1c, 0x43, 0x31 }),
            new EscPosCmd(EscPosCmdType.FsSelectKanjiCharacterCodeSystem, new byte[] { 0x1c, 0x43, 0x32 }),
            new EscPosCmd(EscPosCmdType.FsSelectDoubleDensityPageMode, new byte[] { 0x1c, 0x4C }),                                                                                                                     //c FS  L   1C 4C
            new EscPosCmd(EscPosCmdType.FsSetKanjiCharacerSpacing, new byte[] { 0x1c, 0x53, 0x00, 0x00 }),                                                                                                         //  FS  S   1C 53 00-FF/00-20 00-FF/00-20
            new EscPosCmd(EscPosCmdType.FsSetKanjiCharacerSpacing, new byte[] { 0x1c, 0x53, 0x20, 0x20 }),
            new EscPosCmd(EscPosCmdType.FsSetKanjiCharacerSpacing, new byte[] { 0x1c, 0x53, 0xff, 0xff }),
            new EscPosCmd(EscPosCmdType.FsSetKanjiCharacerSpacing, new byte[] { 0x1c, 0x53, 0x20, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsSetKanjiCharacerSpacing, new byte[] { 0x1c, 0x53, 0x00, 0x20 }),
            new EscPosCmd(EscPosCmdType.FsSetKanjiCharacerSpacing, new byte[] { 0x1c, 0x53, 0xff, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsSetKanjiCharacerSpacing, new byte[] { 0x1c, 0x53, 0x00, 0xff }),
            new EscPosCmd(EscPosCmdType.FsTurnQuadrupleSizeMode, new byte[] { 0x1c, 0x57, 0x00 }),                                                                                                               //  FS  W   1C 57 bnnnnnnnx
            new EscPosCmd(EscPosCmdType.FsTurnQuadrupleSizeMode, new byte[] { 0x1c, 0x57, 0x01 }),
            new EscPosCmd(EscPosCmdType.FsObsoleteReadCheckPaper, new byte[] { 0x1c, 0x61, 0x30, 0x00 }),                                                                                                         //c FS  a 0 1C 61 30 b000000xx
            new EscPosCmd(EscPosCmdType.FsObsoleteReadCheckPaper, new byte[] { 0x1c, 0x61, 0x30, 0x01 }),
            new EscPosCmd(EscPosCmdType.FsObsoleteReadCheckPaper, new byte[] { 0x1c, 0x61, 0x30, 0x02 }),
            new EscPosCmd(EscPosCmdType.FsObsoleteReadCheckPaper, new byte[] { 0x1c, 0x61, 0x30, 0x03 }),
            new EscPosCmd(EscPosCmdType.FsObsoleteLoadCheckPaperToPrintStartingPosition, new byte[] { 0x1c, 0x61, 0x31 }),                                                                                                               //c FS  a 1 1C 61 31
            new EscPosCmd(EscPosCmdType.FsObsoleteEjectCheckPaper, new byte[] { 0x1c, 0x61, 0x32 }),                                                                                                               //c FS  a 2 1C 61 32
            new EscPosCmd(EscPosCmdType.FsObsoleteRequestTransmissionOfCheckPaperReadingResult, new byte[] { 0x1c, 0x62 }),                                                                                                                     //c FS  b  1C 62
            new EscPosCmd(EscPosCmdType.FsCleanMICRMechanism, new byte[] { 0x1c, 0x63 }),                                                                                                                     //c FS  c   1C 63
            new EscPosCmd(EscPosCmdType.FsObsoleteWriteNVUserMemory, new byte[] { 0x1c, 0x67, 0x31, 0x00, 0x80, 0x00, 0x00, 0x00, 0x08, 0x00, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17 }),                     //  FS  g 1 1C 67 31 00 00000000-000003FF 0001-0400 20-FF...
            new EscPosCmd(EscPosCmdType.FsObsoleteReadNVUserMemory, new byte[] { 0x1c, 0x67, 0x32, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04 }),                                                                     //  FS  g 2 1C 67 32 00 00000000-000003FF 0001-0400
            new EscPosCmd(EscPosCmdType.FsObsoleteReadNVUserMemory, new byte[] { 0x1c, 0x67, 0x32, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsObsoleteReadNVUserMemory, new byte[] { 0x1c, 0x67, 0x32, 0x00, 0x80, 0x02, 0x00, 0x00, 0x80, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsObsoleteReadNVUserMemory, new byte[] { 0x1c, 0x67, 0x32, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01 }),
            new EscPosCmd(EscPosCmdType.FsObsoleteReadNVUserMemory, new byte[] { 0x1c, 0x67, 0x32, 0x00, 0xff, 0x03, 0x00, 0x00, 0x01, 0x00 }),
            new EscPosCmd(EscPosCmdType.FsObsoletePrintNVBitimage, new byte[] { 0x1c, 0x70, 0x01, 0x00 }),                                                                                                         //  FS  p   1C 70 01-FF 00-03/30-33
            new EscPosCmd(EscPosCmdType.FsObsoletePrintNVBitimage, new byte[] { 0x1c, 0x70, 0x01, 0x01 }),
            new EscPosCmd(EscPosCmdType.FsObsoletePrintNVBitimage, new byte[] { 0x1c, 0x70, 0x01, 0x02 }),
            new EscPosCmd(EscPosCmdType.FsObsoletePrintNVBitimage, new byte[] { 0x1c, 0x70, 0x01, 0x03 }),
            new EscPosCmd(EscPosCmdType.FsObsoletePrintNVBitimage, new byte[] { 0x1c, 0x70, 0x01, 0x30 }),
            new EscPosCmd(EscPosCmdType.FsObsoletePrintNVBitimage, new byte[] { 0x1c, 0x70, 0x01, 0x31 }),
            new EscPosCmd(EscPosCmdType.FsObsoletePrintNVBitimage, new byte[] { 0x1c, 0x70, 0x01, 0x32 }),
            new EscPosCmd(EscPosCmdType.FsObsoletePrintNVBitimage, new byte[] { 0x1c, 0x70, 0x01, 0x33 }),
            new EscPosCmd(EscPosCmdType.FsObsoleteDefineNVBitimage, new byte[] { 0x1c, 0x71, 0x01, 0x01, 0x00, 0x01, 0x00, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55 })                                        //  FS  q   1C 71 01-FF [0001-03FF 0001-0240 00-FF...]...
        };

        [TestMethod]
        public void TestMethodPrtFss()
        {
            byte[] linqprtfss = prtfssResult.SelectMany(v => v.cmddata).ToArray<byte>();
            var EPTkn = new EscPosTokenizer();
            List<EscPosCmd> escposlist = EPTkn.Scan(linqprtfss, EscPosTokenizer.EscPosPrinter);
            escposlist = EscPosDecoder.Convert(escposlist);
            //FileOutList(escposlist, $"{TestContext.TestResultsDirectory}\\fss.txt");
            FileOutList(escposlist, ".\\fss.txt");

            Assert.AreEqual(prtfssResult.Count, escposlist.Count);
            for (int i = 0; i < prtfssResult.Count; i++)
            {
                Assert.AreEqual(prtfssResult[i].cmdtype, escposlist[i].cmdtype);
                Assert.AreEqual(prtfssResult[i].cmdlength, escposlist[i].cmdlength);
                for (int j = 0; j < prtfssResult[i].cmdlength; j++)
                {
                    Assert.AreEqual(prtfssResult[i].cmddata[j], escposlist[i].cmddata[j]);
                }
            }
        }

        public static readonly List<EscPosCmd> prtgssResult = new()
        {
            new EscPosCmd(EscPosCmdType.GsSelectCharacterSize, new byte[] { 0x1d, 0x21, 0x00 }),                                                                                                                                           //  GS  !   1D 21 b0xxx0yyy
            new EscPosCmd(EscPosCmdType.GsSelectCharacterSize, new byte[] { 0x1d, 0x21, 0x11 }),
            new EscPosCmd(EscPosCmdType.GsSelectCharacterSize, new byte[] { 0x1d, 0x21, 0x44 }),
            new EscPosCmd(EscPosCmdType.GsSelectCharacterSize, new byte[] { 0x1d, 0x21, 0x77 }),
            new EscPosCmd(EscPosCmdType.GsSelectCharacterSize, new byte[] { 0x1d, 0x21, 0x07 }),
            new EscPosCmd(EscPosCmdType.GsSelectCharacterSize, new byte[] { 0x1d, 0x21, 0x70 }),
            new EscPosCmd(EscPosCmdType.GsSelectCharacterSize, new byte[] { 0x1d, 0x21, 0x26 }),
            new EscPosCmd(EscPosCmdType.GsSelectCharacterSize, new byte[] { 0x1d, 0x21, 0x53 }),
            new EscPosCmd(EscPosCmdType.GsSetAbsoluteVerticalPrintPositionInPageMode, new byte[] { 0x1d, 0x24, 0x00, 0x00 }),                                                                                                                                     //c GS  $   1D 24 0000-FFFF
            new EscPosCmd(EscPosCmdType.GsSetAbsoluteVerticalPrintPositionInPageMode, new byte[] { 0x1d, 0x24, 0xff, 0xff }),
            new EscPosCmd(EscPosCmdType.GsExecuteTestPrint, new byte[] { 0x1d, 0x28, 0x41, 0x02, 0x00, 0x00, 0x31 }),                                                                                                                   //  GS  ( A 1D 28 41 02 00 00-05/30-35 01-03/31-33/40
            new EscPosCmd(EscPosCmdType.GsExecuteTestPrint, new byte[] { 0x1d, 0x28, 0x41, 0x02, 0x00, 0x01, 0x32 }),
            new EscPosCmd(EscPosCmdType.GsExecuteTestPrint, new byte[] { 0x1d, 0x28, 0x41, 0x02, 0x00, 0x02, 0x33 }),
            new EscPosCmd(EscPosCmdType.GsExecuteTestPrint, new byte[] { 0x1d, 0x28, 0x41, 0x02, 0x00, 0x03, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsExecuteTestPrint, new byte[] { 0x1d, 0x28, 0x41, 0x02, 0x00, 0x04, 0x32 }),
            new EscPosCmd(EscPosCmdType.GsExecuteTestPrint, new byte[] { 0x1d, 0x28, 0x41, 0x02, 0x00, 0x05, 0x33 }),
            new EscPosCmd(EscPosCmdType.GsExecuteTestPrint, new byte[] { 0x1d, 0x28, 0x41, 0x02, 0x00, 0x30, 0x40 }),
            new EscPosCmd(EscPosCmdType.GsExecuteTestPrint, new byte[] { 0x1d, 0x28, 0x41, 0x02, 0x00, 0x31, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsExecuteTestPrint, new byte[] { 0x1d, 0x28, 0x41, 0x02, 0x00, 0x32, 0x03 }),
            new EscPosCmd(EscPosCmdType.GsExecuteTestPrint, new byte[] { 0x1d, 0x28, 0x41, 0x02, 0x00, 0x33, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsExecuteTestPrint, new byte[] { 0x1d, 0x28, 0x41, 0x02, 0x00, 0x34, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsExecuteTestPrint, new byte[] { 0x1d, 0x28, 0x41, 0x02, 0x00, 0x35, 0x03 }),
            new EscPosCmd(EscPosCmdType.GsCustomizeASBStatusBits, new byte[] { 0x1d, 0x28, 0x42, 0x09, 0x00, 0x61, 0x31, 0x2c, 0x33, 0x2d, 0x45, 0x38, 0x46, 0x37 }),                                                                         //c GS  ( B 1D 28 42 0002/0003/0005/0007/0009 61 00|[31/33/45/46 2C/2D/37/38]...
            new EscPosCmd(EscPosCmdType.GsDeleteSpecifiedRecord, new byte[] { 0x1d, 0x28, 0x43, 0x05, 0x00, 0x00, 0x00, 0x00, 0x20, 0x20 }),                                                                                                 //  GS  ( C 1D 28 43 05 00 00 00/30 00 20-7E 20-7E
            new EscPosCmd(EscPosCmdType.GsDeleteSpecifiedRecord, new byte[] { 0x1d, 0x28, 0x43, 0x05, 0x00, 0x00, 0x30, 0x00, 0x7e, 0x7e }),
            new EscPosCmd(EscPosCmdType.GsStoreDataSpecifiedRecord, new byte[] { 0x1d, 0x28, 0x43, 0x06, 0x00, 0x00, 0x31, 0x00, 0x20, 0x20, 0x30 }),                                                                                           //  GS  ( C 1D 28 43 0006-FFFF 00 01/31 00 20-7E 20-7E 20-FF...
            new EscPosCmd(EscPosCmdType.GsTransmitDataSpecifiedRecord, new byte[] { 0x1d, 0x28, 0x43, 0x05, 0x00, 0x00, 0x02, 0x00, 0x20, 0x20 }),                                                                                                 //  GS  ( C 1D 28 43 05 00 00 02/32 00 20-7E 20-7E
            new EscPosCmd(EscPosCmdType.GsTransmitDataSpecifiedRecord, new byte[] { 0x1d, 0x28, 0x43, 0x05, 0x00, 0x00, 0x32, 0x00, 0x20, 0x20 }),                                                                                                 //  GS  ( C 1D 28 43 05 00 00 02/32 00 20-7E 20-7E
            new EscPosCmd(EscPosCmdType.GsTransmitCapacityNVUserMemory, new byte[] { 0x1d, 0x28, 0x43, 0x03, 0x00, 0x00, 0x03, 0x00 }),                                                                                                             //  GS  ( C 1D 28 43 03 00 00 03/33 00
            new EscPosCmd(EscPosCmdType.GsTransmitCapacityNVUserMemory, new byte[] { 0x1d, 0x28, 0x43, 0x03, 0x00, 0x00, 0x33, 0x00 }),                                                                                                             //  GS  ( C 1D 28 43 03 00 00 03/33 00
            new EscPosCmd(EscPosCmdType.GsTransmitRemainingCapacityNVUserMemory, new byte[] { 0x1d, 0x28, 0x43, 0x03, 0x00, 0x00, 0x04, 0x00 }),                                                                                                             //  GS  ( C 1D 28 43 03 00 00 04/34 00
            new EscPosCmd(EscPosCmdType.GsTransmitRemainingCapacityNVUserMemory, new byte[] { 0x1d, 0x28, 0x43, 0x03, 0x00, 0x00, 0x34, 0x00 }),                                                                                                             //  GS  ( C 1D 28 43 03 00 00 04/34 00
            new EscPosCmd(EscPosCmdType.GsTransmitKeycodeList, new byte[] { 0x1d, 0x28, 0x43, 0x03, 0x00, 0x00, 0x05, 0x00 }),                                                                                                             //  GS  ( C 1D 28 43 03 00 00 05/35 00
            new EscPosCmd(EscPosCmdType.GsTransmitKeycodeList, new byte[] { 0x1d, 0x28, 0x43, 0x03, 0x00, 0x00, 0x35, 0x00 }),                                                                                                             //  GS  ( C 1D 28 43 03 00 00 05/35 00
            new EscPosCmd(EscPosCmdType.GsDeleteAllDataNVMemory, new byte[] { 0x1d, 0x28, 0x43, 0x06, 0x00, 0x00, 0x06, 0x00, 0x43, 0x4c, 0x52 }),                                                                                           //  GS  ( C 1D 28 43 06 00 00 06/36 00 43 4C 52
            new EscPosCmd(EscPosCmdType.GsDeleteAllDataNVMemory, new byte[] { 0x1d, 0x28, 0x43, 0x06, 0x00, 0x00, 0x36, 0x00, 0x43, 0x4c, 0x52 }),                                                                                           //  GS  ( C 1D 28 43 06 00 00 06/36 00 43 4C 52
            new EscPosCmd(EscPosCmdType.GsEnableDisableRealtimeCommand, new byte[] { 0x1d, 0x28, 0x44, 0x05, 0x00, 0x14, 0x01, 0x31, 0x02, 0x31 }),                                                                                                 //  GS  ( D 1D 28 44 0003/0005 14 [01/02 00/01/30/31]...
            new EscPosCmd(EscPosCmdType.GsChangeIntoUserSettingMode, new byte[] { 0x1d, 0x28, 0x45, 0x03, 0x00, 0x01, 0x49, 0x4e }),                                                                                                             //  GS  ( E 1D 28 45 03 00 01 49 4E
            new EscPosCmd(EscPosCmdType.GsEndUserSettingMode, new byte[] { 0x1d, 0x28, 0x45, 0x04, 0x00, 0x02, 0x4f, 0x55, 0x54 }),                                                                                                       //  GS  ( E 1D 28 45 04 00 02 4F 55 54
            new EscPosCmd(EscPosCmdType.GsChangeMeomorySwitch, new byte[] { 0x1d, 0x28, 0x45, 0x0a, 0x00, 0x03, 0x01, 0x32, 0x32, 0x32, 0x32, 0x32, 0x32, 0x32, 0x31 }),                                                                   //  GS  ( E 1D 28 45 000A-FFFA 03 [01-08 30-32...]...
            new EscPosCmd(EscPosCmdType.GsTransmitSettingsMemorySwitch, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x04, 0x01 }),                                                                                                                   //  GS  ( E 1D 28 45 02 00 04 01-08
            new EscPosCmd(EscPosCmdType.GsSetCustomizeSettingValues, new byte[] { 0x1d, 0x28, 0x45, 0x04, 0x00, 0x05, 0x03, 0x06, 0x00 }),                                                                                                       //  GS  ( E 1D 28 45 0004-FFFD 05 [01-03/05-0D/14-16/46-48/61/62/64-69/6F/70/74-C2 0000-FFFF]...
            new EscPosCmd(EscPosCmdType.GsTransmitCustomizeSettingValues, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x06, 0x02 }),                                                                                                                   //  GS  ( E 1D 28 45 02 00 06 01-03/05-0D/14-16/46-48/61/62/64-69/6F-71/74-C1
            new EscPosCmd(EscPosCmdType.GsCopyUserDefinedPage, new byte[] { 0x1d, 0x28, 0x45, 0x04, 0x00, 0x07, 0x0c, 0x1f, 0x1e }),                                                                                                       //  GS  ( E 1D 28 45 04 00 07 0A/0C11/12 30/31 31/30
                                                                                                                                                                                                                                           //  GS  ( E 1D 28 45 0005-FFFF 08 02/03 20-7E 20-7E [08/09/0A/0C 00-FF...]...
            new EscPosCmd(EscPosCmdType.GsDefineColumnFormatCharacterCodePage, new byte[] { 0x1d, 0x28, 0x45, 0x29, 0x00, 0x08, 0x03, 0x20, 0x20, 0x0c, 0x01, 0x02, 0x03, 0x11, 0x12, 0x13, 0x21, 0x22, 0x23, 0x31, 0x32, 0x33, 0x41, 0x42, 0x43, 0x51, 0x52, 0x53, 0x61, 0x62, 0x63, 0x71, 0x72, 0x73, 0x81, 0x82, 0x83, 0x91, 0x92, 0x93, 0xa1, 0xa2, 0xa3, 0xb1, 0xb2, 0xb3 }),
            //  GS  ( E 1D 28 45 0005-FFFF 09 01/02 20-7E 20-7E [10/11/18 00-FF...]...
            new EscPosCmd(EscPosCmdType.GsDefineRasterFormatCharacterCodePage, new byte[] { 0x1d, 0x28, 0x45, 0x35, 0x00, 0x09, 0x02, 0x20, 0x20, 0x18, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f }),
            new EscPosCmd(EscPosCmdType.GsDeleteCharacterCodePage, new byte[] { 0x1d, 0x28, 0x45, 0x03, 0x00, 0x0a, 0x20, 0x20 }),                                                                                                             //  GS  ( E 1D 28 45 03 00 0A 80-FF 80-FF
            new EscPosCmd(EscPosCmdType.GsSetSerialInterface, new byte[] { 0x1d, 0x28, 0x45, 0x03, 0x00, 0x0b, 0x03, 0x30 }),                                                                                                             //  GS  ( E 1D 28 45 0003-0008 0B 01-04 30-39...
            new EscPosCmd(EscPosCmdType.GsTransmitSerialInterface, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x0c, 0x01 }),                                                                                                                   //  GS  ( E 1D 28 45 02 00 0C 01-04
            new EscPosCmd(EscPosCmdType.GsTransmitSerialInterface, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x0c, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsTransmitSerialInterface, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x0c, 0x03 }),
            new EscPosCmd(EscPosCmdType.GsTransmitSerialInterface, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x0c, 0x04 }),
            new EscPosCmd(EscPosCmdType.GsSetBluetoothInterface, new byte[] { 0x1d, 0x28, 0x45, 0x06, 0x00, 0x0d, 0x31, 0x39, 0x39, 0x39, 0x39 }),                                                                                           //  GS  ( E 1D 28 45 0003-0021 0D [31/41/46/49 20-7E...]...
            new EscPosCmd(EscPosCmdType.GsTransmitBluetoothInterface, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x0e, 0x30 }),                                                                                                                   //  GS  ( E 1D 28 45 02 00 0E 30/31/41/46/49
            new EscPosCmd(EscPosCmdType.GsTransmitBluetoothInterface, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x0e, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsTransmitBluetoothInterface, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x0e, 0x41 }),
            new EscPosCmd(EscPosCmdType.GsTransmitBluetoothInterface, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x0e, 0x46 }),
            new EscPosCmd(EscPosCmdType.GsTransmitBluetoothInterface, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x0e, 0x49 }),
            new EscPosCmd(EscPosCmdType.GsSetUSBInterface, new byte[] { 0x1d, 0x28, 0x45, 0x03, 0x00, 0x0f, 0x01, 0x30 }),                                                                                                             //  GS  ( E 1D 28 45 03 00 0F 01/20 30/31
            new EscPosCmd(EscPosCmdType.GsSetUSBInterface, new byte[] { 0x1d, 0x28, 0x45, 0x03, 0x00, 0x0f, 0x01, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsSetUSBInterface, new byte[] { 0x1d, 0x28, 0x45, 0x03, 0x00, 0x0f, 0x20, 0x30 }),
            new EscPosCmd(EscPosCmdType.GsSetUSBInterface, new byte[] { 0x1d, 0x28, 0x45, 0x03, 0x00, 0x0f, 0x20, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsTransmitUSBInterface, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x10, 0x01 }),                                                                                                                   //  GS  ( E 1D 28 45 02 00 10 01/20
            new EscPosCmd(EscPosCmdType.GsTransmitUSBInterface, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x10, 0x20 }),
            new EscPosCmd(EscPosCmdType.GsDeletePaperLayout, new byte[] { 0x1d, 0x28, 0x45, 0x04, 0x00, 0x30, 0x43, 0x4c, 0x52 }),                                                                                                       //  GS  ( E 1D 28 45 04 00 30 43 4C 52
            new EscPosCmd(EscPosCmdType.GsSetPaperLayout, new byte[] { 0x1d, 0x28, 0x45, 0x12, 0x00, 0x31, 0x3b, 0x3b, 0x3b, 0x31, 0x32, 0x30, 0x3b, 0x31, 0x32, 0x30, 0x3b, 0x31, 0x32, 0x30, 0x3b, 0x3b, 0x3b }),                   //  GS  ( E 1D 28 45 0009-0024 31 {34 38/34 39/36 34} 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-3
            new EscPosCmd(EscPosCmdType.GsTransmitPaperLayout, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x32, 0x40 }),                                                                                                                   //  GS  ( E 1D 28 45 02 00 32 40/50
            new EscPosCmd(EscPosCmdType.GsTransmitPaperLayout, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x32, 0x50 }),
            //  GS  ( E 1D 28 45 002D-0048 33 20-7E...
            new EscPosCmd(EscPosCmdType.GsSetControlLabelPaperAndBlackMarks, new byte[] { 0x1d, 0x28, 0x45, 0x31, 0x00, 0x33, 0x30, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f }),
            new EscPosCmd(EscPosCmdType.GsTransmitControlSettingsLabelPaperAndBlackMarks, new byte[] { 0x1d, 0x28, 0x45, 0x11, 0x00, 0x34, 0x3f, 0x75, 0x40, 0x1f, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x00 }),                         //  GS  ( E 1D 28 45 11 00 34 20-7E... 00
            new EscPosCmd(EscPosCmdType.GsSetInternalBuzzerPatterns, new byte[] { 0x1d, 0x28, 0x45, 0x0e, 0x00, 0x63, 0x01, 0x01, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }),                                           //  GS  ( E 1D 28 45 000E-FFFF 63 [01-05 [00/01 00-64]...]...
            new EscPosCmd(EscPosCmdType.GsTransmitInternalBuzzerPatterns, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x64, 0x01 }),                                                                                                                   //  GS  ( E 1D 28 45 02 00 64 01-05
            new EscPosCmd(EscPosCmdType.GsTransmitInternalBuzzerPatterns, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x64, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsTransmitInternalBuzzerPatterns, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x64, 0x03 }),
            new EscPosCmd(EscPosCmdType.GsTransmitInternalBuzzerPatterns, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x64, 0x04 }),
            new EscPosCmd(EscPosCmdType.GsTransmitInternalBuzzerPatterns, new byte[] { 0x1d, 0x28, 0x45, 0x02, 0x00, 0x64, 0x05 }),
            new EscPosCmd(EscPosCmdType.GsTransmitStatusOfCutSheet, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x20, 0x30 }),                                                                                                                   //c GS  ( G 1D 28 47 02 00 20 b0100xxxx
            new EscPosCmd(EscPosCmdType.GsSelectSideOfSlipFaceOrBack, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x30, 0x04 }),                                                                                                                   //c GS  ( G 1D 28 47 02 00 30 04/44
            new EscPosCmd(EscPosCmdType.GsSelectSideOfSlipFaceOrBack, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x30, 0x44 }),
            new EscPosCmd(EscPosCmdType.GsReadMagneticInkCharacterAndTransmitReadingResult, new byte[] { 0x1d, 0x28, 0x47, 0x04, 0x00, 0x3c, 0x01, 0x00, 0x00 }),                                                                                                       //c GS  ( G 1D 28 47 04 00 3C 01 00 00/01
            new EscPosCmd(EscPosCmdType.GsReadMagneticInkCharacterAndTransmitReadingResult, new byte[] { 0x1d, 0x28, 0x47, 0x04, 0x00, 0x3c, 0x01, 0x00, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsRetransmitMagneticInkCharacterReadingResult, new byte[] { 0x1d, 0x28, 0x47, 0x03, 0x00, 0x3d, 0x01, 0x00 }),                                                                                                             //c GS  ( G 1D 28 47 03 00 3D 01 00
            new EscPosCmd(EscPosCmdType.GsReadDataAndTransmitResultingInformation, new byte[] { 0x1d, 0x28, 0x47, 0x09, 0x00, 0x40, 0x01, 0x00, 0x30, 0x03, 0x00, 0x30, 0x00, 0x00 }),                                                                         //c GS  ( G 1D 28 47 0005-0405 40 0000-FFFF 00/01 01-03 00 30 0000 00-FF...
            new EscPosCmd(EscPosCmdType.GsScanImageDataAndTransmitImageScanningResult, new byte[] { 0x1d, 0x28, 0x47, 0x05, 0x00, 0x41, 0x02, 0x00, 0x30, 0x30 }),                                                                                                 //c GS  ( G 1D 28 47 0005-0405 41 0001-FFFF 30/31 30 00-FF...
            new EscPosCmd(EscPosCmdType.GsRetransmitImageScanningResult, new byte[] { 0x1d, 0x28, 0x47, 0x04, 0x00, 0x42, 0x80, 0x00, 0x30 }),                                                                                                       //c GS  ( G 1D 28 47 0003-0004 42 0001-FFFF [30/31]
            new EscPosCmd(EscPosCmdType.GsExecutePreScan, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x43, 0x30 }),                                                                                                                   //c GS  ( G 1D 28 47 02 00 43 30
            new EscPosCmd(EscPosCmdType.GsDeleteImageScanningResultWithSpecifiedDataID, new byte[] { 0x1d, 0x28, 0x47, 0x04, 0x00, 0x44, 0x30, 0x01, 0x00 }),                                                                                                       //c GS  ( G 1D 28 47 04 00 44 30 0001-FFFF
            new EscPosCmd(EscPosCmdType.GsDeleteImageScanningResultWithSpecifiedDataID, new byte[] { 0x1d, 0x28, 0x47, 0x04, 0x00, 0x44, 0x30, 0xff, 0xff }),
            new EscPosCmd(EscPosCmdType.GsDeleteAllImageScanningResult, new byte[] { 0x1d, 0x28, 0x47, 0x05, 0x00, 0x45, 0x30, 0x43, 0x4c, 0x52 }),                                                                                                 //c GS  ( G 1D 28 47 05 00 45 30 43 4C 52
            new EscPosCmd(EscPosCmdType.GsTransmitDataIDListOfImageScanningResult, new byte[] { 0x1d, 0x28, 0x47, 0x04, 0x00, 0x46, 0x30, 0x49, 0x44 }),                                                                                                       //c GS  ( G 1D 28 47 04 00 46 30 49 44
            new EscPosCmd(EscPosCmdType.GsTransmitRemainingCapacityOfNVMemoryForImageDataStorage, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x47, 0x30 }),                                                                                                                   //c GS  ( G 1D 28 47 02 00 47 30
            new EscPosCmd(EscPosCmdType.GsSelectActiveSheet, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x50, 0x01 }),                                                                                                                   //c GS  ( G 1D 28 47 02 00 50 b00xxxxxx
            new EscPosCmd(EscPosCmdType.GsSelectActiveSheet, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x50, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsSelectActiveSheet, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x50, 0x03 }),
            new EscPosCmd(EscPosCmdType.GsSelectActiveSheet, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x50, 0x04 }),
            new EscPosCmd(EscPosCmdType.GsSelectActiveSheet, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x50, 0x08 }),
            new EscPosCmd(EscPosCmdType.GsSelectActiveSheet, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x50, 0x10 }),
            new EscPosCmd(EscPosCmdType.GsSelectActiveSheet, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x50, 0x20 }),
            new EscPosCmd(EscPosCmdType.GsStartPreProcessForCutSheetInsertion, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x51, 0x30 }),                                                                                                                   //c GS  ( G 1D 28 47 02 00 51 30
            new EscPosCmd(EscPosCmdType.GsEndPreProcessForCutSheetInsertion, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x52, 0x30 }),                                                                                                                   //c GS  ( G 1D 28 47 02 00 52 30
            new EscPosCmd(EscPosCmdType.GsExecuteWaitingProcessForCutSheetInsertion, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x53, 0x30 }),                                                                                                                   //c GS  ( G 1D 28 47 02 00 53 30
            new EscPosCmd(EscPosCmdType.GsFeedToPrintStartingPositionForSlip, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x54, 0x01 }),                                                                                                                   //c GS  ( G 1D 28 47 02 00 54 01
            new EscPosCmd(EscPosCmdType.GsFinishProcessingOfCutSheet, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x55, 0x30 }),                                                                                                                   //c GS  ( G 1D 28 47 02 00 55 30/31
            new EscPosCmd(EscPosCmdType.GsFinishProcessingOfCutSheet, new byte[] { 0x1d, 0x28, 0x47, 0x02, 0x00, 0x55, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsSpecifiesProcessIDResponse, new byte[] { 0x1d, 0x28, 0x48, 0x06, 0x00, 0x30, 0x30, 0x20, 0x20, 0x20, 0x20 }),                                                                                           //  GS  ( H 1D 28 48 06 00 30 30 20-7E 20-7E 20-7E 20-7E
            new EscPosCmd(EscPosCmdType.GsSpecifiesOfflineResponse, new byte[] { 0x1d, 0x28, 0x48, 0x03, 0x00, 0x31, 0x30, 0x32 }),                                                                                                             //  GS  ( H 1D 28 48 03 00 31 30 00-02/30-32
            new EscPosCmd(EscPosCmdType.GsSelectPrintControlMode, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x30, 0x00 }),                                                                                                                   //  GS  ( K 1D 28 4B 02 00 30 00-04/30-34
            new EscPosCmd(EscPosCmdType.GsSelectPrintControlMode, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x30, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintControlMode, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x30, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintControlMode, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x30, 0x03 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintControlMode, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x30, 0x04 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintControlMode, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x30, 0x30 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintControlMode, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x30, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintControlMode, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x30, 0x32 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintControlMode, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x30, 0x33 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintControlMode, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x30, 0x34 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintDensity, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x31, 0xfa }),                                                                                                                   //  GS  ( K 1D 28 4B 02 00 31 80-7F
            new EscPosCmd(EscPosCmdType.GsSelectPrintDensity, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x31, 0xfb }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintDensity, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x31, 0xfc }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintDensity, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x31, 0xfd }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintDensity, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x31, 0xfe }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintDensity, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x31, 0xff }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintDensity, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x31, 0x00 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintDensity, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x31, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintDensity, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x31, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintDensity, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x31, 0x03 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintDensity, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x31, 0x04 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintDensity, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x31, 0x05 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintDensity, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x31, 0x06 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintDensity, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x31, 0x07 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintDensity, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x31, 0x08 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x00 }),                                                                                                                   //  GS  ( K 1D 28 4B 02 00 32 00-0D/30-39
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x03 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x04 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x05 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x06 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x07 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x08 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x09 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x0a }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x0b }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x0c }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x0d }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x30 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x32 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x33 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x34 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x35 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x36 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x37 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x38 }),
            new EscPosCmd(EscPosCmdType.GsSelectPrintSpeed, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x32, 0x39 }),
            new EscPosCmd(EscPosCmdType.GsSelectNumberOfPartsThermalHeadEnergizing, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x61, 0x00 }),                                                                                                                   //  GS  ( K 1D 28 4B 02 00 61 00-04/30-34/80
            new EscPosCmd(EscPosCmdType.GsSelectNumberOfPartsThermalHeadEnergizing, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x61, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsSelectNumberOfPartsThermalHeadEnergizing, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x61, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsSelectNumberOfPartsThermalHeadEnergizing, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x61, 0x03 }),
            new EscPosCmd(EscPosCmdType.GsSelectNumberOfPartsThermalHeadEnergizing, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x61, 0x04 }),
            new EscPosCmd(EscPosCmdType.GsSelectNumberOfPartsThermalHeadEnergizing, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x61, 0x30 }),
            new EscPosCmd(EscPosCmdType.GsSelectNumberOfPartsThermalHeadEnergizing, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x61, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsSelectNumberOfPartsThermalHeadEnergizing, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x61, 0x32 }),
            new EscPosCmd(EscPosCmdType.GsSelectNumberOfPartsThermalHeadEnergizing, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x61, 0x33 }),
            new EscPosCmd(EscPosCmdType.GsSelectNumberOfPartsThermalHeadEnergizing, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x61, 0x34 }),
            new EscPosCmd(EscPosCmdType.GsSelectNumberOfPartsThermalHeadEnergizing, new byte[] { 0x1d, 0x28, 0x4b, 0x02, 0x00, 0x61, 0x80 }),
            new EscPosCmd(EscPosCmdType.GsDefineNVGraphicsDataRasterW, new byte[] { 0x1d, 0x28, 0x4c, 0x13, 0x00, 0x30, 0x43, 0x30, 0x20, 0x20, 0x01, 0x40, 0x00, 0x01, 0x00, 0x31, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55 }),             //  GS  ( L 1D 28 4C 000C-FFFF 30 43 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
            new EscPosCmd(EscPosCmdType.GsDefineNVGraphicsDataRasterDW, new byte[] { 0x1d, 0x38, 0x4c, 0x13, 0x00, 0x00, 0x00, 0x30, 0x43, 0x30, 0x20, 0x20, 0x01, 0x40, 0x00, 0x01, 0x00, 0x31, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55 }), //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 43 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
            new EscPosCmd(EscPosCmdType.GsDefineNVGraphicsDataColumnW, new byte[] { 0x1d, 0x28, 0x4c, 0x13, 0x00, 0x30, 0x44, 0x30, 0x20, 0x20, 0x01, 0x08, 0x00, 0x08, 0x00, 0x31, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55 }),             //  GS  ( L 1D 28 4C 000C-FFFF 30 44 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
            new EscPosCmd(EscPosCmdType.GsDefineNVGraphicsDataColumnDW, new byte[] { 0x1d, 0x38, 0x4c, 0x13, 0x00, 0x00, 0x00, 0x30, 0x44, 0x30, 0x20, 0x20, 0x01, 0x08, 0x00, 0x08, 0x00, 0x31, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55 }), //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 44 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
            new EscPosCmd(EscPosCmdType.GsDefineDownloadGraphicsDataRasterW, new byte[] { 0x1d, 0x28, 0x4c, 0x13, 0x00, 0x30, 0x53, 0x30, 0x20, 0x20, 0x01, 0x40, 0x00, 0x01, 0x00, 0x31, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55 }),             //  GS  ( L 1D 28 4C 000C-FFFF 30 53 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
            new EscPosCmd(EscPosCmdType.GsDefineDownloadGraphicsDataRasterDW, new byte[] { 0x1d, 0x38, 0x4c, 0x13, 0x00, 0x00, 0x00, 0x30, 0x53, 0x30, 0x20, 0x20, 0x01, 0x40, 0x00, 0x01, 0x00, 0x31, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55 }), //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 53 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
            new EscPosCmd(EscPosCmdType.GsDefineDownloadGraphicsDataColumnW, new byte[] { 0x1d, 0x28, 0x4c, 0x13, 0x00, 0x30, 0x54, 0x30, 0x20, 0x20, 0x01, 0x08, 0x00, 0x08, 0x00, 0x31, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55 }),             //  GS  ( L 1D 28 4C 000C-FFFF 30 54 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
            new EscPosCmd(EscPosCmdType.GsDefineDownloadGraphicsDataColumnDW, new byte[] { 0x1d, 0x38, 0x4c, 0x13, 0x00, 0x00, 0x00, 0x30, 0x54, 0x30, 0x20, 0x20, 0x01, 0x08, 0x00, 0x08, 0x00, 0x31, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55 }), //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 54 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
            new EscPosCmd(EscPosCmdType.GsStoreGraphicsDataToPrintBufferRasterW, new byte[] { 0x1d, 0x28, 0x4c, 0x13, 0x00, 0x30, 0x70, 0x30, 0x20, 0x20, 0x01, 0x40, 0x00, 0x01, 0x00, 0x31, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55 }),             //  GS  ( L 1D 28 4C 000B-FFFF 30 70 30/34 01/02 01/02 31-34 0001-0960 0001-0960 00-FF...
            new EscPosCmd(EscPosCmdType.GsStoreGraphicsDataToPrintBufferRasterDW, new byte[] { 0x1d, 0x38, 0x4c, 0x13, 0x00, 0x00, 0x00, 0x30, 0x70, 0x30, 0x20, 0x20, 0x01, 0x40, 0x00, 0x01, 0x00, 0x31, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55 }), //  GS  8 L 1D 38 4C 0000000B-FFFFFFFF 30 70 30/34 01/02 01/02 31-34 0001-0960 0001-0960 00-FF...
            new EscPosCmd(EscPosCmdType.GsStoreGraphicsDataToPrintBufferColumnW, new byte[] { 0x1d, 0x28, 0x4c, 0x13, 0x00, 0x30, 0x71, 0x30, 0x20, 0x20, 0x01, 0x08, 0x00, 0x08, 0x00, 0x31, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55 }),             //  GS  ( L 1D 28 4C 000B-FFFF 30 71 30 01/02 01/02 31-33 0001-0800 0001-0080 00-FF...
            new EscPosCmd(EscPosCmdType.GsStoreGraphicsDataToPrintBufferColumnDW, new byte[] { 0x1d, 0x38, 0x4c, 0x13, 0x00, 0x00, 0x00, 0x30, 0x71, 0x30, 0x20, 0x20, 0x01, 0x08, 0x00, 0x08, 0x00, 0x31, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55 }), //  GS  8 L 1D 38 4C 0000000B-FFFFFFFF 30 71 30 01/02 01/02 31-33 0001-0800 0001-0080 00-FF...
            new EscPosCmd(EscPosCmdType.GsTransmitNVGraphicsMemoryCapacity, new byte[] { 0x1d, 0x28, 0x4c, 0x02, 0x00, 0x30, 0x00 }),                                                                                                                   //  GS  ( L 1D 28 4C 02 00 30 00/30
            new EscPosCmd(EscPosCmdType.GsTransmitNVGraphicsMemoryCapacity, new byte[] { 0x1d, 0x28, 0x4c, 0x02, 0x00, 0x30, 0x30 }),
            new EscPosCmd(EscPosCmdType.GsSetReferenceDotDensityGraphics, new byte[] { 0x1d, 0x28, 0x4c, 0x04, 0x00, 0x30, 0x01, 0x32, 0x32 }),                                                                                                       //  GS  ( L 1D 28 4C 04 00 30 01/31 32/33 32/33
            new EscPosCmd(EscPosCmdType.GsSetReferenceDotDensityGraphics, new byte[] { 0x1d, 0x28, 0x4c, 0x04, 0x00, 0x30, 0x31, 0x32, 0x32 }),
            new EscPosCmd(EscPosCmdType.GsPrintGraphicsDataInPrintBuffer, new byte[] { 0x1d, 0x28, 0x4c, 0x02, 0x00, 0x30, 0x02 }),                                                                                                                   //  GS  ( L 1D 28 4C 02 00 30 02/32
            new EscPosCmd(EscPosCmdType.GsPrintGraphicsDataInPrintBuffer, new byte[] { 0x1d, 0x28, 0x4c, 0x02, 0x00, 0x30, 0x32 }),
            new EscPosCmd(EscPosCmdType.GsTransmitRemainingCapacityNVGraphicsMemory, new byte[] { 0x1d, 0x28, 0x4c, 0x02, 0x00, 0x30, 0x03 }),                                                                                                                   //  GS  ( L 1D 28 4C 02 00 30 03/33
            new EscPosCmd(EscPosCmdType.GsTransmitRemainingCapacityNVGraphicsMemory, new byte[] { 0x1d, 0x28, 0x4c, 0x02, 0x00, 0x30, 0x33 }),
            new EscPosCmd(EscPosCmdType.GsTransmitRemainingCapacityDownloadGraphicsMemory, new byte[] { 0x1d, 0x28, 0x4c, 0x02, 0x00, 0x30, 0x04 }),                                                                                                                   //  GS  ( L 1D 28 4C 02 00 30 04/34
            new EscPosCmd(EscPosCmdType.GsTransmitRemainingCapacityDownloadGraphicsMemory, new byte[] { 0x1d, 0x28, 0x4c, 0x02, 0x00, 0x30, 0x34 }),
            new EscPosCmd(EscPosCmdType.GsTransmitKeycodeListDefinedNVGraphics, new byte[] { 0x1d, 0x28, 0x4c, 0x04, 0x00, 0x30, 0x40, 0x4b, 0x43 }),                                                                                                       //  GS  ( L 1D 28 4C 04 00 30 40 4B 43
            new EscPosCmd(EscPosCmdType.GsDeleteAllNVGraphicsData, new byte[] { 0x1d, 0x28, 0x4c, 0x05, 0x00, 0x30, 0x41, 0x43, 0x4c, 0x52 }),                                                                                                 //  GS  ( L 1D 28 4C 05 00 30 41 43 4C 52
            new EscPosCmd(EscPosCmdType.GsDeleteSpecifiedNVGraphicsData, new byte[] { 0x1d, 0x28, 0x4c, 0x04, 0x00, 0x30, 0x42, 0x20, 0x20 }),                                                                                                       //  GS  ( L 1D 28 4C 04 00 30 42 20-7E 20-7E
            new EscPosCmd(EscPosCmdType.GsPrintSpecifiedNVGraphicsData, new byte[] { 0x1d, 0x28, 0x4c, 0x06, 0x00, 0x30, 0x45, 0x20, 0x20, 0x01, 0x01 }),                                                                                           //  GS  ( L 1D 28 4C 06 00 30 45 20-7E 20-7E 01/02 01/02
            new EscPosCmd(EscPosCmdType.GsPrintSpecifiedNVGraphicsData, new byte[] { 0x1d, 0x28, 0x4c, 0x06, 0x00, 0x30, 0x45, 0x20, 0x20, 0x01, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsPrintSpecifiedNVGraphicsData, new byte[] { 0x1d, 0x28, 0x4c, 0x06, 0x00, 0x30, 0x45, 0x20, 0x20, 0x02, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsPrintSpecifiedNVGraphicsData, new byte[] { 0x1d, 0x28, 0x4c, 0x06, 0x00, 0x30, 0x45, 0x20, 0x20, 0x02, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsTransmitKeycodeListDefinedDownloadGraphics, new byte[] { 0x1d, 0x28, 0x4c, 0x04, 0x00, 0x30, 0x50, 0x4b, 0x43 }),                                                                                                       //  GS  ( L 1D 28 4C 04 00 30 50 4B 43
            new EscPosCmd(EscPosCmdType.GsDeleteAllDownloadGraphicsData, new byte[] { 0x1d, 0x28, 0x4c, 0x05, 0x00, 0x30, 0x51, 0x43, 0x4c, 0x52 }),                                                                                                 //  GS  ( L 1D 28 4C 05 00 30 51 43 4C 52
            new EscPosCmd(EscPosCmdType.GsDeleteSpecifiedDownloadGraphicsData, new byte[] { 0x1d, 0x28, 0x4c, 0x04, 0x00, 0x30, 0x52, 0x20, 0x20 }),                                                                                                       //  GS  ( L 1D 28 4C 04 00 30 52 20-7E 20-7E
            new EscPosCmd(EscPosCmdType.GsPrintSpecifiedDownloadGraphicsData, new byte[] { 0x1d, 0x28, 0x4c, 0x06, 0x00, 0x30, 0x55, 0x20, 0x20, 0x01, 0x01 }),                                                                                           //  GS  ( L 1D 28 4C 06 00 30 55 20-7E 20-7E 01/02 01/02
            new EscPosCmd(EscPosCmdType.GsPrintSpecifiedDownloadGraphicsData, new byte[] { 0x1d, 0x28, 0x4c, 0x06, 0x00, 0x30, 0x55, 0x20, 0x20, 0x01, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsPrintSpecifiedDownloadGraphicsData, new byte[] { 0x1d, 0x28, 0x4c, 0x06, 0x00, 0x30, 0x55, 0x20, 0x20, 0x02, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsPrintSpecifiedDownloadGraphicsData, new byte[] { 0x1d, 0x28, 0x4c, 0x06, 0x00, 0x30, 0x55, 0x20, 0x20, 0x02, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsSaveSettingsValuesFromWorkToStorage, new byte[] { 0x1d, 0x28, 0x4d, 0x02, 0x00, 0x31, 0x31 }),                                                                                                                   //  GS  ( M 1D 28 4D 02 00 01/31 01/31
            new EscPosCmd(EscPosCmdType.GsLoadSettingsValuesFromStorageToWork, new byte[] { 0x1d, 0x28, 0x4d, 0x02, 0x00, 0x32, 0x31 }),                                                                                                                   //  GS  ( M 1D 28 4D 02 00 02/32 00/01/30/31
            new EscPosCmd(EscPosCmdType.GsSelectSettingsValuesToWorkAfterInitialize, new byte[] { 0x1d, 0x28, 0x4d, 0x02, 0x00, 0x33, 0x32 }),                                                                                                                   //  GS  ( M 1D 28 4D 02 00 03/33 00/01/30/31
            new EscPosCmd(EscPosCmdType.GsSetCharacterColor, new byte[] { 0x1d, 0x28, 0x4e, 0x02, 0x00, 0x30, 0x30 }),                                                                                                                   //  GS  ( N 1D 28 4E 02 00 30 30-33
            new EscPosCmd(EscPosCmdType.GsSetCharacterColor, new byte[] { 0x1d, 0x28, 0x4e, 0x02, 0x00, 0x30, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsSetCharacterColor, new byte[] { 0x1d, 0x28, 0x4e, 0x02, 0x00, 0x30, 0x32 }),
            new EscPosCmd(EscPosCmdType.GsSetCharacterColor, new byte[] { 0x1d, 0x28, 0x4e, 0x02, 0x00, 0x30, 0x33 }),
            new EscPosCmd(EscPosCmdType.GsSetBackgroundColor, new byte[] { 0x1d, 0x28, 0x4e, 0x02, 0x00, 0x31, 0x30 }),                                                                                                                   //  GS  ( N 1D 28 4E 02 00 31 30-33
            new EscPosCmd(EscPosCmdType.GsSetBackgroundColor, new byte[] { 0x1d, 0x28, 0x4e, 0x02, 0x00, 0x31, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsSetBackgroundColor, new byte[] { 0x1d, 0x28, 0x4e, 0x02, 0x00, 0x31, 0x32 }),
            new EscPosCmd(EscPosCmdType.GsSetBackgroundColor, new byte[] { 0x1d, 0x28, 0x4e, 0x02, 0x00, 0x31, 0x33 }),
            new EscPosCmd(EscPosCmdType.GsTurnShadingMode, new byte[] { 0x1d, 0x28, 0x4e, 0x03, 0x00, 0x32, 0x00, 0x30 }),                                                                                                             //  GS  ( N 1D 28 4E 03 00 32 00/01/30/31 30-33
            new EscPosCmd(EscPosCmdType.GsTurnShadingMode, new byte[] { 0x1d, 0x28, 0x4e, 0x03, 0x00, 0x32, 0x30, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsTurnShadingMode, new byte[] { 0x1d, 0x28, 0x4e, 0x03, 0x00, 0x32, 0x01, 0x30 }),
            new EscPosCmd(EscPosCmdType.GsTurnShadingMode, new byte[] { 0x1d, 0x28, 0x4e, 0x03, 0x00, 0x32, 0x31, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsTurnShadingMode, new byte[] { 0x1d, 0x28, 0x4e, 0x03, 0x00, 0x32, 0x01, 0x32 }),
            new EscPosCmd(EscPosCmdType.GsTurnShadingMode, new byte[] { 0x1d, 0x28, 0x4e, 0x03, 0x00, 0x32, 0x31, 0x33 }),
            new EscPosCmd(EscPosCmdType.GsSetPrinttableArea, new byte[] { 0x1d, 0x28, 0x50, 0x08, 0x00, 0x30, 0xff, 0xff, 0x7e, 0x06, 0x00, 0x00, 0x01 }),                                                                               //  GS  ( P 1D 28 50 08 00 30 FFFF 0001-FFFF 0000 01
            new EscPosCmd(EscPosCmdType.GsDrawLineInPageMode, new byte[] { 0x1d, 0x28, 0x51, 0x0c, 0x00, 0x30, 0x00, 0x00, 0x00, 0x00, 0xff, 0x01, 0x00, 0x00, 0x01, 0x03, 0x30 }),                                                       //  GS  ( Q 1D 28 51 0C 00 30 0000-FFFF 0000-FFFF 0000-FFFF 0000-FFFF 01 01-06 30
            new EscPosCmd(EscPosCmdType.GsDrawRectangleInPageMode, new byte[] { 0x1d, 0x28, 0x51, 0x0e, 0x00, 0x31, 0x00, 0x00, 0x00, 0x00, 0xff, 0x01, 0xff, 0x01, 0x01, 0x03, 0x30, 0x30, 0x01 }),                                           //  GS  ( Q 1D 28 51 0E 00 31 0000-FFFF 0000-FFFF 0000-FFFF 0000-FFFF 01 01-06 30 30 01
            new EscPosCmd(EscPosCmdType.GsDrawHorizontalLineInStandardMode, new byte[] { 0x1d, 0x28, 0x51, 0x09, 0x00, 0x32, 0x00, 0x00, 0xff, 0x01, 0x02, 0x01, 0x06, 0x30 }),                                                                         //  GS  ( Q 1D 28 51 09 00 32 0000-023F 0000-023F 01-FF 01 01-06 30
            new EscPosCmd(EscPosCmdType.GsDrawVerticalLineInStandardMode, new byte[] { 0x1d, 0x28, 0x51, 0x07, 0x00, 0x33, 0x00, 0x00, 0x01, 0x01, 0x05, 0x30 }),                                                                                     //  GS  ( Q 1D 28 51 07 00 33 0000-023F 00/01 01 01-06 30
            new EscPosCmd(EscPosCmdType.GsPDF417SetNumberOfColumns, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x41, 0x00 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 30 41 00-1E
            new EscPosCmd(EscPosCmdType.GsPDF417SetNumberOfColumns, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x41, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetNumberOfColumns, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x41, 0x08 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetNumberOfColumns, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x41, 0x10 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetNumberOfColumns, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x41, 0x1e }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetNumberOfRows, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x42, 0x00 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 30 42 00/03-5A
            new EscPosCmd(EscPosCmdType.GsPDF417SetNumberOfRows, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x42, 0x03 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetNumberOfRows, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x42, 0x10 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetNumberOfRows, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x42, 0x30 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetNumberOfRows, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x42, 0x50 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetNumberOfRows, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x42, 0x5a }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetWidthOfModule, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x43, 0x01 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 30 43 01-08
            new EscPosCmd(EscPosCmdType.GsPDF417SetWidthOfModule, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x43, 0x03 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetWidthOfModule, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x43, 0x05 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetWidthOfModule, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x43, 0x08 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetRowHeight, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x44, 0x02 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 30 44 02-08
            new EscPosCmd(EscPosCmdType.GsPDF417SetRowHeight, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x44, 0x03 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetRowHeight, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x44, 0x04 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetRowHeight, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x44, 0x06 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetRowHeight, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x44, 0x08 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x30, 0x45, 0x30, 0x30 }),                                                                                                       //  GS  ( k 1D 28 6B 04 00 30 45 30/31 30-38/00-28
            new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x30, 0x45, 0x30, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x30, 0x45, 0x30, 0x32 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x30, 0x45, 0x30, 0x33 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x30, 0x45, 0x30, 0x34 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x30, 0x45, 0x30, 0x35 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x30, 0x45, 0x30, 0x36 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x30, 0x45, 0x30, 0x37 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x30, 0x45, 0x30, 0x38 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x30, 0x45, 0x31, 0x00 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x30, 0x45, 0x31, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x30, 0x45, 0x31, 0x08 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x30, 0x45, 0x31, 0x10 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x30, 0x45, 0x31, 0x18 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x30, 0x45, 0x31, 0x20 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x30, 0x45, 0x31, 0x28 }),
            new EscPosCmd(EscPosCmdType.GsPDF417SelectOptions, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x46, 0x00 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 30 46 00/01
            new EscPosCmd(EscPosCmdType.GsPDF417SelectOptions, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x46, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsPDF417StoreData, new byte[] { 0x1d, 0x28, 0x6b, 0x13, 0x00, 0x30, 0x50, 0x30, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f }),             //  GS  ( k 1D 28 6B 0004-FFFF 30 50 30 00-FF...
            new EscPosCmd(EscPosCmdType.GsPDF417PrintSymbolData, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x51, 0x30 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 30 51 30
            new EscPosCmd(EscPosCmdType.GsPDF417TransmitSizeInformation, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x30, 0x52, 0x30 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 30 52 30
            new EscPosCmd(EscPosCmdType.GsQRCodeSelectModel, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x31, 0x41, 0x32, 0x00 }),                                                                                                       //  GS  ( k 1D 28 6B 04 00 31 41 31-33 00
            new EscPosCmd(EscPosCmdType.GsQRCodeSetSizeOfModule, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x31, 0x43, 0x03 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 31 43 01-10
            new EscPosCmd(EscPosCmdType.GsQRCodeSetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x31, 0x45, 0x30 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 31 45 30-33
            new EscPosCmd(EscPosCmdType.GsQRCodeStoreData, new byte[] { 0x1d, 0x28, 0x6b, 0x13, 0x00, 0x31, 0x50, 0x30, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46 }),             //  GS  ( k 1D 28 6B 0004-1BB4 31 50 30 00-FF...
            new EscPosCmd(EscPosCmdType.GsQRCodePrintSymbolData, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x31, 0x51, 0x30 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 31 51 30
            new EscPosCmd(EscPosCmdType.GsQRCodeTransmitSizeInformation, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x31, 0x52, 0x30 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 31 52 30
            new EscPosCmd(EscPosCmdType.GsMaxiCodeSelectMode, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x32, 0x41, 0x33 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 32 41 32-36
            new EscPosCmd(EscPosCmdType.GsMaxiCodeStoreData, new byte[] { 0x1d, 0x28, 0x6b, 0x13, 0x00, 0x32, 0x50, 0x30, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46 }),             //  GS  ( k 1D 28 6B 0004-008D 32 50 30 00-FF...
            new EscPosCmd(EscPosCmdType.GsMaxiCodePrintSymbolData, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x32, 0x51, 0x30 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 32 51 30
            new EscPosCmd(EscPosCmdType.GsMaxiCodeTransmitSizeInformation, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x32, 0x52, 0x30 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 32 52 30
            new EscPosCmd(EscPosCmdType.GsD2GS1DBSetWidthOfModule, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x33, 0x43, 0x02 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 33 43 02-08
            new EscPosCmd(EscPosCmdType.GsD2GS1DBSetExpandStackedMaximumWidth, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x33, 0x47, 0x00, 0x00 }),                                                                                                       //  GS  ( k 1D 28 6B 04 00 33 47 0000/006A-0F70
            new EscPosCmd(EscPosCmdType.GsD2GS1DBStoreData, new byte[] { 0x1d, 0x28, 0x6b, 0x11, 0x00, 0x33, 0x50, 0x30, 0x48, 0x30, 0x34, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }),                         //  GS  ( k 1D 28 6B 0006-0103 33 50 30 20-22/25-2F/30-39/3A-3F/41-5A/61-7A...
            new EscPosCmd(EscPosCmdType.GsD2GS1DBPrintSymbolData, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x33, 0x51, 0x30 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 33 51 30
            new EscPosCmd(EscPosCmdType.GsD2GS1DBTransmitSizeInformation, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x33, 0x52, 0x30 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 33 52 30
            new EscPosCmd(EscPosCmdType.GsCompositeSetWidthOfModule, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x34, 0x43, 0x02 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 34 43 02-08
            new EscPosCmd(EscPosCmdType.GsCompositeSetExpandStackedMaximumWidth, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x34, 0x47, 0x00, 0x00 }),                                                                                                       //  GS  ( k 1D 28 6B 04 00 34 47 0000/006A-0F70
            new EscPosCmd(EscPosCmdType.GsCompositeSelectHRICharacterFont, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x34, 0x48, 0x31 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 34 48 00-05/30-35/61/62
            new EscPosCmd(EscPosCmdType.GsCompositeStoreData, new byte[] { 0x1d, 0x28, 0x6b, 0x0f, 0x00, 0x34, 0x50, 0x30, 0x34, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }),                                     //  GS  ( k 1D 28 6B 0006-093E 34 50 30 00-FF...
            new EscPosCmd(EscPosCmdType.GsCompositePrintSymbolData, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x34, 0x51, 0x30 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 34 51 30
            new EscPosCmd(EscPosCmdType.GsCompositeTransmitSizeInformation, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x34, 0x52, 0x30 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 34 52 30
            new EscPosCmd(EscPosCmdType.GsAztecCodeSetModeTypesAndDataLayer, new byte[] { 0x1d, 0x28, 0x6b, 0x04, 0x00, 0x35, 0x42, 0x00, 0x00 }),                                                                                                       //  GS  ( k 1D 28 6B 04 00 35 42 00/01/30/31 00-20
            new EscPosCmd(EscPosCmdType.GsAztecCodeSetSizeOfModule, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x35, 0x43, 0x03 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 35 43 02-10
            new EscPosCmd(EscPosCmdType.GsAztecCodeSetErrorCollectionLevel, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x35, 0x45, 0x17 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 35 45 05-5F
            new EscPosCmd(EscPosCmdType.GsAztecCodeStoreData, new byte[] { 0x1d, 0x28, 0x6b, 0x13, 0x00, 0x35, 0x50, 0x30, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f }),             //  GS  ( k 1D 28 6B 0004-0EFB 35 50 30 00-FF...
            new EscPosCmd(EscPosCmdType.GsAztecCodePrintSymbolData, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x35, 0x51, 0x30 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 35 51 30
            new EscPosCmd(EscPosCmdType.GsAztecCodeTransmitSizeInformation, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x35, 0x52, 0x30 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 35 52 30
            new EscPosCmd(EscPosCmdType.GsDataMatrixSetSymbolTypeColumnsRows, new byte[] { 0x1d, 0x28, 0x6b, 0x05, 0x00, 0x36, 0x42, 0x00, 0x00, 0x00 }),                                                                                                 //  GS  ( k 1D 28 6B 05 00 36 42 00/01/30/31 00-90 00-90
            new EscPosCmd(EscPosCmdType.GsDataMatrixSetSizeOfModule, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x36, 0x43, 0x03 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 36 43 02-10
            new EscPosCmd(EscPosCmdType.GsDataMatrixStoreData, new byte[] { 0x1d, 0x28, 0x6b, 0x13, 0x00, 0x36, 0x50, 0x30, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f }),             //  GS  ( k 1D 28 6B 0004-0C2F 36 50 30 00-FF...
            new EscPosCmd(EscPosCmdType.GsDataMatrixPrintSymbolData, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x36, 0x51, 0x30 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 36 51 30
            new EscPosCmd(EscPosCmdType.GsDataMatrixTransmitSizeInformation, new byte[] { 0x1d, 0x28, 0x6b, 0x03, 0x00, 0x36, 0x52, 0x30 }),                                                                                                             //  GS  ( k 1D 28 6B 03 00 36 52 30
            new EscPosCmd(EscPosCmdType.GsSetReadOperationsOfCheckPaper, new byte[] { 0x1d, 0x28, 0x7a, 0x03, 0x00, 0x2a, 0x3c, 0x30 }),                                                                                                             //c GS  ( z 1D 28 7A 0003-FFFF 2A [3C/40-42/46 00/01/30/31]...
            new EscPosCmd(EscPosCmdType.GsSetsCancelsOperationToFeedCutSheetsToPrintStartingPosition, new byte[] { 0x1d, 0x28, 0x7a, 0x02, 0x00, 0x30, 0x30 }),                                                                                                                   //c GS  ( z 1D 28 7A 02 00 30 30
            new EscPosCmd(EscPosCmdType.GsStartSavingReverseSidePrintData, new byte[] { 0x1d, 0x28, 0x7a, 0x02, 0x00, 0x3e, 0x30 }),                                                                                                                   //c GS  ( z 1D 28 7A 02 00 3E 30
            new EscPosCmd(EscPosCmdType.GsFinishSavingReverseSidePrintData, new byte[] { 0x1d, 0x28, 0x7a, 0x02, 0x00, 0x3e, 0x31 }),                                                                                                                   //c GS  ( z 1D 28 7A 02 00 3E 31
            new EscPosCmd(EscPosCmdType.GsSetCounterForReverseSidePrint, new byte[] { 0x1d, 0x28, 0x7a, 0x0c, 0x00, 0x3e, 0x33, 0x09, 0x01, 0x00, 0x00, 0x00, 0x20, 0x01, 0x00, 0x00, 0x00 }),                                                       //c GS  ( z 1D 28 7A 0C 00 3E 33 01-09 00000000-3B9ACAFF 00/20/30 00000000-3B9ACAFF
            new EscPosCmd(EscPosCmdType.GsReadCheckDataContinuouslyAndTransmitDataRead, new byte[] { 0x1d, 0x28, 0x7a, 0x09, 0x00, 0x3f, 0x01, 0x00, 0x30, 0x03, 0x00, 0x30, 0x00, 0x00 }),                                                                         //c GS  ( z 1D 28 7A 09 00 3F 01 00 30 00-03 00 30 00 00
            new EscPosCmd(EscPosCmdType.GsObsoleteDefineDownloadedBitimage, new byte[] { 0x1d, 0x2a, 0x01, 0x01, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55 }),                                                                                     //  GS  *   1D 2A 01-FF 01-FF 00-FF...
            new EscPosCmd(EscPosCmdType.GsObsoletePrintDownloadedBitimage, new byte[] { 0x1d, 0x2f, 0x00 }),                                                                                                                                           //  GS  /   1D 2F 00-03/30-33
            new EscPosCmd(EscPosCmdType.GsObsoletePrintDownloadedBitimage, new byte[] { 0x1d, 0x2f, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsObsoletePrintDownloadedBitimage, new byte[] { 0x1d, 0x2f, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsObsoletePrintDownloadedBitimage, new byte[] { 0x1d, 0x2f, 0x03 }),
            new EscPosCmd(EscPosCmdType.GsObsoletePrintDownloadedBitimage, new byte[] { 0x1d, 0x2f, 0x30 }),
            new EscPosCmd(EscPosCmdType.GsObsoletePrintDownloadedBitimage, new byte[] { 0x1d, 0x2f, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsObsoletePrintDownloadedBitimage, new byte[] { 0x1d, 0x2f, 0x32 }),
            new EscPosCmd(EscPosCmdType.GsObsoletePrintDownloadedBitimage, new byte[] { 0x1d, 0x2f, 0x33 }),
            new EscPosCmd(EscPosCmdType.GsStartEndMacroDefinition, new byte[] { 0x1d, 0x3a }),                                                                                                                                                 //  GS  :   1D 3A
            new EscPosCmd(EscPosCmdType.GsTurnWhiteBlackReversePrintMode, new byte[] { 0x1d, 0x42, 0x00 }),                                                                                                                                           //  GS  B   1D 42 bnnnnnnnx
            new EscPosCmd(EscPosCmdType.GsTurnWhiteBlackReversePrintMode, new byte[] { 0x1d, 0x42, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsObsoleteSelectCounterPrintMode, new byte[] { 0x1d, 0x43, 0x30, 0x05, 0x31 }),                                                                                                                               //  GS  C 0 1D 43 30 00-05 00-02/30-32
            new EscPosCmd(EscPosCmdType.GsObsoleteSelectCounterModeA, new byte[] { 0x1d, 0x43, 0x31, 0x01, 0x00, 0xff, 0xff, 0x01, 0x01 }),                                                                                                       //  GS  C 1 1D 43 31 0000-FFFF 0000-FFFF 00-FF 00-FF
            new EscPosCmd(EscPosCmdType.GsObsoleteSetCounter, new byte[] { 0x1d, 0x43, 0x32, 0x01, 0x00 }),                                                                                                                               //  GS  C 2 1D 43 32 0000-FFFF
            new EscPosCmd(EscPosCmdType.GsObsoleteSelectCounterModeB, new byte[] { 0x1d, 0x43, 0x3b, 0x3b, 0x3b, 0x35, 0x3b, 0x3b, 0x31, 0x30, 0x30, 0x3b }),                                                                                     //  GS  C ; 1D 43 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B
                                                                                                                                                                                                                                                  //  GS  D   1D 44 30 43 30 20-7E 20-7E 30/34 31 42 4D 00000042-FFFFFFFF 00-FF...
            new EscPosCmd(EscPosCmdType.GsDefineWindowsBMPNVGraphicsData, new byte[] { 0x1d, 0x44, 0x30, 0x43, 0x30, 0x20, 0x20, 0x30, 0x31, 0x42, 0x4d, 0x5e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3e, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0x00, 0x7e, 0x00, 0x00, 0x00, 0x9d, 0x00, 0x00, 0x00, 0xdb, 0x00, 0x00, 0x00, 0xe7, 0x00, 0x00, 0x00, 0xe7, 0x00, 0x00, 0x00, 0xd9, 0x00, 0x00, 0x00, 0xbc, 0x00, 0x00, 0x00, 0x7e, 0x00, 0x00, 0x00 }),
            //  GS  D   1D 44 30 53 30 20-7E 20-7E 30/34 31 42 4D 00000042-FFFFFFFF 00-FF...
            new EscPosCmd(EscPosCmdType.GsDefineWindowsBMPDownloadGraphicsData, new byte[] { 0x1d, 0x44, 0x30, 0x53, 0x30, 0x20, 0x20, 0x30, 0x31, 0x42, 0x4d, 0x5e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3e, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0x00, 0x7e, 0x00, 0x00, 0x00, 0x9d, 0x00, 0x00, 0x00, 0xdb, 0x00, 0x00, 0x00, 0xe7, 0x00, 0x00, 0x00, 0xe7, 0x00, 0x00, 0x00, 0xd9, 0x00, 0x00, 0x00, 0xbc, 0x00, 0x00, 0x00, 0x7e, 0x00, 0x00, 0x00 }),
            new EscPosCmd(EscPosCmdType.GsObsoleteSelectHeadControlMethod, new byte[] { 0x1d, 0x45, 0x15 }),                                                                                                                                           //c GS  E   1D 45 b000x0x0x
            new EscPosCmd(EscPosCmdType.GsSelectPrintPositionHRICharacters, new byte[] { 0x1d, 0x48, 0x32 }),                                                                                                                                           //  GS  H   1D 48 00-03/30-33
            new EscPosCmd(EscPosCmdType.GsTransmitPrinterID, new byte[] { 0x1d, 0x49, 0x31 }),                                                                                                                                           //  GS  I   1D 49 01-03/31-33/21/23/24/41-45/60/6E-70
            new EscPosCmd(EscPosCmdType.GsSetLeftMargin, new byte[] { 0x1d, 0x4c, 0x00, 0x00 }),                                                                                                                                     //  GS  L   1D 4C 0000-FFFF
            new EscPosCmd(EscPosCmdType.GsSetLeftMargin, new byte[] { 0x1d, 0x4c, 0xff, 0xff }),
            new EscPosCmd(EscPosCmdType.GsSetHorizontalVerticalMotionUnits, new byte[] { 0x1d, 0x50, 0x00, 0x00 }),                                                                                                                                     //  GS  P   1D 50 00-FF 00-FF
            new EscPosCmd(EscPosCmdType.GsObsoletePrintVariableVerticalSizeBitimage, new byte[] { 0x1d, 0x51, 0x30, 0x33, 0x01, 0x00, 0x01, 0x00, 0xaa }),                                                                                                       //  GS  Q 0 1D 51 30 00-03/30-33 0001-10A0 0001-0010 00-FF...
            new EscPosCmd(EscPosCmdType.GsSetPrintPositionBeginningOfPrintLine, new byte[] { 0x1d, 0x54, 0x00 }),                                                                                                                                           //  GS  T   1D 54 00/01/30/31
            new EscPosCmd(EscPosCmdType.GsSetPrintPositionBeginningOfPrintLine, new byte[] { 0x1d, 0x54, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsSetPrintPositionBeginningOfPrintLine, new byte[] { 0x1d, 0x54, 0x30 }),
            new EscPosCmd(EscPosCmdType.GsSetPrintPositionBeginningOfPrintLine, new byte[] { 0x1d, 0x54, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsPaperFullCut, new byte[] { 0x1d, 0x56, 0x00 }),                                                                                                                                           //  GS  V   1D 56 00/30
            new EscPosCmd(EscPosCmdType.GsPaperFullCut, new byte[] { 0x1d, 0x56, 0x30 }),
            new EscPosCmd(EscPosCmdType.GsPaperPartialCut, new byte[] { 0x1d, 0x56, 0x01 }),                                                                                                                                           //  GS  V   1D 56 01/31
            new EscPosCmd(EscPosCmdType.GsPaperPartialCut, new byte[] { 0x1d, 0x56, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsPaperFeedAndFullCut, new byte[] { 0x1d, 0x56, 0x41, 0x04 }),                                                                                                                                     //  GS  V   1D 56 41 00-FF
            new EscPosCmd(EscPosCmdType.GsPaperFeedAndPartialCut, new byte[] { 0x1d, 0x56, 0x42, 0x04 }),                                                                                                                                     //  GS  V   1D 56 42 00-FF
            new EscPosCmd(EscPosCmdType.GsPaperReservedFeedAndFullCut, new byte[] { 0x1d, 0x56, 0x61, 0x04 }),                                                                                                                                     //  GS  V   1D 56 61 00-FF
            new EscPosCmd(EscPosCmdType.GsPaperReservedFeedAndPartialCut, new byte[] { 0x1d, 0x56, 0x62, 0x04 }),                                                                                                                                     //  GS  V   1D 56 62 00-FF
            new EscPosCmd(EscPosCmdType.GsPaperFeedAndFullCutAndTopOfForm, new byte[] { 0x1d, 0x56, 0x67, 0x04 }),                                                                                                                                     //  GS  V   1D 56 67 00-FF
            new EscPosCmd(EscPosCmdType.GsPaperFeedAndPartialCutAndTopOfForm, new byte[] { 0x1d, 0x56, 0x68, 0x04 }),                                                                                                                                     //  GS  V   1D 56 68 00-FF
            new EscPosCmd(EscPosCmdType.GsSetPrintAreaWidth, new byte[] { 0x1d, 0x57, 0x40, 0x02 }),                                                                                                                                     //  GS  W   1D 57 0000-FFFF
            new EscPosCmd(EscPosCmdType.GsSetPrintAreaWidth, new byte[] { 0x1d, 0x57, 0x00, 0x00 }),
            new EscPosCmd(EscPosCmdType.GsSetPrintAreaWidth, new byte[] { 0x1d, 0x57, 0xff, 0xff }),
            new EscPosCmd(EscPosCmdType.GsSetRelativeVerticalPrintPosition, new byte[] { 0x1d, 0x5c, 0x1e, 0x00 }),                                                                                                                                     //  GS  \   1D 5C 8000-7FFF
            new EscPosCmd(EscPosCmdType.GsSetRelativeVerticalPrintPosition, new byte[] { 0x1d, 0x5c, 0x00, 0x80 }),
            new EscPosCmd(EscPosCmdType.GsSetRelativeVerticalPrintPosition, new byte[] { 0x1d, 0x5c, 0x00, 0x00 }),
            new EscPosCmd(EscPosCmdType.GsSetRelativeVerticalPrintPosition, new byte[] { 0x1d, 0x5c, 0xff, 0x7f }),
            new EscPosCmd(EscPosCmdType.GsExecuteMacro, new byte[] { 0x1d, 0x5e, 0x01, 0x00, 0x01 }),                                                                                                                               //  GS  ^   1D 5E 01-FF 00-FF 00/01
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x00 }),                                                                                                                                           //  GS  a   1D 61 b0x00xxxx
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x03 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x04 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x05 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x06 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x07 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x08 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x09 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x0a }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x0b }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x0c }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x0d }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x0e }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x0f }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x40 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x41 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x42 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x43 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x44 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x45 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x46 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x47 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x48 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x49 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x4a }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x4b }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x4c }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x4d }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x4e }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, new byte[] { 0x1d, 0x61, 0x4f }),
            new EscPosCmd(EscPosCmdType.GsTurnSmoothingMode, new byte[] { 0x1d, 0x62, 0x00 }),                                                                                                                                           //  GS  b   1D 62 bnnnnnnnx
            new EscPosCmd(EscPosCmdType.GsTurnSmoothingMode, new byte[] { 0x1d, 0x62, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsObsoletePrintCounter, new byte[] { 0x1d, 0x63 }),                                                                                                                                                 //  GS  c   1D 63
            new EscPosCmd(EscPosCmdType.GsSelectFontHRICharacters, new byte[] { 0x1d, 0x66, 0x30 }),                                                                                                                                           //  GS  f   1D 66 00-04/30-34/61/62
            new EscPosCmd(EscPosCmdType.GsInitializeMaintenanceCounter, new byte[] { 0x1d, 0x67, 0x30, 0x00, 0x14, 0x00 }),                                                                                                                         //  GS  g 0 1D 67 30 00 000A-004F
            new EscPosCmd(EscPosCmdType.GsTransmitMaintenanceCounter, new byte[] { 0x1d, 0x67, 0x32, 0x00, 0xc6, 0x00 }),                                                                                                                         //  GS  g 2 1D 67 32 00 000A-004F/008A-00CF
            new EscPosCmd(EscPosCmdType.GsSetBarcodeHight, new byte[] { 0x1d, 0x68, 0x01 }),                                                                                                                                           //  GS  h   1D 68 01-FF
            new EscPosCmd(EscPosCmdType.GsSetBarcodeHight, new byte[] { 0x1d, 0x68, 0x50 }),
            new EscPosCmd(EscPosCmdType.GsSetBarcodeHight, new byte[] { 0x1d, 0x68, 0xff }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBackInk, new byte[] { 0x1d, 0x6a, 0x00 }),                                                                                                                                           //  GS  j   1D 6A b000000xx
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBackInk, new byte[] { 0x1d, 0x6a, 0x01 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBackInk, new byte[] { 0x1d, 0x6a, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBackInk, new byte[] { 0x1d, 0x6a, 0x03 }),
            new EscPosCmd(EscPosCmdType.GsPrintBarcodeAsciiz, new byte[] { 0x1d, 0x6b, 0x03, 0x34, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x00 }),                                                             //  GS  k   1D 6B 00-06 20/24/25/2A/2B/2D-2F/30-39/41-5A/61-64... 00
            new EscPosCmd(EscPosCmdType.GsPrintBarcodeSpecifiedLength, new byte[] { 0x1d, 0x6b, 0x43, 0x0c, 0x34, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }),                                                             //  GS  k   1D 6B 41-4F 01-FF 00-FF...
            new EscPosCmd(EscPosCmdType.GsTransmitStatus, new byte[] { 0x1d, 0x72, 0x01 }),                                                                                                                                           //  GS  r   1D 72 01/02/04/31/32/34
            new EscPosCmd(EscPosCmdType.GsTransmitStatus, new byte[] { 0x1d, 0x72, 0x02 }),
            new EscPosCmd(EscPosCmdType.GsTransmitStatus, new byte[] { 0x1d, 0x72, 0x04 }),
            new EscPosCmd(EscPosCmdType.GsTransmitStatus, new byte[] { 0x1d, 0x72, 0x31 }),
            new EscPosCmd(EscPosCmdType.GsTransmitStatus, new byte[] { 0x1d, 0x72, 0x32 }),
            new EscPosCmd(EscPosCmdType.GsTransmitStatus, new byte[] { 0x1d, 0x72, 0x34 }),
            new EscPosCmd(EscPosCmdType.GsObsoletePrintRasterBitimage, new byte[] { 0x1d, 0x76, 0x30, 0x33, 0x01, 0x00, 0x01, 0x00, 0xaa }),                                                                                                       //  GS  v 0 1D 76 30 00-03/30-33 0001-FFFF 0001-11FF 00-FF...
            new EscPosCmd(EscPosCmdType.GsSetBarcodeWidth, new byte[] { 0x1d, 0x77, 0x02 }),                                                                                                                                           //  GS  w   1D 77 02-06/44-4C
            new EscPosCmd(EscPosCmdType.GsSetBarcodeWidth, new byte[] { 0x1d, 0x77, 0x03 }),
            new EscPosCmd(EscPosCmdType.GsSetBarcodeWidth, new byte[] { 0x1d, 0x77, 0x04 }),
            new EscPosCmd(EscPosCmdType.GsSetBarcodeWidth, new byte[] { 0x1d, 0x77, 0x05 }),
            new EscPosCmd(EscPosCmdType.GsSetBarcodeWidth, new byte[] { 0x1d, 0x77, 0x06 }),
            new EscPosCmd(EscPosCmdType.GsSetBarcodeWidth, new byte[] { 0x1d, 0x77, 0x44 }),
            new EscPosCmd(EscPosCmdType.GsSetBarcodeWidth, new byte[] { 0x1d, 0x77, 0x45 }),
            new EscPosCmd(EscPosCmdType.GsSetBarcodeWidth, new byte[] { 0x1d, 0x77, 0x46 }),
            new EscPosCmd(EscPosCmdType.GsSetBarcodeWidth, new byte[] { 0x1d, 0x77, 0x47 }),
            new EscPosCmd(EscPosCmdType.GsSetBarcodeWidth, new byte[] { 0x1d, 0x77, 0x48 }),
            new EscPosCmd(EscPosCmdType.GsSetBarcodeWidth, new byte[] { 0x1d, 0x77, 0x49 }),
            new EscPosCmd(EscPosCmdType.GsSetBarcodeWidth, new byte[] { 0x1d, 0x77, 0x4a }),
            new EscPosCmd(EscPosCmdType.GsSetBarcodeWidth, new byte[] { 0x1d, 0x77, 0x4b }),
            new EscPosCmd(EscPosCmdType.GsSetBarcodeWidth, new byte[] { 0x1d, 0x77, 0x4c }),
            new EscPosCmd(EscPosCmdType.GsSetOnlineRecoveryWaitTime, new byte[] { 0x1d, 0x7a, 0x30, 0x06, 0x00 })                                                                                                                                //  GS  z 0 1D 7A 30 00-FF 00-FF
        };

        [TestMethod]
        public void TestMethodPrtGss()
        {
            byte[] linqprtgss = prtgssResult.SelectMany(v => v.cmddata).ToArray<byte>();
            var EPTkn = new EscPosTokenizer();
            List<EscPosCmd> escposlist = EPTkn.Scan(linqprtgss, EscPosTokenizer.EscPosPrinter);
            escposlist = EscPosDecoder.Convert(escposlist);
            //FileOutList(escposlist, $"{TestContext.TestResultsDirectory}\\gss.txt");
            FileOutList(escposlist, ".\\gss.txt");

            Assert.AreEqual(prtgssResult.Count, escposlist.Count);
            for (int i = 0; i < prtgssResult.Count; i++)
            {
                Assert.AreEqual(prtgssResult[i].cmdtype, escposlist[i].cmdtype);
                Assert.AreEqual(prtgssResult[i].cmdlength, escposlist[i].cmdlength);
                for (int j = 0; j < prtgssResult[i].cmdlength; j++)
                {
                    Assert.AreEqual(prtgssResult[i].cmddata[j], escposlist[i].cmddata[j]);
                }
            }
        }

        public static readonly List<EscPosCmd> vfdussResult = new()
        {
            new EscPosCmd(EscPosCmdType.VfdUsOverwriteMode, new byte[] { 0x1f, 0x01 }),                                                                                 //  US  \1  1F 01
            new EscPosCmd(EscPosCmdType.VfdUsVerticalScrollMode, new byte[] { 0x1f, 0x02 }),                                                                                 //  US  \1  1F 02
            new EscPosCmd(EscPosCmdType.VfdUsHorizontalScrollMode, new byte[] { 0x1f, 0x03 }),                                                                                 //  US  \1  1F 03
            new EscPosCmd(EscPosCmdType.VfdUsMoveCursorUp, new byte[] { 0x1f, 0x0a }),                                                                                 //  US  LF  1F 0A
            new EscPosCmd(EscPosCmdType.VfdUsMoveCursorRightMost, new byte[] { 0x1f, 0x0d }),                                                                                 //  US  LF  1F 0D
            new EscPosCmd(EscPosCmdType.VfdUsTurnAnnounciatorOnOff, new byte[] { 0x1f, 0x23, 0x31, 0x00 }),                                                                     //  US  #   1F 23 00/01/30/31 00-14
            new EscPosCmd(EscPosCmdType.VfdUsMoveCursorSpecifiedPosition, new byte[] { 0x1f, 0x24, 0x01, 0x01 }),                                                                     //  US  $   1F 24 01-14 01/02
            new EscPosCmd(EscPosCmdType.VfdUsMoveCursorSpecifiedPosition, new byte[] { 0x1f, 0x24, 0x14, 0x01 }),
            new EscPosCmd(EscPosCmdType.VfdUsMoveCursorSpecifiedPosition, new byte[] { 0x1f, 0x24, 0x01, 0x02 }),
            new EscPosCmd(EscPosCmdType.VfdUsMoveCursorSpecifiedPosition, new byte[] { 0x1f, 0x24, 0x14, 0x02 }),
            new EscPosCmd(EscPosCmdType.VfdUsSelectDisplays, new byte[] { 0x1f, 0x28, 0x41, 0x03, 0x00, 0x30, 0x31, 0x00 }),                                             //  US  ( A 1F 28 41 0003-FFFF 30 [30/31 00-FF]...
            new EscPosCmd(EscPosCmdType.VfdUsChangeIntoUserSettingMode, new byte[] { 0x1f, 0x28, 0x45, 0x03, 0x00, 0x01, 0x49, 0x4e }),                                             //  US  ( E 1F 28 45 03 00 01 49 4E
            new EscPosCmd(EscPosCmdType.VfdUsEndUserSettingMode, new byte[] { 0x1f, 0x28, 0x45, 0x04, 0x00, 0x02, 0x4f, 0x55, 0x54 }),                                       //  US  ( E 1F 28 45 04 00 02 4F 55 54
            new EscPosCmd(EscPosCmdType.VfdUsSetMemorySwitchValues, new byte[] { 0x1f, 0x28, 0x45, 0x0a, 0x00, 0x03, 0x0c, 0x32, 0x32, 0x32, 0x32, 0x32, 0x31, 0x30, 0x30 }),   //  US  ( E 1F 28 45 000A-FFFA 03 [09-0F 30-32]...
            new EscPosCmd(EscPosCmdType.VfdUsSendingDisplayingMemorySwitchValues, new byte[] { 0x1f, 0x28, 0x45, 0x02, 0x00, 0x04, 0x0f }),                                                   //  US  ( E 1F 28 45 02 00 04 09-0F/6D-70/72/73
            new EscPosCmd(EscPosCmdType.VfdUsKanjiCharacterModeOnOff, new byte[] { 0x1f, 0x28, 0x47, 0x02, 0x00, 0x60, 0x00 }),                                                   //  US  ( G 1F 28 47 02 00 60 00/01/30/31
            new EscPosCmd(EscPosCmdType.VfdUsKanjiCharacterModeOnOff, new byte[] { 0x1f, 0x28, 0x47, 0x02, 0x00, 0x60, 0x01 }),
            new EscPosCmd(EscPosCmdType.VfdUsKanjiCharacterModeOnOff, new byte[] { 0x1f, 0x28, 0x47, 0x02, 0x00, 0x60, 0x30 }),
            new EscPosCmd(EscPosCmdType.VfdUsKanjiCharacterModeOnOff, new byte[] { 0x1f, 0x28, 0x47, 0x02, 0x00, 0x60, 0x31 }),
            new EscPosCmd(EscPosCmdType.VfdUsSelectKanjiCharacterCodeSystem, new byte[] { 0x1f, 0x28, 0x47, 0x02, 0x00, 0x61, 0x00 }),                                                   //  US  ( G 1F 28 47 02 00 61 00/01/30/31
            new EscPosCmd(EscPosCmdType.VfdUsSelectKanjiCharacterCodeSystem, new byte[] { 0x1f, 0x28, 0x47, 0x02, 0x00, 0x61, 0x01 }),
            new EscPosCmd(EscPosCmdType.VfdUsSelectKanjiCharacterCodeSystem, new byte[] { 0x1f, 0x28, 0x47, 0x02, 0x00, 0x61, 0x30 }),
            new EscPosCmd(EscPosCmdType.VfdUsSelectKanjiCharacterCodeSystem, new byte[] { 0x1f, 0x28, 0x47, 0x02, 0x00, 0x61, 0x31 }),
            new EscPosCmd(EscPosCmdType.VfdUsDisplayCharWithComma, new byte[] { 0x1f, 0x2c, 0x20 }),                                                                           //  US  ,   1F 2C 20-7E/80-FF
            new EscPosCmd(EscPosCmdType.VfdUsDisplayCharWithComma, new byte[] { 0x1f, 0x2c, 0x7e }),
            new EscPosCmd(EscPosCmdType.VfdUsDisplayCharWithComma, new byte[] { 0x1f, 0x2c, 0x80 }),
            new EscPosCmd(EscPosCmdType.VfdUsDisplayCharWithComma, new byte[] { 0x1f, 0x2c, 0xff }),
            new EscPosCmd(EscPosCmdType.VfdUsDisplayCharWithPeriod, new byte[] { 0x1f, 0x2e, 0x20 }),                                                                           //  US  .   1F 2E 20-7E/80-FF
            new EscPosCmd(EscPosCmdType.VfdUsDisplayCharWithPeriod, new byte[] { 0x1f, 0x2e, 0x7e }),
            new EscPosCmd(EscPosCmdType.VfdUsDisplayCharWithPeriod, new byte[] { 0x1f, 0x2e, 0x80 }),
            new EscPosCmd(EscPosCmdType.VfdUsDisplayCharWithPeriod, new byte[] { 0x1f, 0x2e, 0xff }),
            new EscPosCmd(EscPosCmdType.VfdUsStartEndMacroDefinition, new byte[] { 0x1f, 0x3a }),                                                                                 //  US  :   1F 3A
            new EscPosCmd(EscPosCmdType.VfdUsDisplayCharWithSemicolon, new byte[] { 0x1f, 0x3b, 0x20 }),                                                                           //  US  ;   1F 3B 20-7E/80-FF
            new EscPosCmd(EscPosCmdType.VfdUsDisplayCharWithSemicolon, new byte[] { 0x1f, 0x3b, 0x7e }),
            new EscPosCmd(EscPosCmdType.VfdUsDisplayCharWithSemicolon, new byte[] { 0x1f, 0x3b, 0x80 }),
            new EscPosCmd(EscPosCmdType.VfdUsDisplayCharWithSemicolon, new byte[] { 0x1f, 0x3b, 0xff }),
            new EscPosCmd(EscPosCmdType.VfdUsExecuteSelfTest, new byte[] { 0x1f, 0x40 }),                                                                                 //  US  @   1F 40
            new EscPosCmd(EscPosCmdType.VfdUsMoveCursorBottom, new byte[] { 0x1f, 0x42 }),                                                                                 //  US  B   1F 42
            new EscPosCmd(EscPosCmdType.VfdUsTurnCursorDisplayModeOnOff, new byte[] { 0x1f, 0x43, 0x00 }),                                                                           //  US  C   1F 43 00/01/30/31
            new EscPosCmd(EscPosCmdType.VfdUsTurnCursorDisplayModeOnOff, new byte[] { 0x1f, 0x43, 0x01 }),
            new EscPosCmd(EscPosCmdType.VfdUsTurnCursorDisplayModeOnOff, new byte[] { 0x1f, 0x43, 0x30 }),
            new EscPosCmd(EscPosCmdType.VfdUsTurnCursorDisplayModeOnOff, new byte[] { 0x1f, 0x43, 0x31 }),
            new EscPosCmd(EscPosCmdType.VfdUsSetDisplayBlinkInterval, new byte[] { 0x1f, 0x45, 0x00 }),                                                                           //  US  E   1F 45 00-FF
            new EscPosCmd(EscPosCmdType.VfdUsSetDisplayBlinkInterval, new byte[] { 0x1f, 0x45, 0x01 }),
            new EscPosCmd(EscPosCmdType.VfdUsSetDisplayBlinkInterval, new byte[] { 0x1f, 0x45, 0xff }),
            new EscPosCmd(EscPosCmdType.VfdUsSetAndDisplayCountTime, new byte[] { 0x1f, 0x54, 0x08, 0x00 }),                                                                     //  US  T   1F 54 00-17 00-3B
            new EscPosCmd(EscPosCmdType.VfdUsDisplayCounterTime, new byte[] { 0x1f, 0x55 }),                                                                                 //  US  U   1F 55
            new EscPosCmd(EscPosCmdType.VfdUsBrightnessAdjustment, new byte[] { 0x1f, 0x58, 0x01 }),                                                                           //  US  X   1F 58 01-04
            new EscPosCmd(EscPosCmdType.VfdUsBrightnessAdjustment, new byte[] { 0x1f, 0x58, 0x02 }),
            new EscPosCmd(EscPosCmdType.VfdUsBrightnessAdjustment, new byte[] { 0x1f, 0x58, 0x03 }),
            new EscPosCmd(EscPosCmdType.VfdUsBrightnessAdjustment, new byte[] { 0x1f, 0x58, 0x04 }),
            new EscPosCmd(EscPosCmdType.VfdUsExecuteMacro, new byte[] { 0x1f, 0x5e, 0x01, 0x01 }),                                                                     //  US  ^   1F 5E 00-FF 00-FF
            new EscPosCmd(EscPosCmdType.VfdUsTurnReverseMode, new byte[] { 0x1f, 0x72, 0x00 }),                                                                           //  US  r   1F 72 00/01/30/31
            new EscPosCmd(EscPosCmdType.VfdUsTurnReverseMode, new byte[] { 0x1f, 0x72, 0x01 }),
            new EscPosCmd(EscPosCmdType.VfdUsTurnReverseMode, new byte[] { 0x1f, 0x72, 0x30 }),
            new EscPosCmd(EscPosCmdType.VfdUsTurnReverseMode, new byte[] { 0x1f, 0x72, 0x31 }),
            new EscPosCmd(EscPosCmdType.VfdUsStatusConfirmationByDTRSignal, new byte[] { 0x1f, 0x76, 0x00 }),                                                                           //  US  v   1F 76 00/01/30/31
            new EscPosCmd(EscPosCmdType.VfdUsStatusConfirmationByDTRSignal, new byte[] { 0x1f, 0x76, 0x01 }),
            new EscPosCmd(EscPosCmdType.VfdUsStatusConfirmationByDTRSignal, new byte[] { 0x1f, 0x76, 0x30 }),
            new EscPosCmd(EscPosCmdType.VfdUsStatusConfirmationByDTRSignal, new byte[] { 0x1f, 0x76, 0x31 })
        };

        [TestMethod]
        public void TestMethodVfdUss()
        {
            byte[] linqvfduss = vfdussResult.SelectMany(v => v.cmddata).ToArray<byte>();
            var EPTkn = new EscPosTokenizer();
            List<EscPosCmd> escposlist = EPTkn.Scan(linqvfduss, EscPosTokenizer.EscPosLineDisplay);
            escposlist = EscPosDecoder.Convert(escposlist);
            //FileOutList(escposlist, $"{TestContext.TestResultsDirectory}\\uss.txt");
            FileOutList(escposlist, ".\\uss.txt");

            Assert.AreEqual(vfdussResult.Count, escposlist.Count);
            for (int i = 0; i < vfdussResult.Count; i++)
            {
                Assert.AreEqual(vfdussResult[i].cmdtype, escposlist[i].cmdtype);
                Assert.AreEqual(vfdussResult[i].cmdlength, escposlist[i].cmdlength);
                for (int j = 0; j < vfdussResult[i].cmdlength; j++)
                {
                    Assert.AreEqual(vfdussResult[i].cmddata[j], escposlist[i].cmddata[j]);
                }
            }
        }
    }
}