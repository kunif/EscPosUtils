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

    public partial class EscPosEncoder
    {
        public enum HumanReadableIndicator
        {
            None = '0',
            Above = '1',
            Below = '2',
            Both = '3',
        }

        private HumanReadableIndicator _D1BarcodeText;

        public HumanReadableIndicator D1BarcodeText
        {
            get { return _D1BarcodeText; }
            set
            {
                _D1BarcodeText = value;
                byte[] cmd = { 0x1D, 0x48, (byte)value };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsSelectPrintPositionHRICharacters, cmd));
            }
        }

        //--------------------------------------

        private FontType _D1BarcodeTextFontType;

        public FontType D1BarcodeTextFontType
        {
            get { return _D1BarcodeTextFontType; }
            set
            {
                _D1BarcodeTextFontType = value;
                byte[] cmd = { 0x1D, 0x66, (byte)value };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsSelectFontHRICharacters, cmd));
            }
        }

        //--------------------------------------

        private int _D1BarcodeHeight;

        public int D1BarcodeHeight
        {
            get { return _D1BarcodeHeight; }
            set
            {
                int iwork = ((value >= 1) && (value <= 0xFF)) ? value : 0xFF;
                _D1BarcodeHeight = iwork;
                byte[] cmd = { 0x1D, 0x68, (byte)iwork };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsSetBarcodeHight, cmd));
            }
        }

        //--------------------------------------

        private int _D1BarcodeModuleWidth;

        public int D1BarcodeModuleWidth
        {
            get { return _D1BarcodeModuleWidth; }
            set
            {
                int iwork = ((value >= 2) && (value <= 6)) ? value : (((value >= 68) && (value <= 76)) ? value : _D1BarcodeModuleWidth);
                _D1BarcodeModuleWidth = iwork;
                byte[] cmd = { 0x1D, 0x77, (byte)iwork };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsSetBarcodeWidth, cmd));
            }
        }

        //--------------------------------------

        public enum BarcodeType
        {
            None = -1,
            UPCA = 65,
            UPCE = 66,
            EAN13 = 67,
            EAN8 = 68,
            CODE39 = 69,
            ITF = 70,
            CODABAR = 71,
            CODE93 = 72,
            CODE128 = 73,
            GS1_128 = 74,
            GS1DataBarOmnidirectional = 75,
            GS1DataBarTruncated = 76,
            GS1DataBarLimited = 77,
            GS1DataBarExpanded = 78,
            CODE128auto = 79,
        }

        public class BarcodeData
        {
            public BarcodeType barcodeType = BarcodeType.None;
            public byte[] data = Array.Empty<byte>();
        }

        private BarcodeData _D1BarcodeData = new();

        public BarcodeData D1BarcodeData
        {
            get { return _D1BarcodeData; }
            set
            {
                if ((value.barcodeType != BarcodeType.None) && (value.data.Length > 0))
                {
                    _D1BarcodeData = value;
                    byte[] cmd = { 0x1D, 0x6B, (byte)value.barcodeType, (byte)value.data.Length };
                    CommandList.Add(new EscPosCmd(EscPosCmdType.GsPrintBarcodeSpecifiedLength, cmd.Concat(value.data).ToArray()));
                }
                else
                {
                    _D1BarcodeData.barcodeType = BarcodeType.None;
                    _D1BarcodeData.data = Array.Empty<byte>();
                }
            }
        }

        //--------------------------------------

        private int _PDF417Columns;

        public int PDF417Columns
        {
            get { return _PDF417Columns; }
            set
            {
                int iwork = ((value >= 0) && (value <= 30)) ? value : _PDF417Columns;
                _PDF417Columns = iwork;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x41, (byte)iwork };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsPDF417SetNumberOfColumns, cmd));
            }
        }

        //--------------------------------------

        private int _PDF417Rows;

        public int PDF417Rows
        {
            get { return _PDF417Rows; }
            set
            {
                int iwork = ((value == 0) || ((value >= 3) && (value <= 90))) ? value : _PDF417Rows;
                _PDF417Rows = iwork;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x42, (byte)iwork };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsPDF417SetNumberOfRows, cmd));
            }
        }

        //--------------------------------------

        private int _PDF417ModuleWidth;

        public int PDF417ModuleWidth
        {
            get { return _PDF417ModuleWidth; }
            set
            {
                int iwork = ((value >= 2) && (value <= 8)) ? value : _PDF417ModuleWidth;
                _PDF417ModuleWidth = iwork;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x43, (byte)iwork };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsPDF417SetWidthOfModule, cmd));
            }
        }

        //--------------------------------------

        private int _PDF417RowHeight;

        public int PDF417RowHeight
        {
            get { return _PDF417RowHeight; }
            set
            {
                int iwork = ((value >= 2) && (value <= 8)) ? value : _PDF417RowHeight;
                _PDF417RowHeight = iwork;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x44, (byte)iwork };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsPDF417SetRowHeight, cmd));
            }
        }

        //--------------------------------------

        private int _PDF417ErrorCollectionLevel;

        public int PDF417ErrorCollectionLevel
        {
            get { return _PDF417ErrorCollectionLevel; }
            set
            {
                int iwork = (((value >= 1) && (value <= 40)) || ((value >= 48) && (value <= 56))) ? value : _PDF417ErrorCollectionLevel;
                _PDF417ErrorCollectionLevel = iwork;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x30, 0x45, (byte)((iwork >= 48) ? 48 : 49), (byte)iwork };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsPDF417SetErrorCollectionLevel, cmd));
            }
        }

        //--------------------------------------

        private int _PDF417Options;

        public int PDF417Options
        {
            get { return _PDF417Options; }
            set
            {
                int iwork = ((value >= 0) && (value <= 1)) ? value : _PDF417Options;
                _PDF417Options = iwork;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x46, (byte)iwork };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsPDF417SelectOptions, cmd));
            }
        }

        //--------------------------------------

        private byte[] _PDF417Data = Array.Empty<byte>();

        public byte[] PDF417Data
        {
            get { return _PDF417Data; }
            set
            {
                if ((value != null) && (value.Length >= 1) && (value.Length <= 65532))
                {
                    int iwork = value.Length + 3;
                    _PDF417Data = value;
                    byte[] cmd = { 0x1D, 0x28, 0x6B, (byte)iwork, (byte)(iwork >> 8), 0x30, 0x50, 0x30 };
                    cmd = cmd.Concat(value).ToArray();
                    CommandList.Add(new EscPosCmd(EscPosCmdType.GsPDF417StoreData, cmd));
                }
            }
        }

        //--------------------------------------

        public enum QRModel
        {
            Model1 = '1',
            Model2 = '2',
            MicroQR = '3',
        }

        private QRModel _QRCodeModel;

        public QRModel QRCodeModel
        {
            get { return _QRCodeModel; }
            set
            {
                _QRCodeModel = value;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x41, (byte)value, 0x00 };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsQRCodeSelectModel, cmd));
            }
        }

        //--------------------------------------

        private int _QRCodeModuleSize;

        public int QRCodeModuleSize
        {
            get { return _QRCodeModuleSize; }
            set
            {
                int iwork = ((value >= 1) && (value <= 16)) ? value : _QRCodeModuleSize;
                _QRCodeModuleSize = iwork;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, (byte)iwork };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsQRCodeSetSizeOfModule, cmd));
            }
        }

        //--------------------------------------

        public enum QRErrorCollectionLevel
        {
            L = '0',
            M = '1',
            Q = '2',
            H = '3'
        }

        private QRErrorCollectionLevel _QRCodeErrorCollectionLevel;

        public QRErrorCollectionLevel QRCodeErrorCollectionLevel
        {
            get { return _QRCodeErrorCollectionLevel; }
            set
            {
                _QRCodeErrorCollectionLevel = value;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, (byte)value };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsQRCodeSetErrorCollectionLevel, cmd));
            }
        }

        //--------------------------------------

        private byte[] _QRCodeData = Array.Empty<byte>();

        public byte[] QRCodeData
        {
            get { return _QRCodeData; }
            set
            {
                if ((value != null) && (value.Length >= 1) && (value.Length <= 7089))
                {
                    int iwork = value.Length + 3;
                    _QRCodeData = value;
                    byte[] cmd = { 0x1D, 0x28, 0x6B, (byte)iwork, (byte)(iwork >> 8), 0x31, 0x50, 0x30 };
                    cmd = cmd.Concat(value).ToArray();
                    CommandList.Add(new EscPosCmd(EscPosCmdType.GsQRCodeStoreData, cmd));
                }
            }
        }

        //--------------------------------------

        public enum MaxiCode
        {
            Mode2 = '2',
            Mode3 = '3',
            Mode4 = '4',
            Mode5 = '5',
            Mode6 = '6'
        }

        private MaxiCode _MaxiCodeMode;

        public MaxiCode MaxiCodeMode
        {
            get { return _MaxiCodeMode; }
            set
            {
                _MaxiCodeMode = value;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x32, 0x41, (byte)value };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsMaxiCodeSelectMode, cmd));
            }
        }

        //--------------------------------------

        private byte[] _MaxiCodeData = Array.Empty<byte>();

        public byte[] MaxiCodeData
        {
            get { return _MaxiCodeData; }
            set
            {
                if ((value != null) && (value.Length >= 1) && (value.Length <= 138))
                {
                    int iwork = value.Length + 3;
                    _MaxiCodeData = value;
                    byte[] cmd = { 0x1D, 0x28, 0x6B, (byte)iwork, (byte)(iwork >> 8), 0x32, 0x50, 0x30 };
                    cmd = cmd.Concat(value).ToArray();
                    CommandList.Add(new EscPosCmd(EscPosCmdType.GsMaxiCodeStoreData, cmd));
                }
            }
        }

        //--------------------------------------

        private int _D2GS1ModuleWidth;

        public int D2GS1ModuleWidth
        {
            get { return _D2GS1ModuleWidth; }
            set
            {
                int iwork = ((value >= 2) && (value <= 8)) ? value : _D2GS1ModuleWidth;
                _D2GS1ModuleWidth = iwork;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x33, 0x43, (byte)iwork };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsD2GS1DBSetWidthOfModule, cmd));
            }
        }

        //--------------------------------------

        private int _D2GS1ExpandedStackedMaxWidth;

        public int D2GS1ExpandedStackedMaxWidth
        {
            get { return _D2GS1ExpandedStackedMaxWidth; }
            set
            {
                int iwork = ((value == 0) || ((value >= 106) && (value <= 3952))) ? value : _D2GS1ExpandedStackedMaxWidth;
                _D2GS1ExpandedStackedMaxWidth = iwork;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x33, 0x47, (byte)iwork, (byte)(iwork >> 8) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsD2GS1DBSetExpandStackedMaximumWidth, cmd));
            }
        }

        //--------------------------------------

        public enum D2GS1BarcodeType
        {
            None = -1,
            GS1DataBarStacked = 72,
            GS1DataBarStackedOmnidirectional = 73,
            GS1DataBarExpandedStacked = 76,
        }

        public class D2GS1BarcdeData
        {
            public D2GS1BarcodeType d2GS1Type = D2GS1BarcodeType.None;
            public byte[] data = Array.Empty<byte>();
        }

        private D2GS1BarcdeData _D2GS1Data = new();

        public D2GS1BarcdeData D2GS1Data
        {
            get { return _D2GS1Data; }
            set
            {
                if ((value.d2GS1Type != D2GS1BarcodeType.None) && (value.data.Length >= 1) && (value.data.Length <= 256))
                {
                    int iwork = value.data.Length + 4;
                    _D2GS1Data = value;
                    byte[] cmd = { 0x1D, 0x28, 0x6B, (byte)iwork, (byte)(iwork >> 8), 0x33, 0x50, 0x30 };
                    CommandList.Add(new EscPosCmd(EscPosCmdType.GsD2GS1DBStoreData, cmd.Concat(value.data).ToArray()));
                }
                else
                {
                    _D2GS1Data.d2GS1Type = D2GS1BarcodeType.None;
                    _D2GS1Data.data = Array.Empty<byte>();
                }
            }
        }

        //--------------------------------------

        private int _CompositeCodeModuleWidth;

        public int CompositeCodeModuleWidth
        {
            get { return _CompositeCodeModuleWidth; }
            set
            {
                int iwork = ((value >= 2) && (value <= 8)) ? value : _CompositeCodeModuleWidth;
                _CompositeCodeModuleWidth = iwork;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x34, 0x43, (byte)iwork };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsCompositeSetWidthOfModule, cmd));
            }
        }

        //--------------------------------------

        private int _CompositeCodeExpandedStackedMaxWidth;

        public int CompositeCodeExpandedStackedMaxWidth
        {
            get { return _CompositeCodeExpandedStackedMaxWidth; }
            set
            {
                int iwork = ((value == 0) || ((value >= 106) && (value <= 3952))) ? value : _CompositeCodeExpandedStackedMaxWidth;
                _CompositeCodeExpandedStackedMaxWidth = iwork;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x34, 0x47, (byte)iwork, (byte)(iwork >> 8) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsCompositeSetExpandStackedMaximumWidth, cmd));
            }
        }

        //--------------------------------------

        public enum CompositeFontType
        {
            None = '0',
            A = '1',
            B = '2',
            C = '3',
            D = '4',
            E = '5',
            SpecialA = 'a',
            SpecialB = 'b'
        }

        private CompositeFontType _CompositeCodeTextFont;

        public CompositeFontType CompositeCodeTextFont
        {
            get { return _CompositeCodeTextFont; }
            set
            {
                _CompositeCodeTextFont = value;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x38, 0x48, (byte)value };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsCompositeSelectHRICharacterFont, cmd));
            }
        }

        //--------------------------------------

        public enum CompositeCodeType
        {
            None = -1,
            EAN8 = 0x3041,
            EAN13 = 0x3042,
            UPCA = 0x3043,
            UPCE_6 = 0x3044,
            UPCE_11 = 0x3045,
            GS1DataBarOmnidirectional = 0x3046,
            GS1DataBarTruncated = 0x3047,
            GS1DataBarStacked = 0x3048,
            GS1DataBarStackedOmnidirectional = 0x3049,
            GS1DataBarLimited = 0x304A,
            GS1DataBarExpanded = 0x304B,
            GS1DataBarExpandedStacked = 0x304C,
            GS1_128 = 0x304D,
            CC_auto = 0x3141,
            CC_C = 0x3142
        }

        public class CompositeCodeData
        {
            public CompositeCodeType compositeCodeType = CompositeCodeType.None;
            public byte[] data = Array.Empty<byte>();
        }

        private CompositeCodeData _CompositeCodeData = new();

        public CompositeCodeData CompositeBarcodeData
        {
            get { return _CompositeCodeData; }
            set
            {
                if ((value.compositeCodeType != CompositeCodeType.None) && (value.data.Length > 0))
                {
                    int iwork = value.data.Length + 5;
                    _CompositeCodeData = value;
                    byte[] cmd = { 0x1D, 0x28, 0x6B, (byte)iwork, (byte)(iwork >> 8), (byte)((int)value.compositeCodeType >> 8), (byte)value.compositeCodeType };
                    CommandList.Add(new EscPosCmd(EscPosCmdType.GsCompositeStoreData, cmd.Concat(value.data).ToArray()));
                }
                else
                {
                    _CompositeCodeData.compositeCodeType = CompositeCodeType.None;
                    _CompositeCodeData.data = Array.Empty<byte>();
                }
            }
        }

        //--------------------------------------

        public enum AztecCodeMode
        {
            FullRange = '0',
            Compact = '1'
        }

        public class AztecModeLayers
        {
            public AztecCodeMode codeMode;
            public int layers;
        }

        private AztecModeLayers _AztecCodeModeLayers = new();

        public AztecModeLayers AztecCodeModeLayers
        {
            get { return _AztecCodeModeLayers; }
            set
            {
                if (((value.codeMode == AztecCodeMode.FullRange) && ((value.layers == 0) || ((value.layers >= 4) && (value.layers <= 32))))
                  || ((value.codeMode == AztecCodeMode.Compact) && ((value.layers >= 0) && (value.layers <= 4)))
                )
                {
                    _AztecCodeModeLayers = value;
                    byte[] cmd = { 0x1D, 0x28, 0x6B, 0x05, 0x00, 0x35, 0x42, (byte)value.codeMode, (byte)value.layers };
                    CommandList.Add(new EscPosCmd(EscPosCmdType.GsAztecCodeSetModeTypesAndDataLayer, cmd));
                }
            }
        }

        //--------------------------------------

        private int _AztecCodeModuleSize;

        public int AztecCodeModuleSize
        {
            get { return _AztecCodeModuleSize; }
            set
            {
                int iwork = ((value >= 2) && (value <= 16)) ? value : _AztecCodeModuleSize;
                _AztecCodeModuleSize = iwork;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x35, 0x43, (byte)iwork };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsAztecCodeSetSizeOfModule, cmd));
            }
        }

        //--------------------------------------

        private int _AztecCodeErrorCollectionLevel;

        public int AztecCodeErrorCollectionLevel
        {
            get { return _AztecCodeErrorCollectionLevel; }
            set
            {
                int iwork = ((value >= 5) && (value <= 95)) ? value : _AztecCodeErrorCollectionLevel;
                _AztecCodeErrorCollectionLevel = iwork;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x35, 0x45, (byte)iwork };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsAztecCodeSetErrorCollectionLevel, cmd));
            }
        }

        //--------------------------------------

        private byte[] _AztecCodeData = Array.Empty<byte>();

        public byte[] AztecCodeData
        {
            get { return _AztecCodeData; }
            set
            {
                if ((value != null) && (value.Length >= 1) && (value.Length <= 3832))
                {
                    int iwork = value.Length + 3;
                    _AztecCodeData = value;
                    byte[] cmd = { 0x1D, 0x28, 0x6B, (byte)iwork, (byte)(iwork >> 8), 0x35, 0x50, 0x30 };
                    cmd = cmd.Concat(value).ToArray();
                    CommandList.Add(new EscPosCmd(EscPosCmdType.GsAztecCodeStoreData, cmd));
                }
            }
        }

        //--------------------------------------

        public enum DataMatrixType
        {
            Square = '0',
            Rectangle = '1'
        }

        public class DataMatrixMode
        {
            public DataMatrixType matrixType = DataMatrixType.Square;
            public int rows;
            public int columns;
        }

        private DataMatrixMode _DataMatrixAttribute = new();

        public DataMatrixMode DataMatrixAttribute
        {
            get { return _DataMatrixAttribute; }
            set
            {
                _DataMatrixAttribute = value;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x05, 0x00, 0x36, 0x42, (byte)value.matrixType, (byte)value.rows, (byte)value.columns };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsDataMatrixSetSymbolTypeColumnsRows, cmd));
            }
        }

        //--------------------------------------

        private int _DataMatrixModuleSize;

        public int DataMatrixModuleSize
        {
            get { return _DataMatrixModuleSize; }
            set
            {
                int iwork = ((value >= 2) && (value <= 16)) ? value : _DataMatrixModuleSize;
                _DataMatrixModuleSize = iwork;
                byte[] cmd = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x36, 0x43, (byte)iwork };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsDataMatrixSetSizeOfModule, cmd));
            }
        }

        //--------------------------------------

        private byte[] _DataMatrixData = Array.Empty<byte>();

        public byte[] DataMatrixData
        {
            get { return _DataMatrixData; }
            set
            {
                if ((value != null) && (value.Length >= 1) && (value.Length <= 3116))
                {
                    int iwork = value.Length + 3;
                    _DataMatrixData = value;
                    byte[] cmd = { 0x1D, 0x28, 0x6B, (byte)iwork, (byte)(iwork >> 8), 0x36, 0x50, 0x30 };
                    cmd = cmd.Concat(value).ToArray();
                    CommandList.Add(new EscPosCmd(EscPosCmdType.GsDataMatrixStoreData, cmd));
                }
            }
        }

        //--------------------------------------

        public enum D2BarcodeType
        {
            PDF417 = '0',
            QRCode = '1',
            MaxiCode = '2',
            D2GS1DataBar = '3',
            CompositeCode = '4',
            AztecCode = '5',
            DataMatrix = '6'
        }

        public void D2BarcodePrintSymbolData(D2BarcodeType d2BarcodeType)
        {
#pragma warning disable CS8524 // The switch expression does not handle all possible inputs when it does
            EscPosCmdType cmdType = d2BarcodeType switch
            {
                D2BarcodeType.PDF417 => EscPosCmdType.GsPDF417PrintSymbolData,
                D2BarcodeType.QRCode => EscPosCmdType.GsQRCodePrintSymbolData,
                D2BarcodeType.MaxiCode => EscPosCmdType.GsMaxiCodePrintSymbolData,
                D2BarcodeType.D2GS1DataBar => EscPosCmdType.GsD2GS1DBPrintSymbolData,
                D2BarcodeType.CompositeCode => EscPosCmdType.GsCompositePrintSymbolData,
                D2BarcodeType.AztecCode => EscPosCmdType.GsAztecCodePrintSymbolData,
                D2BarcodeType.DataMatrix => EscPosCmdType.GsDataMatrixPrintSymbolData,
            };
#pragma warning restore CS8524 // The switch expression does not handle all possible inputs when it does
            byte[] cmd = { 0x1D, 0x28, 0x6B, 0x03, 0x00, (byte)d2BarcodeType, 0x51, 0x30 };
            CommandList.Add(new EscPosCmd(cmdType, cmd));
        }

        public void D2BarcodeTransmitSizeInformation(D2BarcodeType d2BarcodeType)
        {
#pragma warning disable CS8524 // The switch expression does not handle all possible inputs when it does
            EscPosCmdType cmdType = d2BarcodeType switch
            {
                D2BarcodeType.PDF417 => EscPosCmdType.GsPDF417TransmitSizeInformation,
                D2BarcodeType.QRCode => EscPosCmdType.GsQRCodeTransmitSizeInformation,
                D2BarcodeType.MaxiCode => EscPosCmdType.GsMaxiCodeTransmitSizeInformation,
                D2BarcodeType.D2GS1DataBar => EscPosCmdType.GsD2GS1DBTransmitSizeInformation,
                D2BarcodeType.CompositeCode => EscPosCmdType.GsCompositeTransmitSizeInformation,
                D2BarcodeType.AztecCode => EscPosCmdType.GsAztecCodeTransmitSizeInformation,
                D2BarcodeType.DataMatrix => EscPosCmdType.GsDataMatrixTransmitSizeInformation,
            };
#pragma warning restore CS8524 // The switch expression does not handle all possible inputs when it does
            byte[] cmd = { 0x1D, 0x28, 0x6B, 0x03, 0x00, (byte)d2BarcodeType, 0x52, 0x3 };
            CommandList.Add(new EscPosCmd(cmdType, cmd));
        }

        //--------------------------------------
    }
}