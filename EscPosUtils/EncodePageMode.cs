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
    using System.Drawing;
    using System.Linq;

    public partial class EscPosEncoder
    {
        //--------------------------------------

        public int PaperWidth { get; private set; }
        public int MaxHeight { get; private set; }

        //--------------------------------------

        private Boolean _PageMode;

        public Boolean PageMode
        {
            get { return _PageMode; }
            set
            {
                if (_PageMode != value)
                {
                    _PageMode = value;
                    byte[] cmd = { 0x1B, (byte)(_PageMode ? 0x4C : 0x53) };
                    if (_PageMode)
                    {
                        CommandList.Add(new EscPosCmd(EscPosCmdType.EscSelectPageMode, cmd));
                    }
                    else
                    {
                        _PageModeArea = new Rectangle(0, 0, PaperWidth, MaxHeight);
                        CommandList.Add(new EscPosCmd(EscPosCmdType.EscSelectStandardMode, cmd));
                    }
                }
            }
        }

        //--------------------------------------

        public enum PageAction
        {
            PrintAndContinue,
            PrintAndToStandardMode,
            ClearAndContinue,
            ClearAndToStandardMode,
            StandardModeEndJob
        }

        public void PageModeData(PageAction action)
        {
            byte[] cmd = { 0x00 };
            if (_PageMode && (action != PageAction.StandardModeEndJob))
            {
                switch (action)
                {
                    case PageAction.PrintAndContinue:
                        cmd = new byte[] { 0x1B, 0x0C };
                        CommandList.Add(new EscPosCmd(EscPosCmdType.EscPageModeFormFeed, cmd));
                        break;

                    case PageAction.PrintAndToStandardMode:
                        cmd[0] = 0x0C;
                        CommandList.Add(new EscPosCmd(EscPosCmdType.PrintAndReturnToStandardMode, cmd));
                        _PageMode = false;
                        _PageModeArea = new Rectangle(0, 0, PaperWidth, MaxHeight);
                        break;

                    case PageAction.ClearAndContinue:
                        cmd[0] = 0x18;
                        CommandList.Add(new EscPosCmd(EscPosCmdType.Cancel, cmd));
                        break;

                    case PageAction.ClearAndToStandardMode:
                        cmd = new byte[] { 0x1B, 0x53 };
                        CommandList.Add(new EscPosCmd(EscPosCmdType.EscSelectStandardMode, cmd));
                        _PageMode = false;
                        _PageModeArea = new Rectangle(0, 0, PaperWidth, MaxHeight);
                        break;
                }
            }
            else if (!_PageMode && (action == PageAction.StandardModeEndJob))
            {
                cmd[0] = 0x0C;
                CommandList.Add(new EscPosCmd(EscPosCmdType.EndOfJob, cmd));
            }
        }

        //--------------------------------------

        public enum BufferAction
        {
            Clear = '0',
            Print = '1'
        }

        public void MoveToBeginningOfLine(BufferAction action)
        {
            if (_PageMode)
            { return; }
            byte[] cmd = { 0x1D, 0x54, (byte)action };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsSetPrintPositionBeginningOfPrintLine, cmd));
        }

        //--------------------------------------

        public void HorizontalTab(int count)
        {
            if (count <= 0)
            { return; }
            for (int i = 0; i < count; i++)
            {
                byte[] cmd = { 0x09 };
                CommandList.Add(new EscPosCmd(EscPosCmdType.HorizontalTab, cmd));
            }
        }

        //--------------------------------------
        public enum NewLineType
        {
            LineFeed,
            CarriageReturn,
            LineFeedCarriageReturn,
            CarriageReturnLineFeed
        }

        public void NewLine(NewLineType newlineType, int count)
        {
            if (count <= 0)
            { return; }
#pragma warning disable CS8524 // The switch expression does not handle all possible inputs when it does
            EscPosCmdType cmdType = newlineType switch
            {
                NewLineType.LineFeed => EscPosCmdType.PrintAndLineFeed,
                NewLineType.CarriageReturn => EscPosCmdType.PrintAndCarriageReturn,
                NewLineType.LineFeedCarriageReturn => EscPosCmdType.PrintAndLineFeedCarriageReturn,
                NewLineType.CarriageReturnLineFeed => EscPosCmdType.PrintAndCarriageReturnLineFeed,
            };
#pragma warning restore CS8524 // The switch expression does not handle all possible inputs when it does
            byte[] cmd = { 0x00 };
            switch (newlineType)
            {
                case NewLineType.LineFeed:
                    cmd[0] = 0x0A;
                    break;

                case NewLineType.CarriageReturn:
                    cmd[0] = 0x0D;
                    break;

                case NewLineType.LineFeedCarriageReturn:
                    cmd = new byte[] { 0x0A, 0x0D };
                    break;

                case NewLineType.CarriageReturnLineFeed:
                    cmd = new byte[] { 0x0D, 0x0A };
                    break;
            }
            for (int i = 0; i < count; i++)
            {
                CommandList.Add(new EscPosCmd(cmdType, cmd));
            }
        }

        //--------------------------------------

        private int _LeftMargin;  // StandardMode only

        public int LeftMargin
        {
            get { return _LeftMargin; }
            set
            {
                if ((value >= 0) && (value <= 0xFFFF))
                {
                    _LeftMargin = (value > PaperWidth) ? PaperWidth : value;
                    byte[] cmd = { 0x1D, 0x4C, (byte)(_LeftMargin & 0xFF), (byte)((_LeftMargin >> 8) & 0xFF) };
                    CommandList.Add(new EscPosCmd(EscPosCmdType.GsSetLeftMargin, cmd));
                }
            }
        }

        //--------------------------------------

        private int _PrintAreaWidth;  // StandardMode only

        public int PrintAreaWidth
        {
            get { return _PrintAreaWidth; }
            set
            {
                if ((value >= 0) && (value <= 0xFFFF))
                {
                    _PrintAreaWidth = ((_LeftMargin + value) > PaperWidth) ? (PaperWidth - _LeftMargin) : value;
                    byte[] cmd = { 0x1D, 0x57, (byte)(_PrintAreaWidth & 0xFF), (byte)((_PrintAreaWidth >> 8) & 0xFF) };
                    CommandList.Add(new EscPosCmd(EscPosCmdType.GsSetPrintAreaWidth, cmd));
                }
            }
        }

        //--------------------------------------

        public enum AxisType
        {
            Horizontal,
            Vertical
        }

        public enum ValueSpecifyType
        {
            Absolute,
            Relative
        }

        public void MovePrintPosition(AxisType axis, ValueSpecifyType specifyType, int position)
        {
            if (((axis == AxisType.Vertical) && !_PageMode)
              || ((specifyType == ValueSpecifyType.Absolute) && ((position < 0) || (position > 0xFFFF)))
              || ((specifyType == ValueSpecifyType.Relative) && ((position < -32768) || (position > 32767)))
            )
            { return; }
            EscPosCmdType cmdType = EscPosCmdType.None;
            byte code1 = 0x00;
            byte code2 = 0x00;
            switch (axis)
            {
                case AxisType.Horizontal:
                    switch (specifyType)
                    {
                        case ValueSpecifyType.Absolute:
                            cmdType = EscPosCmdType.EscAbsoluteHorizontalPrintPosition;
                            code1 = 0x1B;
                            code2 = 0x24;
                            break;

                        case ValueSpecifyType.Relative:
                            cmdType = EscPosCmdType.EscSetRelativeHorizontalPrintPosition;
                            code1 = 0x1B;
                            code2 = 0x5C;
                            break;
                    }
                    break;

                case AxisType.Vertical:
                    switch (specifyType)
                    {
                        case ValueSpecifyType.Absolute:
                            cmdType = EscPosCmdType.GsSetAbsoluteVerticalPrintPositionInPageMode;
                            code1 = 0x1D;
                            code2 = 0x24;
                            break;

                        case ValueSpecifyType.Relative:
                            cmdType = EscPosCmdType.GsSetRelativeVerticalPrintPosition;
                            code1 = 0x1D;
                            code2 = 0x5C;
                            break;
                    }
                    break;
            }
            byte[] cmd = new byte[] { code1, code2, (byte)position, (byte)(position >> 8) };
            CommandList.Add(new EscPosCmd(cmdType, cmd));
        }

        //--------------------------------------

        public enum FeedType
        {
            Lines,
            Dots
        }

        public void PrintAndFeed(FeedType feedType, int number)
        {
            if ((number < -255) || (number > 255))
            { return; }
            Boolean normal = number >= 0;
            int iwork = normal ? number : number * -1;
            byte code = 0;
            EscPosCmdType cmdType = EscPosCmdType.None;
            switch (feedType)
            {
                case FeedType.Lines:
                    code = (byte)(normal ? 0x64 : 0x65);
                    cmdType = normal ? EscPosCmdType.EscPrintAndFeedNLines : EscPosCmdType.EscPrintAndReverseFeedNLines;
                    break;

                case FeedType.Dots:
                    code = (byte)(normal ? 0x4A : 0x4B);
                    cmdType = normal ? EscPosCmdType.EscPrintAndFeedPaper : EscPosCmdType.EscPrintAndReverseFeed;
                    break;
            }
            byte[] cmd = { 0x1B, code, (byte)iwork };
            CommandList.Add(new EscPosCmd(cmdType, cmd));
        }

        //--------------------------------------

        public enum Alignment
        {
            Left = '0',
            Center = '1',
            Right = '2'
        }

        private Alignment _Justification;  // StandardMode only

        public Alignment Justification
        {
            get { return _Justification; }
            set
            {
                _Justification = value;
                byte[] cmd = { 0x1B, 0x61, (byte)((int)value) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscSelectJustification, cmd));
            }
        }

        //--------------------------------------

        private byte[] _TabStops = Array.Empty<byte>();

        public byte[] TabStops
        {
            get { return _TabStops; }
            set
            {
                if (value != null)
                {
                    int iwork = (value.Length < 32) ? value.Length : 32;
                    byte[] tabs = new byte[iwork];
                    if (iwork != 0)
                    {
                        Array.Copy(value, 0, tabs, 0, iwork);
                    }
                    _TabStops = tabs;
                    byte[] cmd = { 0x1B, 0x44 };
                    cmd = cmd.Concat(tabs).ToArray();
                    if (iwork < 32)
                    {
                        cmd = cmd.Concat(new byte[] { 0x00 }).ToArray();
                    }
                    CommandList.Add(new EscPosCmd(EscPosCmdType.EscHorizontalTabPosition, cmd));
                }
            }
        }

        //--------------------------------------

        private System.Drawing.Rectangle _PageModeArea;

        public System.Drawing.Rectangle PageModeArea
        {
            get { return _PageModeArea; }
            set
            {
                if ((value.Top < 0) || (value.Top > 0xFFFF)
                 || (value.Left < 0) || (value.Left > 0xFFFF)
                 || (value.Width < 1) || (value.Width > 0xFFFF)
                 || (value.Height < 1) || (value.Height > 0xFFFF)
                )
                {
                    return;
                }
                _PageModeArea = value;
                byte[] cmd = { 0x1B, 0x57, (byte)(value.Left & 0xFF), (byte)((value.Left >> 8) & 0xFF), (byte)(value.Top & 0xFF), (byte)((value.Top >> 8) & 0xFF), (byte)(value.Width & 0xFF), (byte)((value.Width >> 8) & 0xFF), (byte)(value.Height & 0xFF), (byte)((value.Height >> 8) & 0xFF) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscSetPrintAreaInPageMode, cmd));
            }
        }

        //--------------------------------------

        public enum Direction
        {
#pragma warning disable CA1069 // Enums should not have duplicate values
            Normal = '0',
            Left90 = '1',
            Rotate180 = '2',
            Right90 = '3',
            TopToBottom = '0',
            LeftToRight = '1',
            BottomToTop = '2',
            RightToLeft = '3'
#pragma warning restore CA1069 // Enums should not have duplicate values
        }

        private Direction _PageDirection;

        public Direction PageDirection
        {
            get { return _PageDirection; }
            set
            {
                _PageDirection = value;
                byte[] cmd = { 0x1B, 0x54, (byte)((int)value) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscSelectPrintDirection, cmd));
            }
        }

        //--------------------------------------
    }
}