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
    using System.Linq;
    using System.Text;

    public partial class EscPosEncoder
    {
        //--------------------------------------

        public void InitializeMaintenanceCounter(int counternumber)
        {
            if ((counternumber < 10) || (counternumber > 79))
            {
                return;
            }
            byte[] cmd = { 0x1D, 0x67, 0x30, 0x00, (byte)counternumber, (byte)(counternumber >> 8) };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsInitializeMaintenanceCounter, cmd));
        }

        //--------------------------------------

        public void TransmitMaintenanceCounter(int counternumber)
        {
            if (!(((counternumber >= 10) && (counternumber <= 79)) || ((counternumber >= 138) && (counternumber <= 207))) )
            {
                return;
            }
            byte[] cmd = { 0x1D, 0x67, 0x30, 0x00, (byte)counternumber, (byte)(counternumber >> 8) };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsTransmitMaintenanceCounter, cmd));
        }

        //--------------------------------------

        public enum TestPaperType
        {
            BasicSheet = 0x30,
            RollPaper1 = 0x31,
            RollPaper2 = 0x32,
            SlipFaceSide = 0x33,
            Validation = 0x34,
            SlipBackSide = 0x35
        }

        public enum TestPatternType
        {
            HexadecimalDump = 0x31,
            PrinterStatus = 0x32,
            RollingPattern = 0x33,
            AutomaticSettingOfPaperLayout = 0x40
        }

        public void ExecuteTestPrint(TestPaperType paper, TestPatternType pattern)
        {
            byte[] cmd = { 0x1D, 0x28, 0x41, 0x02, 0x00, (byte)paper, (byte)pattern };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsExecuteTestPrint, cmd));
        }

        //--------------------------------------
    }
}