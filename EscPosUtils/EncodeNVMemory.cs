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

        public void ObsoleteWriteNVUserMemory(int address, byte[] data)
        {
            int datalen = data.Length;
            byte[] cmd = { 0x1C, 0x67, 0x31, (byte)address, (byte)(address >> 8), (byte)(address >> 16), (byte)(address >> 24), (byte)datalen, (byte)(datalen >> 8) };
            cmd = cmd.Concat(data).ToArray();
            CommandList.Add(new EscPosCmd(EscPosCmdType.FsObsoleteWriteNVUserMemory, cmd));
        }

        //--------------------------------------

        public void ObsoleteReadNVUserMemory(int address, int readlen)
        {
            byte[] cmd = { 0x1C, 0x67, 0x32, (byte)address, (byte)(address >> 8), (byte)(address >> 16), (byte)(address >> 24), (byte)readlen, (byte)(readlen >> 8) };
            CommandList.Add(new EscPosCmd(EscPosCmdType.FsObsoleteReadNVUserMemory, cmd));
        }

        //--------------------------------------

        public void DeleteSpecifiedRecord(byte kc1 = 0x20, byte kc2 = 0x20)
        {
            byte[] cmd = { 0x1D, 0x28, 0x43, 0x05, 0x00, 0x00, 0x30, 0x00, kc1, kc2 };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsDeleteSpecifiedRecord, cmd));
        }

        //--------------------------------------

        public void StoreDataSpecifiedRecord(byte[] data, byte kc1 = 0x20, byte kc2 = 0x20)
        {
            int cmdlen = 5 + data.Length;
            byte[] cmd = { 0x1D, 0x28, 0x43, (byte)cmdlen, (byte)(cmdlen>>8), 0x00, 0x31, 0x00, kc1, kc2 };
            cmd = cmd.Concat(data).ToArray();
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsStoreDataSpecifiedRecord, cmd));
        }

        //--------------------------------------

        public void TransmitDataSpecifiedRecord(byte kc1 = 0x20, byte kc2 = 0x20)
        {
            byte[] cmd = { 0x1D, 0x28, 0x43, 0x05, 0x00, 0x00, 0x32, 0x00, kc1, kc2 };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsTransmitDataSpecifiedRecord, cmd));
        }

        //--------------------------------------

        public void TransmitCapacityNVUserMemory()
        {
            byte[] cmd = { 0x1D, 0x28, 0x43, 0x03, 0x00, 0x00, 0x33, 0x00 };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsTransmitCapacityNVUserMemory, cmd));
        }

        //--------------------------------------

        public void TransmitRemainingCapacityNVUserMemory()
        {
            byte[] cmd = { 0x1D, 0x28, 0x43, 0x05, 0x00, 0x00, 0x34, 0x00 };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsTransmitRemainingCapacityNVUserMemory, cmd));
        }

        //--------------------------------------

        public void TransmitKeycodeList()
        {
            byte[] cmd = { 0x1D, 0x28, 0x43, 0x05, 0x00, 0x00, 0x35, 0x00 };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsTransmitKeycodeList, cmd));
        }

        //--------------------------------------

        public void DeleteAllDataNVMemory()
        {
            byte[] cmd = { 0x1D, 0x28, 0x43, 0x05, 0x00, 0x00, 0x36, 0x00, 0x43, 0x4C, 0x52 };
            CommandList.Add(new EscPosCmd(EscPosCmdType.GsDeleteAllDataNVMemory, cmd));
        }

        //--------------------------------------
    }
}