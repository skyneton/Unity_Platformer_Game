﻿using System;
using Network;

public class PacketInHealthUpdate : Packet
{
    public void Write(ByteBuf buf)
    {
    }

    public void Read(NetworkManager networkManager, ByteBuf buf)
    {
        try
        {
            InGameDataManager.inGameManager.PlayerHealthUpdate(new Guid(buf.Read(16)).ToString(), buf.ReadFloat());
        }
        catch (NullReferenceException)
        {
            
        }
    }
}