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

namespace kunif.EscPosDecode
{
    using kunif.EscPosUtils;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    internal class EscPosDecode
    {
        private static long deviceType = EscPosTokenizer.EscPosPrinter;
        private static Boolean decode = true;
        private static Boolean stdout = true;
        private static Boolean graphicsoutput = false;
        private static int initialcodepage = -1;
        private static Boolean initialkanjion = false;
        private static byte initialICS = 0;
        private static string inputpath = "";
        private static string outputpath = "";
        private static string graphicspath = "";
        private static int sbcsfontpattern = 1;
        private static int mbcsfontpattern = 1;
        private static int vfdfontpattern = 1;

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

            byte[] escposdata = ReadFile(inputpath);

            if ((escposdata is null) || (escposdata.Length == 0))
            {
                Console.Error.WriteLine("No input data.");
                return 11;
            }

            var EPToken = new EscPosTokenizer();
            List<EscPosCmd> escposlist = EPToken.Scan(escposdata, deviceType, sbcsfontpattern, mbcsfontpattern, vfdfontpattern);

            if ((escposlist is null) || (escposlist.Count == 0))
            {
                Console.Error.WriteLine("No tokenized data.");
                return 12;
            }

            if (decode)
            {
                escposlist = EscPosDecoder.Convert(escposlist);
                escposlist = Convertstrings(escposlist);
            }

            string result = string.Empty;
            Boolean graphicsexist = false;

            foreach (EscPosCmd item in escposlist)
            {
                if (string.IsNullOrEmpty(item.paramdetail))
                {
                    result += string.Format(CultureInfo.InvariantCulture, "<! {0}\n  | {1} !>\n", item.cmdtype.ToString(), BitConverter.ToString(item.cmddata));
                }
                else
                {
                    result += string.Format(CultureInfo.InvariantCulture, "<! {0}\n  | {1}\n  # {2} !>\n", item.cmdtype.ToString(), BitConverter.ToString(item.cmddata), item.paramdetail);
                }
                if (item.somebinary != null)
                {
                    graphicsexist = true;
                }
            }

            if (stdout)
            {
                Console.Write(result);
            }
            else
            {
                WritedFile(outputpath, result);
            }
            if (graphicsoutput && graphicsexist)
            {
                try
                {
                    if (!Directory.Exists(graphicspath))
                    {
                        Directory.CreateDirectory(graphicspath);
                    }
                    Outputgraphics(escposlist);
                }
                catch { }
            }
            return 0;
        }

        private static List<EscPosCmd> Convertstrings(List<EscPosCmd> escposlist)
        {
            Boolean prtutf8 = false;
            Encoding prtencoding = Encoding.Default;
            Dictionary<byte, string> prtembedded = new Dictionary<byte, string>();
            Dictionary<string, string> prtreplaced = new Dictionary<string, string>();
            UTF8Encoding utf8encoding = new UTF8Encoding(); ;
            Encoding vfdencoding = Encoding.Default;
            Dictionary<byte, string> vfdembedded = new Dictionary<byte, string>();
            Dictionary<string, string> vfdreplaced = new Dictionary<string, string>();

            try
            {
                if (initialcodepage < 0)
                {
                    initialcodepage = CultureInfo.CurrentCulture.TextInfo.OEMCodePage;
                    initialkanjion = !Encoding.GetEncoding(initialcodepage).IsSingleByte;
                }
            }
            catch
            {
                return escposlist;
            }
            int prtcodepage = initialcodepage;
            int prtICS = initialICS;
            int vfdcodepage = initialcodepage;
            int vfdICS = initialICS;
            Boolean prtkanjimode = initialkanjion;
            Boolean vfdkanjimode = initialkanjion;

            try
            {
                if (prtcodepage < 0x100)
                {
                    prtembedded = EscPosDecoder.GetEmbeddedESCtCodePage(prtcodepage);
                }
                else
                {
                    prtencoding = Encoding.GetEncoding(prtcodepage);
                }
                if (prtICS != 0)
                {
                    prtreplaced = EscPosDecoder.s_StringESCRICS[(byte)prtICS];
                }
                if (vfdcodepage < 0x100)
                {
                    vfdembedded = EscPosDecoder.GetEmbeddedESCtCodePage(vfdcodepage);
                }
                else
                {
                    prtencoding = Encoding.GetEncoding(vfdcodepage);
                }
                if ((vfdICS >= 1) && (vfdICS <= 17))
                {
                    vfdreplaced = EscPosDecoder.s_StringESCRICS[(byte)vfdICS];
                }
                else
                {
                    vfdICS = 0;
                }
            }
            catch
            {
                return escposlist;
            }

            foreach (EscPosCmd item in escposlist)
            {
                switch (item.cmdtype)
                {
                    case EscPosCmdType.EscInitialize:
                        prtcodepage = initialcodepage;
                        prtICS = initialICS;
                        prtkanjimode = initialkanjion;
                        try
                        {
                            if (prtcodepage < 0x100)
                            {
                                prtembedded = EscPosDecoder.GetEmbeddedESCtCodePage(prtcodepage);
                            }
                            else
                            {
                                prtencoding = Encoding.GetEncoding(prtcodepage);
                            }
                            if (prtICS != 0)
                            {
                                prtreplaced = EscPosDecoder.s_StringESCRICS[(byte)prtICS];
                            }
                        }
                        catch { }
                        break;

                    case EscPosCmdType.EscSelectInternationalCharacterSet:
                        try
                        {
                            if (EscPosDecoder.s_StringESCRICS.ContainsKey(item.cmddata[2]))
                            {
                                prtICS = item.cmddata[2];
                                if (prtICS != 0)
                                {
                                    prtreplaced = EscPosDecoder.s_StringESCRICS[(byte)prtICS];
                                }
                            }
                        }
                        catch { }
                        break;

                    case EscPosCmdType.EscSelectCharacterCodeTable:
                        try
                        {
                            if (EscPosDecoder.PrtESCtCodePage.TryGetValue(item.cmddata[2], out prtcodepage))
                            {
                                if (prtcodepage < 0x100)
                                {
                                    prtembedded = EscPosDecoder.GetEmbeddedESCtCodePage(prtcodepage);
                                }
                                else
                                {
                                    prtencoding = Encoding.GetEncoding(prtcodepage);
                                }
                            }
                        }
                        catch { }
                        break;

                    case EscPosCmdType.FsSelectKanjiCharacterMode:
                        prtkanjimode = true;
                        break;

                    case EscPosCmdType.FsCancelKanjiCharacterMode:
                        prtkanjimode = false;
                        break;

                    case EscPosCmdType.FsSelectCharacterEncodeSystem:
                        {
                            byte m = item.cmddata[6];
                            prtutf8 = ((m == 2) || (m == 50));
                        }
                        break;

                    case EscPosCmdType.PrtPrintables:
                        try
                        {
                            List<string> listwork = new List<string>();
                            if (prtutf8)
                            {
                                listwork.AddRange(utf8encoding.GetChars(item.cmddata).Select(c => c.ToString()));
                            }
                            else if (prtcodepage < 0x100)
                            {
                                listwork.AddRange(item.cmddata.Select(c => prtembedded[c]));
                            }
                            else
                            {
                                listwork.AddRange(prtencoding.GetChars(item.cmddata).Select(c => c.ToString()));
                            }
                            item.paramdetail = string.Join("",(prtICS == 0 ? listwork : listwork.Select(s => (prtreplaced.ContainsKey(s) ? prtreplaced[s] : s))));
                        }
                        catch { }
                        break;

                    case EscPosCmdType.VfdEscInitialize:
                        vfdcodepage = initialcodepage;
                        vfdICS = initialICS;
                        vfdkanjimode = initialkanjion;
                        try
                        {
                            if (vfdcodepage < 0x100)
                            {
                                vfdembedded = EscPosDecoder.GetEmbeddedESCtCodePage(vfdcodepage);
                            }
                            else
                            {
                                prtencoding = Encoding.GetEncoding(vfdcodepage);
                            }
                        }
                        catch { }
                        break;

                    case EscPosCmdType.VfdEscSelectInternationalCharacterSet:
                        if (EscPosDecoder.s_StringESCRICS.ContainsKey(item.cmddata[2]))
                        {
                            vfdICS = item.cmddata[2];
                            if ((vfdICS >= 1) && (vfdICS <= 17))
                            {
                                vfdreplaced = EscPosDecoder.s_StringESCRICS[(byte)vfdICS];
                            }
                            else
                            {
                                vfdICS = 0;
                            }
                        }
                        break;

                    case EscPosCmdType.VfdEscSelectCharacterCodeTable:
                        try
                        {
                            if (EscPosDecoder.VfdESCtCodePage.TryGetValue(item.cmddata[2], out vfdcodepage))
                            {
                                if (vfdcodepage < 0x100)
                                {
                                    vfdembedded = EscPosDecoder.GetEmbeddedESCtCodePage(vfdcodepage);
                                }
                                else
                                {
                                    prtencoding = Encoding.GetEncoding(vfdcodepage);
                                }
                            }
                        }
                        catch { }
                        break;

                    case EscPosCmdType.VfdUsKanjiCharacterModeOnOff:
                        vfdkanjimode = item.cmddata[6] switch
                        {
                            0 => false,
                            48 => false,
                            1 => true,
                            49 => true,
                            _ => vfdkanjimode,
                        };
                        break;

                    case EscPosCmdType.VfdDisplayables:
                        try
                        {
                            List<string> listwork = new List<string>();
                            if (vfdcodepage < 0x100)
                            {
                                listwork.AddRange(item.cmddata.Select(c => vfdembedded[c]));
                            }
                            else
                            {
                                listwork.AddRange(vfdencoding.GetChars(item.cmddata).Select(c => c.ToString()));
                            }
                            item.paramdetail = string.Join("", (vfdICS == 0 ? listwork : listwork.Select(s => (vfdreplaced.ContainsKey(s) ? vfdreplaced[s] : s))));
                        }
                        catch { }
                        break;
                };
            }
            return escposlist;
        }
        private static void Outputgraphics(List<EscPosCmd> escposlist)
        {
            int serialnumber = 0;
            foreach (EscPosCmd item in escposlist)
            {
                if (item.somebinary != null)
                {
                    switch (item.cmdtype)
                    {
                        case EscPosCmdType.EscDefineUserDefinedCharacters1224:
                        case EscPosCmdType.EscDefineUserDefinedCharacters1024:
                        case EscPosCmdType.EscDefineUserDefinedCharacters0924:
                        case EscPosCmdType.EscDefineUserDefinedCharacters0917:
                        case EscPosCmdType.EscDefineUserDefinedCharacters0909:
                        case EscPosCmdType.EscDefineUserDefinedCharacters0709:
                        case EscPosCmdType.EscDefineUserDefinedCharacters0816:
                        case EscPosCmdType.VfdEscDefineUserDefinedCharacters0816:
                        case EscPosCmdType.VfdEscDefineUserDefinedCharacters0507:
                            try
                            {
                                System.Drawing.Bitmap[] bmparray = (System.Drawing.Bitmap[])item.somebinary;
                                string cmdtype = "_" + item.cmdtype.ToString();
                                string snstring = serialnumber.ToString("D4") + "_";
                                byte code = item.cmddata[3];
                                foreach (System.Drawing.Bitmap bitmap in bmparray)
                                {
                                    string filename = graphicspath + "\\" + snstring + code.ToString("X2") + cmdtype + ".bmp";
                                    bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Bmp);
                                    code++;
                                }
                                serialnumber++;
                            }
                            catch { }
                            break;
                        case EscPosCmdType.GsDefineColumnFormatCharacterCodePage:
                        case EscPosCmdType.GsDefineRasterFormatCharacterCodePage:
                            try
                            {
                                System.Drawing.Bitmap[] bmparray = (System.Drawing.Bitmap[])item.somebinary;
                                string cmdtype = "_" + item.cmdtype.ToString();
                                string snstring = serialnumber.ToString("D4") + "_";
                                byte code = item.cmddata[7];
                                foreach (System.Drawing.Bitmap bitmap in bmparray)
                                {
                                    string filename = graphicspath + "\\" + snstring + code.ToString("X2") + cmdtype + ".bmp";
                                    bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Bmp);
                                    code++;
                                }
                                serialnumber++;
                            }
                            catch { }
                            break;
                        case EscPosCmdType.GsDefineNVGraphicsDataRasterW:
                        case EscPosCmdType.GsDefineNVGraphicsDataRasterDW:
                        case EscPosCmdType.GsDefineNVGraphicsDataColumnW:
                        case EscPosCmdType.GsDefineNVGraphicsDataColumnDW:
                        case EscPosCmdType.GsDefineDownloadGraphicsDataRasterW:
                        case EscPosCmdType.GsDefineDownloadGraphicsDataRasterDW:
                        case EscPosCmdType.GsDefineDownloadGraphicsDataColumnW:
                        case EscPosCmdType.GsDefineDownloadGraphicsDataColumnDW:
                            try
                            {
                                System.Drawing.Bitmap[] bmparray = (System.Drawing.Bitmap[])item.somebinary;
                                string cmdtype = "_" + item.cmdtype.ToString();
                                string snstring = serialnumber.ToString("D4") + "_";
                                int indexnumber = 0;
                                foreach (System.Drawing.Bitmap bitmap in bmparray)
                                {
                                    string filename = graphicspath + "\\" + snstring + indexnumber.ToString("D3") + cmdtype + ".bmp";
                                    bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Bmp);
                                    indexnumber++;
                                }
                                serialnumber++;
                            }
                            catch { }
                            break;
                        case EscPosCmdType.FsDefineUserDefinedKanjiCharacters2424:
                        case EscPosCmdType.FsDefineUserDefinedKanjiCharacters2024:
                        case EscPosCmdType.FsDefineUserDefinedKanjiCharacters1616:
                            try
                            {
                                int code = (item.cmddata[2] * 0x100) + item.cmddata[3];
                                System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)item.somebinary;
                                string filename = graphicspath + "\\" + serialnumber.ToString("D4") + "_" + code.ToString("X4") + "_" + item.cmdtype.ToString() + ".bmp";
                                bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Bmp);
                                serialnumber++;
                            }
                            catch { }
                            break;
                        case EscPosCmdType.EscSelectBitImageMode:
                        case EscPosCmdType.FsObsoleteDefineNVBitimage:
                        case EscPosCmdType.GsStoreGraphicsDataToPrintBufferRasterW:
                        case EscPosCmdType.GsStoreGraphicsDataToPrintBufferRasterDW:
                        case EscPosCmdType.GsStoreGraphicsDataToPrintBufferColumnW:
                        case EscPosCmdType.GsStoreGraphicsDataToPrintBufferColumnDW:
                        case EscPosCmdType.GsObsoleteDefineDownloadedBitimage:
                        case EscPosCmdType.GsDefineWindowsBMPNVGraphicsData:
                        case EscPosCmdType.GsDefineWindowsBMPDownloadGraphicsData:
                        case EscPosCmdType.GsObsoletePrintVariableVerticalSizeBitimage:
                        case EscPosCmdType.GsObsoletePrintRasterBitimage:
                            try
                            {
                                System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)item.somebinary;
                                string filename = graphicspath + "\\" + serialnumber.ToString("D4") + "_" + item.cmdtype.ToString() + ".bmp";
                                bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Bmp);
                                serialnumber++;
                            }
                            catch { }
                            break;
                    }
                }
            }
        }
        private static byte[] ReadFile(string filePath)
        {
            byte[] buffer = Array.Empty<byte>();
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
            }
            catch { }
            return buffer;
        }

        private static void WritedFile(string filePath, string data)
        {
            try
            {
                FileMode mode = File.Exists(filePath) ? FileMode.Truncate : FileMode.Create;
                FileStream ostrm = new FileStream(filePath, mode, FileAccess.Write);
                using var fs = new StreamWriter(ostrm);
                fs.Write(data);
            }
            catch { }
            return;
        }

        private static void HelpGeneral()
        {
            Console.WriteLine("Usage: EscPosDecode  InputFilePath  [Options]");
            Console.WriteLine("  InputFilePath :  specify input ESC/POS binary file path. required.");
            Console.WriteLine("  -H :  display this usage message.");
            Console.WriteLine("  -F :  display supported font size pattern detail message.");
            Console.WriteLine("  -L :  display supported CodePage and International character set type list message.");
            Console.WriteLine("  -D {Printer | LineDisplay} :  specify initial device type. default is Printer.");
            Console.WriteLine("  -O OutputFilePath :  specify output tokenized (and decoded) text file path. default is STDOUT.");
            Console.WriteLine("  -T :  specify Tokenize only. default are both tokenize and decode.");
            Console.WriteLine("  -C CodePage :  specify initial CodePage number. default system setting of Language for non-Unicode programs.");
            Console.WriteLine("  -I International character set type :  specify initial International character set type. default 0");
            //Console.WriteLine("  -K {ON | OFF} :  specify initial kanji on/off mode. default/specified codepage sbcs off/mbcs on.");
            Console.WriteLine("  -G GraphicsFolderPath :  specify decoded graphics&font output folder path. default&tokenize only are not output.");
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
                return 1;
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
                                deviceType = EscPosTokenizer.EscPosPrinter;
                            }
                            else if (device.Equals("linedisplay"))
                            {
                                deviceType = EscPosTokenizer.EscPosLineDisplay;
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
                                stdout = false;
                            }
                            else
                            {
                                result = 1;
                                break;
                            }
                        }
                        break;
                    // specify Tokenize only
                    case "-T":
                        decode = false;
                        graphicsoutput = false;
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
                                if (EscPosDecoder.s_StringESCRICS.ContainsKey(bwork))
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
                    // specify Graphics&font output folder path with next parameter
                    case "-G":
                        {
                            i++;
                            if (i >= args.Length)
                            {
                                result = 1;
                                break;
                            }
                            if (!args[i].StartsWith("-"))
                            {
                                graphicspath = args[i];
                                graphicsoutput = true;
                            }
                            else
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
            if ((result == 0) && (string.IsNullOrEmpty(inputpath)))
            {
                result = 1;
            }
            return result;
        }
    }
}