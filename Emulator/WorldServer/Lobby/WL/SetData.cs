﻿using System;

namespace WorldServer.Lobby.WL
{
    class SetData : Packet
    {
        public SetData(Byte IP1, Byte IP2, Byte IP3, Byte IP4, UInt32 Port, Byte Population) : base()
        {
            WriteByte((Byte)OpCodes.WL_SET_DATA);
            WriteByte(IP1);
            WriteByte(IP2);
            WriteByte(IP3);
            WriteByte(IP4);
            WriteD(Port);
            WriteByte(Population);
        }
    }
}
