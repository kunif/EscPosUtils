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

    public partial class EscPosEncoder
    {
        //--------------------------------------

        private Boolean _AutomaticStatusBackOption;

        public Boolean AutomaticStatusBackOption
        {
            get { return _AutomaticStatusBackOption; }
            set
            {
                _AutomaticStatusBackOption = value;
                byte[] cmd = { 0x1C, 0x28, 0x65, 0x02, 0x00, 0x33, (byte)(value ? 0x08 : 0x00) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.FsEnableDisableAutomaticStatusBackOptional, cmd));
            }
        }

        //--------------------------------------

        [Flags]
        public enum ASBcommand
        {
            None = 0x00,
            Drawer = 0x01,
            OnlineOffline = 0x02,
            Error = 0x04,
            RollPaperSensor = 0x08,
            PanelSwitch = 0x40
        }

        private ASBcommand _ASBNotify;

        public ASBcommand ASBNotify
        {
            get { return _ASBNotify; }
            set
            {
                _ASBNotify = value;
                byte[] cmd = { 0x1D, 0x61, (byte)value };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBack, cmd));
            }
        }

        //--------------------------------------

        public enum PaperSensor
        {
            None = 0,
            NearEnd = 3,
            Empty = 0x0C,
            Empty_NearEnd = 0x0F
        }

        private PaperSensor _PaperEndSensor;

        public PaperSensor PaperEndSensor
        {
            get { return _PaperEndSensor; }
            set
            {
                _PaperEndSensor = value;
                byte[] cmd = { 0x1B, 0x63, 0x33, (byte)((int)value) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscSelectPaperSensorsPaperEndSignals, cmd));
            }
        }

        private PaperSensor _PrintStopSensor;

        public PaperSensor PrintStopSensor
        {
            get { return _PrintStopSensor; }
            set
            {
                _PrintStopSensor = value;
                byte[] cmd = { 0x1B, 0x63, 0x34, (byte)((int)value) };
                CommandList.Add(new EscPosCmd(EscPosCmdType.EscSelectPaperSensorsStopPrinting, cmd));
            }
        }

        //--------------------------------------

        public enum TransmitStatusType
        {
            PaperSensor = 0x31,
            DrawerKickConnector = 0x32,
            InkCartridge = 0x34
        }

        public void TransmitStatus(TransmitStatusType statusType)
        {
            byte[] cmd = { 0x1D, 0x72, (byte)statusType };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsTransmitStatus, cmd));
        }

        //--------------------------------------

        [Flags]
        public enum InkStatusNotify
        {
            None = 0x00,
            Mechanism = 0x01,
            Capacity = 0x02
        }

        private InkStatusNotify _InkNotify;

        public InkStatusNotify InkNotify
        {
            get { return _InkNotify; }
            set
            {
                _InkNotify = value;
                byte[] cmd = { 0x1D, 0x61, (byte)value };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsEnableDisableAutomaticStatusBackInk, cmd));
            }
        }

        //--------------------------------------

        public void SetOnlineRecoveryWaitTime(byte paperSetWait, byte resumeCheckWait)
        {
            byte[] cmd = { 0x1D, 0x7A, 0x20, paperSetWait, resumeCheckWait };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsSetOnlineRecoveryWaitTime, cmd));
        }

        //--------------------------------------
    }
}