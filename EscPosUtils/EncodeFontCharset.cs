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

namespace kunif.EscPosUtils
{
    using System;
    using System.Linq;
    using System.Text;

    public partial class EscPosEncoder
    {
        //--------------------------------------

        public enum CodeTable
        {
            PC437 = 0,
            PC932 = 1,
            PC850 = 2,
            PC860 = 3,
            PC863 = 4,
            PC865 = 5,
            Page6_Hiragana = 6,
            Page7_OnePassKanji = 7,
            Page8_OnePassKanji = 8,
            PC851 = 11,
            PC853 = 12,
            PC857 = 13,
            PC737 = 14,
            ISO8859_7 = 15,
            WPC1252 = 16,
            PC866 = 17,
            PC852 = 18,
            PC858 = 19,
            Page20_Thai_42 = 20,
            Page21_Thai_11 = 21,
            Page22_Thai_13 = 22,
            Page23_Thai_14 = 23,
            Page24_Thai_16 = 24,
            Page25_Thai_17 = 25,
            Page26_Thai_18 = 26,
            Page30_TCVN_3 = 30,
            Page31_TCVN_3 = 31,
            PC720 = 32,
            WPC775 = 33,
            PC855 = 34,
            PC861 = 35,
            PC862 = 36,
            PC864 = 37,
            PC869 = 38,
            ISO8859_2 = 39,
            ISO8859_15 = 40,
            PC1098 = 41,
            PC1118 = 42,
            PC1119 = 43,
            PC1125 = 44,
            WPC1250 = 45,
            WPC1251 = 46,
            WPC1253 = 47,
            WPC1254 = 48,
            WPC1255 = 49,
            WPC1256 = 50,
            WPC1257 = 51,
            WPC1258 = 52,
            KZ_1048 = 53,
            Devanagari = 66,
            Bengali = 67,
            Tamil = 68,
            Telugu = 69,
            Assamese = 70,
            Oriya = 71,
            Kannada = 72,
            Malayalam = 73,
            Gujarati = 74,
            Punjabi = 75,
            Marathi = 82,
            Page254 = 254,
            Page255 = 255
        }

        private CodeTable _CharacterCodeTable;
        private int _prtcodepage;

        public CodeTable CharacterCodeTable
        {
            get { return _CharacterCodeTable; }
            set
            {
                _CharacterCodeTable = value;
                EscPosDecoder.PrtESCtCodePage.TryGetValue((byte)_CharacterCodeTable, out _prtcodepage);
                if (_prtcodepage < 0x100)
                {
                    _prtembedded = GetEmbeddedESCtCodePage(_prtcodepage);
                }
                else
                {
                    _prtencoding = Encoding.GetEncoding(_prtcodepage);
                }
                byte[] cmd = { 0x1B, 0x74, (byte)((int)value) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscSelectCharacterCodeTable, cmd));
            }
        }

        //--------------------------------------

        public enum InternationalCharacter
        {
            USA = 0,
            France = 1,
            Germany = 2,
            UnitedKingdom = 3,
            Denmark_I = 4,
            Sweden = 5,
            Italy = 6,
            Spain_I = 7,
            Japan = 8,
            Norway = 9,
            Denmark_II = 10,
            Spain_II = 11,
            LatinAmerica = 12,
            Korea = 13,
            Slovenia_Croatia = 14,
            China = 15,
            Vietnam = 16,
            Arabia = 17,
            Devanagari = 66,
            Bengali = 67,
            Tamil = 68,
            Telugu = 69,
            Assamese = 70,
            Oriya = 71,
            Kannada = 72,
            Malayalam = 73,
            Gujarati = 74,
            Punjabi = 75,
            Marathi = 82
        }

        private InternationalCharacter _InternationalCharacertSet;

        public InternationalCharacter InternationalCharacterSet
        {
            get { return _InternationalCharacertSet; }
            set
            {
                _InternationalCharacertSet = value;
                if ((byte)_InternationalCharacertSet != 0)
                {
                    _prtreplaced = b_StringESCRICS[(byte)_InternationalCharacertSet];
                }
                byte[] cmd = { 0x1B, 0x52, (byte)((int)value) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscSelectInternationalCharacterSet, cmd));
            }
        }

        //--------------------------------------

        private Boolean _KanjiMode;

        public Boolean KanjiMode
        {
            get { return _KanjiMode; }
            set
            {
                _KanjiMode = value;
                if (_KanjiMode)
                {
                    byte[] cmd = { 0x1C, 0x26 };
                    CommandList.Add(new EscPosCmd(EscPosCmdType.FsSelectKanjiCharacterMode, cmd));
                }
                else
                {
                    byte[] cmd = { 0x1C, 0x2E };
                    CommandList.Add(new EscPosCmd(EscPosCmdType.FsCancelKanjiCharacterMode, cmd));
                }
            }
        }

        //--------------------------------------

        private Boolean _UTF8Mode;

        public Boolean UTF8Mode
        {
            get { return _UTF8Mode; }
            set
            {
                _UTF8Mode = value;
                byte[] cmd = { 0x1C, 0x28, 0x43, 0x02, 0x00, 0x30, (byte)(_UTF8Mode ? 0x31 : 0x32) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.FsSelectCharacterEncodeSystem, cmd));
            }
        }

        //--------------------------------------

        public enum FontType
        {
            A = '0',
            B = '1',
            C = '2',
            D = '3',
            E = '4',
            SpecialA = 'a',
            SpecialB = 'b'
        }

        private FontType _SBCSFontType;

        public FontType SBCSFontType
        {
            get { return _SBCSFontType; }
            set
            {
                _SBCSFontType = value;
                byte[] cmd = { 0x1B, 0x4D, (byte)((int)value) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscSelectCharacterFont, cmd));
            }
        }

        public int FontWidth { get; private set; }
        public int FontHeight { get; private set; }

        //--------------------------------------

        public enum FontPriority
        {
            ANK = 0,
            Japanese = 11,
            SimplifiedChinese = 20,
            TraditionalChinese = 30,
            Korean = 41
        }

        private FontPriority _FontPriority1st;

        public FontPriority FontPriority1st
        {
            get { return _FontPriority1st; }
            set
            {
                _FontPriority1st = value;
                byte[] cmd = { 0x1C, 0x28, 0x43, 0x03, 0x00, 0x3C, 0x00, (byte)((int)value) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.FsSetFontPriority, cmd));
            }
        }

        private FontPriority _FontPriority2nd;

        public FontPriority FontPriority2nd
        {
            get { return _FontPriority2nd; }
            set
            {
                _FontPriority2nd = value;
                byte[] cmd = { 0x1C, 0x28, 0x43, 0x03, 0x00, 0x3C, 0x01, (byte)((int)value) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.FsSetFontPriority, cmd));
            }
        }

        //--------------------------------------

        private FontType _KanjiFontType;

        public FontType KanjiFontType
        {
            get { return _KanjiFontType; }
            set
            {
                _KanjiFontType = value;
                byte[] cmd = { 0x1C, 0x28, 0x41, 0x02, 0x00, 0x30, (byte)((int)value) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.FsSelectKanjiCharacterFont, cmd));
            }
        }

        public int KanjiFontWidth { get; private set; }
        public int KanjiFontHeight { get; private set; }

        //--------------------------------------

        private Boolean _DownloadCharacterSet;

        public Boolean DownloadCharacterSet
        {
            get { return _DownloadCharacterSet; }
            set
            {
                _DownloadCharacterSet = value;
                byte[] cmd = { 0x1B, 0x25, (byte)(value ? 0x31 : 0x30) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscSelectUserDefinedCharacterSet, cmd));
            }
        }

        //--------------------------------------

        public class CharacterDefine
        {
            public int x;
            public int y;
            public byte[] data = Array.Empty<byte>();
        }

        public void DefineDowloadCharacters(int c1, int c2, CharacterDefine[] characters)
        {
            if ((c1 > c2)
              || ((c1 < 32) || (c1 > 126))
              || ((c2 < 32) || (c2 > 126))
              || ((c2 - c1 + 1) != characters.Length)
            )
            { return; }
            int fonty = (FontHeight % 8) != 0 ? (FontHeight / 8) + 1 : FontHeight / 8;
            byte[] cmd = { 0x1B, 0x26, (byte)fonty, (byte)c1, (byte)c2 };
            Boolean irregulardata = false;
            byte[] fontdata = Array.Empty<byte>();
            foreach (CharacterDefine item in characters)
            {
                if ((item.y != fonty) || (item.x < 0) || (item.x > FontWidth))
                {
                    irregulardata = true;
                    break;
                }
                fontdata = fontdata.Concat(new[] { (byte)item.y }).ToArray();
                if (item.x > 0)
                {
                    fontdata = fontdata.Concat(item.data).ToArray();
                }
            }
            if (!irregulardata)
            {
                EscPosCmdType cmdType = FontHeight switch
                {
                    24 => FontWidth switch
                    {
                        12 => EscPosCmdType.EscDefineUserDefinedCharacters1224,
                        10 => EscPosCmdType.EscDefineUserDefinedCharacters1024,
                        9 => EscPosCmdType.EscDefineUserDefinedCharacters0924,
                        _ => EscPosCmdType.None,
                    },
                    17 => EscPosCmdType.EscDefineUserDefinedCharacters0917,
                    16 => EscPosCmdType.EscDefineUserDefinedCharacters0816,
                    9 => FontWidth switch
                    {
                        9 => EscPosCmdType.EscDefineUserDefinedCharacters0909,
                        7 => EscPosCmdType.EscDefineUserDefinedCharacters0709,
                        _ => EscPosCmdType.None,
                    },
                    _ => EscPosCmdType.None,
                };
                CommandList.Add(new EscPosCmd(cmdType, cmd.Concat(fontdata).ToArray()));
            }
        }

        //--------------------------------------

        public void DefineUserDefinedKanjiCharacter(int code, byte[] fontdata)
        {
            byte c1 = (byte)(code / 256);
            byte c2 = (byte)(code % 256);
            if (!(c1 switch
            {
                0x77 => ((c2 >= 0x21) && (c2 <= 0x7E)),
                0xEC => (((c2 >= 0x40) && (c2 <= 0x7E)) || ((c2 >= 0x80) && (c2 <= 0x9E))),
                0xFE => ((c2 >= 0xA1) && (c2 <= 0xFE)),
                _ => false
            }))
            { return; }
            int datalength = KanjiFontWidth * KanjiFontHeight / 8;
            if (fontdata.Length != datalength)
            { return; }
            byte[] cmd = { 0x1C, 0x32, c1, c2 };
            cmd = cmd.Concat(fontdata).ToArray();
            EscPosCmdType cmdType = datalength switch
            {
                72 => EscPosCmdType.FsDefineUserDefinedKanjiCharacters2424,
                60 => EscPosCmdType.FsDefineUserDefinedKanjiCharacters2024,
                32 => EscPosCmdType.FsDefineUserDefinedKanjiCharacters1616,
                _ => EscPosCmdType.None,
            };
            CommandList.Add(new EscPosCmd(cmdType, cmd));
        }

        //--------------------------------------

        public void CancelDownloadCharacters(byte[] codes)
        {
            if ((codes == null) || (codes.Length == 0))
            { return; }
            byte[] cmd = { 0x1B, 0x3F, 0x00 };
            foreach (byte code in codes)
            {
                if ((code < 32) || (code > 126))
                { continue; }
                cmd[2] = code;
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscCancelUserDefinedCharacters, cmd));
            }
        }

        //--------------------------------------

        public void CancelUserDefinedKanjiCharacters(int[] codes)
        {
            if ((codes == null) || (codes.Length == 0))
            { return; }
            byte[] cmd = { 0x1C, 0x3F, 0x00, 0x00 };
            foreach (int code in codes)
            {
                byte c1 = (byte)(code / 256);
                byte c2 = (byte)(code % 256);
                if ((c1 switch
                {
                    0x77 => ((c2 >= 0x21) && (c2 <= 0x7E)),
                    0xEC => (((c2 >= 0x40) && (c2 <= 0x7E)) || ((c2 >= 0x80) && (c2 <= 0x9E))),
                    0xFE => ((c2 >= 0xA1) && (c2 <= 0xFE)),
                    _ => false
                }))
                {
                    cmd[2] = c1;
                    cmd[3] = c2;
                    CommandList.Add(new EscPosCmd(EscPosCmdType.FsCancelUserDefinedKanjiCharacters, cmd));
                }
            }
        }

        //--------------------------------------
    }
}