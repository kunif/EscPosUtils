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
    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.CodeAnalysis.Scripting;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    // Class to pass as argument of C# script
    public class EncoderArgs
    {
        public EscPosEncoder enc { get; set; }
    }

    internal class EscPosEncode
    {
#pragma warning disable IDE0052 // Remove unread private members
        private static long deviceType = EscPosEncoder.EscPosPrinter;
        private static Boolean stdout = true;
        private static int initialcodepage = -1;
        private static Boolean initialkanjion = false;
        private static byte initialICS = 0;
        private static int initialwidth = 384;
        private static int initialheight = 1662;
        private static string inputpath = "";
        private static string outputpath = "./EncResult.bin";
        private static int sbcsfontpattern = 1;
        private static int mbcsfontpattern = 1;
        private static int vfdfontpattern = 1;

        //
        private static string source = "";

        private static Type argstype = typeof(EncoderArgs);
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

            if (!File.Exists(inputpath))
            {
                Console.Error.WriteLine("File Not Exists.");
                return 11;
            }
            source = File.ReadAllText(inputpath);
            if (source.Length <= 0)
            {
                Console.Error.WriteLine("No input data.");
                return 12;
            }

            try
            {
                string cwd = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

                List<Assembly> assembly = new List<Assembly>()
                {
                    Assembly.GetAssembly(typeof(System.Dynamic.DynamicObject)),  // System.Code
                    Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo)),  // Microsoft.CSharp
                    Assembly.GetAssembly(typeof(System.Dynamic.ExpandoObject)),
                    Assembly.GetAssembly(typeof(System.Data.DataTable)),
                    Assembly.GetAssembly(typeof(System.Text.Encoding)),
                    Assembly.LoadFrom("KGySoft.CoreLibraries.dll"),
                    Assembly.LoadFrom("KGySoft.Drawing.dll"),
                    Assembly.LoadFrom("kunif.EscPosUtils.dll"),
                    Assembly.LoadFrom("System.Drawing.Common.dll"),
                    Assembly.LoadFrom("Microsoft.Win32.SystemEvents.dll"),
                };
                try
                {
                    assembly.AddRange(new List<Assembly>(){
                            Assembly.LoadFrom("System.Collections.Immutable.dll"),
                            Assembly.LoadFrom("System.Reflection.Metadata.dll"),
                            Assembly.LoadFrom("System.Runtime.CompilerServices.Unsafe.dll"),
                            Assembly.LoadFrom("System.Text.Encoding.CodePages.dll"),
                        }
                    );
                }
                catch { }

                Directory.SetCurrentDirectory(cwd);

                List<string> import = new List<string>()
                {
                    "System",
                    "System.Collections.Generic",
                    "System.Data",
                    "System.Drawing",
                    "System.Drawing.Imaging",
                    "System.Dynamic",
                    "System.IO",
                    "System.Linq",
                    "System.Reflection",
                    "System.Text",
                    "kunif.EscPosUtils",
                    "kunif.EscPosUtils.EscPosEncoder"
                };
                var opt = ScriptOptions.Default.AddReferences(assembly).AddImports(import);

                EncoderArgs encoderArgs = new EncoderArgs();
                encoderArgs.enc = new EscPosEncoder();
                encoderArgs.enc.Configuration(deviceType, sbcsfontpattern, mbcsfontpattern, vfdfontpattern, initialwidth, initialheight, initialcodepage, (EscPosEncoder.InternationalCharacter)Enum.ToObject(typeof(EscPosEncoder.InternationalCharacter), initialICS));

                Script<object>? script = CSharpScript.Create(source, globalsType: argstype, options: opt);
                ScriptState<object>? result = script.RunAsync(globals: encoderArgs).Result;

                byte[] buffer = encoderArgs.enc.CommandList.SelectMany(v => v.cmddata).ToArray<byte>();
                if (buffer.Length > 0)
                {
                    if (stdout)
                    {
                        using (Stream outStream = Console.OpenStandardOutput())
                        {
                            outStream.Write(buffer, 0, buffer.Length);
                        }
                    }
                    else
                    {
                        WriteFile(outputpath, buffer);
                    }
                }
            }
            catch (CompilationErrorException ex)
            {
                Console.Error.WriteLine("[Compile Error]");
                Console.Error.WriteLine(ex.Message);
            }
            catch (TaskCanceledException)
            {
                Console.Error.WriteLine("Task is canceled");
            }
            catch (OperationCanceledException)
            {
                Console.Error.WriteLine("Operation is canceled");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
            return 0;
        }

        private static void WriteFile(string filePath, byte[] data, bool append = true)
        {
            try
            {
                FileMode mode = append ? FileMode.Append : File.Exists(filePath) ? FileMode.Truncate : FileMode.Create;
                using (FileStream ostrm = new(filePath, mode, FileAccess.Write))
                {
                    ostrm.Write(data, 0, data.Length);
                }
            }
            catch { }
            return;
        }

        private static void HelpGeneral()
        {
            Console.WriteLine("Usage: EscPosEncode  [Options]");
            Console.WriteLine("  InputFilePath :  specify input ESC/POS encoding script file path. required.");
            Console.WriteLine("  -H :  display this usage message.");
            Console.WriteLine("  -F :  display supported font size pattern detail message.");
            Console.WriteLine("  -L :  display supported CodePage and International character set type list message.");
            Console.WriteLine("  -D {Printer | LineDisplay} :  specify initial device type. default is Printer.");
            Console.WriteLine("  -O OutputFilePath :  specify output encoded binary file path. default is STDOUT.");
            //Console.WriteLine("  -T :  specify Tokenize only. default are both tokenize and decode.");
            Console.WriteLine("  -C CodePage :  specify initial CodePage number. default system setting of Language for non-Unicode programs.");
            Console.WriteLine("  -I International character set type :  specify initial International character set type. default 0");
            //Console.WriteLine("  -K {ON | OFF} :  specify initial kanji on/off mode. default/specified codepage sbcs off/mbcs on.");
            //Console.WriteLine("  -G GraphicsFolderPath :  specify decoded graphics&font output folder path. default&tokenize only are not output.");
            Console.WriteLine("  -W PaperWidth :  specify paper width by dot. default is 384.");
            Console.WriteLine("  -P PageHeight :  specify page mode maximum height by dot. default is 1662.");
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
                    // specify Paper Width by dot with next parameter
                    case "-W":
                        {
                            i++;
                            if (i >= args.Length)
                            {
                                result = 1;
                                break;
                            }
                            try
                            {
                                int iwork = Convert.ToInt32(args[i]);
                                if (iwork > 0)
                                {
                                    initialwidth = iwork;
                                }
                                else
                                {
                                    result = 1;
                                    break;
                                }
                            }
                            catch
                            {
                                result = 1;
                                break;
                            }
                        }
                        break;
                    // specify Page Height by dot with next parameter
                    case "-P":
                        {
                            i++;
                            if (i >= args.Length)
                            {
                                result = 1;
                                break;
                            }
                            try
                            {
                                int iwork = Convert.ToInt32(args[i]);
                                if (iwork > 0)
                                {
                                    initialheight = iwork;
                                }
                                else
                                {
                                    result = 1;
                                    break;
                                }
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
            if ((result == 0) && (string.IsNullOrEmpty(inputpath)))
            {
                result = 1;
            }
            return result;
        }
    }
}