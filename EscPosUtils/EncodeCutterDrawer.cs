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
        //--------------------------------------

        public enum CutType
        {
            FullCutOnly = '0',
            PartialCutOnly = '1',
            FeedAndFullCut = 'A',
            FeedAndPartialCut = 'B',
            ReserveFeedAndFullCut = 'a',
            ReserveFeedAndPartialCut = 'b',
            FeedAndFullCutThenTopOfPaper = 'g',
            FeedAndPartialCutThenTopOfPaper = 'h'
        }

        public void CutPaper(CutType cutType, int feeddots)
        {
            if ((feeddots < 0) || (feeddots > 0xFF))
            { return; }
            byte[] cmd = { 0x1D, 0x56, (byte)cutType };
            if ((cutType != CutType.FullCutOnly) && (cutType != CutType.PartialCutOnly))
            {
                cmd = cmd.Concat(new[] { (byte)feeddots }).ToArray();
            }
#pragma warning disable CS8524 // The switch expression does not handle all possible inputs when it does
            EscPosCmdType cmdType = cutType switch
            {
                CutType.FullCutOnly => EscPosCmdType.GsPaperFullCut,
                CutType.PartialCutOnly => EscPosCmdType.GsPaperPartialCut,
                CutType.FeedAndFullCut => EscPosCmdType.GsPaperFeedAndFullCut,
                CutType.FeedAndPartialCut => EscPosCmdType.GsPaperFeedAndPartialCut,
                CutType.ReserveFeedAndFullCut => EscPosCmdType.GsPaperReservedFeedAndFullCut,
                CutType.ReserveFeedAndPartialCut => EscPosCmdType.GsPaperReservedFeedAndPartialCut,
                CutType.FeedAndFullCutThenTopOfPaper => EscPosCmdType.GsPaperFeedAndFullCutAndTopOfForm,
                CutType.FeedAndPartialCutThenTopOfPaper => EscPosCmdType.GsPaperFeedAndPartialCutAndTopOfForm,
            };
#pragma warning restore CS8524 // The switch expression does not handle all possible inputs when it does
            CommandList.Add(new EscPosCmd(cmdType, cmd));
        }

        //--------------------------------------

        public enum DrawerKick
        {
            Pin2 = '0',
            Pin5 = '1'
        }

        public void GeneratePule(DrawerKick drawerKick, int onDuration, int offDuration)
        {
            byte[] cmd = { 0x1B, 0x70, (byte)drawerKick, (byte)onDuration, (byte)offDuration };
            CommandList.Add(new EscPosCmd(EscPosCmdType.EscGeneratePulse, cmd));
        }

        //--------------------------------------

        private Boolean _PanelButton;

        public Boolean PanelButton
        {
            get { return _PanelButton; }
            set
            {
                _PanelButton = value;
                byte[] cmd = { 0x1B, 0x63, 0x35, (byte)(_PanelButton ? 0x31 : 0x30) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscEnableDisablePanelButton, cmd));
            }
        }

        //--------------------------------------

        public void ReturnHome()
        {
            byte[] cmd = { 0x1B, 0x3C };
            CommandList.Add(new EscPosCmd(EscPosCmdType.EscReturnHome, cmd));
        }

        //--------------------------------------
    }
}