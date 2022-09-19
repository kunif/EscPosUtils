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
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;

    public partial class EscPosEncoder
    {
        //--------------------------------------

        public const long EscPosPrinter = 1;
        public const long EscPosLineDisplay = 2;

        public List<EscPosCmd> CommandList = new() { };
        private readonly EscPosTokenizer _escPosTokenizer = new();
        private Encoding _prtencoding = Encoding.GetEncoding(437);
        private Dictionary<char, byte[]> _prtembedded = new();
        private Dictionary<char, byte[]> _prtreplaced = new();
        private readonly UTF8Encoding _utf8encoding = new();

        private Dictionary<byte, SbcsFontSizeInfo> SbcsFontList = new();
        private Dictionary<byte, KanjiFontSizeInfo> KanjiFontList = new();
        private SbcsFontSizeInfo CurrentSbcsFontInfo = new();
        private KanjiFontSizeInfo CurrentKanjiFontInfo = new();
        private VfdFontSizeInfo CurrentVfdFontInfo = new();
        private long _targetDevice = EscPosPrinter;
        private int _defaultlinespacing = 30;
        private int _defaultcodepage = 437;
        private CodeTable _defaultcodetable = CodeTable.PC437;
        private Encoding _defaultencoding = Encoding.GetEncoding(437);
        private Dictionary<char, byte[]> _defaultembedded = new();
        private Dictionary<char, byte[]> _defaultreplaced = new();
        private InternationalCharacter _defaultintenationalCharacter = InternationalCharacter.USA;

        //--------------------------------------

        public void Configuration(long initialDevice = EscPosPrinter, int SbcsFontPattern = 1, int KanjiFontPattern = 1, int LineDisplayFontPattern = 1, int paperWidth = 384, int pageHeight = 1662, int codepage = 437, InternationalCharacter internationalchar = InternationalCharacter.USA)
        {
            InternalInit();

            _targetDevice = initialDevice;

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

            PaperWidth = paperWidth;
            MaxHeight = pageHeight;

            _defaultlinespacing = CurrentSbcsFontInfo.height == 9 ? 12 : 30;

            _defaultintenationalCharacter = internationalchar;
            if (EscPosDecoder.PrtESCtCodePage.ContainsValue(codepage))
            {
                _defaultcodepage = codepage;
                _defaultcodetable = (CodeTable)Enum.ToObject(typeof(CodeTable), EscPosDecoder.PrtESCtCodePage.FirstOrDefault(x => x.Value == codepage).Key);
                if (_defaultcodepage < 0x100)
                {
                    _defaultembedded = GetEmbeddedESCtCodePage(_defaultcodepage);
                }
                else
                {
                    _defaultencoding = Encoding.GetEncoding(_defaultcodepage);
                }
            }
            else
            {
                _defaultcodepage = 437;
                _defaultcodetable = CodeTable.PC437;
            }
            if ((byte)_defaultintenationalCharacter != 0)
            {
                _defaultreplaced = b_StringESCRICS[(byte)_defaultintenationalCharacter];
            }
        }

        //--------------------------------------

        internal void InternalInit()
        {
            _BatchPrint = BatchMode.NormalDisable;
            _PageMode = false;
            _LeftMargin = 0;
            _PrintAreaWidth = PaperWidth;
            _Justification = Alignment.Left;
            _TabStops = Array.Empty<byte>();
            _PageModeArea = new Rectangle(0, 0, PaperWidth, MaxHeight);
            _PageDirection = Direction.Normal;
            _CharacterCodeTable = _defaultcodetable;
            _prtcodepage = _defaultcodepage;
            _prtencoding = _defaultencoding;
            _prtembedded = _defaultembedded;
            _prtreplaced = _defaultreplaced;
            _InternationalCharacertSet = _defaultintenationalCharacter;
            _KanjiMode = false;
            _UTF8Mode = false;
            _SBCSFontType = FontType.A;
            _FontPriority1st = FontPriority.ANK;
            _FontPriority2nd = FontPriority.Japanese;
            _KanjiFontType = FontType.A;
            _DownloadCharacterSet = false;
            _PrintColor = DualColor.Black;
            _ForegroundColor = MultiColor.Color1;
            _BackgroundColor = MultiColor.None;
            _ShadingMode.shadeMode = ShadeMode.Off;
            _ShadingMode.color = MultiColor.None;
            _CharScale.width = 1;   // The range of values is 1 to 8, and the corresponding actual setting value is 0 to 7.
            _CharScale.height = 1;  // The range of values is 1 to 8, and the corresponding actual setting value is 0 to 7.
            _StandardModeLineSpacing = _defaultlinespacing; // 30 or 12
            _PageModeLineSpacing = _defaultlinespacing;     // 30 or 12
            _StandardModeRightSpacing = 0;
            _PageModeRightSpacing = 0;
            _StandardModeKanjiSideSpacing.left = 0;
            _StandardModeKanjiSideSpacing.right = 0;
            _PageModeKanjiSideSpacing.left = 0;
            _PageModeKanjiSideSpacing.right = 0;
            _DoubleStrike = false;
            _Emphasis = false;
            _Reverse = false;
            _Shading = false;
            _Smoothing = false;
            _SBCSUnderline = Underline.Off;
            _KanjiUnderline = Underline.Off;
            _UpsideDown = false;
            _Right90 = Right90gap.Off;
            _PaperEndSensor = PaperSensor.None;
            _PrintStopSensor = PaperSensor.None;
            _StandardModeUnidirectionalPrint = false;
            _PageModeUnidirectionalPrint = false;

            _PanelButton = true;
            _AutomaticStatusBackOption = false;
            _ASBNotify = ASBcommand.None;

            _D1BarcodeText = HumanReadableIndicator.None;
            _D1BarcodeTextFontType = FontType.A;
            _D1BarcodeHeight = 162;
            _D1BarcodeModuleWidth = 3;
            _D1BarcodeData = new();
            _PDF417Columns = 0;
            _PDF417Rows = 0;
            _PDF417ModuleWidth = 3;
            _PDF417RowHeight = 3;
            _PDF417ErrorCollectionLevel = 1;
            _PDF417Options = 0;
            _PDF417Data = Array.Empty<byte>();
            _QRCodeModel = QRModel.Model2;
            _QRCodeModuleSize = 3;
            _QRCodeErrorCollectionLevel = QRErrorCollectionLevel.L;
            _QRCodeData = Array.Empty<byte>();
            _MaxiCodeMode = MaxiCode.Mode2;
            _MaxiCodeData = Array.Empty<byte>();
            _D2GS1ModuleWidth = 2;
            _D2GS1ExpandedStackedMaxWidth = 160;// TM-m10, TM-m30, TM-m30II, TM-m30II-H, TM-m30II-S, TM-m30II-SL, TM-P20, TM-P60, TM-T20, TM-T20II, TM-T20III, TM-T20X:
                                                // 141 // TM-L90, TM-P60II, TM-P80, TM-T70II, TM-T88V, TM-T88VI, TM-T90II:
            _D2GS1Data = new();
            _CompositeCodeModuleWidth = 2;
            _CompositeCodeExpandedStackedMaxWidth = 160;// TM-m10, TM-m30, TM-m30II, TM-m30II-H, TM-m30II-S, TM-m30II-SL, TM-P20, TM-P60, TM-T20, TM-T20II, TM-T20III, TM-T20X:
                                                        // 141 // TM-L90, TM-P60II, TM-P80, TM-T70II, TM-T88V, TM-T88VI, TM-T90II:
            _CompositeCodeTextFont = CompositeFontType.None;
            _AztecCodeModeLayers.codeMode = AztecCodeMode.FullRange;
            _AztecCodeModeLayers.layers = 0;
            _AztecCodeModuleSize = 3;
            _AztecCodeErrorCollectionLevel = 23;
            _AztecCodeData = Array.Empty<byte>();
            _DataMatrixAttribute.matrixType = DataMatrixType.Square;
            _DataMatrixAttribute.rows = 0;
            _DataMatrixAttribute.columns = 0;
            _DataMatrixModuleSize = 3;
            _DataMatrixData = Array.Empty<byte>();

            _UserSettingMode = false;
        }

        public void Initialize()
        {
            InternalInit();

            byte[] cmd = { 0x1B, 0x40 };
            CommandList.Add(new EscPosCmd(EscPosCmdType.EscInitialize, cmd));
        }

        //--------------------------------------

        public void Printables(string buffer)
        {
            List<byte> listwork = new();
            if (_UTF8Mode)
            {
                listwork.AddRange(_utf8encoding.GetBytes(buffer));
            }
            else if (_prtcodepage < 0x100)
            {
                listwork.AddRange(buffer.SelectMany<char, byte>(c => (_prtreplaced.ContainsKey(c) ? _prtreplaced[c] : _prtembedded[c])));
            }
            else
            {
                listwork.AddRange(buffer.SelectMany<char, byte>(c => ((byte)_InternationalCharacertSet == 0) ? _prtencoding.GetBytes(c.ToString()) : (_prtreplaced.ContainsKey(c) ? _prtreplaced[c] : _prtencoding.GetBytes(c.ToString()))));
            }
            Printables(listwork.ToArray<byte>());
        }

        public void Printables(byte[] buffer)
        {
            List<EscPosCmd> escposlist = _escPosTokenizer.Scan(buffer);
            CommandList.AddRange(escposlist);
        }

        //--------------------------------------

        public enum BatchMode
        {
            NormalDisable = 0x3030,
            NormalEnable = 0x3031,
            ReverseDisable = 0x3130,
            ReverseEnable = 0x3131
        }

        private BatchMode _BatchPrint;

        public BatchMode BatchPrint
        {
            get { return _BatchPrint; }
            set
            {
                _BatchPrint = value;
                byte[] cmd = { 0x1B, 0x28, 0x59, 0x02, 0x00, (byte)value, (byte)((int)value >> 8) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscSpecifyBatchPrint, cmd));
            }
        }

        //--------------------------------------

        public enum DualColor
        {
            Black = '0',
            Red = '1',
        }

        private DualColor _PrintColor;

        public DualColor PrintColor
        {
            get { return _PrintColor; }
            set
            {
                _PrintColor = value;
                byte[] cmd = { 0x1B, 0x72, (byte)((int)value) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscSelectPrinterColor, cmd));
            }
        }

        //--------------------------------------

        public enum MultiColor
        {
            None = '0',
            Color1 = '1',
            Color2 = '2',
            Color3 = '3',
        }

        private MultiColor _ForegroundColor;

        public MultiColor ForegroundColor
        {
            get { return _ForegroundColor; }
            set
            {
                _ForegroundColor = value;
                byte[] cmd = { 0x1C, 0x28, 0x4E, 0x02, 0x00, 0x30, (byte)value };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsSetCharacterColor, cmd));
            }
        }

        private MultiColor _BackgroundColor;

        public MultiColor BackgroundColor
        {
            get { return _BackgroundColor; }
            set
            {
                _BackgroundColor = value;
                byte[] cmd = { 0x1C, 0x28, 0x4E, 0x02, 0x00, 0x31, (byte)value };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsSetBackgroundColor, cmd));
            }
        }

        public enum ShadeMode
        {
            Off = '0',
            On = '1'
        }

        public class ShadeConfig
        {
            public ShadeMode shadeMode;
            public MultiColor color;
        }

        private ShadeConfig _ShadingMode = new();

        public ShadeConfig ShadingMode
        {
            get { return _ShadingMode; }
            set
            {
                _ShadingMode = value;
                byte[] cmd = { 0x1C, 0x28, 0x4E, 0x02, 0x00, 0x31, (byte)value.shadeMode, (byte)value.color };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsTurnShadingMode, cmd));
            }
        }

        //--------------------------------------

        public class Scale
        {
            public int width;
            public int height;
        }

        private Scale _CharScale = new();

        public Scale CharScale
        {
            get { return _CharScale; }
            set
            {
                if (((value.width < 1) || (value.width > 8))
                  && ((value.height < 1) || (value.height > 8))
                )
                {
                    return;
                }
                if ((value.width >= 1) && (value.width <= 8))
                {
                    _CharScale.width = value.width;
                }
                if ((value.height >= 1) && (value.height <= 8))
                {
                    _CharScale.height = value.height;
                }
                byte[] cmd = { 0x1D, 0x21, (byte)(((_CharScale.width - 1) << 4) + (_CharScale.height - 1)) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsSelectCharacterSize, cmd));
            }
        }

        //--------------------------------------

        private int _StandardModeLineSpacing;
        private int _PageModeLineSpacing;

        public int LineSpacing
        {
            get { return _PageMode ? _PageModeLineSpacing : _StandardModeLineSpacing; }
            set
            {
                int iwork = value <= 0xFF ? value : 0xFF;
                if (_PageMode)
                {
                    _PageModeLineSpacing = iwork;
                }
                else
                {
                    _StandardModeLineSpacing = iwork;
                }
                byte[] cmd = { 0x1B, 0x33, (byte)iwork };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscLineSpacing, cmd));
            }
        }

        public int DefaultLineSpacing
        {
            get { return _defaultlinespacing; }
            set
            {
                if (_PageMode)
                {
                    _PageModeLineSpacing = _defaultlinespacing;
                }
                else
                {
                    _StandardModeLineSpacing = _defaultlinespacing;
                }
                byte[] cmd = { 0x1B, 0x32 };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscSelectDefaultLineSpacing, cmd));
            }
        }

        //--------------------------------------

        private int _StandardModeRightSpacing;
        private int _PageModeRightSpacing;

        public int RightSpacing
        {
            get { return _PageMode ? _PageModeRightSpacing : _StandardModeRightSpacing; }
            set
            {
                int iwork = value <= 0xFF ? value : 0xFF;
                if (_PageMode)
                {
                    _PageModeRightSpacing = iwork;
                }
                else
                {
                    _StandardModeRightSpacing = iwork;
                }
                byte[] cmd = { 0x1B, 0x20, (byte)iwork };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscRightSideSpacing, cmd));
            }
        }

        //--------------------------------------

        public class SideSpacing
        {
            public int left;
            public int right;
        }

        private SideSpacing _StandardModeKanjiSideSpacing = new();
        private SideSpacing _PageModeKanjiSideSpacing = new();

        public SideSpacing KanjiSideSpacing
        {
            get { return _PageMode ? _PageModeKanjiSideSpacing : _StandardModeKanjiSideSpacing; }
            set
            {
                SideSpacing swork = new();
                swork.left = value.left <= 0xFF ? value.left : 0xFF;
                swork.right = value.right <= 0xFF ? value.right : 0xFF;
                if (_PageMode)
                {
                    _PageModeKanjiSideSpacing = swork;
                }
                else
                {
                    _StandardModeKanjiSideSpacing = swork;
                }
                byte[] cmd = { 0x1C, 0x53, (byte)swork.left, (byte)swork.right };
                CommandList.Add(new EscPosCmd(EscPosCmdType.FsSetKanjiCharacerSpacing, cmd));
            }
        }

        //--------------------------------------

        private Boolean _DoubleStrike;

        public Boolean DoubleStrike
        {
            get { return _DoubleStrike; }
            set
            {
                _DoubleStrike = value;
                byte[] cmd = { 0x1B, 0x47, (byte)(value ? 0x31 : 0x30) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscTurnDoubleStrikeMode, cmd));
            }
        }

        //--------------------------------------

        private Boolean _Emphasis;

        public Boolean Emphasis
        {
            get { return _Emphasis; }
            set
            {
                _Emphasis = value;
                byte[] cmd = { 0x1B, 0x45, (byte)(value ? 0x31 : 0x30) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscTurnEmphasizedMode, cmd));
            }
        }

        //--------------------------------------

        private Boolean _Reverse;

        public Boolean Reverse
        {
            get { return _Reverse; }
            set
            {
                _Reverse = value;
                byte[] cmd = { 0x1D, 0x42, (byte)(value ? 0x31 : 0x30) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsTurnWhiteBlackReversePrintMode, cmd));
            }
        }

        //--------------------------------------

        private Boolean _Shading;

        public Boolean Shading
        {
            get { return _Shading; }
            set
            {
                _Shading = value;
                byte[] cmd = { 0x1D, 0x28, 0x4E, 0x03, 0x00, 0x32, (byte)(value ? 0x31 : 0x30), 0x30 };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsTurnShadingMode, cmd));
            }
        }

        //--------------------------------------

        private Boolean _Smoothing;

        public Boolean Smoothing
        {
            get { return _Smoothing; }
            set
            {
                _Smoothing = value;
                byte[] cmd = { 0x1D, 0x62, (byte)(value ? 0x31 : 0x30) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsTurnSmoothingMode, cmd));
            }
        }

        //--------------------------------------

        public enum Underline
        {
            Off = '0',
            On1dot = '1',
            On2dot = '2'
        }

        private Underline _SBCSUnderline;  // SBCS only

        public Underline SBCSUnderline
        {
            get { return _SBCSUnderline; }
            set
            {
                _SBCSUnderline = value;
                byte[] cmd = { 0x1B, 0x2D, (byte)((int)value) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscUnderlineMode, cmd));
            }
        }

        private Underline _KanjiUnderline;  // CJK MBCS only

        public Underline KanjiUnderline
        {
            get { return _KanjiUnderline; }
            set
            {
                _KanjiUnderline = value;
                byte[] cmd = { 0x1C, 0x2D, (byte)((int)value) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.FsTurnKanjiUnderlineMode, cmd));
            }
        }

        //--------------------------------------

        private Boolean _UpsideDown;  // StandardMode only

        public Boolean UpsideDown
        {
            get { return _UpsideDown; }
            set
            {
                _UpsideDown = value;
                byte[] cmd = { 0x1B, 0x7B, (byte)(_UpsideDown ? 0x31 : 0x30) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscTurnUpsideDownPrintMode, cmd));
            }
        }

        //--------------------------------------

        public enum Right90gap
        {
            Off = '0',
            Gap1dot = '1',
            Gap1_5dot = '2'
        }

        private Right90gap _Right90;  // StandardMode only

        public Right90gap Right90
        {
            get { return _Right90; }
            set
            {
                _Right90 = value;
                byte[] cmd = { 0x1B, 0x56, (byte)((int)value) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscTurn90digreeClockwiseRotationMode, cmd));
            }
        }

        //--------------------------------------

        private Boolean _PageModeUnidirectionalPrint;
        private Boolean _StandardModeUnidirectionalPrint;

        public Boolean UnidirectionalPrint
        {
            get { return _PageMode ? _PageModeUnidirectionalPrint : _StandardModeUnidirectionalPrint; }
            set
            {
                if (_PageMode)
                {
                    _PageModeUnidirectionalPrint = value;
                }
                else
                {
                    _StandardModeUnidirectionalPrint = value;
                }
                byte[] cmd = { 0x1B, 0x55, (byte)(value ? 0x31 : 0x30) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscTurnUnidirectionalPrintMode, cmd));
            }
        }

        //--------------------------------------

        public class MotionUnits
        {
            public int horizontal;
            public int vertical;
        }

        private MotionUnits _BasicPitch = new();

        public MotionUnits BasicPitch
        {
            get { return _BasicPitch; }
            set
            {
                if (((value.horizontal < 0) || (value.horizontal > 255))
                  && ((value.vertical < 0) || (value.vertical > 255))
                )
                {
                    return;
                }
                if ((value.horizontal >= 0) && (value.horizontal <= 255))
                {
                    _BasicPitch.horizontal = value.horizontal;
                }
                if ((value.vertical >= 0) && (value.vertical <= 255))
                {
                    _BasicPitch.vertical = value.vertical;
                }
                byte[] cmd = { 0x1D, 0x50, (byte)_BasicPitch.horizontal, (byte)_BasicPitch.vertical };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsSetHorizontalVerticalMotionUnits, cmd));
            }
        }

        //--------------------------------------
    }
}