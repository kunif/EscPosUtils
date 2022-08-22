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

        //--------------------------------------

        public enum LayoutPaperType
        {
            ReceiptNoMark = '0',
            DieCutLabelNoMark = '1',
            DieCutLabelWithMark = '2',
            ReceiptWithMark = '3'
        }

        public void PaperLayoutSetting(LayoutPaperType papertype, string sb, string sc, string sd, string se, string sf, string sg, string sh)
        {
            string layout = ";" + sb + ";" + sc + ";" + sd + ";" + se + ";" + sf + ";" + sg + ";" + sh + ";";
            int length = layout.Length + 2;
            byte[] cmd = { 0x1C, 0x28, 0x4C, (byte)length, (byte)(length >> 8), 0x21, (byte)papertype };
            CommandList.Add(new EscPosCmd(EscPosCmdType.FsPaperLayoutSetting, cmd.Concat(Encoding.ASCII.GetBytes(layout)).ToArray()));
        }

        //--------------------------------------

        public enum UnitType
        {
            SubMilimeter = 64,
            Dots = 80
        }

        public void PaperLayoutInformationTransmission(UnitType unittype)
        {
            byte[] cmd = { 0x1C, 0x28, 0x4C, 0x02, 0x00, 0x22, (byte)unittype };
            CommandList.Add(new EscPosCmd(EscPosCmdType.FsPaperLayoutInformationTransmission, cmd));
        }

        //--------------------------------------

        public void TransmitPositioningInformation()
        {
            byte[] cmd = { 0x1C, 0x28, 0x4C, 0x02, 0x00, 0x30, 0x30 };
            CommandList.Add(new EscPosCmd(EscPosCmdType.FsTransmitPositioningInformation, cmd));
        }

        //--------------------------------------

        public enum TargetPosition
        {
            LabelPeeling = 'A',
            Cutting = 'B',
            PrintStarting = 'C'
        }

        public enum MovingRule
        {
            StayAlready = '0',
            ForceNext = '1',
            CurrentTop = '2'
        }

        public void FeedPaper(TargetPosition targetposition, MovingRule movingrule)
        {
            if ((targetposition != TargetPosition.PrintStarting) && (movingrule == MovingRule.CurrentTop))
            { return; }
            EscPosCmdType cmdType = targetposition switch
            {
                TargetPosition.LabelPeeling => EscPosCmdType.FsFeedPaperLabelPeelingPosition,
                TargetPosition.Cutting => EscPosCmdType.FsFeedPaperCuttingPosition,
                TargetPosition.PrintStarting => EscPosCmdType.FsFeedPaperPrintStartingPosition,
                _ => throw new NotImplementedException()
            };
            byte[] cmd = { 0x1C, 0x28, 0x4C, 0x02, 0x00, (byte)targetposition, (byte)movingrule };
            CommandList.Add(new EscPosCmd(cmdType, cmd));
        }

        //--------------------------------------

        public void PaperLayoutErrorSpecialMarginSetting(int specialmargin)
        {
            if ((specialmargin < 0) || (specialmargin > 50))
            { return; }
            byte[] margin = Encoding.ASCII.GetBytes(specialmargin.ToString());
            byte[] cmd = { 0x1C, 0x28, 0x4C, (byte)(margin.Length + 1), 0x00, 0x43 };
            CommandList.Add(new EscPosCmd(EscPosCmdType.FsPaperLayoutErrorSpecialMarginSetting, cmd.Concat(margin).ToArray()));
        }

        //--------------------------------------
    }
}