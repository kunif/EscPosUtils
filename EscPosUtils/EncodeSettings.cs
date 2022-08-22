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

        private Boolean _UserSettingMode;

        public Boolean UserSettingMode
        {
            get { return _UserSettingMode; }
            set
            {
                Boolean action = _UserSettingMode != value;
                _UserSettingMode = value;
                if (action)
                {
                    if (value)
                    {
                        byte[] cmd = { 0x1D, 0x28, 0x45, 0x03, 0x00, 0x01, 0x49, 0x4E };
                        CommandList.Add(new EscPosCmd(EscPosCmdType.GsChangeIntoUserSettingMode, cmd));
                    }
                    else
                    {
                        byte[] cmd = { 0x1D, 0x28, 0x45, 0x04, 0x00, 0x02, 0x4F, 0x55, 0x54 };
                        CommandList.Add(new EscPosCmd(EscPosCmdType.GsEndUserSettingMode, cmd));
                    }
                }
            }
        }

        //--------------------------------------

        public class SwitchValue
        {
            public int number;
            public string value = string.Empty;
        }

        public void ChangeMeomorySwitch(List<SwitchValue> newvalues)
        {
            byte[] data = Array.Empty<byte>();
            for (int i = 0; i < newvalues.Count; i++)
            {
                if ((newvalues[i].number < 1) || (newvalues[i].number > 8) || (newvalues[i].value.Length != 8) || (newvalues[i].value.Where(x => (x < '0' || x > '2')).Any()))
                {
                    continue;
                }
                data = data.Concat(new[] { (byte)newvalues[i].number }).ToArray();
                data = data.Concat(Encoding.ASCII.GetBytes(newvalues[i].value)).ToArray();
            }
            if (data.Length == 0)
            {
                return;
            }
            int length = data.Length + 1;
            byte[] cmd = { 0x1D, 0x28, 0x45, (byte)length, (byte)(length >> 8), 0x03 };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsChangeMeomorySwitch, cmd.Concat(data).ToArray()));
        }

        //--------------------------------------

        public void TransmitSettingsMemorySwitch(int switchnumber)
        {
            if ((switchnumber < 1) || (switchnumber > 8))
            {
                return;
            }
            byte[] cmd = { 0x1D, 0x28, 0x45, 0x02, 0x00, 0x04, (byte)switchnumber };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsTransmitSettingsMemorySwitch, cmd));
        }

        //--------------------------------------

        public class CustomizeValue
        {
            public int number;
            public int value;
        }

        public void SetCustomizeSettingValues(List<CustomizeValue> newvalues)
        {
            byte[] data = Array.Empty<byte>();
            for (int i = 0; i < newvalues.Count; i++)
            {
                if ((newvalues[i].number < 1) || (newvalues[i].number > 195))
                {
                    continue;
                }
                byte[] value = new byte[3];
                value[0] = (byte)newvalues[i].number;
                value[1] = (byte)newvalues[i].value;
                value[2] = (byte)(newvalues[i].value >> 8);
                data = data.Concat(value).ToArray();
            }
            if (data.Length == 0)
            {
                return;
            }
            int length = data.Length + 1;
            byte[] cmd = { 0x1D, 0x28, 0x45, (byte)length, (byte)(length >> 8), 0x05 };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsSetCustomizeSettingValues, cmd.Concat(data).ToArray()));
        }

        //--------------------------------------

        public void TransmitCustomizeSettingValues(int valuenumber)
        {
            if ((valuenumber < 1) || (valuenumber > 195))
            {
                return;
            }
            byte[] cmd = { 0x1D, 0x28, 0x45, 0x02, 0x00, 0x06, (byte)valuenumber };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsTransmitCustomizeSettingValues, cmd));
        }

        //--------------------------------------

        public enum FontConfiguration
        {
            w09h17 = 10,
            w12h24 = 12,
            w08h16 = 17,
            w10h24 = 18
        }

        public FontConfiguration UserDefinedFontConfiguration { get; private set; }

        public void CopyUserDefinedPage(FontConfiguration fontconfiguration, Boolean toworkarea)
        {
            if (UserSettingMode)
            {
                UserDefinedFontConfiguration = fontconfiguration;
            }
            else
            {
                UserDefinedFontConfiguration = (FontConfiguration)0;
                return;
            }
            byte[] cmd = { 0x1D, 0x28, 0x45, 0x04, 0x00, 0x07, (byte)fontconfiguration, (byte)(toworkarea ? 0x31 : 0x30), (byte)(toworkarea ? 0x30 : 0x31) };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsCopyUserDefinedPage, cmd));
        }

        //--------------------------------------

        public void DefineColumnFormatCharacterCodePage(int c1, int c2, CharacterDefine[] characters)
        {
            if ((c1 > c2)
              || ((c1 < 128) || (c1 > 255))
              || ((c2 < 128) || (c2 > 255))
              || ((c2 - c1 + 1) != characters.Length)
              || (UserDefinedFontConfiguration == 0)
            )
            { return; }
            int fontx = 0;
            int fonty = 0;

            switch (UserDefinedFontConfiguration)
            {
                case FontConfiguration.w09h17:
                    fontx = 9;
                    fonty = 3;
                    break;

                case FontConfiguration.w12h24:
                    fontx = 12;
                    fonty = 3;
                    break;

                case FontConfiguration.w08h16:
                    fontx = 8;
                    fonty = 2;
                    break;

                case FontConfiguration.w10h24:
                    fontx = 10;
                    fonty = 3;
                    break;
            }
            Boolean irregulardata = false;
            byte[] fontdata = Array.Empty<byte>();
            foreach (CharacterDefine item in characters)
            {
                if ((item.y != fonty) || (item.x < 0) || (item.x > fontx))
                {
                    irregulardata = true;
                    break;
                }
                fontdata = fontdata.Concat(new[] { (byte)item.x }).ToArray();
                if (item.x > 0)
                {
                    fontdata = fontdata.Concat(item.data).ToArray();
                }
            }
            if (!irregulardata)
            {
                int length = fontdata.Length + 4;
                byte[] cmd = { 0x1D, 0x28, 0x45, (byte)length, (byte)(length >> 8), 0x08, (byte)fonty, (byte)c1, (byte)c2 };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsDefineColumnFormatCharacterCodePage, cmd.Concat(fontdata).ToArray()));
            }
        }

        //--------------------------------------

        public void DefineRasterFormatCharacterCodePage(int c1, int c2, CharacterDefine[] characters)
        {
            if ((c1 > c2)
              || ((c1 < 128) || (c1 > 255))
              || ((c2 < 128) || (c2 > 255))
              || ((c2 - c1 + 1) != characters.Length)
              || (UserDefinedFontConfiguration == 0)
            )
            { return; }
            int fontx = 0;
            int fonty = 0;

            switch (UserDefinedFontConfiguration)
            {
                case FontConfiguration.w09h17:
                    fontx = 2;
                    fonty = 17;
                    break;

                case FontConfiguration.w12h24:
                    fontx = 2;
                    fonty = 24;
                    break;

                case FontConfiguration.w08h16:
                    fontx = 1;
                    fonty = 16;
                    break;

                case FontConfiguration.w10h24:
                    fontx = 2;
                    fonty = 24;
                    break;
            }
            Boolean irregulardata = false;
            byte[] fontdata = Array.Empty<byte>();
            foreach (CharacterDefine item in characters)
            {
                if ((item.x != fontx) || (item.y < 0) || (item.y > fonty))
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
                int length = fontdata.Length + 4;
                byte[] cmd = { 0x1D, 0x28, 0x45, (byte)length, (byte)(length >> 8), 0x09, (byte)fontx, (byte)c1, (byte)c2 };
                CommandList.Add(new EscPosCmd(EscPosCmdType.GsDefineRasterFormatCharacterCodePage, cmd.Concat(fontdata).ToArray()));
            }
        }

        //--------------------------------------

        public void DeleteCharacterCodePage(int c1, int c2)
        {
            if ((c1 > c2)
              || ((c1 < 128) || (c1 > 255))
              || ((c2 < 128) || (c2 > 255))
              || (UserDefinedFontConfiguration == 0)
            )
            { return; }
            byte[] cmd = { 0x1D, 0x28, 0x45, 0x03, 0x00, 0x0A, (byte)c1, (byte)c2 };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsDeleteCharacterCodePage, cmd));
        }

        //--------------------------------------

        public enum SerialInformationType
        {
            BaudRate = 1,
            Parity = 2,
            FlowControl = 3,
            DataBits = 4
        }

        public void SetSerialInterface(SerialInformationType serialInformationType, int value)
        {
            string strvalue = serialInformationType switch
            {
                SerialInformationType.BaudRate => value switch   // BaudRate
                {
                    1200 => "1200",
                    2400 => "2400",
                    4800 => "4800",
                    9600 => "9600",
                    19200 => "19200",
                    38400 => "38400",
                    57600 => "57600",
                    115200 => "115200",
                    _ => ""
                },
                SerialInformationType.Parity => value switch   // Parity
                {
                    0 => "0",   // None
                    1 => "1",   // Odd
                    2 => "2",   // Even
                    _ => ""
                },
                SerialInformationType.FlowControl => value switch   // FlowControl
                {
                    0 => "0",   // DSR/DTR
                    1 => "1",   // XON/XOFF
                    _ => ""
                },
                SerialInformationType.DataBits => value switch   // DataBits
                {
                    7 => "7",
                    8 => "8",
                    _ => ""
                },
                _ => ""
            };
            if (string.IsNullOrWhiteSpace(strvalue))
            {
                return;
            }
            int length = strvalue.Length + 2;
            byte[] cmd = { 0x1D, 0x28, 0x45, (byte)length, (byte)(length >> 8), 0x0B, (byte)serialInformationType };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsSetSerialInterface, cmd.Concat(Encoding.ASCII.GetBytes(strvalue)).ToArray()));
        }

        //--------------------------------------

        public void TransmitSerialInterface(SerialInformationType serialInformationType)
        {
            byte[] cmd = { 0x1D, 0x28, 0x45, 0x02, 0x00, 0x0C, (byte)serialInformationType };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsTransmitSerialInterface, cmd));
        }

        //--------------------------------------

        public enum BluetoothInformationType
        {
            DeviceAddress = '0',
            PassKey = '1',
            DeviceName = 'A',
            BundleSeedID = 'F',
            AutomaticReconnection = 'I'
        }

        public void SetBluetoothInterface(BluetoothInformationType bluetoothInformationType, string value)
        {
            if (bluetoothInformationType == BluetoothInformationType.DeviceAddress)
            {
                return;
            }
            int length = value.Length + 2;
            byte[] cmd = { 0x1D, 0x28, 0x45, (byte)length, (byte)(length >> 8), 0x0D, (byte)bluetoothInformationType };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsSetBluetoothInterface, cmd.Concat(Encoding.ASCII.GetBytes(value)).ToArray()));
        }

        //--------------------------------------

        public void TransmitBluetoothInterface(BluetoothInformationType bluetoothInformationType)
        {
            byte[] cmd = { 0x1D, 0x28, 0x45, 0x02, 0x00, 0x0E, (byte)bluetoothInformationType };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsTransmitBluetoothInterface, cmd));
        }

        //--------------------------------------

        public enum USBInformationType
        {
            Class = 1,
            IEEE1284DeviceID = 32
        }

        public enum USBInformationValue
        {
#pragma warning disable CA1069 // Enums should not have duplicate values
            VendorClass = '0',
            PrinterClass = '1',
            DoesNotTransmitDeviceID = '0',
            TransmitDeviceID = '1'
#pragma warning restore CA1069 // Enums should not have duplicate values
        }

        public void SetUSBInterface(USBInformationType usbInformationType, USBInformationValue value)
        {
            byte[] cmd = { 0x1D, 0x28, 0x45, 0x03, 0x00, 0x0F, (byte)usbInformationType, (byte)value };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsSetUSBInterface, cmd));
        }

        //--------------------------------------

        public void TransmitUSBInterface(USBInformationType usbInformationType)
        {
            byte[] cmd = { 0x1D, 0x28, 0x45, 0x02, 0x00, 0x10, (byte)usbInformationType };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsTransmitUSBInterface, cmd));
        }

        //--------------------------------------

        public void DeletePaperLayout()
        {
            byte[] cmd = { 0x1D, 0x28, 0x45, 0x04, 0x00, 0x30, 0x43, 0x4C, 0x52 };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsDeletePaperLayout, cmd));
        }

        //--------------------------------------

        public enum LayoutBasePoint
        {
            None = '0',
            TopOfBlackmark = '1',
            BottomOfLabel = '@'
        }

        public void SetPaperLayout(LayoutBasePoint layoutbase, string sb, string sc, string sd, string se, string sf, string sg, string sh)
        {
            string layout = ";" + sb + ";" + sc + ";" + sd + ";" + se + ";" + sf + ";" + sg + ";" + sh + ";";
            int length = layout.Length + 2;
            byte[] cmd = { 0x1D, 0x28, 0x45, (byte)length, (byte)(length >> 8), 0x31, (byte)layoutbase };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsSetPaperLayout, cmd.Concat(Encoding.ASCII.GetBytes(layout)).ToArray()));
        }

        //--------------------------------------

        public void TransmitPaperLayout(UnitType unittype)
        {
            byte[] cmd = { 0x1D, 0x28, 0x45, 0x02, 0x00, 0x32, (byte)unittype };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsTransmitPaperLayout, cmd));
        }

        //--------------------------------------

        public void SaveSettingsValuesFromWorkToStorage()
        {
            byte[] cmd = { 0x1D, 0x28, 0x4D, 0x02, 0x00, 0x31, 0x31 };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsSaveSettingsValuesFromWorkToStorage, cmd));
        }

        //--------------------------------------

        public void LoadSettingsValuesFromStorageToWork(Boolean initialvalue)
        {
            byte[] cmd = { 0x1D, 0x28, 0x4D, 0x02, 0x00, 0x32, (byte)(initialvalue ? 0x30 : 0x31) };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsLoadSettingsValuesFromStorageToWork, cmd));
        }

        //--------------------------------------

        public void SelectSettingsValuesToWorkAfterInitialize(Boolean initialvalue)
        {
            byte[] cmd = { 0x1D, 0x28, 0x4D, 0x02, 0x00, 0x33, (byte)(initialvalue ? 0x30 : 0x31) };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsSelectSettingsValuesToWorkAfterInitialize, cmd));
        }

        //--------------------------------------
    }
}