/*

   Copyright (C) 2022 Kunio Fukuchi

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

namespace kunif.EscPosEncode
{
    using kunif.EscPosUtils;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;

    internal class EscPosEncode
    {
#pragma warning disable IDE0052 // Remove unread private members
        private static long deviceType = EscPosEncoder.EscPosPrinter;
        private static Boolean stdout = true;
        private static int initialcodepage = -1;
        private static Boolean initialkanjion = false;
        private static byte initialICS = 0;
        private static string inputpath = "";
        private static string outputpath = "./EncResult.bin";
        private static int sbcsfontpattern = 1;
        private static int mbcsfontpattern = 1;
        private static int vfdfontpattern = 1;
#pragma warning restore IDE0052 // Remove unread private members

        private static int Main(string[] args)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            // Test if input arguments were supplied.
            int options = Options(args);
            switch (options)
            {
                case 1: HelpGeneral(); return options;
                case 2: HelpFontPattern(); return options;
                case 3: HelpCodePage(); return options;
                default: break;
            }

            //byte[] escposdata = ReadFile(inputpath);

            //if ((escposdata is null) || (escposdata.Length == 0))
            //{
            //    Console.Error.WriteLine("No input data.");
            //    return 11;
            //}

            //var EPEnc = new EscPosEncoder();
            //EPEnc.Configuration(deviceType, sbcsfontpattern, mbcsfontpattern, vfdfontpattern, 384, 1662, initialcodepage, (EscPosEncoder.InternationalCharacter)Enum.ToObject(typeof(EscPosEncoder.InternationalCharacter), initialICS));

            //SimpleReceipt(ref EPEnc);
            //byte[] result = EPEnc.CommandList.SelectMany(v => v.cmddata).ToArray<byte>();
            //WriteFile("./EncSimpleReceipt.bin", result);

            //EPEnc.CommandList.Clear();
            //EPEnc.Configuration(deviceType, sbcsfontpattern, mbcsfontpattern, vfdfontpattern, 384, 1662, initialcodepage, (EscPosEncoder.InternationalCharacter)Enum.ToObject(typeof(EscPosEncoder.InternationalCharacter), initialICS));
            //ReceiptWithBarcode(ref EPEnc);
            //result = EPEnc.CommandList.SelectMany(v => v.cmddata).ToArray<byte>();
            //WriteFile("./EncLogoWithBarcode.bin", result);

            //EPEnc.CommandList.Clear();
            //EPEnc.Configuration(deviceType, sbcsfontpattern, mbcsfontpattern, vfdfontpattern, 384, 1662, initialcodepage, (EscPosEncoder.InternationalCharacter)Enum.ToObject(typeof(EscPosEncoder.InternationalCharacter), initialICS));
            //PagemodePrint(ref EPEnc);
            //result = EPEnc.CommandList.SelectMany(v => v.cmddata).ToArray<byte>();
            //WriteFile("./EncPageMode.bin", result);

            //EPEnc.CommandList.Clear();
            //EPEnc.Configuration(deviceType, sbcsfontpattern, mbcsfontpattern, vfdfontpattern, 384, 1662, initialcodepage, (EscPosEncoder.InternationalCharacter)Enum.ToObject(typeof(EscPosEncoder.InternationalCharacter), initialICS));
            //PaperCut(ref EPEnc);
            //result = EPEnc.CommandList.SelectMany(v => v.cmddata).ToArray<byte>();
            //WriteFile("./EncPaperCut.bin", result);

            //EPEnc.CommandList.Clear();
            //EPEnc.Configuration(deviceType, sbcsfontpattern, mbcsfontpattern, vfdfontpattern, 384, 1662, initialcodepage, (EscPosEncoder.InternationalCharacter)Enum.ToObject(typeof(EscPosEncoder.InternationalCharacter), initialICS));
            //NVGraphics(ref EPEnc);
            //result = EPEnc.CommandList.SelectMany(v => v.cmddata).ToArray<byte>();
            //WriteFile("./EncGraphics.bin", result);

            var EPEnc = new EscPosEncoder();
            EPEnc.Configuration(deviceType, sbcsfontpattern, mbcsfontpattern, vfdfontpattern, 384, 1662, initialcodepage, (EscPosEncoder.InternationalCharacter)Enum.ToObject(typeof(EscPosEncoder.InternationalCharacter), initialICS));
            SimpleReceipt(ref EPEnc);

            ReceiptWithBarcode(ref EPEnc);

            PagemodePrint(ref EPEnc);

            Label(ref EPEnc);

            PaperCut(ref EPEnc);

            NVGraphics(ref EPEnc);

            byte[] result = EPEnc.CommandList.SelectMany(v => v.cmddata).ToArray<byte>();
            if (stdout)
            {
                using (Stream outStream = Console.OpenStandardOutput())
                {
                    outStream.Write(result, 0, result.Length);
                }
            }
            else
            {
                WriteFile(outputpath, result);
            }

            return 0;
        }

        private static void SimpleReceipt(ref EscPosEncoder enc)
        {
            enc.Initialize();
            enc.LineSpacing = 18;
            enc.Justification = EscPosEncoder.Alignment.Center;
            enc.CharScale = new EscPosEncoder.Scale() { width = 2, height = 2 };
            enc.Printables("╔═══════════╗\x0a");
            enc.Printables("║  ESC/POS  ║\x0a");
            enc.Printables("║   ");
            enc.CharScale = new EscPosEncoder.Scale() { width = 1, height = 1 };
            enc.Printables("Thank you ");
            enc.CharScale = new EscPosEncoder.Scale() { width = 2, height = 2 };
            enc.Printables("   ║\x0a");
            enc.Printables("╚═══════════╝\x0a");

            enc.DefaultLineSpacing = 30;
            enc.CharScale = new EscPosEncoder.Scale() { width = 1, height = 1 };

            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 4);
            enc.Printables("NOVEMBER 1, 2012 10:30");
            enc.PrintAndFeed(EscPosEncoder.FeedType.Lines, 3);

            enc.Justification = EscPosEncoder.Alignment.Left;
            enc.Printables("TM-Uxxx 6.75\x0a");
            enc.Printables("TM-Hxxx 6.00\x0a");
            enc.Printables("PS-xxx 1.70\x0a\x0a");

            enc.CharScale = new EscPosEncoder.Scale() { width = 1, height = 2 };
            enc.Printables("TOTAL 14.45\x0a");
            enc.CharScale = new EscPosEncoder.Scale() { width = 1, height = 1 };
            enc.Printables("---------------------------------------\x0a");
            enc.Printables("PAID 50.00\x0a");
            enc.Printables("CHANGE 35.55\x0a");

            enc.GeneratePule(EscPosEncoder.DrawerKick.Pin2, 2, 20);
            enc.CutPaper(EscPosEncoder.CutType.FeedAndPartialCut, 0);

            //==================

            enc.Initialize();
            enc.LineSpacing = 18;
            enc.Justification = EscPosEncoder.Alignment.Center;
            enc.CharScale = new EscPosEncoder.Scale() { width = 2, height = 2 };
            enc.Printables("╔═══════════╗\x0a");
            enc.Printables("║  ESC/POS  ║\x0a");
            enc.Printables("║   ");
            enc.CharScale = new EscPosEncoder.Scale() { width = 1, height = 1 };
            enc.Printables("Thank you ");
            enc.CharScale = new EscPosEncoder.Scale() { width = 2, height = 2 };
            enc.Printables("   ║\x0a");
            enc.Printables("╚═══════════╝\x0a");

            enc.DefaultLineSpacing = 30;
            enc.CharScale = new EscPosEncoder.Scale() { width = 1, height = 1 };

            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 4);
            enc.Printables("NOVEMBER 1, 2012 10:30");
            enc.PrintAndFeed(EscPosEncoder.FeedType.Lines, 3);

            enc.Justification = EscPosEncoder.Alignment.Left;
            enc.Printables("TM-Uxxx 6.75\x0a");
            enc.Printables("TM-Hxxx 6.00\x0a");
            enc.Printables("PS-xxx 1.70\x0a\x0a");

            enc.CharScale = new EscPosEncoder.Scale() { width = 1, height = 2 };
            enc.Printables("TOTAL 14.45\x0a");
            enc.CharScale = new EscPosEncoder.Scale() { width = 1, height = 1 };
            enc.Printables("---------------------------------------\x0a");
            enc.Printables("PAID 50.00\x0a");
            enc.Printables("CHANGE 35.55\x0a");

            enc.GeneratePule(EscPosEncoder.DrawerKick.Pin2, 2, 20);
            enc.CutPaper(EscPosEncoder.CutType.FeedAndPartialCut, 0);
        }

        private static readonly EscPosEncoder.EscPosBitmap yourreceipt = new()
        {
            imageFormat = EscPosEncoder.ImageFormat.Rasters,
            width = 240,
            height = 40,
            monochrome = true,
            planeCount = 1,
            totalSize = 1201,
            planes = new EscPosEncoder.Plane[] {
                new EscPosEncoder.Plane {
                    color = (byte)'1',
                    data = new byte[] {
                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                        0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03,
                        0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03,
                        0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03,
                        0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03,
                        0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03,
                        0xC0, 0x18, 0x00, 0x60, 0x1F, 0xE0, 0x18, 0x00, 0x61, 0xFF, 0xF0, 0x00, 0x03, 0xFF, 0xE0, 0x3F, 0xFF, 0xC0, 0x3F, 0xC0, 0x3F, 0xFF, 0xC1, 0xF0, 0x7F, 0xFC, 0x07, 0xFF, 0xF8, 0x03,
                        0xC0, 0x18, 0x00, 0x60, 0x3F, 0xF0, 0x18, 0x00, 0x61, 0xFF, 0xF8, 0x00, 0x03, 0xFF, 0xF0, 0x3F, 0xFF, 0xC0, 0x7F, 0xE0, 0x3F, 0xFF, 0xC1, 0xF0, 0x7F, 0xFE, 0x07, 0xFF, 0xF8, 0x03,
                        0xC0, 0x0C, 0x00, 0xC0, 0x70, 0x38, 0x18, 0x00, 0x61, 0x80, 0x1C, 0x00, 0x03, 0x00, 0x38, 0x30, 0x00, 0x00, 0xE0, 0x70, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x07, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x0C, 0x00, 0xC0, 0x60, 0x18, 0x18, 0x00, 0x61, 0x80, 0x0E, 0x00, 0x03, 0x00, 0x1C, 0x30, 0x00, 0x00, 0xC0, 0x38, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x03, 0x80, 0x0C, 0x00, 0x03,
                        0xC0, 0x06, 0x01, 0x80, 0xE0, 0x1C, 0x18, 0x00, 0x61, 0x80, 0x06, 0x00, 0x03, 0x00, 0x0C, 0x30, 0x00, 0x01, 0xC0, 0x18, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x01, 0x80, 0x0C, 0x00, 0x03,
                        0xC0, 0x06, 0x01, 0x80, 0xC0, 0x0C, 0x18, 0x00, 0x61, 0x80, 0x06, 0x00, 0x03, 0x00, 0x0C, 0x30, 0x00, 0x01, 0x80, 0x0C, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x01, 0x80, 0x0C, 0x00, 0x03,
                        0xC0, 0x03, 0x03, 0x00, 0xC0, 0x0C, 0x18, 0x00, 0x61, 0x80, 0x06, 0x00, 0x03, 0x00, 0x0C, 0x30, 0x00, 0x03, 0x80, 0x0C, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x01, 0x80, 0x0C, 0x00, 0x03,
                        0xC0, 0x03, 0x03, 0x01, 0x80, 0x06, 0x18, 0x00, 0x61, 0x80, 0x06, 0x00, 0x03, 0x00, 0x0C, 0x30, 0x00, 0x03, 0x00, 0x00, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x01, 0x80, 0x0C, 0x00, 0x03,
                        0xC0, 0x01, 0x86, 0x01, 0x80, 0x06, 0x18, 0x00, 0x61, 0x80, 0x06, 0x00, 0x03, 0x00, 0x0C, 0x30, 0x00, 0x03, 0x00, 0x00, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x01, 0x80, 0x0C, 0x00, 0x03,
                        0xC0, 0x01, 0x86, 0x01, 0x80, 0x06, 0x18, 0x00, 0x61, 0x80, 0x0E, 0x00, 0x03, 0x00, 0x1C, 0x30, 0x00, 0x03, 0x00, 0x00, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x03, 0x80, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0xCC, 0x01, 0x80, 0x06, 0x18, 0x00, 0x61, 0x80, 0x1C, 0x00, 0x03, 0x00, 0x38, 0x30, 0x00, 0x03, 0x00, 0x00, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x07, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0xCC, 0x01, 0x80, 0x06, 0x18, 0x00, 0x61, 0xFF, 0xF8, 0x00, 0x03, 0xFF, 0xF0, 0x30, 0x00, 0x03, 0x00, 0x00, 0x30, 0x00, 0x00, 0xC0, 0x7F, 0xFE, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0x78, 0x01, 0x80, 0x06, 0x18, 0x00, 0x61, 0xFF, 0xF0, 0x00, 0x03, 0xFF, 0xE0, 0x3F, 0xFF, 0x83, 0x00, 0x00, 0x3F, 0xFF, 0x80, 0xC0, 0x7F, 0xFC, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0x78, 0x01, 0x80, 0x06, 0x18, 0x00, 0x61, 0x83, 0x00, 0x00, 0x03, 0x06, 0x00, 0x3F, 0xFF, 0x83, 0x00, 0x00, 0x3F, 0xFF, 0x80, 0xC0, 0x60, 0x00, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0x30, 0x01, 0x80, 0x06, 0x18, 0x00, 0x61, 0x81, 0x80, 0x00, 0x03, 0x03, 0x00, 0x30, 0x00, 0x03, 0x00, 0x00, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x00, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0x30, 0x01, 0x80, 0x06, 0x18, 0x00, 0x61, 0x81, 0x80, 0x00, 0x03, 0x03, 0x00, 0x30, 0x00, 0x03, 0x00, 0x00, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x00, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0x30, 0x01, 0x80, 0x06, 0x18, 0x00, 0x61, 0x80, 0xC0, 0x00, 0x03, 0x01, 0x80, 0x30, 0x00, 0x03, 0x00, 0x00, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x00, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0x30, 0x01, 0x80, 0x06, 0x18, 0x00, 0x61, 0x80, 0xC0, 0x00, 0x03, 0x01, 0x80, 0x30, 0x00, 0x03, 0x00, 0x00, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x00, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0x30, 0x01, 0x80, 0x06, 0x18, 0x00, 0x61, 0x80, 0x60, 0x00, 0x03, 0x00, 0xC0, 0x30, 0x00, 0x03, 0x00, 0x00, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x00, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0x30, 0x00, 0xC0, 0x0C, 0x18, 0x00, 0x61, 0x80, 0x60, 0x00, 0x03, 0x00, 0xC0, 0x30, 0x00, 0x01, 0x80, 0x0C, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x00, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0x30, 0x00, 0xC0, 0x0C, 0x1C, 0x00, 0xE1, 0x80, 0x30, 0x00, 0x03, 0x00, 0x60, 0x30, 0x00, 0x01, 0x80, 0x0C, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x00, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0x30, 0x00, 0xE0, 0x1C, 0x0C, 0x00, 0xC1, 0x80, 0x30, 0x00, 0x03, 0x00, 0x60, 0x30, 0x00, 0x01, 0xC0, 0x18, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x00, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0x30, 0x00, 0x60, 0x18, 0x0E, 0x01, 0xC1, 0x80, 0x18, 0x00, 0x03, 0x00, 0x30, 0x30, 0x00, 0x00, 0xC0, 0x38, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x00, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0x30, 0x00, 0x70, 0x38, 0x07, 0x03, 0x81, 0x80, 0x1C, 0x00, 0x03, 0x00, 0x38, 0x30, 0x00, 0x00, 0xE0, 0x70, 0x30, 0x00, 0x00, 0xC0, 0x60, 0x00, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0x30, 0x00, 0x3F, 0xF0, 0x03, 0xFF, 0x01, 0x80, 0x0E, 0x00, 0x03, 0x00, 0x1C, 0x3F, 0xFF, 0xC0, 0x7F, 0xE0, 0x3F, 0xFF, 0xC1, 0xF0, 0x60, 0x00, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0x30, 0x00, 0x1F, 0xE0, 0x01, 0xFE, 0x01, 0x80, 0x0E, 0x00, 0x03, 0x00, 0x1C, 0x3F, 0xFF, 0xC0, 0x3F, 0xC0, 0x3F, 0xFF, 0xC1, 0xF0, 0x60, 0x00, 0x00, 0x0C, 0x00, 0x03,
                        0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03,
                        0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03,
                        0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03,
                        0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03,
                        0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03,
                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
                    }
                }
            }
        };

        private static void ReceiptWithBarcode(ref EscPosEncoder enc)
        {
            enc.Initialize();
            enc.TabStops = new byte[] { 35 };
            enc.BasicPitch = new EscPosEncoder.MotionUnits() { horizontal = 180, vertical = 180 };

            //System.Drawing.Image image = System.Drawing.Image.FromFile("ReceiptLOGO.bmp");
            //EscPosEncoder.EscPosBitmap logo = (EscPosEncoder.EscPosBitmap)enc.ConvertEscPosBitmap(image, EscPosEncoder.ImageFormat.Rasters, 0, 0, EscPosEncoder.DithererType.None);
            //enc.DefineGraphics(EscPosEncoder.BufferType.DownloadGraphics, logo, true, 1, 1, (byte)'A', (byte)'1');

            using (MemoryStream ms = new(File.ReadAllBytes("SampleLogo.bmp")))
            using (Bitmap bitmap = new(ms))
            {
                EscPosEncoder.EscPosBitmap logo = EscPosEncoder.ConvertEscPosBitmap(bitmap, EscPosEncoder.ImageFormat.Rasters, 0, 0, EscPosEncoder.DithererType.None);
                enc.DefineGraphics(EscPosEncoder.BufferType.DownloadGraphics, logo, true, 1, 1, (byte)'A', (byte)'1');
            }

            //EscPosEncoder.EscPosBitmap data = new EscPosEncoder.EscPosBitmap();
            //data.imageFormat = EscPosEncoder.ImageFormat.WindowsBMP;
            //data.planeCount = 1;
            //data.planes = new EscPosEncoder.Plane[] { new EscPosEncoder.Plane() { color = 0x31, data = File.ReadAllBytes("ReceiptLOGO.bmp") } };
            //data.totalSize = data.planes[0].data.Length + 1;
            //using (MemoryStream ms = new MemoryStream(data.planes[0].data))
            //using (Bitmap bitmap = new Bitmap(ms))
            //{
            //    data.width = bitmap.Width;
            //    data.height = bitmap.Height;
            //    data.monochrome = (bitmap.PixelFormat == PixelFormat.Format1bppIndexed);
            //}
            //enc.DefineGraphics(EscPosEncoder.BufferType.DownloadGraphics, data, true, 1, 1, (byte)'A', (byte)'1');

            enc.Justification = EscPosEncoder.Alignment.Center;

            enc.PrintGraphics(EscPosEncoder.BufferType.DownloadGraphics, 1, 2, (byte)'A', (byte)'1');

            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 8);
            enc.Printables("Thank you\x0a");
            enc.Printables("NOVEMBER 1, 2012 15:00");
            enc.PrintAndFeed(EscPosEncoder.FeedType.Lines, 3);

            enc.Justification = EscPosEncoder.Alignment.Left;
            enc.Printables("TM-Hxxx\t 6.00\x0a");
            enc.Printables("PS-xxx\t 1.70\x0a\x0a");

            enc.CharScale = new EscPosEncoder.Scale() { width = 1, height = 2 };
            enc.Printables("TOTAL\t 7.70\x0a");
            enc.CharScale = new EscPosEncoder.Scale() { width = 1, height = 1 };
            enc.Printables("---------------------------------------\x0a");
            enc.Printables("PAID\t 10.00\x0a");
            enc.Printables("CHANGE\t 2.30\x0a");
            enc.PrintAndFeed(EscPosEncoder.FeedType.Lines, 3);

            enc.Justification = EscPosEncoder.Alignment.Center;
            enc.Printables("<< Bonus points : 14 >>");
            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 35);
            enc.D1BarcodeHeight = 50;
            enc.D1BarcodeText = EscPosEncoder.HumanReadableIndicator.Below;
            enc.D1BarcodeTextFontType = EscPosEncoder.FontType.B;
            enc.D1BarcodeData = new EscPosEncoder.BarcodeData() { barcodeType = EscPosEncoder.BarcodeType.CODE39, data = Encoding.ASCII.GetBytes("*00014*") };

            enc.CutPaper(EscPosEncoder.CutType.FeedAndPartialCut, 30);

            //==================

            enc.Initialize();
            enc.TabStops = new byte[] { 35 };
            enc.BasicPitch = new EscPosEncoder.MotionUnits() { horizontal = 180, vertical = 180 };

            enc.DefineGraphics(EscPosEncoder.BufferType.DownloadGraphics, yourreceipt, true, 1, 1, (byte)'A', (byte)'1');

            enc.Justification = EscPosEncoder.Alignment.Center;

            enc.PrintGraphics(EscPosEncoder.BufferType.DownloadGraphics, 1, 2, (byte)'A', (byte)'1');

            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 8);
            enc.Printables("Thank you\x0a");
            enc.Printables("NOVEMBER 1, 2012 15:00");
            enc.PrintAndFeed(EscPosEncoder.FeedType.Lines, 3);

            enc.Justification = EscPosEncoder.Alignment.Left;
            enc.Printables("TM-Hxxx\t 6.00\x0a");
            enc.Printables("PS-xxx\t 1.70\x0a\x0a");

            enc.CharScale = new EscPosEncoder.Scale() { width = 1, height = 2 };
            enc.Printables("TOTAL\t 7.70\x0a");
            enc.CharScale = new EscPosEncoder.Scale() { width = 1, height = 1 };
            enc.Printables("---------------------------------------\x0a");
            enc.Printables("PAID\t 10.00\x0a");
            enc.Printables("CHANGE\t 2.30\x0a");
            enc.PrintAndFeed(EscPosEncoder.FeedType.Lines, 3);

            enc.Justification = EscPosEncoder.Alignment.Center;
            enc.Printables("<< Bonus points : 14 >>");
            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 35);
            enc.D1BarcodeHeight = 50;
            enc.D1BarcodeText = EscPosEncoder.HumanReadableIndicator.Below;
            enc.D1BarcodeTextFontType = EscPosEncoder.FontType.B;
            enc.D1BarcodeData = new EscPosEncoder.BarcodeData() { barcodeType = EscPosEncoder.BarcodeType.CODE39, data = Encoding.ASCII.GetBytes("*00014*") };

            enc.CutPaper(EscPosEncoder.CutType.FeedAndPartialCut, 30);
        }

        private static readonly EscPosEncoder.EscPosBitmap borderpiece = new()
        {
            imageFormat = EscPosEncoder.ImageFormat.Column08dot,
            width = 25,
            height = 8,
            monochrome = true,
            planeCount = 1,
            totalSize = 26,
            planes = new EscPosEncoder.Plane[] {
                new EscPosEncoder.Plane {
                    color = (byte)'1',
                    data = new byte[] {
                        0x80,0x80,0x80,0x60,0x80,0x80,0x80,0x8C,0xA5,0x51,0x4E,0x20,0x1A,0x20,0x4E,0x51,0xA5,0x8C,0x80,0x80,0x80,0x60,0x80,0x80,0x80
                    }
                }
            }
        };

        private static readonly EscPosEncoder.EscPosBitmap xmastree1 = new()
        {
            imageFormat = EscPosEncoder.ImageFormat.Column08dot,
            width = 34,
            height = 8,
            monochrome = true,
            planeCount = 1,
            totalSize = 35,
            planes = new EscPosEncoder.Plane[] {
                new EscPosEncoder.Plane {
                    color = (byte)'1',
                    data = new byte[] {
                        0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,0x01,0x03,0x05,0x29,0x28,0x38,0x30,0x20,0xE0,0x20,0x31,0x38,0x28,0x29,0x05,0x03,0x01,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
                    }
                }
            }
        };

        private static readonly EscPosEncoder.EscPosBitmap xmastree2 = new()
        {
            imageFormat = EscPosEncoder.ImageFormat.Column08dot,
            width = 34,
            height = 8,
            monochrome = true,
            planeCount = 1,
            totalSize = 35,
            planes = new EscPosEncoder.Plane[] {
                new EscPosEncoder.Plane {
                    color = (byte)'1',
                    data = new byte[] {
                        0x00,0x00,0x00,0x00,0x00,0x08,0x18,0x19,0x2A,0x4C,0x8C,0x80,0x02,0x47,0xE2,0x40,0x00,0x80,0xC4,0x8E,0x04,0x80,0x8C,0x4C,0x2A,0x19,0x18,0x08,0x00,0x00,0x00,0x00,0x00,0x00
                    }
                }
            }
        };

        private static readonly EscPosEncoder.EscPosBitmap xmastree3 = new()
        {
            imageFormat = EscPosEncoder.ImageFormat.Column08dot,
            width = 34,
            height = 8,
            monochrome = true,
            planeCount = 1,
            totalSize = 35,
            planes = new EscPosEncoder.Plane[] {
                new EscPosEncoder.Plane {
                    color = (byte)'1',
                    data = new byte[] {
                        0x00,0x00,0x00,0x40,0x40,0xC0,0xC0,0x40,0x5C,0x54,0x57,0x55,0x55,0x75,0x75,0x55,0x55,0x55,0x75,0x75,0x55,0x55,0x57,0x54,0x5C,0x40,0xC0,0xC0,0x40,0x40,0x40,0x00,0x00,0x00
                    }
                }
            }
        };

        private static readonly EscPosEncoder.EscPosBitmap xmascake1 = new()
        {
            imageFormat = EscPosEncoder.ImageFormat.Column08dot,
            width = 48,
            height = 8,
            monochrome = true,
            planeCount = 1,
            totalSize = 49,
            planes = new EscPosEncoder.Plane[] {
                new EscPosEncoder.Plane {
                    color = (byte)'1',
                    data = new byte[] {
                        0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x07,0x07,0x09,0x09,0x7D,0x7C,0x10,0x10,0x16,0x16,0x10,0x70,0xF8,0xFC,0x7E,0x3E,0x3E,0x7E,0xFE,0xFC,0x70,0x10,0x16,0x16,0x10,0x10,0x7C,0x7D,0x09,0x09,0x07,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
                    }
                }
            }
        };

        private static readonly EscPosEncoder.EscPosBitmap xmascake2 = new()
        {
            imageFormat = EscPosEncoder.ImageFormat.Column08dot,
            width = 48,
            height = 8,
            monochrome = true,
            planeCount = 1,
            totalSize = 49,
            planes = new EscPosEncoder.Plane[] {
                new EscPosEncoder.Plane {
                    color = (byte)'1',
                    data = new byte[] {
                        0x1F,0x1F,0x24,0x21,0x40,0x42,0x42,0x41,0xF1,0xF1,0x29,0x59,0x59,0x8C,0x8C,0xD4,0xD4,0xA4,0xA4,0xD4,0xD4,0x8C,0x8C,0xD4,0xD4,0xAC,0xAC,0xC4,0xC4,0xAC,0xAC,0x94,0x94,0xAC,0xAC,0x49,0x49,0x29,0xF1,0xF1,0x41,0x42,0x42,0x40,0x23,0x24,0x1F,0x1F
                    }
                }
            }
        };

        private static readonly EscPosEncoder.EscPosBitmap xmascake3 = new()
        {
            imageFormat = EscPosEncoder.ImageFormat.Column08dot,
            width = 48,
            height = 8,
            monochrome = true,
            planeCount = 1,
            totalSize = 49,
            planes = new EscPosEncoder.Plane[] {
                new EscPosEncoder.Plane {
                    color = (byte)'1',
                    data = new byte[] {
                        0xF0,0xF0,0x4C,0x4C,0x24,0x24,0x24,0x12,0x12,0x12,0x12,0x12,0x12,0x89,0xA9,0xA9,0x89,0x89,0x99,0x99,0x89,0x80,0x89,0xC9,0xC9,0x89,0x99,0x99,0x89,0xC9,0xC9,0x89,0xA9,0xA9,0x89,0x12,0x12,0x12,0x12,0x12,0x12,0x24,0x24,0x24,0x4C,0x4C,0xF0,0xF0
                    }
                }
            }
        };

        private static void PagemodePrint(ref EscPosEncoder enc)
        {
            enc.Initialize();
            enc.PageMode = true;
            enc.BasicPitch = new EscPosEncoder.MotionUnits() { horizontal = 203, vertical = 203 };

            enc.PageModeArea = new System.Drawing.Rectangle { X = 6, Y = 0, Width = 500, Height = 750 };

            enc.PageDirection = EscPosEncoder.Direction.LeftToRight;
            for (int i = 0; i < 10; i++)
            {
                enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, borderpiece, false);
            }

            enc.PageDirection = EscPosEncoder.Direction.BottomToTop;
            for (int i = 0; i < 15; i++)
            {
                enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, borderpiece, false);
            }

            enc.PageDirection = EscPosEncoder.Direction.RightToLeft;
            for (int i = 0; i < 10; i++)
            {
                enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, borderpiece, false);
            }

            enc.PageDirection = EscPosEncoder.Direction.TopToBottom;
            for (int i = 0; i < 15; i++)
            {
                enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, borderpiece, false);
            }

            enc.PageModeArea = new System.Drawing.Rectangle { X = 140, Y = 118, Width = 360, Height = 528 };
            enc.PageDirection = EscPosEncoder.Direction.BottomToTop;

            enc.CharScale = new EscPosEncoder.Scale() { width = 2, height = 2 };
            enc.Printables("\x0a Merry Christmas !!\x0a\x0a");

            enc.CharScale = new EscPosEncoder.Scale() { width = 2, height = 1 };
            enc.Printables(" 10% OFF COUPON");

            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 200);
            enc.Printables("ESC/POS DEPARTMENT STORE");

            enc.PageModeArea = new System.Drawing.Rectangle { X = 50, Y = 588, Width = 72, Height = 68 };
            enc.PageDirection = EscPosEncoder.Direction.BottomToTop;
            enc.LineSpacing = 24;

            enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, xmastree1, false);
            enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, xmastree2, false);
            enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, xmastree3, false);

            enc.PageModeArea = new System.Drawing.Rectangle { X = 314, Y = 102, Width = 72, Height = 96 };
            enc.PageDirection = EscPosEncoder.Direction.BottomToTop;

            enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, xmascake1, false);
            enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, xmascake2, false);
            enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, xmascake3, false);

            enc.PageModeArea = new System.Drawing.Rectangle { X = 276, Y = 226, Width = 70, Height = 312 };
            enc.PageDirection = EscPosEncoder.Direction.BottomToTop;

            enc.D1BarcodeText = EscPosEncoder.HumanReadableIndicator.Below;
            enc.D1BarcodeTextFontType = EscPosEncoder.FontType.B;
            enc.D1BarcodeHeight = 40;
            enc.D1BarcodeModuleWidth = 2;
            enc.MovePrintPosition(EscPosEncoder.AxisType.Vertical, EscPosEncoder.ValueSpecifyType.Absolute, 40);
            enc.D1BarcodeData = new EscPosEncoder.BarcodeData() { barcodeType = EscPosEncoder.BarcodeType.CODE39, data = Encoding.ASCII.GetBytes("*10% OFF*") };

            enc.PageModeData(EscPosEncoder.PageAction.PrintAndContinue);
            enc.CutPaper(EscPosEncoder.CutType.FeedAndPartialCut, 80);
            enc.PageMode = false;

            //==================

            enc.Initialize();
            enc.PageMode = true;
            enc.BasicPitch = new EscPosEncoder.MotionUnits() { horizontal = 203, vertical = 203 };

            enc.PageModeArea = new System.Drawing.Rectangle { X = 6, Y = 0, Width = 500, Height = 750 };

            enc.PageDirection = EscPosEncoder.Direction.LeftToRight;
            for (int i = 0; i < 10; i++)
            {
                enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, borderpiece, false);
            }

            enc.PageDirection = EscPosEncoder.Direction.BottomToTop;
            for (int i = 0; i < 15; i++)
            {
                enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, borderpiece, false);
            }

            enc.PageDirection = EscPosEncoder.Direction.RightToLeft;
            for (int i = 0; i < 10; i++)
            {
                enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, borderpiece, false);
            }

            enc.PageDirection = EscPosEncoder.Direction.TopToBottom;
            for (int i = 0; i < 15; i++)
            {
                enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, borderpiece, false);
            }

            enc.PageModeArea = new System.Drawing.Rectangle { X = 140, Y = 118, Width = 360, Height = 528 };
            enc.PageDirection = EscPosEncoder.Direction.BottomToTop;

            enc.CharScale = new EscPosEncoder.Scale() { width = 2, height = 2 };
            enc.Printables("\x0a Merry Christmas !!\x0a\x0a");

            enc.CharScale = new EscPosEncoder.Scale() { width = 2, height = 1 };
            enc.Printables(" 10% OFF COUPON");

            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 200);
            enc.Printables("ESC/POS DEPARTMENT STORE");

            enc.PageModeArea = new System.Drawing.Rectangle { X = 50, Y = 588, Width = 72, Height = 68 };
            enc.PageDirection = EscPosEncoder.Direction.BottomToTop;
            enc.LineSpacing = 24;

            enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, xmastree1, false);
            enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, xmastree2, false);
            enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, xmastree3, false);

            enc.PageModeArea = new System.Drawing.Rectangle { X = 314, Y = 102, Width = 72, Height = 96 };
            enc.PageDirection = EscPosEncoder.Direction.BottomToTop;

            enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, xmascake1, false);
            enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, xmascake2, false);
            enc.DefineGraphics(EscPosEncoder.BufferType.Immediate, xmascake3, false);

            enc.PageModeArea = new System.Drawing.Rectangle { X = 276, Y = 226, Width = 70, Height = 312 };
            enc.PageDirection = EscPosEncoder.Direction.BottomToTop;

            enc.D1BarcodeText = EscPosEncoder.HumanReadableIndicator.Below;
            enc.D1BarcodeTextFontType = EscPosEncoder.FontType.B;
            enc.D1BarcodeHeight = 40;
            enc.D1BarcodeModuleWidth = 2;
            enc.MovePrintPosition(EscPosEncoder.AxisType.Vertical, EscPosEncoder.ValueSpecifyType.Absolute, 40);
            enc.D1BarcodeData = new EscPosEncoder.BarcodeData() { barcodeType = EscPosEncoder.BarcodeType.CODE39, data = Encoding.ASCII.GetBytes("*10% OFF*") };

            enc.PageModeData(EscPosEncoder.PageAction.PrintAndContinue);
            enc.CutPaper(EscPosEncoder.CutType.FeedAndPartialCut, 80);
            enc.PageMode = false;
        }

        private static void Label(ref EscPosEncoder enc)
        {
            enc.UserSettingMode = true;
            enc.SetCustomizeSettingValues(newvalues: new List<EscPosEncoder.CustomizeValue>() { new EscPosEncoder.CustomizeValue() { number = 117, value = 60 } });
            enc.SetPaperLayout(EscPosEncoder.LayoutBasePoint.TopOfBlackmark, "305", "50", "70", "25", "215", "50", "495");
            enc.UserSettingMode = false;

            enc.Justification = EscPosEncoder.Alignment.Center;
            enc.SBCSFontType = EscPosEncoder.FontType.A;
            enc.Emphasis = true;
            enc.CharScale = new EscPosEncoder.Scale() { height = 1, width = 2 };
            enc.Printables("ESC/POS");
            enc.CharScale = new EscPosEncoder.Scale() { height = 1, width = 1 };
            enc.Printables(" Rental Video\x0a");
            enc.Emphasis = false;

            enc.D1BarcodeText = EscPosEncoder.HumanReadableIndicator.Below;
            enc.D1BarcodeTextFontType = EscPosEncoder.FontType.B;
            enc.D1BarcodeHeight = 35;
            enc.D1BarcodeData = new EscPosEncoder.BarcodeData() { barcodeType = EscPosEncoder.BarcodeType.CODE39, data = Encoding.ASCII.GetBytes("*00061*") };
            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 40);

            enc.SBCSUnderline = EscPosEncoder.Underline.On1dot;
            enc.Printables("NAME ");
            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 70);
            enc.SBCSUnderline = EscPosEncoder.Underline.Off;

            enc.Justification = EscPosEncoder.Alignment.Right;
            enc.SBCSFontType = EscPosEncoder.FontType.B;
            enc.Printables("NOV. 1, 2012\x0a");
            enc.FeedPaper(EscPosEncoder.TargetPosition.LabelPeeling, EscPosEncoder.MovingRule.ForceNext);

            enc.FeedPaper(EscPosEncoder.TargetPosition.PrintStarting, EscPosEncoder.MovingRule.CurrentTop);

            enc.Justification = EscPosEncoder.Alignment.Center;
            enc.SBCSFontType = EscPosEncoder.FontType.A;
            enc.Emphasis = true;
            enc.CharScale = new EscPosEncoder.Scale() { height = 1, width = 2 };
            enc.Printables("ESC/POS");
            enc.CharScale = new EscPosEncoder.Scale() { height = 1, width = 1 };
            enc.Printables(" Rental Video\x0a");
            enc.Emphasis = false;

            enc.D1BarcodeText = EscPosEncoder.HumanReadableIndicator.Below;
            enc.D1BarcodeTextFontType = EscPosEncoder.FontType.B;
            enc.D1BarcodeHeight = 35;
            enc.D1BarcodeData = new EscPosEncoder.BarcodeData() { barcodeType = EscPosEncoder.BarcodeType.CODE39, data = Encoding.ASCII.GetBytes("*00062*") };
            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 40);

            enc.SBCSUnderline = EscPosEncoder.Underline.On1dot;
            enc.Printables("NAME ");
            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 70);
            enc.SBCSUnderline = EscPosEncoder.Underline.Off;

            enc.Justification = EscPosEncoder.Alignment.Right;
            enc.SBCSFontType = EscPosEncoder.FontType.B;
            enc.Printables("NOV. 1, 2012\x0a");
            enc.FeedPaper(EscPosEncoder.TargetPosition.LabelPeeling, EscPosEncoder.MovingRule.ForceNext);

            enc.FeedPaper(EscPosEncoder.TargetPosition.PrintStarting, EscPosEncoder.MovingRule.CurrentTop);

            //==================

            enc.UserSettingMode = true;
            enc.SetCustomizeSettingValues(newvalues: new List<EscPosEncoder.CustomizeValue>() { new EscPosEncoder.CustomizeValue() { number = 117, value = 60 } });
            enc.SetPaperLayout(EscPosEncoder.LayoutBasePoint.TopOfBlackmark, "305", "50", "70", "25", "215", "50", "495");
            enc.UserSettingMode = false;

            enc.Justification = EscPosEncoder.Alignment.Center;
            enc.SBCSFontType = EscPosEncoder.FontType.A;
            enc.Emphasis = true;
            enc.CharScale = new EscPosEncoder.Scale() { height = 1, width = 2 };
            enc.Printables("ESC/POS");
            enc.CharScale = new EscPosEncoder.Scale() { height = 1, width = 1 };
            enc.Printables(" Rental Video\x0a");
            enc.Emphasis = false;

            enc.D1BarcodeText = EscPosEncoder.HumanReadableIndicator.Below;
            enc.D1BarcodeTextFontType = EscPosEncoder.FontType.B;
            enc.D1BarcodeHeight = 35;
            enc.D1BarcodeData = new EscPosEncoder.BarcodeData() { barcodeType = EscPosEncoder.BarcodeType.CODE39, data = Encoding.ASCII.GetBytes("*00061*") };
            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 40);

            enc.SBCSUnderline = EscPosEncoder.Underline.On1dot;
            enc.Printables("NAME ");
            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 70);
            enc.SBCSUnderline = EscPosEncoder.Underline.Off;

            enc.Justification = EscPosEncoder.Alignment.Right;
            enc.SBCSFontType = EscPosEncoder.FontType.B;
            enc.Printables("NOV. 1, 2012\x0a");
            enc.FeedPaper(EscPosEncoder.TargetPosition.LabelPeeling, EscPosEncoder.MovingRule.ForceNext);

            enc.FeedPaper(EscPosEncoder.TargetPosition.PrintStarting, EscPosEncoder.MovingRule.CurrentTop);

            enc.Justification = EscPosEncoder.Alignment.Center;
            enc.SBCSFontType = EscPosEncoder.FontType.A;
            enc.Emphasis = true;
            enc.CharScale = new EscPosEncoder.Scale() { height = 1, width = 2 };
            enc.Printables("ESC/POS");
            enc.CharScale = new EscPosEncoder.Scale() { height = 1, width = 1 };
            enc.Printables(" Rental Video\x0a");
            enc.Emphasis = false;

            enc.D1BarcodeText = EscPosEncoder.HumanReadableIndicator.Below;
            enc.D1BarcodeTextFontType = EscPosEncoder.FontType.B;
            enc.D1BarcodeHeight = 35;
            enc.D1BarcodeData = new EscPosEncoder.BarcodeData() { barcodeType = EscPosEncoder.BarcodeType.CODE39, data = Encoding.ASCII.GetBytes("*00062*") };
            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 40);

            enc.SBCSUnderline = EscPosEncoder.Underline.On1dot;
            enc.Printables("NAME ");
            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 70);
            enc.SBCSUnderline = EscPosEncoder.Underline.Off;

            enc.Justification = EscPosEncoder.Alignment.Right;
            enc.SBCSFontType = EscPosEncoder.FontType.B;
            enc.Printables("NOV. 1, 2012\x0a");
            enc.FeedPaper(EscPosEncoder.TargetPosition.LabelPeeling, EscPosEncoder.MovingRule.ForceNext);

            enc.FeedPaper(EscPosEncoder.TargetPosition.PrintStarting, EscPosEncoder.MovingRule.CurrentTop);
        }

        private static void PaperCut(ref EscPosEncoder enc)
        {
            enc.Initialize();

            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 16);
            enc.Printables("NOV-1-2012 13:00\x0a");
            enc.Printables("Table No.6\x0a");
            enc.Printables("TM-Uxxx ..... No.1\x0a");
            enc.CutPaper(EscPosEncoder.CutType.FeedAndPartialCut, 20);

            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 16);
            enc.Printables("NOV-1-2012 13:00\x0a");
            enc.Printables("Table No.6\x0a");
            enc.Printables("TM-Uxxx ..... No.2\x0a");
            enc.CutPaper(EscPosEncoder.CutType.FeedAndPartialCut, 20);

            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 16);
            enc.Printables("NOV-1-2012 13:00\x0a");
            enc.Printables("Table No.6\x0a");
            enc.Printables("TM-Uxxx ..... No.3\x0a");
            enc.CutPaper(EscPosEncoder.CutType.FeedAndPartialCut, 20);

            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 16);

            //==================

            enc.Initialize();

            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 16);
            enc.Printables("NOV-1-2012 13:00\x0a");
            enc.Printables("Table No.6\x0a");
            enc.Printables("TM-Uxxx ..... No.1\x0a");
            enc.CutPaper(EscPosEncoder.CutType.FeedAndPartialCut, 20);

            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 16);
            enc.Printables("NOV-1-2012 13:00\x0a");
            enc.Printables("Table No.6\x0a");
            enc.Printables("TM-Uxxx ..... No.2\x0a");
            enc.CutPaper(EscPosEncoder.CutType.FeedAndPartialCut, 20);

            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 16);
            enc.Printables("NOV-1-2012 13:00\x0a");
            enc.Printables("Table No.6\x0a");
            enc.Printables("TM-Uxxx ..... No.3\x0a");
            enc.CutPaper(EscPosEncoder.CutType.FeedAndPartialCut, 20);

            enc.PrintAndFeed(EscPosEncoder.FeedType.Dots, 16);
        }

        private static readonly EscPosEncoder.EscPosBitmap stampmark = new()
        {
            imageFormat = EscPosEncoder.ImageFormat.Rasters,
            width = 128,
            height = 120,
            monochrome = true,
            planeCount = 1,
            totalSize = 1921,
            planes = new EscPosEncoder.Plane[] {
                new EscPosEncoder.Plane {
                    color = (byte)'1',
                    data = new byte[] {
                        0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
                        0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x0C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x3C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0xFE,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x81,0xFE,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x03,0xF8,0x00,0x00,0x00,0x01,
                        0x83,0xFF,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x87,0x80,0x00,0x00,0x01,
                        0x87,0xFD,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x3F,0x00,0x78,0x00,0x00,0x01,
                        0x87,0xF1,0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x3E,0x00,0x1F,0x00,0x00,0x01,
                        0x83,0xE7,0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x5C,0x00,0x0F,0xC0,0x00,0x01,
                        0x80,0x4F,0xC0,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x4E,0x00,0x07,0xE0,0x00,0x01,
                        0x80,0x7F,0xE0,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x4F,0x00,0x01,0xF0,0x00,0x01,
                        0x80,0x3F,0xF0,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x87,0x80,0x01,0xF8,0x00,0x01,
                        0x80,0x3F,0xF0,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x83,0x80,0x00,0xFC,0x00,0x01,
                        0x80,0x1F,0xC8,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x81,0xC0,0x00,0x7E,0x00,0x01,
                        0x80,0x1F,0x98,0x00,0x00,0x00,0x00,0x00,0x00,0x01,0x00,0xC0,0x00,0x3F,0x00,0x01,
                        0x80,0x0E,0x3C,0x00,0x00,0x00,0x00,0x00,0x00,0x01,0x00,0x60,0x00,0x3F,0x00,0x01,
                        0x80,0x0C,0xFC,0x00,0x00,0x00,0x00,0x00,0x00,0x01,0x00,0x10,0x00,0x1F,0x80,0x01,
                        0x80,0x05,0xFE,0x00,0x00,0x00,0x00,0x00,0x00,0x02,0x00,0x0C,0x00,0x1F,0x80,0x01,
                        0x80,0x07,0xFF,0x00,0x00,0x00,0x00,0x00,0x00,0x02,0x00,0x03,0xFF,0xFF,0x80,0x01,
                        0x80,0x03,0xFF,0x00,0x00,0x00,0x00,0x00,0x00,0x02,0x00,0x00,0x3F,0xFF,0x80,0x01,
                        0x80,0x03,0xFF,0x80,0x00,0x00,0x00,0x00,0x00,0x06,0x00,0x00,0x01,0xFF,0x80,0x01,
                        0x80,0x01,0xFF,0x80,0x00,0x00,0x00,0x00,0x00,0x07,0xFF,0x80,0x00,0x1F,0x80,0x01,
                        0x80,0x01,0xFF,0xC0,0x00,0x00,0x00,0x00,0x00,0x07,0x80,0x7F,0x00,0x01,0x00,0x01,
                        0x80,0x00,0xFF,0xE0,0x00,0x00,0x00,0x00,0x00,0x0F,0xC0,0x00,0xFF,0x01,0x00,0x01,
                        0x80,0x00,0xFF,0xE0,0x00,0x00,0x00,0x00,0x00,0x0F,0xE0,0x00,0x00,0xFA,0x00,0x01,
                        0x80,0x00,0x7F,0xF0,0x00,0x00,0x00,0x00,0x00,0x0F,0xF8,0x00,0x00,0x06,0x00,0x01,
                        0x80,0x00,0x7F,0xF8,0x00,0x00,0x00,0x00,0x00,0x1F,0xFF,0xC0,0x00,0x1C,0x00,0x01,
                        0x80,0x00,0x3F,0xF8,0x00,0x00,0x00,0x00,0x00,0x1F,0xFF,0xFF,0xFF,0xFC,0x00,0x01,
                        0x80,0x00,0x3F,0xFC,0x00,0x00,0x00,0x00,0x00,0x1F,0xFF,0xFF,0xFF,0xF8,0x00,0x01,
                        0x80,0x00,0x1F,0xFE,0x00,0x00,0x00,0x00,0x00,0x1F,0xFF,0xFF,0xFF,0xF8,0x00,0x01,
                        0x80,0x00,0x1F,0xFF,0x80,0x00,0x00,0x00,0x00,0x1F,0xFF,0xFF,0xFF,0xF0,0x00,0x01,
                        0x80,0x00,0x0F,0xFF,0xF0,0x00,0x00,0x00,0x00,0x1F,0xFF,0xFF,0xFF,0xE0,0x00,0x01,
                        0x80,0x00,0x0F,0xFF,0xFE,0x00,0x00,0x00,0x00,0x1F,0xFF,0xFF,0xFF,0xC0,0x00,0x01,
                        0x80,0x00,0x07,0xFF,0xFF,0x80,0x00,0x00,0x00,0x0F,0xFF,0xFF,0xFF,0x80,0x00,0x01,
                        0x80,0x00,0x03,0xFF,0xFF,0xE0,0x00,0x00,0x00,0x0F,0xFF,0xFF,0xFF,0x00,0x00,0x01,
                        0x80,0x00,0x03,0xFF,0xFF,0xF0,0x00,0x00,0x00,0x0F,0xFF,0xFF,0xFE,0x00,0x00,0x01,
                        0x80,0x00,0x07,0xFF,0xFF,0xFC,0x00,0x00,0x00,0x0F,0xFF,0xFF,0xFC,0x00,0x00,0x01,
                        0x80,0x00,0x07,0xFF,0xFF,0xFC,0x00,0x00,0x00,0x07,0xFF,0xFF,0xF8,0x00,0x00,0x01,
                        0x80,0x00,0x0F,0xFF,0xFF,0xFE,0x00,0x00,0x00,0x07,0xFF,0xFF,0xF0,0x00,0x00,0x01,
                        0x80,0x00,0x0F,0xFF,0xFF,0xFE,0x00,0x00,0x00,0x07,0xFF,0xFF,0xE0,0x00,0x00,0x01,
                        0x80,0x00,0x1F,0xFF,0xFF,0xFF,0x00,0x00,0x00,0x03,0xFF,0xFF,0xC0,0x00,0x00,0x01,
                        0x80,0x00,0x1F,0xFF,0xFF,0xFF,0x00,0x00,0x00,0x03,0xFF,0xFF,0x80,0x00,0x00,0x01,
                        0x80,0x00,0x1F,0xFF,0xFF,0xFE,0x80,0x00,0x00,0x03,0xFF,0xFF,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x1F,0xFF,0xFF,0xFC,0x80,0x00,0x00,0x03,0xFF,0xFE,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x1F,0xFF,0xFF,0xF8,0xC0,0x00,0x00,0x03,0xFF,0xFC,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x1F,0xFF,0xFF,0xF1,0x40,0x00,0x00,0x03,0xFF,0xE0,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x1F,0xFF,0xFF,0xE3,0x20,0x00,0x00,0x07,0x80,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x1F,0xFF,0xFF,0xC3,0x20,0x00,0x00,0x07,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x1F,0xFF,0xFF,0x02,0x10,0x00,0x00,0x0F,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x1F,0xFF,0xFC,0x06,0x10,0x00,0x00,0x0E,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x1F,0xFF,0xE0,0x04,0x08,0x00,0x00,0x0E,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x0F,0xFF,0x00,0x0C,0x08,0x00,0x00,0x1C,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x08,0x00,0x00,0x1C,0x04,0x00,0x00,0x38,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x08,0x00,0x00,0x38,0x04,0x00,0x00,0x38,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x04,0x00,0x00,0xF0,0x02,0x00,0x00,0x70,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x04,0x00,0x01,0xE0,0x02,0x00,0x00,0xE0,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x04,0x00,0x07,0xC0,0x01,0x00,0x00,0xE0,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x02,0x00,0x1F,0x80,0x01,0x00,0x01,0xC0,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x02,0x00,0xFE,0x00,0x00,0x80,0x01,0xC0,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x01,0x03,0xFC,0x00,0x00,0x80,0x03,0x80,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x01,0x0F,0xF0,0x00,0x00,0x58,0x03,0x80,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0xFF,0xC0,0x00,0x00,0x67,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0xFF,0x00,0x00,0x00,0x63,0xC7,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x7C,0x00,0x00,0x00,0x73,0xFE,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x78,0x00,0x00,0x00,0x73,0xFE,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x40,0x00,0x00,0x00,0x3B,0xFE,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x20,0x00,0x00,0x00,0x3B,0xFF,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x20,0x00,0x00,0x00,0x1B,0xFF,0x80,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x20,0x00,0x00,0x00,0x1F,0xFF,0x80,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x10,0x00,0x00,0x00,0x0F,0xFF,0xC0,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x10,0x00,0x00,0x00,0x0F,0xFF,0xE0,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x08,0x00,0x00,0x00,0x07,0xFF,0xE0,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x08,0x00,0x00,0x00,0x03,0xFF,0xE0,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x04,0x00,0x00,0x00,0x01,0xFF,0xC0,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x04,0x00,0x00,0x00,0x00,0x9C,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x02,0x00,0x00,0x00,0x00,0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x02,0x00,0x00,0x00,0x00,0x40,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x01,0x00,0x00,0x00,0x00,0x40,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x01,0x00,0x00,0x00,0x00,0x40,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x80,0x00,0x00,0x00,0x20,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x80,0x00,0x00,0x00,0x20,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x40,0x00,0x00,0x00,0x70,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x40,0x00,0x00,0x00,0xE0,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x20,0x00,0x00,0x01,0xF8,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x20,0x00,0x00,0x03,0xF8,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x20,0x00,0x00,0x0F,0xFC,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x10,0x00,0x00,0x3F,0xFC,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x10,0x00,0x03,0xFF,0xFC,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x08,0x00,0x3F,0xFF,0xFC,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x08,0x03,0xFF,0xFF,0xFC,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x04,0x3F,0xFF,0xFF,0xFC,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x07,0xFF,0xFF,0xFF,0xF8,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x03,0xFF,0xFF,0xFF,0xF8,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x03,0xFF,0xFF,0xFF,0xF0,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x01,0xFF,0xFF,0xFF,0xE0,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x01,0xFF,0xFF,0xFF,0xC0,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0xFF,0xFF,0xFF,0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0xFF,0xFF,0xFF,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x7F,0xFF,0xFE,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x7F,0xFF,0xF8,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x3F,0xFF,0xE0,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x1F,0xFF,0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x0F,0xFE,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x07,0xF0,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,
                        0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF
                    }
                }
            }
        };

        private static void NVGraphics(ref EscPosEncoder enc)
        {
            enc.Initialize();

            //System.Drawing.Image image = System.Drawing.Image.FromFile("RevStamp.bmp");
            //EscPosEncoder.EscPosBitmap RevStamp = (EscPosEncoder.EscPosBitmap)enc.ConvertEscPosBitmap(image, EscPosEncoder.ImageFormat.Rasters, 0, 0, EscPosEncoder.DithererType.None);
            //enc.DefineGraphics(EscPosEncoder.BufferType.NVGraphics, RevStamp, true, 1, 1, (byte)'G', (byte)'1');
            EscPosEncoder.EscPosBitmap data = new();
            data.imageFormat = EscPosEncoder.ImageFormat.WindowsBMP;
            data.planeCount = 1;
            data.planes = new EscPosEncoder.Plane[] { new EscPosEncoder.Plane() { color = 0x31, data = File.ReadAllBytes("StampMark.bmp") } };
            data.totalSize = data.planes[0].data.Length + 1;
            using (MemoryStream ms = new(data.planes[0].data))
            using (Bitmap bitmap = new(ms))
            {
                data.width = bitmap.Width;
                data.height = bitmap.Height;
                data.monochrome = (bitmap.PixelFormat == PixelFormat.Format1bppIndexed);
            }
            enc.DefineGraphics(EscPosEncoder.BufferType.NVGraphics, data, true, 1, 1, (byte)'G', (byte)'1');

            enc.PrintGraphics(EscPosEncoder.BufferType.NVGraphics, 1, 1, (byte)'G', (byte)'1');

            enc.CutPaper(EscPosEncoder.CutType.FeedAndPartialCut, 0);

            //==================

            enc.Initialize();

            using (MemoryStream ms = new(File.ReadAllBytes("StampMark.bmp")))
            using (Bitmap bitmap = new(ms))
            {
                EscPosEncoder.EscPosBitmap stamp = EscPosEncoder.ConvertEscPosBitmap(bitmap, EscPosEncoder.ImageFormat.Rasters, 0, 0, EscPosEncoder.DithererType.None);
                enc.DefineGraphics(EscPosEncoder.BufferType.NVGraphics, stamp, true, 1, 1, (byte)'G', (byte)'1');
            }

            enc.PrintGraphics(EscPosEncoder.BufferType.NVGraphics, 1, 1, (byte)'G', (byte)'1');

            enc.CutPaper(EscPosEncoder.CutType.FeedAndPartialCut, 0);
        }

        //private static byte[] ReadFile(string filePath)
        //{
        //    byte[] buffer = Array.Empty<byte>();
        //    try
        //    {
        //        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
        //        buffer = new byte[fs.Length];
        //        fs.Read(buffer, 0, buffer.Length);
        //    }
        //    catch { }
        //    return buffer;
        //}

        private static void WriteFile(string filePath, byte[] data, bool append = true)
        {
            try
            {
                FileMode mode = append ? FileMode.Append : File.Exists(filePath) ? FileMode.Truncate : FileMode.Create;
                FileStream ostrm = new(filePath, mode, FileAccess.Write);
                ostrm.Write(data, 0, data.Length);
            }
            catch { }
            return;
        }

        private static void HelpGeneral()
        {
            Console.WriteLine("Usage: EscPosEncode  [Options]");
            //Console.WriteLine("  InputFilePath :  specify input ESC/POS binary file path. required.");
            Console.WriteLine("  -H :  display this usage message.");
            Console.WriteLine("  -F :  display supported font size pattern detail message.");
            Console.WriteLine("  -L :  display supported CodePage and International character set type list message.");
            Console.WriteLine("  -D {Printer | LineDisplay} :  specify initial device type. default is Printer.");
            Console.WriteLine("  -O OutputFilePath :  specify output tokenized (and decoded) text file path. default is STDOUT.");
            //Console.WriteLine("  -T :  specify Tokenize only. default are both tokenize and decode.");
            Console.WriteLine("  -C CodePage :  specify initial CodePage number. default system setting of Language for non-Unicode programs.");
            Console.WriteLine("  -I International character set type :  specify initial International character set type. default 0");
            //Console.WriteLine("  -K {ON | OFF} :  specify initial kanji on/off mode. default/specified codepage sbcs off/mbcs on.");
            //Console.WriteLine("  -G GraphicsFolderPath :  specify decoded graphics&font output folder path. default&tokenize only are not output.");
            Console.WriteLine("  -S FontPattern :  specify SBCS supported font size pattern 1 to 9. default is 1.");
            Console.WriteLine("  -M FontPattern :  specify CJK MBCS supported font size pattern 1 to 5. default is 1.");
            Console.WriteLine("  -V FontPattern :  specify LineDisplay supported font size pattern 1 or 2. default is 1.");
        }

        private static void HelpFontPattern()
        {
            Console.WriteLine("Supported Font size pattern:");
            Console.WriteLine("SBCS(Single Byte Character Sequence)");
            Console.WriteLine("Pattern   1            2            3            4            5");
            Console.WriteLine("Font Width Height Width Height Width Height Width Height Width Height");
            Console.WriteLine("  A    12 x 24      12 x 24      12 x 24      12 x 24      12 x 24");
            Console.WriteLine("  B    10 x 24      10 x 24      10 x 24       8 x 16       9 x 17");
            Console.WriteLine("  C     8 x 16       8 x 16       9 x 17");
            Console.WriteLine("sp.A                24 x 48");
            Console.WriteLine("");
            Console.WriteLine("Pattern   6            7            8            9");
            Console.WriteLine("Font Width Height Width Height Width Height Width Height");
            Console.WriteLine("  A    12 x 24      12 x 24      12 x 24       9 x 9");
            Console.WriteLine("  B     9 x 24       9 x 24       9 x 24       7 x 9");
            Console.WriteLine("  C     9 x 17       9 x 17");
            Console.WriteLine("  D    10 x 24      10 x 24");
            Console.WriteLine("  E     8 x 16       8 x 16");
            Console.WriteLine("sp.A                12 x 24      12 x 24");
            Console.WriteLine("sp.B                 9 x 24       9 x 24");
            Console.WriteLine("");
            Console.WriteLine("CJK MBCS(Multi Byte Character Sequence)");
            Console.WriteLine("Pattern   1            2            3            4            5");
            Console.WriteLine("Font Width Height Width Height Width Height Width Height Width Height");
            Console.WriteLine("  A    24 x 24      24 x 24      24 x 24      24 x 24      16 x 16");
            Console.WriteLine("  B    20 x 24      20 x 24      16 x 16");
            Console.WriteLine("  C    16 x 16");
            Console.WriteLine("");
            Console.WriteLine("LineDisplay");
            Console.WriteLine("Pattern   1            2");
            Console.WriteLine("     Width Height Width Height");
            Console.WriteLine("        8 x 16       5 x 7");
        }

        private static void HelpCodePage()
        {
            Console.WriteLine("Supported CodePgae list:");
            Console.WriteLine("System or bundled package supported");
            Console.WriteLine("  437: USA, Standard Europe  720: Arabic                737: Greek                 775: Baltic Rim");
            Console.WriteLine("  850: Multilingual          852: Latin 2               855: Cyrillic              857: Turkish");
            Console.WriteLine("  858: Euro                  860: Portuguese            861: Icelandic             862: Hebrew");
            Console.WriteLine("  863: Canadian-French       864: Arabic                865: Nordic                866: Cyrillic #2");
            Console.WriteLine("  869: Greek                 932: Japanese              1250: Latin 2              1251: Cyrillic");
            Console.WriteLine("  1252: Windows              1253: Greek                1254: Turkish              1255: Hebrew");
            Console.WriteLine("  1256: Arabic               1257: Baltic Rim           1258: Vietnamese           28592: ISO8859-2 Latin 2");
            Console.WriteLine("  28597: ISO8859-7 Greek     28605: ISO8859-15 Latin 9  57002: Devanagari, Marathi 57003: Bengali");
            Console.WriteLine("  57004: Tamil               57005: Telugu              57006: Assamese            57007: Oriya");
            Console.WriteLine("  57008: Kannada             57009: Malayalam           57010: Gujarati            57011: Punjabi");
            Console.WriteLine("");
            Console.WriteLine("Depends on Printer Model");
            Console.WriteLine("  949: Korea                 950: Traditiona Chinese    54936: GB18030             65001: UTF-8");
            Console.WriteLine("");
            Console.WriteLine("Special embedding support");
            Console.WriteLine("  6: Hiragana                7: OnePass Kanji 1         8:OnePass Kanji 2          11: PC851 Greek");
            Console.WriteLine("  12: PC853 Turkish          20-26: ASCII+ThaiSpecific  30: TCVN-3 1               31: TCVN-3 2");
            Console.WriteLine("  41: PC1098 Farsi           42: PC1118 Lithuanian      43: PC1119 Lithuanian      44: PC1125 Ukrainian");
            Console.WriteLine("  53: KZ-1048 Kazakhstan     254-255: ASCII+HexaDecimal");
            Console.WriteLine("");
            Console.WriteLine("International Character Set type:");
            Console.WriteLine("  0: U.S.A.       1: France       2: Germany      3: U.K.         4: Denmark I    5: Sweden");
            Console.WriteLine("  6: Italy        7: Spain I      8: Japan        9: Norway       10: Denmark II  11: Spain II");
            Console.WriteLine("  12: Latin America   13: Korea   14: Slovenia/Croatia   15: China   16: Vietnam   17: Arabia");
            Console.WriteLine(" India");
            Console.WriteLine("  66: Devanagari  67: Bengali     68: Tamil       69: Telugu      70: Assamese    71: Oriya");
            Console.WriteLine("  72: Kannada     73: Malayalam   74: Gujarati    75: Punjabi     82: Marathi");
        }

        private static int Options(string[] args)
        {
            int result = 0;

            if (args.Length == 0)
            {
                //return 1;
                return 0;
            }
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToUpper())
                {
                    // specify initial Device with next parameter
                    case "-D":
                        {
                            i++;
                            if (i >= args.Length)
                            {
                                result = 1;
                                break;
                            }
                            string device = args[i].ToLower();
                            if (device.Equals("printer"))
                            {
                                deviceType = EscPosEncoder.EscPosPrinter;
                            }
                            else if (device.Equals("linedisplay"))
                            {
                                deviceType = EscPosEncoder.EscPosLineDisplay;
                            }
                            else
                            {
                                result = 1;
                                break;
                            }
                        }
                        break;
                    // specify Output file path with next parameter
                    case "-O":
                        {
                            i++;
                            if (i >= args.Length)
                            {
                                result = 1;
                                break;
                            }
                            if (!args[i].StartsWith("-"))
                            {
                                outputpath = args[i];
                                EscPosEncode.stdout = false;
                            }
                            else
                            {
                                result = 1;
                                break;
                            }
                        }
                        break;
                    // specify Codepage with next parameter
                    case "-C":
                        {
                            i++;
                            if (i >= args.Length)
                            {
                                result = 3;
                                break;
                            }
                            try
                            {
                                int iwork = Convert.ToInt32(args[i]);
                                if (EscPosDecoder.PrtESCtCodePage.ContainsValue(iwork) && (iwork < 0x100))
                                {
                                    initialcodepage = iwork;
                                    initialkanjion = false;
                                }
                                else
                                {
                                    try
                                    {
                                        Encoding ework = Encoding.GetEncoding((iwork >= 0 ? iwork : 20127));
                                        initialcodepage = ework.CodePage;
                                        if (!EscPosDecoder.PrtESCtCodePage.ContainsValue(initialcodepage) && ework.IsSingleByte)
                                        {
                                            initialcodepage = 20127;
                                        }
                                        initialkanjion = !ework.IsSingleByte;
                                    }
                                    catch
                                    {
                                        result = 3;
                                        break;
                                    }
                                }
                            }
                            catch
                            {
                                result = 3;
                                break;
                            }
                        }
                        break;
                    // specify International character set type with next parameter
                    case "-I":
                        {
                            i++;
                            if (i >= args.Length)
                            {
                                result = 3;
                                break;
                            }
                            try
                            {
                                byte bwork = Convert.ToByte(args[i]);
                                if ((bwork == 0) || (EscPosDecoder.s_StringESCRICS.ContainsKey(bwork)))
                                {
                                    initialICS = bwork;
                                }
                                else
                                {
                                    result = 3;
                                    break;
                                }
                            }
                            catch
                            {
                                result = 3;
                                break;
                            }
                        }
                        break;
                    // specify Kanji ON/OFF with next parameter
                    case "-K":
                        {
                            i++;
                            if (i >= args.Length)
                            {
                                result = 1;
                                break;
                            }
                            try
                            {
                                initialkanjion = args[i].ToUpper() switch
                                {
                                    "ON" => true,
                                    "OFF" => false,
                                    _ => initialkanjion,
                                };
                            }
                            catch
                            {
                                result = 1;
                                break;
                            }
                        }
                        break;
                    // specify Single Byte Character Sequence supported font size pattern with next parameter
                    case "-S":
                        {
                            i++;
                            if (i >= args.Length)
                            {
                                result = 2;
                                break;
                            }
                            if (!args[i].StartsWith("-"))
                            {
                                if (int.TryParse(args[i], out int number))
                                {
                                    if ((number >= 1) && (number <= 9))
                                    {
                                        sbcsfontpattern = number;
                                    }
                                    else
                                    {
                                        result = 2;
                                        break;
                                    }
                                }
                                else
                                {
                                    result = 2;
                                    break;
                                }
                            }
                            else
                            {
                                result = 2;
                                break;
                            }
                        }
                        break;
                    // specify Multi Byte Character Sequence supported font size pattern with next parameter
                    case "-M":
                        {
                            i++;
                            if (i >= args.Length)
                            {
                                result = 2;
                                break;
                            }
                            if (!args[i].StartsWith("-"))
                            {
                                if (int.TryParse(args[i], out int number))
                                {
                                    if ((number >= 1) && (number <= 5))
                                    {
                                        mbcsfontpattern = number;
                                    }
                                    else
                                    {
                                        result = 2;
                                        break;
                                    }
                                }
                                else
                                {
                                    result = 2;
                                    break;
                                }
                            }
                            else
                            {
                                result = 2;
                                break;
                            }
                        }
                        break;
                    // specify LineDisplay supported font size pattern with next parameter
                    case "-V":
                        {
                            i++;
                            if (i >= args.Length)
                            {
                                result = 2;
                                break;
                            }
                            if (!args[i].StartsWith("-"))
                            {
                                if (int.TryParse(args[i], out int number))
                                {
                                    if ((number >= 1) && (number <= 2))
                                    {
                                        vfdfontpattern = number;
                                    }
                                    else
                                    {
                                        result = 2;
                                        break;
                                    }
                                }
                                else
                                {
                                    result = 2;
                                    break;
                                }
                            }
                            else
                            {
                                result = 2;
                                break;
                            }
                        }
                        break;
                    // specify Help message display
                    case "-H":
                        result = 1;
                        break;
                    // specify supported Font size pattern detail help message display
                    case "-F":
                        result = 2;
                        break;
                    // specify supported Codepage list help message display
                    case "-L":
                        result = 3;
                        break;
                    // specify input file path
                    default:
                        inputpath = args[i];
                        break;
                }
                if (result != 0)
                {
                    break;
                }
            }
            //if ((result == 0) && (string.IsNullOrEmpty(inputpath)))
            //{
            //    result = 1;
            //}
            return result;
        }
    }
}